using UnityEngine;
using UnityEngine.InputSystem;

namespace BasePlatformer.Player
{
    /// <summary>
    /// 점프(수직 이동) 구현 - 기본 점프 + 가변 점프.
    ///
    /// 스펙 참조: Docs/Design/CharacterControls/versions/캐릭터컨트롤_스펙_ver1.md
    ///   - 3.1장 (기본 점프): 최대 점프 높이 3.0타일, 최대 높이까지 0.35초
    ///   - 3.2장 (가변 점프): 짧게 탭(최소 입력) 시 최소 높이 1.2타일,
    ///                        버튼을 떼는 순간 상승 속도를 약 50% 즉시 감소
    ///     (스펙 0장: "가변 점프는 MVP 핵심 기획서의 '기본 점프' 항목에 포함된 세부 사양"
    ///      이므로 PlayerJump 클래스 하나에서 같이 처리한다.)
    ///
    /// 단위: "1 타일 = 1 Unity Unit" (Tilemap Grid cellSize = 1 고정 기준, PlayerMovement.cs와 동일)
    ///
    /// 점프 구현 방식 (기술기획서_ver4.md 4.1):
    ///   - Rigidbody2D의 기본 중력에 맡기지 않고, 목표 점프 높이/시간으로부터
    ///     초기 속도와 중력 가속도를 직접 역산해서 사용한다.
    ///     h = 0.5 * v0 * t  =>  v0 = 2h/t,  g = v0/t = 2h/t^2
    ///   - gravityScale은 0으로 두고(PlayerMovement 설정과 동일), 이 스크립트가
    ///     매 FixedUpdate마다 직접 수직 속도를 갱신한다.
    ///
    /// 가변 점프 로직:
    ///   - 점프 버튼을 떼는 순간(release), 아직 상승 중(velocity.y > 0)이면 속도를 50%로 즉시 감쇠.
    ///   - 다만 "짧게 탭해도 최소 1.2타일은 올라간다"는 스펙을 보장하기 위해,
    ///     이번 점프에서 지금까지 올라간 높이(jumpStartY 기준)가 아직 minJumpHeight에 못 미치면,
    ///     남은 높이를 채울 수 있는 속도(floorVelocity)와 50% 감쇠 속도 중 더 큰 값을 사용한다.
    ///     -> 즉시 탭(release) 시: 50% 감쇠 속도(~8.57) < floorVelocity(~10.84) 이므로 floorVelocity가
    ///        적용되어 정확히 1.2타일에서 정점을 찍는다 (스펙 수치와 정합).
    ///     -> 조금 더 길게 누르다 뗀 경우: 이미 1.2타일에 근접/초과했거나 50% 감쇠 속도가 더 크므로
    ///        자연스럽게 더 높은 궤적이 나온다 (탭 길이에 따라 연속적으로 증가하는 점프 높이).
    ///
    /// 코요테 타임 (캐릭터컨트롤_스펙_ver1.md 3.3장):
    ///   - 적용 시간: 0.12초. 발판에서 떨어진 직후에도 그 시간 동안은 점프가 가능하다.
    ///   - 적용 조건: 발판에서 떨어진 직후, 점프 입력이 없었던 경우에만 적용.
    ///     그라운드에 있는 동안 코요테 타이머를 항상 가득 채워두고, 공중에서는 줄어들게 하다가,
    ///     점프가 실제로 발생하는 순간 타이머를 0으로 소진시킨다. 따라서 스스로 점프해서 공중에 뜬
    ///     경우는 이미 소진된 상태라 코요테 타임의 영향을 받지 않고, 발판을 걸어서 벗어난 경우에만
    ///     그라운드에 있을 때 채워졌던 값이 그대로 남아 grace window로 동작한다.
    ///
    /// 접지 판정: 정식 지형 충돌 시스템("일반 발판 충돌 처리" ToDo)이 아직 없으므로,
    /// 발 밑을 살짝 겹치는 OverlapBox로 Ground 레이어를 감지하는 최소 구현을 사용한다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerJump : MonoBehaviour
    {
        [Header("기본 점프 스펙 (캐릭터컨트롤_스펙_ver1.md 3.1장)")]
        [Tooltip("최대 점프 높이 (타일 = Unity Unit)")]
        [SerializeField] private float maxJumpHeight = 3.0f;

        [Tooltip("최대 점프 높이까지 도달하는 시간(초)")]
        [SerializeField] private float timeToApex = 0.35f;

        [Header("가변 점프 스펙 (캐릭터컨트롤_스펙_ver1.md 3.2장)")]
        [Tooltip("짧게 탭(최소 입력) 했을 때 보장되는 최소 점프 높이 (타일)")]
        [SerializeField] private float minJumpHeight = 1.2f;

        [Tooltip("점프 버튼을 떼는 순간, 상승 속도에 곱해지는 비율 (50% 감소 = 0.5배로 남김)")]
        [SerializeField] private float jumpCutMultiplier = 0.5f;

        [Header("코요테 타임 스펙 (캐릭터컨트롤_스펙_ver1.md 3.3장)")]
        [Tooltip("발판에서 떨어진 후에도 점프를 허용해주는 시간(초)")]
        [SerializeField] private float coyoteTime = 0.12f;

        [Header("접지 판정 (임시 - 정식 충돌 시스템 구현 전까지)")]
        [Tooltip("바닥으로 인식할 레이어")]
        [SerializeField] private LayerMask groundLayer;

        [Tooltip("발 밑 접지 판정 박스의 두께(Unity Unit)")]
        [SerializeField] private float groundCheckThickness = 0.05f;

        private Rigidbody2D rb;
        private Collider2D bodyCollider;

        private float jumpVelocity;   // 역산된 초기 점프 속도 (위 방향, +)
        private float gravity;        // 역산된 중력 가속도 (아래 방향으로 적용, 양수 값)

        private float jumpStartY;        // 이번 점프가 시작된 월드 Y 좌표 (최소 높이 보장 계산용)
        private bool isJumpCutApplied;    // 이번 점프에서 가변 점프 컷이 이미 적용됐는지 (중복 적용 방지)
        private float coyoteTimer;        // 남은 코요테 타임 (그라운드에 있으면 항상 coyoteTime으로 채워짐)

        /// <summary> 현재 접지 상태 여부 </summary>
        public bool IsGrounded { get; private set; }

        private InputAction jumpAction;

        // Input System의 WasPressedThisFrame()/WasReleasedThisFrame()은 "Dynamic Update"(일반 렌더 프레임)
        // 기준으로 판정된다. FixedUpdate는 별도의 고정 간격으로 호출되기 때문에, FixedUpdate 안에서 직접
        // 읽으면 눌리거나 떼진 순간의 프레임에 FixedUpdate가 호출되지 않는 경우 입력을 통째로 놓칠 수 있다.
        // -> Update에서 먼저 buffer에 잡아두고 FixedUpdate에서 소비한다.
        private bool jumpPressedBuffered;
        private bool jumpReleasedBuffered;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();

            // h = 0.5 * v0 * t  =>  v0 = 2h/t
            // g = v0 / t = 2h / t^2
            float t = Mathf.Max(timeToApex, 0.0001f);
            jumpVelocity = 2f * maxJumpHeight / t;
            gravity = 2f * maxJumpHeight / (t * t);

            // Jump 액션: Space / W / 위쪽 화살표
            jumpAction = new InputAction(name: "Jump", type: InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Keyboard>/w");
            jumpAction.AddBinding("<Keyboard>/upArrow");
        }

        private void OnEnable()
        {
            jumpAction.Enable();
        }

        private void OnDisable()
        {
            jumpAction.Disable();
            jumpPressedBuffered = false;
            jumpReleasedBuffered = false;
        }

        private void Update()
        {
            // 눌리거나 떼진 순간을 놓치지 않도록 buffer에 OR로 누적해두고, FixedUpdate에서 소비한다.
            if (jumpAction.WasPressedThisFrame())
            {
                jumpPressedBuffered = true;
            }

            if (jumpAction.WasReleasedThisFrame())
            {
                jumpReleasedBuffered = true;
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();

            // 코요테 타이머 갱신: 그라운드에 있으면 항상 가득 채워두고, 공중에서는 흘러내린다.
            if (IsGrounded)
            {
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer -= Time.fixedDeltaTime;
            }

            bool jumpRequested = jumpPressedBuffered;
            jumpPressedBuffered = false; // 소비

            bool releaseRequested = jumpReleasedBuffered;
            jumpReleasedBuffered = false; // 소비

            bool canJump = coyoteTimer > 0f;

            if (jumpRequested && canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                IsGrounded = false;
                coyoteTimer = 0f; // 점프가 실제로 발생했으므로 즉시 소진 (중복/연속 코요테 점프 방지)
                jumpStartY = rb.position.y;
                isJumpCutApplied = false;
            }
            else
            {
                float newVerticalSpeed = rb.linearVelocity.y - gravity * Time.fixedDeltaTime;

                // 접지 상태이고 더 떨어지려는 방향이면 0으로 고정 (바닥에 안정적으로 서있도록)
                if (IsGrounded && newVerticalSpeed < 0f)
                {
                    newVerticalSpeed = 0f;
                }

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, newVerticalSpeed);
            }

            // 가변 점프: 버튼을 뗀 시점에 아직 상승 중이면 속도를 즉시 감쇠 (최소 점프 높이는 보장)
            if (releaseRequested && !isJumpCutApplied && rb.linearVelocity.y > 0f)
            {
                ApplyJumpCut();
            }
        }

        private void ApplyJumpCut()
        {
            float heightClimbed = rb.position.y - jumpStartY;
            float remainingForMin = minJumpHeight - heightClimbed;

            float cutVelocity = rb.linearVelocity.y * jumpCutMultiplier;
            float resultVelocity = cutVelocity;

            if (remainingForMin > 0f)
            {
                // 남은 높이를 채우는 데 필요한 속도(v = sqrt(2*g*h))와 50% 감쇠 속도 중 더 큰 값을 사용해
                // 최소 점프 높이를 보장한다.
                float floorVelocity = Mathf.Sqrt(2f * gravity * remainingForMin);
                resultVelocity = Mathf.Max(cutVelocity, floorVelocity);
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, resultVelocity);
            isJumpCutApplied = true;
        }

        private void UpdateGroundedState()
        {
            Vector2 size = bodyCollider.bounds.size;
            Vector2 center = bodyCollider.bounds.center;
            Vector2 checkCenter = new Vector2(center.x, center.y - size.y / 2f);
            Vector2 checkSize = new Vector2(size.x * 0.9f, groundCheckThickness);

            IsGrounded = Physics2D.OverlapBox(checkCenter, checkSize, 0f, groundLayer);
        }
    }
}
