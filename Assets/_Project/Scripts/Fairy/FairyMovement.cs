using UnityEngine;

public class FairyMovement : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("플레이어의 Transform을 할당해주세요.")]
    public Transform player;

    [Tooltip("플레이어로부터 유지할 기본 오프셋(거리 및 방향)")]
    public Vector3 offset = new Vector3(-1.5f, 1.5f, 0f);

    [Header("Follow Settings")]
    [Tooltip("요정이 플레이어를 따라가는 데 걸리는 지연 시간 (값이 클수록 더 부드럽고 느리게 따라갑니다)")]
    public float smoothTime = 0.3f;

    [Tooltip("요정의 최대 이동 속도")]
    public float maxSpeed = 15f;

    [Header("Hover Settings (둥둥 떠다니는 효과)")]
    public bool enableHover = true;
    [Tooltip("위아래로 움직이는 폭")]
    public float hoverAmplitude = 0.2f;
    [Tooltip("위아래로 움직이는 속도")]
    public float hoverFrequency = 2f;

    public enum UpdateType { Update, LateUpdate, FixedUpdate }
    
    [Header("Optimization / Fix Jitter")]
    [Tooltip("떨림(Jitter)이 발생할 경우 이 값을 변경해보세요. 플레이어가 Rigidbody로 이동한다면 FixedUpdate를 추천합니다.")]
    public UpdateType updateType = UpdateType.LateUpdate;

    private Vector3 currentVelocity = Vector3.zero;
    private float hoverTimer = 0f;
    private Vector3 currentSmoothPosition;

    private void Start()
    {
        // 초기 위치 설정
        currentSmoothPosition = transform.position;
    }

    private void Update()
    {
        if (updateType == UpdateType.Update)
            FollowTarget(Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (updateType == UpdateType.LateUpdate)
            FollowTarget(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (updateType == UpdateType.FixedUpdate)
            FollowTarget(Time.fixedDeltaTime);
    }

    private void FollowTarget(float deltaTime)
    {
        if (player == null) return;

        // 1. 목표 위치 계산
        Vector3 targetPosition = player.position + offset;

        // 2. 현재 부드러운 이동 위치를 업데이트 (hover 효과 제외)
        currentSmoothPosition = Vector3.SmoothDamp(
            currentSmoothPosition, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime, 
            maxSpeed, 
            deltaTime
        );

        // 3. 최종 위치에 둥둥 떠다니는 효과(Hover) 적용
        Vector3 finalPosition = currentSmoothPosition;
        if (enableHover)
        {
            hoverTimer += deltaTime;
            finalPosition.y += Mathf.Sin(hoverTimer * hoverFrequency) * hoverAmplitude;
        }

        // 4. 오브젝트에 최종 위치 적용
        transform.position = finalPosition;

        // 5. 플레이어 방향에 따른 시선 변경 (좌우 반전)
        // 플레이어가 요정보다 오른쪽에 있으면 오른쪽을, 왼쪽에 있으면 왼쪽을 보게 합니다.
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}
