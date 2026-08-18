using System.Collections;
using UnityEngine;
using BasePlatformer.Monsters;
using BasePlatformer.Player;
using BasePlatformer.Terrain;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ChestnutMonsterMovement : MonoBehaviour, IMonsterMovement, IBarrierConsumable
{
    // ─────────────────────────────────────────────
    //  상태 정의
    // ─────────────────────────────────────────────
    private enum State { Patrol, Chase, JumpAttack }
    private State currentState = State.Patrol;

    [Header("Monster Data")]
    public ChestnutMonsterData monsterData;

    // ─────────────────────────────────────────────
    //  내부 변수
    // ─────────────────────────────────────────────
    private LayerMask whatIsGround;
    private LayerMask obstacleLayer;
    private float moveSpeed;
    private float detectionRange;
    private float attackRangeX;
    private float attackRangeY;
    private float attackCooldown;
    private int attackDamage;
    private float knockback;
    private float minHorizontalNormalX = 0.9f;
    private float jumpAngle = 45f;
    private bool defaultLeftFacing = false;

    private CompositeCollider2D groundComposite;
    private bool movingRight = true;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float flipCooldown = 0f;
    private float attackTimer = 0f;
    private Transform playerTransform;

    private bool isJumping = false;
    private bool hasHitPlayer = false;

    // 씬 시작 시 CompositeCollider2D에서 추출한 플랫폼 경계 목록
    private struct PlatformBounds
    {
        public float minX;
        public float maxX;
        public float topY;
    }
    private System.Collections.Generic.List<PlatformBounds> platforms = new System.Collections.Generic.List<PlatformBounds>();
    private readonly System.Collections.Generic.List<Vector2> pathPoints = new System.Collections.Generic.List<Vector2>();

    // ─────────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        whatIsGround = LayerMask.GetMask("Ground");
        obstacleLayer = LayerMask.GetMask("Hazard");
        animator = GetComponentInChildren<Animator>();

        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj != null)
        {
            groundComposite = groundObj.GetComponent<CompositeCollider2D>();
        }
        else
        {
            Debug.LogWarning("[Chestnut] 씬에서 'Ground' 오브젝트를 찾을 수 없습니다!");
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
            minHorizontalNormalX = monsterData.minHorizontalNormalX;
            jumpAngle = monsterData.jumpAngle;
            defaultLeftFacing = monsterData.defaultLeftFacing;
        }
    }

    private void Start()
    {
        BuildPlatformList();
        if (defaultLeftFacing)
        {
            Vector3 scaler = transform.localScale;
            scaler.x = -Mathf.Abs(scaler.x);
            transform.localScale = scaler;
        }
    }

    private void BuildPlatformList()
    {
        platforms.Clear();
        if (groundComposite == null) return;

        Transform compositeTransform = groundComposite.transform;

        for (int i = 0; i < groundComposite.pathCount; i++)
        {
            pathPoints.Clear();
            groundComposite.GetPath(i, pathPoints);

            if (pathPoints.Count == 0) continue;

            float minX = float.MaxValue, maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var localPos in pathPoints)
            {
                Vector2 worldPos = compositeTransform.TransformPoint(localPos);
                if (worldPos.x < minX) minX = worldPos.x;
                if (worldPos.x > maxX) maxX = worldPos.x;
                if (worldPos.y > maxY) maxY = worldPos.y;
            }

            platforms.Add(new PlatformBounds { minX = minX, maxX = maxX, topY = maxY });
        }
    }

    // ─────────────────────────────────────────────
    //  메인 루프
    // ─────────────────────────────────────────────
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

        if (animator != null && HasParameter(animator, "Speed"))
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
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

    // ─────────────────────────────────────────────
    //  상태 전환 로직
    // ─────────────────────────────────────────────
    private void UpdateState()
    {
        if (isJumping) return;

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
                bool inRange = distX <= detectionRange;
                bool samePlatform = IsPlayerOnSamePlatform();
                bool pathClear = IsPathClear();
                if (inRange && samePlatform && pathClear)
                    currentState = State.Chase;
                break;

            case State.Chase:
                if (!IsPlayerOnSamePlatform() || !IsPathClear())
                {
                    currentState = State.Patrol;
                    break;
                }

                if (distX <= attackRangeX && distY <= attackRangeY && attackTimer <= 0f)
                {
                    currentState = State.JumpAttack;
                }
                break;

            case State.JumpAttack:
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  상태별 행동 실행
    // ─────────────────────────────────────────────
    private void ExecuteState()
    {
        switch (currentState)
        {
            case State.Patrol:
                ExecutePatrol();
                break;

            case State.Chase:
                ExecuteChase();
                break;

            case State.JumpAttack:
                ExecuteJumpAttack();
                break;
        }
    }

    private void ExecutePatrol()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        if (IsLedgeAhead())
            Flip();
    }

    private void ExecuteChase()
    {
        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        bool playerIsRight = playerTransform.position.x > transform.position.x;

        if (playerIsRight != movingRight && distX > 0.1f)
            ForceFlip(playerIsRight);

        if (distX <= 0.1f)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
    }

    private void ExecuteJumpAttack()
    {
        if (isJumping || playerTransform == null) return;

        isJumping = true;
        attackTimer = attackCooldown;

        bool playerIsRight = playerTransform.position.x > transform.position.x;
        ForceFlip(playerIsRight);

        Vector2 launchVelocity = CalculateParabolicVelocity(transform.position, playerTransform.position, jumpAngle);
        rb.linearVelocity = launchVelocity;
    }

    private Vector2 CalculateParabolicVelocity(Vector3 target, float angle)
    {
        return CalculateParabolicVelocity(transform.position, target, angle);
    }

    private Vector2 CalculateParabolicVelocity(Vector3 startPos, Vector3 targetPos, float angle)
    {
        Vector2 dir = targetPos - startPos;
        float h = dir.y;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance < 0.01f) distance = 0.01f;

        float radAngle = angle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        // v = sqrt((g * d^2) / (2 * (d * tan(theta) - h))) / cos(theta)
        float tanTheta = Mathf.Tan(radAngle);
        float cosTheta = Mathf.Cos(radAngle);

        float denominator = 2f * (distance * tanTheta - h);

        float speed;
        if (denominator <= 0f)
        {
            // 각도가 너무 낮거나 높이차가 커서 물리적으로 불가능할 경우 대체속도 계산
            speed = Mathf.Sqrt(distance * gravity);
        }
        else
        {
            speed = Mathf.Sqrt((gravity * distance * distance) / denominator) / cosTheta;
        }

        Vector2 velocity = dir.normalized * speed * cosTheta;
        velocity.y = speed * Mathf.Sin(radAngle);

        return velocity;
    }

    // ─────────────────────────────────────────────
    //  충돌 처리 (플레이어 충돌 시 데미지 후 사라짐 / 바닥 착지 시 상태 복귀)
    // ─────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isJumping)
        {
            if (((1 << collision.gameObject.layer) & whatIsGround) != 0)
            {
                // 바닥에 착지하면 점프 상태 해제 후 추적/순찰 상태로 복귀
                isJumping = false;
                currentState = State.Chase;
                return;
            }
        }

        if (currentState == State.Patrol)
            CheckWallCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Patrol)
            CheckWallCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPlayerHit(collision.gameObject);

        if (currentState == State.Patrol &&
            ((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Flip();
        }
    }

    private void CheckPlayerHit(GameObject target)
    {
        if (hasHitPlayer) return;

        // 플레이어 캐릭터 또는 Marker 체크
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth == null && target.transform.parent != null)
        {
            playerHealth = target.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            hasHitPlayer = true;
            float dirX = (target.transform.position.x > transform.position.x) ? 1f : -1f;
            Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * knockback;
            playerHealth.TakeDamage(attackDamage, knockbackDir);

            Destroy(gameObject);
        }
    }

    private void CheckWallCollision(Collision2D collision)
    {
        CheckPlayerHit(collision.gameObject);

        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Flip();
            return;
        }

        bool hitWall = false;
        int contactCount = collision.contactCount;
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (Mathf.Abs(contact.normal.x) >= minHorizontalNormalX)
            {
                hitWall = true;
                break;
            }
        }

        if (hitWall) Flip();
    }

    // ─────────────────────────────────────────────
    //  플랫폼 공유 여부 및 경로 체크
    // ─────────────────────────────────────────────
    private bool IsPlayerOnSamePlatform()
    {
        if (playerTransform == null) return false;

        float mx = transform.position.x;
        float px = playerTransform.position.x;

        Vector2 monsterOrigin = new Vector2(mx, col.bounds.center.y);
        RaycastHit2D monsterHit = Physics2D.Raycast(monsterOrigin, Vector2.down, 20f, whatIsGround);

        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        float playerCenterY = playerCol != null ? playerCol.bounds.center.y : playerTransform.position.y + 1f;
        Vector2 playerOrigin = new Vector2(px, playerCenterY);
        RaycastHit2D playerHit = Physics2D.Raycast(playerOrigin, Vector2.down, 20f, whatIsGround);

        foreach (var plat in platforms)
        {
            bool monsterInX = mx >= plat.minX && mx <= plat.maxX;
            bool playerInX = px >= plat.minX && px <= plat.maxX;

            if (monsterInX && playerInX)
            {
                if (monsterHit.collider != null && playerHit.collider != null)
                {
                    if (Mathf.Abs(monsterHit.point.y - playerHit.point.y) < 0.2f)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsPathClear()
    {
        if (playerTransform == null) return false;

        bool playerIsRight = playerTransform.position.x > transform.position.x;
        Vector2 direction = playerIsRight ? Vector2.right : Vector2.left;

        float checkDist = Mathf.Min(detectionRange, Mathf.Abs(playerTransform.position.x - transform.position.x));
        float originX = playerIsRight ? col.bounds.max.x : col.bounds.min.x;

        float[] rayHeights = new float[]
        {
            col.bounds.min.y + 0.1f,
            col.bounds.center.y,
            col.bounds.max.y - 0.1f
        };

        foreach (float y in rayHeights)
        {
            Vector2 rayOrigin = new Vector2(originX, y);
            RaycastHit2D obstacleHit = Physics2D.Raycast(rayOrigin, direction, checkDist, obstacleLayer);
            if (obstacleHit.collider != null)
            {
                return false;
            }
        }

        float midX = (transform.position.x + playerTransform.position.x) * 0.5f;
        Vector2 ledgeCheckOrigin = new Vector2(midX, col.bounds.min.y);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, 0.5f, whatIsGround);
        if (ledgeHit.collider == null)
        {
            return false;
        }

        return true;
    }

    private bool IsLedgeAhead()
    {
        Vector2 boundsMin = col.bounds.min;
        Vector2 boundsMax = col.bounds.max;

        float originX = movingRight ? boundsMax.x : boundsMin.x;
        float originY = boundsMin.y;

        Vector2 origin = new Vector2(originX, originY);
        float ledgeCheckDistance = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, whatIsGround);

        return hit.collider == null;
    }

    private void Flip()
    {
        if (flipCooldown > 0f) return;
        flipCooldown = 0.2f;

        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void ForceFlip(bool toRight)
    {
        if (movingRight == toRight) return;

        movingRight = toRight;
        Vector3 scaler = transform.localScale;
        float facingSign = defaultLeftFacing ? (toRight ? -1f : 1f) : (toRight ? 1f : -1f);
        scaler.x = Mathf.Abs(scaler.x) * facingSign;
        transform.localScale = scaler;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.DetectionRange * 2f, 1f, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2f, monsterData.AttackRangeY * 2f, 0f));
    }
#endif
}
