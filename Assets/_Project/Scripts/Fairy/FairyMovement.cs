using UnityEngine;

public class FairyMovement : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    public Vector3 offset = new Vector3(-1.5f, 1.5f, 0f);

    [Header("Follow Settings")]
    public float smoothTime = 0.3f;
    public float maxSpeed = 15f;

    [Header("Hover Settings")]
    public bool enableHover = true;
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 2f;

    public enum UpdateType { Update, LateUpdate, FixedUpdate }

    [Header("Optimization / Fix Jitter")]
    public UpdateType updateType = UpdateType.LateUpdate;

    private Vector3 currentVelocity = Vector3.zero;
    private float hoverTimer = 0f;
    private Vector3 currentSmoothPosition;
    private bool followLocked;

    private void Start()
    {
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
        if (followLocked || player == null)
            return;

        Vector3 targetPosition = player.position + offset;

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
            finalPosition.y += Mathf.Sin(hoverTimer * hoverFrequency) * hoverAmplitude;
        }

        transform.position = finalPosition;

        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void LockFollow()
    {
        followLocked = true;
    }

    public void UnlockFollow()
    {
        followLocked = false;
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
