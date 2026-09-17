using UnityEngine;

/// <summary>
/// 정령이 변신해서 활성화되는 자식 플랫폼 오브젝트에 붙이는 컴포넌트.
/// 설정값 전달 후 물리 프레임에서 안전 공간을 확인하고
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

    [Tooltip("밀어올려질 목표 높이의 추가 Y 오프셋 (플랫폼 상단과 실제 콜라이더 바닥 사이의 여유)")]
    public float targetYOffset = 0.15f;

    [Tooltip("끼임 판정 허용치: 엔티티 바닥이 플랫폼 상단보다 이 값 이상 아래에 있어야 끼임으로 판정")]
    public float clipThreshold = 0.05f;

    [Header("Gizmos")]
    [Tooltip("에디터 상에서 감지 영역 기즈모를 그릴지 여부")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.5f, 0.35f);

    private Collider2D platformCollider;
    private Transform platformRoot;
    private System.Action onBlocked;
    private float timer;
    private bool initialized;
    private readonly System.Collections.Generic.HashSet<Rigidbody2D> pushedBodies = new();
    private readonly System.Collections.Generic.Dictionary<Rigidbody2D, float> pendingMoves = new();

    private void OnEnable()
    {
        // Instantiate의 OnEnable에서는 아직 호출자가 설정값을 전달하지 않았다.
        initialized = false;
        pushedBodies.Clear();
    }

    private void Start()
    {
        // 씬에 직접 배치된 플랫폼도 지원한다.
        if (!initialized) Initialize(pushUpForce, transform, null);
    }

    public bool Initialize(float force, Transform root, System.Action blocked)
    {
        pushUpForce = force;
        platformRoot = root;
        onBlocked = blocked;
        // 개별 타일의 내부 모서리가 캐릭터를 옆에서 막지 않도록 실제로 합친다.
        var tilemap = GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>();
        var composite = GetComponent<CompositeCollider2D>();
        if (tilemap != null && composite != null && composite.enabled)
        {
            tilemap.ProcessTilemapChanges();
            tilemap.compositeOperation = Collider2D.CompositeOperation.Merge;
            composite.GenerateGeometry();
        }
        Physics2D.SyncTransforms();
        platformCollider = FindSolidCollider(gameObject);
        initialized = true;
        timer = Mathf.Max(0f, pushDuration);
        return ResolveOverlaps();
    }

    private void FixedUpdate()
    {
        if (!initialized || timer <= 0f) return;
        timer -= Time.fixedDeltaTime;
        if (!ResolveOverlaps())
        {
            timer = 0f;
            onBlocked?.Invoke();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 활성화 시간이 지난 뒤에는 위에서 내려오다 발이 깊게 박힌 경우만 복구한다.
        if (!initialized || timer > 0f) return;
        if (!ResolveOverlaps(false)) onBlocked?.Invoke();
    }

    private Bounds GetPlatformBounds()
    {
        if (!IsUsableCollider(platformCollider)) platformCollider = FindSolidCollider(gameObject);
        if (platformCollider != null) return platformCollider.bounds;
        return new Bounds(transform.position, Vector3.zero);
    }

    private static bool IsUsableCollider(Collider2D col)
    {
        return col != null && col.enabled && col.gameObject.activeInHierarchy && !col.isTrigger
            && col.shapeCount > 0 && col.compositeOperation == Collider2D.CompositeOperation.None;
    }

    public static Collider2D FindSolidCollider(GameObject root)
    {
        foreach (Collider2D col in root.GetComponentsInChildren<Collider2D>())
            if (IsUsableCollider(col)) return col;
        return null;
    }

    private bool IsOwnCollider(Collider2D col)
    {
        return col.transform.IsChildOf(platformRoot != null ? platformRoot : transform);
    }

    private bool ResolveOverlaps(bool activation = true)
    {
        Physics2D.SyncTransforms();
        if (!IsUsableCollider(platformCollider)) platformCollider = FindSolidCollider(gameObject);
        if (platformCollider == null)
            return false;

        Bounds platformBounds = GetPlatformBounds();
        pendingMoves.Clear();
        // 축소된 감지 마진 때문에 실제로 겹친 몸체를 놓치지 않는다.
        Vector2 extra = Vector2.Max(pushBoxExtraMargin, Vector2.zero);
        Vector2 size = (Vector2)platformBounds.size + extra + 2f * new Vector2(Mathf.Abs(pushBoxOffset.x), Mathf.Abs(pushBoxOffset.y));
        foreach (Collider2D col in Physics2D.OverlapBoxAll((Vector2)platformBounds.center + pushBoxOffset, size, 0f))
        {
            Rigidbody2D body = col.attachedRigidbody;
            if (col.isTrigger || IsOwnCollider(col) || body == null || body.bodyType != RigidbodyType2D.Dynamic
                || Physics2D.GetIgnoreLayerCollision(platformCollider.gameObject.layer, col.gameObject.layer)
                || Physics2D.GetIgnoreCollision(platformCollider, col)) continue;

            Bounds b = col.bounds;
            if (!activation && (body.linearVelocity.y > 0.1f || b.max.y <= platformBounds.max.y
                || b.min.y >= platformBounds.max.y - Mathf.Max(0.001f, clipThreshold))) continue;
            // 주변에 있다는 이유로 정상 착지하거나 밑을 지나가는 개체를 밀지 않는다.
            if (b.max.x <= platformBounds.min.x || b.min.x >= platformBounds.max.x
                || b.max.y <= platformBounds.min.y || b.min.y >= platformBounds.max.y) continue;
            ColliderDistance2D distance = platformCollider.Distance(col);
            if (!distance.isOverlapped) continue;

            float lift = platformBounds.max.y - b.min.y + Mathf.Max(0.01f, targetYOffset);
            if (!pendingMoves.TryGetValue(body, out float existing) || lift > existing)
                pendingMoves[body] = lift;
        }

        // 모든 대상의 경로를 먼저 검사한다. 한 명이라도 갇히면 아무도 옮기지 않는다.
        foreach (var move in pendingMoves)
            if (!HasRoomAbove(move.Key, move.Value)) return false;

        foreach (var move in pendingMoves)
        {
            Rigidbody2D body = move.Key;
            body.position += Vector2.up * move.Value;
            // 위치 보정은 입력을 잠그지 않는다. 상승 속도는 설치당 한 번만 적용한다.
            float verticalSpeed = Mathf.Max(0f, body.linearVelocity.y);
            if (activation && pushedBodies.Add(body)) verticalSpeed = Mathf.Max(verticalSpeed, Mathf.Max(0f, pushUpForce));
            body.linearVelocity = new Vector2(body.linearVelocity.x, verticalSpeed);
        }
        Physics2D.SyncTransforms();
        return true;
    }

    private bool HasRoomAbove(Rigidbody2D body, float lift)
    {
        foreach (Collider2D shape in body.GetComponentsInChildren<Collider2D>())
        {
            if (!shape.enabled || !shape.gameObject.activeInHierarchy || shape.isTrigger || shape.attachedRigidbody != body) continue;
            Bounds b = shape.bounds;
            // 현재 위치부터 목적지까지의 몸체 영역을 검사하여 얇은 천장 통과도 차단한다.
            Vector2 center = (Vector2)b.center + Vector2.up * (lift * 0.5f);
            Vector2 size = new Vector2(Mathf.Max(0.001f, b.size.x - 0.002f), b.size.y + lift - 0.002f);
            foreach (Collider2D obstacle in Physics2D.OverlapBoxAll(center, size, 0f))
            {
                if (obstacle.isTrigger || obstacle.attachedRigidbody == body || IsOwnCollider(obstacle)
                    || Physics2D.GetIgnoreLayerCollision(shape.gameObject.layer, obstacle.gameObject.layer)
                    || Physics2D.GetIgnoreCollision(shape, obstacle)) continue;
                // 현재 발밑의 바닥은 상승을 막지 않는다.
                if (obstacle.bounds.max.y <= b.min.y + 0.01f) continue;
                return false;
            }
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Bounds bounds = GetPlatformBounds();
        Vector2 boxCenter = (Vector2)bounds.center + pushBoxOffset;
        Vector2 boxSize = (Vector2)bounds.size + Vector2.Max(pushBoxExtraMargin, Vector2.zero)
            + 2f * new Vector2(Mathf.Abs(pushBoxOffset.x), Mathf.Abs(pushBoxOffset.y));

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(boxCenter, boxSize);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
