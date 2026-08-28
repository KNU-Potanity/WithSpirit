using UnityEngine;
using BasePlatformer.Terrain;

public class MonsterProjectile : MonoBehaviour, IBarrierConsumable
{
    [Header("Settings")]
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private int damage;
    private LayerMask groundLayer;
    private bool isInitialized = false;
    private bool isDestroyed = false;

    private void Awake()
    {
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }

        // Rigidbody2D가 없으면 추가하여 물리 충돌/트리거 이벤트 보장
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Vector2 dir, float moveSpeed, int attackDamage, LayerMask groundMask)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        damage = attackDamage;
        groundLayer = groundMask.value != 0 ? groundMask : LayerMask.GetMask("Ground");
        isInitialized = true;

        // 일정 시간 후 자동으로 투사체 파괴
        Destroy(gameObject, lifetime);

        // 방향에 따라 스프라이트/오브젝트 Flip 처리
        if (direction.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (direction.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void Update()
    {
        if (!isInitialized || isDestroyed) return;

        float moveDist = speed * Time.deltaTime;
        Vector2 moveVec = direction * moveDist;
        Vector2 currentPos = transform.position;

        // 프레임 간 이동 경로 레이캐스트 검사 (터널링으로 인한 Ground/Barrier/Player 관통 방지)
        LayerMask checkMask = groundLayer | LayerMask.GetMask("Ground", "Hazard", "Barrier", "Player");
        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, moveDist + 0.05f, checkMask);
        if (hit.collider != null)
        {
            HandleImpact(hit.collider.gameObject);
            return;
        }

        transform.position = currentPos + moveVec;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void HandleImpact(GameObject target)
    {
        if (isDestroyed || target == null) return;

        // 몬스터 자신과의 충돌은 무시 (Monster 레이어 또는 몬스터 관련 컴포넌트)
        if (target.layer == LayerMask.NameToLayer("Monster")
            || target.GetComponent<BasePlatformer.Monsters.IMonsterMovement>() != null
            || target.GetComponent<BasePlatformer.Monsters.MonsterHealth>() != null)
            return;

        // Player (또는 플레이어 관련 충돌체)와 충돌 시 데미지 처리
        if (target.CompareTag("Player") || target.name.Contains("Player"))
        {
            isDestroyed = true;
            var playerHealth = target.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = target.GetComponent<BasePlatformer.Player.PlayerHealth>();
            }

            if (playerHealth != null)
            {
                Vector2 knockbackDir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
            }

            Destroy(gameObject);
            return;
        }

        // Ground, Hazard, Barrier 등 장애물 충돌 시 소멸 (레이어 및 이름 기반)
        int targetLayerMask = 1 << target.layer;
        bool isObstacle = (targetLayerMask & groundLayer) != 0
            || (targetLayerMask & LayerMask.GetMask("Ground", "Hazard", "Barrier")) != 0
            || target.name.Contains("Ground");

        if (isObstacle)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}
