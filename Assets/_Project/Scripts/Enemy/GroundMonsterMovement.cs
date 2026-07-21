using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GroundMonsterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("몬스터의 이동 속도")]
    public float moveSpeed = 3f;
    
    [Tooltip("현재 오른쪽으로 이동 중인지 여부")]
    public bool movingRight = true;

    [Header("Detection Settings")]
    [Tooltip("바닥으로 인식할 레이어 (낭떠러지 감지용)")]
    public LayerMask whatIsGround;

    [Tooltip("방향을 반대로 바꿀 위험 요소(가시 등)의 레이어")]
    public LayerMask hazardLayer;
    
    [Tooltip("벽으로 인식할 접촉 법선(Normal)의 수평 최소값 (0.9 = 거의 수직인 벽만 벽으로 인정)")]
    public float minHorizontalNormalX = 0.9f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float flipCooldown = 0f;

    // 공격 애니메이션이 재생되는 동안 true — 이동/재트리거를 막습니다.
    private bool isAttacking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        // Animator는 자식 오브젝트(예: Mushroom_Move_0)에 붙어있으므로 GetComponentInChildren로 찾습니다.
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        if (flipCooldown > 0f)
        {
            flipCooldown -= Time.fixedDeltaTime;
        }

        if (isAttacking)
        {
            // 공격 애니메이션 재생 중에는 제자리에 멈춥니다 (X축 이동 정지, Y축 중력은 유지).
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            // X축 이동 적용 (Y축은 중력 유지)
            rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

            // 낭떠러지 감지
            if (IsLedgeAhead())
            {
                Flip();
            }
        }

        // Animator의 Speed 파라미터 갱신 (Idle <-> Walk 전환용). 공격 중에는 0으로 고정합니다.
        if (animator != null)
        {
            animator.SetFloat("Speed", isAttacking ? 0f : Mathf.Abs(rb.linearVelocity.x));
        }
    }

    private bool IsLedgeAhead()
    {
        // 콜라이더의 경계(Bounds)를 사용하여 이동하는 방향의 제일 앞쪽 하단 모서리 좌표를 구합니다.
        Vector2 boundsMin = col.bounds.min;
        Vector2 boundsMax = col.bounds.max;

        // 오른쪽 이동 중이면 오른쪽 아래(max.x, min.y), 왼쪽이면 왼쪽 아래(min.x, min.y)
        float originX = movingRight ? boundsMax.x : boundsMin.x;
        float originY = boundsMin.y;
        
        Vector2 origin = new Vector2(originX, originY);

        // 모서리에서 아주 짧은 거리(0.1f)만 아래로 레이저를 쏩니다.
        // 이렇게 하면 아래층 바닥이 있어도 거리가 닿지 않으므로 확실하게 낭떠러지로 인식합니다.
        float ledgeCheckDistance = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, whatIsGround);

        return hit.collider == null;
    }

    // PlayerGroundDetector 방식을 참고한 벽/장애물 충돌 처리
private void OnCollisionEnter2D(Collision2D collision)
    {
        // 데미지/공격 판정을 먼저 처리해 isAttacking이 이번 프레임에 바로 반영되도록 합니다.
        ApplyDamage(collision.gameObject);

        // 공격 애니메이션 재생 중에는 방향 전환 없이 가만히 서서 공격만 합니다.
        if (!isAttacking)
        {
            CheckWallCollision(collision);
        }
    }

private void OnCollisionStay2D(Collision2D collision)
    {
        // 데미지/공격 판정을 먼저 처리해 isAttacking이 이번 프레임에 바로 반영되도록 합니다.
        ApplyDamage(collision.gameObject);

        // 공격 애니메이션 재생 중에는 방향 전환 없이 가만히 서서 공격만 합니다.
        if (!isAttacking)
        {
            CheckWallCollision(collision);
        }
    }

    private void CheckWallCollision(Collision2D collision)
    {
        // 물리적인 벽뿐만 아니라 일반 충돌체를 가진 Hazard인 경우에도 방향 전환
        if (((1 << collision.gameObject.layer) & hazardLayer) != 0)
        {
            Flip();
            return;
        }

        bool hitWall = false;

        int contactCount = collision.contactCount;
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            // 법선(Normal)의 x 절댓값이 1에 가까우면 수평(벽)에 부딪힌 것입니다.
            if (Mathf.Abs(contact.normal.x) >= minHorizontalNormalX)
            {
                hitWall = true;
                break;
            }
        }

        if (hitWall)
        {
            Flip();
        }
    }

private void OnTriggerEnter2D(Collider2D collision)
    {
        // 데미지/공격 판정을 먼저 처리해 isAttacking이 이번 프레임에 바로 반영되도록 합니다.
        ApplyDamage(collision.gameObject);

        // 공격 애니메이션 재생 중에는 방향 전환 없이 가만히 서서 공격만 합니다.
        if (!isAttacking && ((1 << collision.gameObject.layer) & hazardLayer) != 0)
        {
            Flip();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        ApplyDamage(collision.gameObject);
    }

    private void ApplyDamage(GameObject other)
    {
        // 플레이어인지 확인
        if (other.GetComponent<BasePlatformer.Player.PlayerMovement>() == null) return;

        var playerHealth = other.GetComponent<BasePlatformer.Player.PlayerHealth>();
        if (playerHealth == null) return;

        // 이미 공격 애니메이션이 재생 중이면 다시 트리거하지 않음 (애니메이션이 끝날 때까지 대기)
        if (isAttacking) return;

        // 플레이어가 몬스터의 어느 쪽에 있는지에 따라 넉백 방향 결정
        float dirX = (other.transform.position.x > transform.position.x) ? 1f : -1f;
        Vector2 knockbackDir = new Vector2(dirX, 1f).normalized;

        playerHealth.TakeDamage(1, knockbackDir);

        StartCoroutine(DoAttack());
    }

private IEnumerator DoAttack()
    {
        isAttacking = true;

        // 물리적으로도 완전히 멈추도록 Kinematic으로 전환 — 플레이어와 겹쳐도 밀려나지 않습니다.
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 트리거 직후 한 프레임 대기 — Animator가 Attack 상태로 실제 전환될 시간을 줍니다.
        yield return null;

        if (animator != null)
        {
            // Attack 상태(State 이름 "Attack")의 재생이 끝날 때까지 대기
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
        }

        // 원래대로 Dynamic으로 복귀 (중력/충돌 반응 다시 적용)
        rb.bodyType = RigidbodyType2D.Dynamic;
        isAttacking = false;
    }

    private void Flip()
    {
        // 짧은 시간 내에 연속해서 뒤돌지 않도록 쿨다운 적용
        if (flipCooldown > 0f) return;
        flipCooldown = 0.2f; 

        movingRight = !movingRight;
        
        // 그래픽 좌우 반전
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}
