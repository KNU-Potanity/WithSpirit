using UnityEngine;

/// <summary>
/// 정령이 변신해서 활성화되는 자식 플랫폼 오브젝트에 붙이는 컴포넌트.
/// OnEnable 시 또는 OnTriggerStay2D / OnCollisionStay2D를 통해 
/// 겹쳐있는 플레이어와 몬스터를 위로 밀쳐냅니다.
/// </summary>
public class FairyPlatformEntityPusher : MonoBehaviour
{
    [Header("Push Settings")]
    [Tooltip("플랫폼 활성화 시 위로 밀어올리는 힘")]
    public float pushUpForce = 1f;

    [Tooltip("활성화 후 몇 초 동안 밀쳐내기 판정을 유지할지")]
    public float pushDuration = 0.3f;

    [Header("Push Detection Area & Offset Settings")]
    [Tooltip("밀쳐내기 감지 영역의 중심 오프셋 (X, Y)")]
    public Vector2 pushBoxOffset = Vector2.zero;

    [Tooltip("플랫폼 Bounds 대비 추가로 확장할 감지 마진 (X: 좌우 확장량, Y: 상하 확장량)")]
    public Vector2 pushBoxExtraMargin = new Vector2(0.4f, 0.6f);

    [Tooltip("밀어올려질 목표 높이의 추가 Y 오프셋 (플랫폼 상단 + 엔티티 절반 높이 + 이 값)")]
    public float targetYOffset = 0.15f;

    [Tooltip("끼임 판정 허용치: 엔티티 바닥이 플랫폼 상단보다 이 값 이상 아래에 있어야 끼임으로 판정")]
    public float clipThreshold = 0.05f;

    [Header("Gizmos")]
    [Tooltip("에디터 상에서 감지 영역 기즈모를 그릴지 여부")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.5f, 0.35f);

    private Collider2D platformCollider;
    private float timer = 0f;
    private bool isPushing = false;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        timer = pushDuration;
        isPushing = true;
        PushOverlappingEntities();
    }

    private void Update()
    {
        if (!isPushing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isPushing = false;
            return;
        }

        PushOverlappingEntities();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PushEntityIfClipping(other, GetPlatformBounds());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isPushing)
            PushEntity(other, GetPlatformBounds());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PushEntityIfClipping(collision.collider, GetPlatformBounds());
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isPushing)
            PushEntity(collision.collider, GetPlatformBounds());
    }

    private Bounds GetPlatformBounds()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponent<CompositeCollider2D>();
            if (platformCollider == null)
                platformCollider = GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
            if (platformCollider == null)
                platformCollider = GetComponent<Collider2D>();
        }

        if (platformCollider != null && platformCollider.bounds.size.x > 0.01f)
        {
            return platformCollider.bounds;
        }
        else if (TryGetComponent<Renderer>(out var renderer))
        {
            return renderer.bounds;
        }
        else
        {
            return new Bounds(transform.position, new Vector3(2f, 1f, 0f));
        }
    }

    private void PushOverlappingEntities()
    {
        Physics2D.SyncTransforms();

        Bounds bounds = GetPlatformBounds();
        Vector2 boxCenter = (Vector2)bounds.center + pushBoxOffset;
        Vector2 boxSize = (Vector2)bounds.size + pushBoxExtraMargin;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
        foreach (var col in hitColliders)
        {
            PushEntity(col, bounds);
        }
    }

    private void PushEntity(Collider2D col, Bounds platformBounds)
    {
        if (col == null || col == platformCollider || col.transform.IsChildOf(transform))
            return;

        Rigidbody2D rb = col.attachedRigidbody;
        // Static 또는 Kinematic 바디(지형, 배경 등)는 절대로 건드리지 않고 Dynamic 전용으로 제한
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            float entityHeight = col.bounds.size.y;
            float platformTopY = platformBounds.max.y;
            float targetY = platformTopY + (entityHeight * 0.5f) + targetYOffset;

            if (rb.position.y < targetY)
            {
                rb.position = new Vector2(rb.position.x, targetY);

                var playerMovement = col.GetComponent<BasePlatformer.Player.PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.ApplyKnockback(Vector2.up * pushUpForce, 0.15f);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, pushUpForce);
                }
            }
        }
    }

    /// <summary>
    /// Enter 콜백 전용: 몸체는 플랫폼 위에 있으나 발만 플랫폼 상단에 끼인 경우에만 위로 밀어냄.
    /// 밑에서 점프하여 부딪히거나, 옆면 충돌, 정상 착지 시에는 동작하지 않습니다.
    /// </summary>
    private void PushEntityIfClipping(Collider2D col, Bounds platformBounds)
    {
        if (col == null || col == platformCollider || col.transform.IsChildOf(transform))
            return;

        Rigidbody2D rb = col.attachedRigidbody;
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        // 1. 밑에서 위로 점프하여 상승 중인 상태면 무시 (밑에서 박고 워프되는 것 방지)
        if (rb.linearVelocity.y > 0.1f)
            return;

        // 2. 수평(X)으로 플랫폼 영역 안에 있는지 체크 (옆면 충돌 무시)
        bool horizontalOverlap = col.bounds.max.x > platformBounds.min.x + 0.05f
                              && col.bounds.min.x < platformBounds.max.x - 0.05f;
        if (!horizontalOverlap) return;

        float entityTop = col.bounds.max.y;
        float entityBottom = col.bounds.min.y;
        float platformTop = platformBounds.max.y;

        // 3. 머리는 플랫폼 상단보다 위에 있고, 발만 플랫폼 상단 아래로 파고들었을 때만 '발 끼임'으로 판정
        bool isHeadAbovePlatform = entityTop > platformTop;
        bool isFeetClipping = entityBottom < platformTop - clipThreshold;

        if (isHeadAbovePlatform && isFeetClipping)
        {
            PushEntity(col, platformBounds);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Bounds bounds = GetPlatformBounds();
        Vector2 boxCenter = (Vector2)bounds.center + pushBoxOffset;
        Vector2 boxSize = (Vector2)bounds.size + pushBoxExtraMargin;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(boxCenter, boxSize);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
