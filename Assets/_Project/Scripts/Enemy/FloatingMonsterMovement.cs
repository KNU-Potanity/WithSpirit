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

    [Header("State")]
    public MonsterState currentState = MonsterState.Patrol;

    [Header("Target")]
    public Transform player;
    public LayerMask obstacleLayer;

    [Header("Ranges")]
    public float chaseRange = 10f;
    public float attackRangeX = 1.5f;
    public float attackRangeY = 2f;

    [Header("Movement (SmoothDamp)")]
    public float smoothTime = 0.3f;
    public float maxSpeed = 5f;
    private Vector3 currentVelocity;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    public int attackDamage = 1;
    public float knockbackForce = 5f;
    private float lastAttackTime;

    private void Update()
    {
        if (player == null) return;

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
            else if (distanceToPlayer <= chaseRange && HasLineOfSight())
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
        transform.position = Vector3.SmoothDamp(transform.position, transform.position, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
    }

    private void UpdateChase()
    {
        // 플레이어 방향으로 자연스럽게 이동 (FairyMovement 방식)
        transform.position = Vector3.SmoothDamp(transform.position, player.position, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);

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

        // 공격 범위 안에 플레이어가 존재하는지 다시 한 번 확인 후 데미지/넉백
        float distX = Mathf.Abs(player.position.x - transform.position.x);
        float distY = Mathf.Abs(player.position.y - transform.position.y);
        if (distX <= attackRangeX && distY <= attackRangeY)
        {
            var playerHealth = player.GetComponent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth != null)
            {
                float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
                Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized;
                playerHealth.TakeDamage(attackDamage, knockbackDir);
            }
        }

        lastAttackTime = Time.time;

        // 쿨타임만큼 휴식 (정지 상태 유지)
        yield return new WaitForSeconds(attackCooldown);

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2, attackRangeY * 2, 0));
    }
}
