using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RangedMonsterMovement : MonoBehaviour
{
    private enum State { Idle, Chase, Attack }
    private State currentState = State.Idle;

    [Header("Monster Data")]
    public RangedMonsterData monsterData;

    [Header("Projectile Settings")]
    public Transform spawnPoint; // 투사체 발사 위치 (미설정 시 transform.position)
    public float projectileSpeed = 10f;

    // 내부 변수
    private LayerMask whatIsGround;
    private float moveSpeed;
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private int attackDamage;

    private bool movingRight = true;
    private Rigidbody2D rb;
    private Animator animator;
    private float attackTimer = 0f;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        }
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
                    rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
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

            projScript.Initialize(fireDirection, projectileSpeed, attackDamage, whatIsGround);
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
}
