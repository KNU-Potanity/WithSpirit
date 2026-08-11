using System.Collections;
using UnityEngine;
using BasePlatformer.Monsters;
using BasePlatformer.Player;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlantMonsterMovement : MonoBehaviour, IMonsterMovement
{
    private enum State { Idle, Attack }
    private State currentState = State.Idle;

    [Header("Monster Data")]
    public MonsterData monsterData;

    [Header("Facing Settings")]
    [Tooltip("원본 에셋 스프라이트가 기본적으로 왼쪽을 바라보고 있으면 체크하세요.")]
    public bool defaultLeftFacing = false;

    // 내부 변수
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private int attackDamage;
    private float knockback;
    private float attackDelay;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator animator;
    private float attackTimer = 0f;
    private Transform playerTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // 제자리에 고정 (이동 불가)
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }

        if (monsterData != null)
        {
            detectionRange = monsterData.DetectionRange;
            attackRangeX = monsterData.AttackRangeX;
            attackRangeY = monsterData.AttackRangeY;
            attackCooldown = monsterData.CoolTime;
            attackDamage = monsterData.Damage;
            knockback = monsterData.Knockback;
            attackDelay = monsterData.AttackAnimDelay;
        }
    }

    private void Start()
    {
        if (defaultLeftFacing)
        {
            facingRight = false;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void FixedUpdate()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("PlayerStartMarker");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        UpdateState();
        ExecuteState();
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

        // 1. 공격 범위(AttackRangeX, AttackRangeY) 내에 들어오면 공격 상태
        if (distX <= attackRangeX && distY <= attackRangeY)
        {
            currentState = State.Attack;
        }
        else
        {
            currentState = State.Idle;
        }
    }

    private void ExecuteState()
    {
        // 식충식물은 이동하지 않으므로 속도는 0 유지
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (playerTransform != null)
        {
            float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);

            // 감지 범위(DetectionRange) 내에 플레이어가 들어와 있을 때만 플레이어 쪽을 바라봄
            if (distX <= detectionRange)
            {
                float dirX = playerTransform.position.x - transform.position.x;
                if (Mathf.Abs(dirX) > 0.1f)
                {
                    bool playerIsRight = dirX > 0f;
                    if (playerIsRight != facingRight)
                    {
                        Flip(playerIsRight);
                    }
                }
            }
        }

        if (currentState == State.Attack && attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    private void PerformAttack()
    {
        if (animator != null && HasParameter(animator, "Attack"))
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(DealDamageCoroutine(attackDelay));
    }

    private IEnumerator DealDamageCoroutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (playerTransform != null)
        {
            float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
            float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                DealDamageToTarget(playerTransform.gameObject);
            }
        }
    }

    private void Flip(bool toRight)
    {
        facingRight = toRight;
        Vector3 scale = transform.localScale;
        float facingSign = defaultLeftFacing ? (toRight ? -1f : 1f) : (toRight ? 1f : -1f);
        scale.x = Mathf.Abs(scale.x) * facingSign;
        transform.localScale = scale;
    }



    private void DealDamageToTarget(GameObject target)
    {
        var playerHealth = target.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = target.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            float dirX = (target.transform.position.x > transform.position.x) ? 1f : -1f;
            Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * knockback;
            playerHealth.TakeDamage(attackDamage, knockbackDir);
        }
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        // 감지 범위 (노란색) - 바라보는 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.DetectionRange * 2f, 1f, 0f));

        // 공격 범위 (빨간색) - 물기 공격을 시도하는 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2f, monsterData.AttackRangeY * 2f, 0f));
    }
#endif
}
