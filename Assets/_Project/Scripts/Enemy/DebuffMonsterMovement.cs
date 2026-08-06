using UnityEngine;
using BasePlatformer.Monsters;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DebuffMonsterMovement : MonoBehaviour, IMonsterMovement
{
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    [Header("Monster Data")]
    public DebuffMonsterData monsterData;

    // 내부 변수
    private LayerMask whatIsGround;
    private LayerMask obstacleLayer;
    private float moveSpeed;
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private float attackDamage;
    private float knockback;
    private float attackDelay;

    private bool movingRight = true;
    private Rigidbody2D rb;
    private Animator animator;
    private float attackTimer = 0f;
    private float flipCooldown = 0f;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        whatIsGround = LayerMask.GetMask("Ground");
        obstacleLayer = LayerMask.GetMask("Hazard");

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
        }
    }

    private void FixedUpdate()
    {
        if (flipCooldown > 0f)
            flipCooldown -= Time.fixedDeltaTime;

        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

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
            currentState = State.Patrol;
            return;
        }

        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

        switch (currentState)
        {
            case State.Patrol:
                if (distX <= detectionRange)
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                if (distX > detectionRange)
                {
                    currentState = State.Patrol;
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
            case State.Patrol:
                rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
                if (IsLedgeAhead())
                {
                    Flip();
                }
                break;

            case State.Chase:
                if (playerTransform != null)
                {
                    float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
                    bool playerIsRight = playerTransform.position.x > transform.position.x;

                    if (playerIsRight != movingRight && distX > 0.1f)
                    {
                        ForceFlip(playerIsRight);
                    }

                    if (distX <= 0.1f)
                        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    else
                        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
                }
                break;

            case State.Attack:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                if (attackTimer <= 0f)
                {
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }
                    StartCoroutine(DealDamageCoroutine(attackDelay));
                    attackTimer = attackCooldown;
                }
                break;
        }
    }

    private System.Collections.IEnumerator DealDamageCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerTransform != null)
        {
            float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
            float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                ApplyDamageAndDebuff(playerTransform.gameObject);
            }
        }
    }

    private bool IsLedgeAhead()
    {
        Vector2 origin = transform.position;
        float dir = movingRight ? 1f : -1f;
        origin.x += dir * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.1f, whatIsGround);
        return hit.collider == null;
    }

    private void Flip()
    {
        if (flipCooldown > 0f) return;
        flipCooldown = 0.2f;

        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void ForceFlip(bool toRight)
    {
        if (movingRight == toRight) return;
        movingRight = toRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (toRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPatrolCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckPatrolCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == State.Patrol &&
            ((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Flip();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
    }

    private void CheckPatrolCollision(Collision2D collision)
    {
        if (currentState != State.Patrol) return;

        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Flip();
            return;
        }

        bool hitWall = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (Mathf.Abs(contact.normal.x) >= 0.9f)
            {
                hitWall = true;
                break;
            }
        }

        if (hitWall) Flip();
    }

    private void ApplyDamageAndDebuff(GameObject target)
    {
        if (target.CompareTag("Player") || target.name.Contains("Player"))
        {
            // 1. 데미지 및 넉백
            var playerHealth = target.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth == null) playerHealth = target.GetComponent<BasePlatformer.Player.PlayerHealth>();

            if (playerHealth != null)
            {
                float dirX = (target.transform.position.x > transform.position.x) ? 1f : -1f;
                Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * knockback;
                playerHealth.TakeDamage((int)attackDamage, knockbackDir);
            }

            // 2. 디버프 (둔화 - 이동속도 50% 감소 및 하늘색 시각 효과)
            var playerMovement = target.GetComponentInParent<BasePlatformer.Player.PlayerMovement>();
            if (playerMovement == null) playerMovement = target.GetComponent<BasePlatformer.Player.PlayerMovement>();

            if (playerMovement != null && monsterData != null)
            {
                if (monsterData.DebuffType == DebuffTypes.Slow)
                {
                    playerMovement.ApplySlowDebuff(monsterData.SpeedDecreaseRate, monsterData.Duration);
                }
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
