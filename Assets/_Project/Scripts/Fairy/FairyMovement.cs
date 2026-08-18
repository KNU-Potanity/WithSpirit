using UnityEngine;

public class FairyMovement : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform followTarget;
    public Vector3 offset = new Vector3(-1.5f, 1.5f, 0f);

    [Header("Follow Settings")]
    public float smoothTime = 0.45f;
    public float maxSpeed = 10f;

    [Header("Hover Settings")]
    public bool enableHover = true;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 2f;
    public float phaseOffset = 0f;

    public enum UpdateType { Update, LateUpdate, FixedUpdate }

    [Header("Optimization / Fix Jitter")]
    public UpdateType updateType = UpdateType.LateUpdate;

    private Vector3 currentVelocity = Vector3.zero;
    private float hoverTimer = 0f;
    private Vector3 currentSmoothPosition;
    // bool 대신 카운터로 관리: 여러 컨트롤러가 독립적으로 Lock/Unlock해도 안전
    private int lockRefCount;

    /// <summary>하위 호환성을 위해 player 프로퍼티 제공</summary>
    public Transform player
    {
        get => followTarget;
        set => followTarget = value;
    }

    private void Start()
    {
        currentSmoothPosition = transform.position;
    }

    /// <summary>
    /// 따라갈 타겟과 오프셋, 호버 phaseOffset을 설정합니다.
    /// </summary>
    public void SetTarget(Transform newTarget, Vector3 newOffset, float newPhaseOffset = 0f)
    {
        followTarget = newTarget;
        offset = newOffset;
        phaseOffset = newPhaseOffset;
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
        if (lockRefCount > 0 || followTarget == null)
            return;

        Vector3 targetPosition = followTarget.position + offset;

        currentSmoothPosition = Vector3.SmoothDamp(
            currentSmoothPosition,
            targetPosition,
            ref currentVelocity,
            smoothTime,
            maxSpeed,
            deltaTime
        );

        Vector3 finalPosition = currentSmoothPosition;
        if (enableHover)
        {
            hoverTimer += deltaTime;
            finalPosition.y += Mathf.Sin((hoverTimer + phaseOffset) * hoverFrequency) * hoverAmplitude;
        }

        transform.position = finalPosition;

        if (followTarget.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (followTarget.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public bool IsFollowing => lockRefCount == 0 && gameObject.activeSelf;

    public void LockFollow()
    {
        lockRefCount++;
    }

    public void UnlockFollow()
    {
        lockRefCount = Mathf.Max(0, lockRefCount - 1);
        if (lockRefCount == 0)
            SyncFollowState(transform.position);
    }

    public void SyncFollowState(Vector3 position)
    {
        currentSmoothPosition = position;
        currentVelocity = Vector3.zero;
        hoverTimer = 0f;
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        if (targetPosition.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (targetPosition.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}
