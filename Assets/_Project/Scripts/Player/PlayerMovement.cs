using UnityEngine;
using UnityEngine.InputSystem;

namespace BasePlatformer.Player
{
    /// <summary>
    /// 좌우 이동(가속/감속) 구현.
    ///
    /// 스펙 참조: Docs/Design/CharacterControls/versions/캐릭터컨트롤_스펙_ver1.md - 2장 (이동 스펙)
    ///   - 최고 이동 속도: 6.0 타일/초
    ///   - 최고 속도까지 가속 시간: 0.15초
    ///   - 정지까지 감속 시간: 0.10초
    ///   - 방향 전환 시: 정지와 동일한 감속 후 재가속 적용
    ///
    /// 단위: 본 프로젝트는 Tilemap Grid의 cellSize = 1 로 고정했으므로
    ///       "1 타일 = 1 Unity Unit" 으로 그대로 환산해서 사용한다.
    ///
    /// 기술기획서_ver3.md 4.1: 이동 값은 Rigidbody2D 물리 시뮬레이션에 맡기지 않고
    ///       스크립트에서 직접 가속도를 계산해 속도를 갱신한다.
    ///
    /// 충돌/지형 처리는 별도 ToDo 항목("일반 발판 충돌 처리")에서 다루므로,
    /// 이 스크립트는 좌우 이동 로직만 담당하고 Y축 속도는 건드리지 않는다.
    /// (확장 기능 문서의 "이동 로직과 충돌 로직 분리" 원칙)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [Header("이동 스펙 (캐릭터컨트롤_스펙_ver1.md 2장)")]
        //[Tooltip("최고 이동 속도 (타일/초 = Unity Unit/초)")]
        private float maxMoveSpeed => playerData != null ? playerData.MoveSpeed : 6f;

        [Tooltip("0 -> 최고 속도까지 걸리는 시간(초)")]
        [SerializeField] private float accelerationTime = 0.15f;

        [Tooltip("최고 속도 -> 0까지 걸리는 시간(초). 방향 전환 시에도 동일하게 적용")]
        [SerializeField] private float decelerationTime = 0.10f;

        [Header("이동 경계")]
        [Tooltip("플레이어가 이 X 좌표 왼쪽으로는 이동할 수 없음 (레벨 왼쪽 끝, CameraBounds 기준)")]
        [SerializeField] private float leftBoundX = -0.5f;
        [Tooltip("플레이어가 이 X 좌표 오른쪽으로는 이동할 수 없음")]
        [SerializeField] private float rightBoundX = 60.5f;

        private Rigidbody2D rb;
        private InputAction moveAction;

        /// <summary> 현재 좌우 이동 속도 (Unity Unit/초, +면 우측, -면 좌측) </summary>
        public float CurrentSpeed { get; private set; }

        /// <summary> 가장 최근에 읽은 좌우 입력값 (-1 ~ 1) </summary>
        public float MoveInput { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // Move 액션: A/D, 좌우 화살표 키를 1D Axis 컴포지트로 바인딩.
            // (Input System 패키지 사용 - 기술기획서_ver3.md 5장)
            moveAction = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Axis");
            moveAction.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/a")
                .With("Negative", "<Keyboard>/leftArrow")
                .With("Positive", "<Keyboard>/d")
                .With("Positive", "<Keyboard>/rightArrow");
        }

        private void OnEnable()
        {
            moveAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
        }

        private void Update()
        {
            MoveInput = moveAction.ReadValue<float>();
        }

        private float knockbackTimer = 0f;

        public void ApplyKnockback(Vector2 force, float duration)
        {
            knockbackTimer = duration;
            CurrentSpeed = 0f; // 기존 이동 속도 초기화
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        private void FixedUpdate()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.fixedDeltaTime;
                ApplyBoundaries();
                return;
            }

            float targetSpeed = MoveInput * maxMoveSpeed;

            // 가속 중인지 판정:
            bool startingFromRest = Mathf.Approximately(CurrentSpeed, 0f) && !Mathf.Approximately(targetSpeed, 0f);
            bool sameDirectionSpeedUp =
                Mathf.Sign(CurrentSpeed) == Mathf.Sign(targetSpeed) &&
                Mathf.Abs(targetSpeed) > Mathf.Abs(CurrentSpeed);
            bool isAccelerating = startingFromRest || sameDirectionSpeedUp;

            float accelRate = maxMoveSpeed / Mathf.Max(accelerationTime, 0.0001f);
            float decelRate = maxMoveSpeed / Mathf.Max(decelerationTime, 0.0001f);
            float rate = isAccelerating ? accelRate : decelRate;

            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, targetSpeed, rate * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector2(CurrentSpeed, rb.linearVelocity.y);

            ApplyBoundaries();
        }

        private void ApplyBoundaries()
        {
            // 왼쪽 경계 클램프: leftBoundX 이하로는 이동 불가
            if (rb.position.x < leftBoundX)
            {
                rb.position = new Vector2(leftBoundX, rb.position.y);
                if (CurrentSpeed < 0f) CurrentSpeed = 0f;
            }

            // 오른쪽 경계 클램프: rightBoundX 이상으로는 이동 불가
            if (rb.position.x > rightBoundX)
            {
                rb.position = new Vector2(rightBoundX, rb.position.y);
                if (CurrentSpeed > 0f) CurrentSpeed = 0f;
            }
        }
    }
}
