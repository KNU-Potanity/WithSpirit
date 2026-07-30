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

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isPushing)
            PushEntity(other, GetPlatformBounds());
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
        Vector2 boxSize = bounds.size;
        boxSize.x += 0.4f;
        boxSize.y += 0.6f;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(bounds.center, boxSize, 0f);
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
            float targetY = platformTopY + (entityHeight * 0.5f) + 0.15f;

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
}
