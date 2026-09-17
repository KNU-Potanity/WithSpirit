using UnityEngine;
using BasePlatformer.Monsters;
using BasePlatformer.Terrain;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RangedMonsterMovement : MonoBehaviour, IMonsterMovement, IBarrierConsumable
{
    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    [Header("Monster Data")]
    public RangedMonsterData monsterData;

    [Header("Projectile Settings")]
    public Transform spawnPoint; // 투사체 발사 위치 (미설정 시 transform.position)

    // 내부 변수
    private LayerMask whatIsGround;
    private float moveSpeed;
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private float attackDamage;
    private float knockback;
    private float projectileSpeed;
    private float attackDelay;

    private bool movingRight = true;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float attackTimer = 0f;
    private Transform playerTransform;

    // GroundMonsterMovement와 동일하게 Composite 경로의 월드 경계를 캐시한다.
    private struct PlatformBounds
    {
        public float minX;
        public float maxX;
        public float topY;
    }

    private readonly System.Collections.Generic.List<PlatformBounds> platforms =
        new System.Collections.Generic.List<PlatformBounds>();
    private readonly System.Collections.Generic.List<Vector2> pathPoints =
        new System.Collections.Generic.List<Vector2>();
    private const float GroundHeightTolerance = 0.2f;
    private const float LedgeMargin = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        whatIsGround = LayerMask.GetMask("Ground");

        if (monsterData != null)
        {
            moveSpeed = monsterData.MoveSpeed;
            detectionRange = monsterData.DetectionRange;
            attackRangeX = monsterData.AttackRangeX;
            attackRangeY = monsterData.AttackRangeY;
            attackCooldown = monsterData.CoolTime;
            attackDamage = monsterData.Damage;
            knockback = monsterData.Knockback;
            attackDelay = monsterData.AttackAnimDelay;
            projectileSpeed = monsterData.ProjectileSpeed;
        }
    }

    private void Start()
    {
        BuildPlatformList();
    }

    private void BuildPlatformList()
    {
        platforms.Clear();
        GameObject groundObj = GameObject.Find("Ground");
        CompositeCollider2D groundComposite = groundObj != null
            ? groundObj.GetComponent<CompositeCollider2D>() : null;

        if (groundComposite == null)
        {
            Debug.LogWarning("[RangedMonster] Ground의 CompositeCollider2D가 없어 추적 이동을 중지합니다.", this);
            return;
        }

        for (int i = 0; i < groundComposite.pathCount; i++)
        {
            pathPoints.Clear();
            groundComposite.GetPath(i, pathPoints);
            if (pathPoints.Count == 0) continue;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float topY = float.MinValue;
            foreach (Vector2 localPos in pathPoints)
            {
                Vector2 worldPos = groundComposite.transform.TransformPoint(localPos + groundComposite.offset);
                minX = Mathf.Min(minX, worldPos.x);
                maxX = Mathf.Max(maxX, worldPos.x);
                topY = Mathf.Max(topY, worldPos.y);
            }

            platforms.Add(new PlatformBounds { minX = minX, maxX = maxX, topY = topY });
        }
    }

    private bool CanMoveOnPlatform(float horizontalSpeed)
    {
        Bounds bounds = col.bounds;
        float nextLeadingX = (horizontalSpeed > 0f ? bounds.max.x : bounds.min.x)
            + horizontalSpeed * Time.fixedDeltaTime;

        foreach (PlatformBounds platform in platforms)
        {
            // 다른 높이나 건너편 플랫폼으로 이동 가능하다고 판단하지 않는다.
            if (bounds.center.x < platform.minX || bounds.center.x > platform.maxX
                || Mathf.Abs(bounds.min.y - platform.topY) > GroundHeightTolerance)
                continue;

            if (nextLeadingX >= platform.minX + LedgeMargin
                && nextLeadingX <= platform.maxX - LedgeMargin)
                return true;
        }

        // 발밑 플랫폼을 확인하지 못한 경우에도 수평 이동은 멈춘다.
        return false;
    }

    private void FixedUpdate()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        // 플레이어 탐색 (PlayerStartMarker)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("PlayerStartMarker");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        UpdateState();
        ExecuteState();

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    private void UpdateState()
    {
        if (playerTransform == null)
        {
            currentState = State.Idle;
            return;
        }

        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

        switch (currentState)
        {
            case State.Idle:
                if (distX <= detectionRange)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                if (distX > detectionRange)
                {
                    currentState = State.Idle;
                }
                else if (distX <= attackRangeX && distY <= attackRangeY)
                {
                    currentState = State.Attack;
                }
                break;

            case State.Attack:
                if (distX > attackRangeX || distY > attackRangeY)
                {
                    currentState = State.Chase;
                }
                break;
        }
    }

    private void ExecuteState()
    {
        switch (currentState)
        {
            case State.Idle:
                // 가만히 대기
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;

            case State.Chase:
                if (playerTransform != null)
                {
                    float dirX = playerTransform.position.x - transform.position.x;
                    if (dirX > 0.1f && !movingRight) Flip();
                    else if (dirX < -0.1f && movingRight) Flip();

                    float moveDir = movingRight ? 1f : -1f;
                    float horizontalSpeed = moveDir * moveSpeed;
                    if (Mathf.Abs(dirX) <= 0.1f || !CanMoveOnPlatform(horizontalSpeed))
                        horizontalSpeed = 0f;
                    rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
                }
                break;

            case State.Attack:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                if (playerTransform != null)
                {
                    float dirX = playerTransform.position.x - transform.position.x;
                    if (dirX > 0.1f && !movingRight) Flip();
                    else if (dirX < -0.1f && movingRight) Flip();
                }

                if (attackTimer <= 0f)
                {
                    PerformAttack();
                    attackTimer = attackCooldown;
                }
                break;
        }
    }

    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(SpawnProjectileCoroutine(attackDelay));
    }

    private System.Collections.IEnumerator SpawnProjectileCoroutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (monsterData != null && monsterData.ProjectilePrefab != null)
        {
            Vector3 firePos = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject projObj = Instantiate(monsterData.ProjectilePrefab, firePos, Quaternion.identity);

            Vector2 fireDirection = movingRight ? Vector2.right : Vector2.left;

            MonsterProjectile projScript = projObj.GetComponent<MonsterProjectile>();
            if (projScript == null)
            {
                projScript = projObj.AddComponent<MonsterProjectile>();
            }

            projScript.Initialize(fireDirection, projectileSpeed, (int)attackDamage, whatIsGround);
        }
        else
        {
            Debug.LogWarning("[RangedMonster] MonsterData에 ProjectilePrefab이 설정되지 않았습니다.");
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContactDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleContactDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleContactDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleContactDamage(collision.gameObject);
    }

    private void HandleContactDamage(GameObject target)
    {
        // 사망 후 페이드아웃 중에는 접촉 데미지 무시
        // (OnCollision/OnTrigger 콜백은 enabled=false여도 호출됨)
        var health = GetComponent<BasePlatformer.Monsters.MonsterHealth>();
        if (health != null && health.IsDead) return;

        if (target.CompareTag("Player") || target.name.Contains("Player"))
        {
            var playerHealth = target.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = target.GetComponent<BasePlatformer.Player.PlayerHealth>();
            }

            if (playerHealth != null)
            {
                float dirX = (target.transform.position.x > transform.position.x) ? 1f : -1f;
                Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * knockback;
                playerHealth.TakeDamage((int)attackDamage, knockbackDir);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        // 감지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.DetectionRange * 2f, 1f, 0f));

        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2f, monsterData.AttackRangeY * 2f, 0f));
    }
#endif
}
