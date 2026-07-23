using System.Collections;
using UnityEngine;

public class FloatingMonsterMovement : MonoBehaviour
{
    public enum MonsterState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Monster Data")]
    public FloatingMonsterData monsterData;

    [Header("Movement (SmoothDamp)")]
    // (smoothTime은 FloatingMonsterData에서 가져옴)
    private Vector3 currentVelocity;

    [Header("Attack Settings")]
    // (attackDelay는 MonsterData에서 가져옴)

    // 내부 변수
    private float moveSpeed;
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private int attackDamage;
    private float knockback;
    private float attackDelay;
    private float smoothTime;
    private LayerMask obstacleLayer;
    private Transform player;

    private MonsterState currentState = MonsterState.Patrol;
    private float lastAttackTime;
    private Animator animator;

    private void Awake()
    {
        obstacleLayer = LayerMask.GetMask("Ground");
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.Find("PlayerStartMarker");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[FloatingMonster] 씬에서 'PlayerStartMarker' 오브젝트를 찾을 수 없습니다!");
        }

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
            smoothTime = monsterData.smoothTime;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("PlayerStartMarker");
            if (playerObj != null) player = playerObj.transform;
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);

        // State Transitions
        if (currentState != MonsterState.Attack)
        {
            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    ChangeState(MonsterState.Attack);
                }
                else
                {
                    // 쿨타임 중이면서 공격 범위 안일 때는 정지 유지
                    ChangeState(MonsterState.Patrol); 
                }
            }
            else if (distanceToPlayer <= detectionRange && HasLineOfSight())
            {
                ChangeState(MonsterState.Chase);
            }
            else
            {
                ChangeState(MonsterState.Patrol);
            }
        }

        // State Behaviors
        switch (currentState)
        {
            case MonsterState.Patrol:
                UpdatePatrol();
                break;
            case MonsterState.Chase:
                UpdateChase();
                break;
            case MonsterState.Attack:
                // Attack logic is handled in Coroutine
                break;
        }
    }

    private void ChangeState(MonsterState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        
        if (currentState == MonsterState.Attack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void UpdatePatrol()
    {
        // 기본 정지 상태: 부드럽게 멈춤
        transform.position = Vector3.SmoothDamp(transform.position, transform.position, ref currentVelocity, smoothTime, moveSpeed, Time.deltaTime);
    }

    private void UpdateChase()
    {
        // 플레이어 방향으로 자연스럽게 이동 (FairyMovement 방식)
        transform.position = Vector3.SmoothDamp(transform.position, player.position, ref currentVelocity, smoothTime, moveSpeed, Time.deltaTime);

        // 시선 방향 전환 (스프라이트 좌우 반전)
        FlipTowardsPlayer();
    }

    private bool HasLineOfSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);
        
        // 플레이어 방향으로 레이캐스트를 쏴서 장애물이 있는지 확인
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        
        // hit.collider가 없으면(null) 장애물이 없다는 뜻
        return hit.collider == null;
    }

    private IEnumerator AttackRoutine()
    {
        // 일단 정지
        currentVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 딜레이 대기
        yield return new WaitForSeconds(attackDelay);

        // 공격 범위 안에 플레이어가 존재하는지 다시 한 번 확인 후 데미지/넉백
        if (player != null)
        {
            float distX = Mathf.Abs(player.position.x - transform.position.x);
            float distY = Mathf.Abs(player.position.y - transform.position.y);
            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                var playerHealth = player.GetComponent<BasePlatformer.Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
                    Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * knockback;
                    playerHealth.TakeDamage(attackDamage, knockbackDir);
                }
            }
        }

        lastAttackTime = Time.time;

        // 쿨타임에서 딜레이를 뺀 시간만큼 휴식 (전체 공격 주기를 쿨타임으로 맞춤)
        float remainingCooldown = Mathf.Max(0f, attackCooldown - attackDelay);
        yield return new WaitForSeconds(remainingCooldown);

        // 쿨타임이 끝나면 Patrol(정지) 상태로 복귀 후 다음 프레임에서 재평가
        ChangeState(MonsterState.Patrol);
    }

    private void FlipTowardsPlayer()
    {
        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, monsterData.DetectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2, monsterData.AttackRangeY * 2, 0));
    }
}
