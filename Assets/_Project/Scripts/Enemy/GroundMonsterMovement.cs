using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GroundMonsterMovement : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  상태 정의
    // ─────────────────────────────────────────────
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    // ─────────────────────────────────────────────
    //  이동 설정
    // ─────────────────────────────────────────────
    [Header("Monster Data")]
    public GroundMonsterData monsterData;
 
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
    private float attackDelay;
    private float minHorizontalNormalX = 0.9f;
    private CompositeCollider2D groundComposite;

    private bool movingRight = true;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float flipCooldown = 0f;
    private float attackTimer = 0f;
    private Transform playerTransform;

    // 씬 시작 시 CompositeCollider2D에서 추출한 플랫폼 경계 목록
    private struct PlatformBounds
    {
        public float minX;  // 플랫폼 왼쪽 끝 X
        public float maxX;  // 플랫폼 오른쪽 끝 X
        public float topY;  // 플랫폼 상단 Y
    }
    private System.Collections.Generic.List<PlatformBounds> platforms
        = new System.Collections.Generic.List<PlatformBounds>();

    // ─────────────────────────────────────────────
    //  초기화
    // ─────────────────────────────────────────────
private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        whatIsGround = LayerMask.GetMask("Ground");
        obstacleLayer = LayerMask.GetMask("Hazard");
        // Animator는 자식 오브젝트(예: Mushroom_Move_0)에 붙어있으므로 GetComponentInChildren로 찾습니다.
        animator = GetComponentInChildren<Animator>();

        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj != null)
        {
            groundComposite = groundObj.GetComponent<CompositeCollider2D>();
        }
        else
        {
            Debug.LogWarning("[Monster] 씬에서 'Ground' 오브젝트를 찾을 수 없습니다! (플랫폼 인식 불가)");
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
            minHorizontalNormalX = monsterData.minHorizontalNormalX;
        }
    }

    private void Start()
    {
        BuildPlatformList();
    }

    // CompositeCollider2D의 각 경로(Path)에서 X범위·상단Y를 추출해 목록을 구성
    // GC Alloc 방지를 위해 클래스 멤버 변수로 재사용
    private readonly System.Collections.Generic.List<Vector2> pathPoints = new System.Collections.Generic.List<Vector2>();

    private void BuildPlatformList()
    {
        platforms.Clear();
        if (groundComposite == null)
        {
            Debug.LogWarning("[Monster] groundComposite가 할당되지 않았습니다. 플랫폼 판정이 동작하지 않습니다.");
            return;
        }

        Transform compositeTransform = groundComposite.transform;

        for (int i = 0; i < groundComposite.pathCount; i++)
        {
            pathPoints.Clear(); // 1. 매 루프마다 이전 데이터 초기화
            groundComposite.GetPath(i, pathPoints);

            if (pathPoints.Count == 0) continue;

            float minX = float.MaxValue, maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var localPos in pathPoints)
            {
                // 2. 로컬 좌표를 월드 좌표로 변환
                Vector2 worldPos = compositeTransform.TransformPoint(localPos);

                // 3. 월드 좌표 기준으로 min/max 계산
                if (worldPos.x < minX) minX = worldPos.x;
                if (worldPos.x > maxX) maxX = worldPos.x;
                if (worldPos.y > maxY) maxY = worldPos.y;
            }

            platforms.Add(new PlatformBounds { minX = minX, maxX = maxX, topY = maxY });
        }

        Debug.Log($"[Monster] 플랫폼 {platforms.Count}개 감지 완료 (월드 좌표 기준)");
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

        // 플레이어 오브젝트를 매 프레임 탐색 (PlayerStartMarker 기준)
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.Find("PlayerStartMarker");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        UpdateState();
        ExecuteState();

        // Animator의 Speed 파라미터 갱신 (Idle <-> Walk 전환용)
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }
    }

    // ─────────────────────────────────────────────
    //  상태 전환 로직
    // ─────────────────────────────────────────────
    private void UpdateState()
    {
        if (playerTransform == null)
        {
            Debug.Log("[Monster] playerTransform is NULL - 플레이어를 찾지 못함");
            currentState = State.Patrol;
            return;
        }

        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

        switch (currentState)
        {
            case State.Patrol:
                bool inRange    = distX <= detectionRange;
                bool samePlatform = IsPlayerOnSamePlatform();
                bool pathClear  = IsPathClear();
                // 감지 범위 내에 있고 → 같은 플랫폼 + 경로에 장애물 없으면 추적
                if (inRange && samePlatform && pathClear)
                    currentState = State.Chase;
                break;

            case State.Chase:
                // 경로가 막히거나 플랫폼을 벗어나면 순찰 복귀
                if (!IsPlayerOnSamePlatform() || !IsPathClear())
                {
                    currentState = State.Patrol;
                    break;
                }
                // 공격 범위 안이면 공격 상태로
                if (distX <= attackRangeX && distY <= attackRangeY)
                    currentState = State.Attack;
                break;

            case State.Attack:
                // 공격 범위 벗어나면 다시 추적
                if (distX > attackRangeX || distY > attackRangeY)
                {
                    currentState = State.Chase;
                    break;
                }
                // 경로가 막히면 순찰 복귀
                if (!IsPlayerOnSamePlatform() || !IsPathClear())
                    currentState = State.Patrol;
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

            case State.Attack:
                ExecuteAttack();
                break;
        }
    }

    // 기존 순찰 로직 (낭떠러지/벽 만나면 방향 전환)
    private void ExecutePatrol()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        if (IsLedgeAhead())
            Flip();
    }

    // 플레이어 방향으로 이동
    private void ExecuteChase()
    {
        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        bool playerIsRight = playerTransform.position.x > transform.position.x;

        // 바라보는 방향과 이동 방향이 다르고, 일정 거리 이상 떨어져 있을 때만 반전 (좌우 진동 방지)
        if (playerIsRight != movingRight && distX > 0.1f)
            ForceFlip(playerIsRight);

        // 플레이어 바로 위/아래에 있어서 좌우 거리가 매우 가까울 땐 X축 이동을 멈춰서 진동을 방지
        if (distX <= 0.1f)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
    }

    // 정지 후 쿨타임 지나면 애니메이션 재생 후 딜레이를 두고 데미지 적용
    private void ExecuteAttack()
    {
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
    }

    private System.Collections.IEnumerator DealDamageCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerTransform != null)
        {
            float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
            float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

            // 딜레이 후에도 공격 범위 안에 있을 때만 데미지 적용 (플레이어가 타이밍 맞춰 회피 가능)
            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                DealDamageToPlayer();
            }
        }
    }

    // ─────────────────────────────────────────────
    //  플랫폼 공유 여부 체크
    //  → 몬스터와 플레이어의 발 밑으로 레이캐스트를 쏴서
    //    바닥의 Y 좌표가 일치하는지 확인 (같은 층계 판정)
    // ─────────────────────────────────────────────
    private bool IsPlayerOnSamePlatform()
    {
        if (playerTransform == null) return false;

        float mx = transform.position.x;
        float px = playerTransform.position.x;

        // 몬스터 발밑 바닥 확인
        Vector2 monsterOrigin = new Vector2(mx, col.bounds.center.y);
        RaycastHit2D monsterHit = Physics2D.Raycast(monsterOrigin, Vector2.down, 20f, whatIsGround);

        // 플레이어 발밑 바닥 확인
        Collider2D playerCol = playerTransform.GetComponent<Collider2D>();
        float playerCenterY = playerCol != null ? playerCol.bounds.center.y : playerTransform.position.y + 1f;
        Vector2 playerOrigin = new Vector2(px, playerCenterY);
        RaycastHit2D playerHit = Physics2D.Raycast(playerOrigin, Vector2.down, 20f, whatIsGround);

        foreach (var plat in platforms)
        {
            bool monsterInX = mx >= plat.minX && mx <= plat.maxX;
            bool playerInX  = px >= plat.minX && px <= plat.maxX;

            // 두 개체가 동일한 플랫폼의 X 범위 내에 있는지 우선 확인 (낭떠러지 방지)
            if (monsterInX && playerInX)
            {
                if (monsterHit.collider != null && playerHit.collider != null)
                {
                    // X축 범위도 일치하고, 실제 발밑 바닥의 Y 좌표도 같다면 완벽히 같은 층계
                    if (Mathf.Abs(monsterHit.point.y - playerHit.point.y) < 0.2f)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // ─────────────────────────────────────────────
    //  경로 장애물 체크
    //  → 플레이어 방향으로 레이를 쏴서 장애물이나 낭떠러지가 있으면 false
    // ─────────────────────────────────────────────
    private bool IsPathClear()
    {
        if (playerTransform == null) return false;

        bool playerIsRight = playerTransform.position.x > transform.position.x;
        Vector2 direction = playerIsRight ? Vector2.right : Vector2.left;

        float checkDist = Mathf.Min(detectionRange, Mathf.Abs(playerTransform.position.x - transform.position.x));

        float originX = playerIsRight ? col.bounds.max.x : col.bounds.min.x;

        // 1) 수평 방향으로 장애물(벽, Hazard 등) 체크 (상/중/하 3줄)
        float[] rayHeights = new float[]
        {
            col.bounds.min.y + 0.1f,   // 발 끝
            col.bounds.center.y,       // 몸통 중앙
            col.bounds.max.y - 0.1f    // 머리 끝
        };

        foreach (float y in rayHeights)
        {
            Vector2 rayOrigin = new Vector2(originX, y);
            RaycastHit2D obstacleHit = Physics2D.Raycast(rayOrigin, direction, checkDist, obstacleLayer);
            if (obstacleHit.collider != null)
            {
                //Debug.Log($"[Monster] 추적 중지: 장애물 감지됨 ({obstacleHit.collider.name}) / 높이: {y}");
                return false;
            }
            Debug.DrawRay(rayOrigin, direction * checkDist, Color.red, 0.1f); // Scene 창에서 레이 확인용
        }

        // 2) 가는 방향 바닥 끝(낭떠러지) 체크
        float midX = (transform.position.x + playerTransform.position.x) * 0.5f;
        Vector2 ledgeCheckOrigin = new Vector2(midX, col.bounds.min.y);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, 0.5f, whatIsGround);
        if (ledgeHit.collider == null)
        {
            //Debug.Log("[Monster] 추적 중지: 중간에 낭떠러지 감지됨");
            return false;
        }

        return true;
    }

    // ─────────────────────────────────────────────
    //  낭떠러지 감지 (순찰 전용)
    // ─────────────────────────────────────────────
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

    // ─────────────────────────────────────────────
    //  충돌 처리 (순찰 상태 전용 방향 전환)
    // ─────────────────────────────────────────────
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Patrol)
            CheckWallCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (currentState == State.Patrol)
            CheckWallCollision(collision);
    }

    private void CheckWallCollision(Collision2D collision)
    {
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
        // 사용하지 않으므로 비워두거나 제거
    }

    // ─────────────────────────────────────────────
    //  데미지 처리
    // ─────────────────────────────────────────────

    // 공격 상태에서 쿨타임 후 적용하는 데미지
    private void DealDamageToPlayer()
    {
        if (playerTransform == null) return;

        var playerHealth = playerTransform.GetComponent<BasePlatformer.Player.PlayerHealth>();
        if (playerHealth != null)
        {
            float dirX = (playerTransform.position.x > transform.position.x) ? 1f : -1f;
            Vector2 knockbackDir = new Vector2(dirX, 2f).normalized * knockback;
            playerHealth.TakeDamage(attackDamage, knockbackDir);
        }
    }

    // ─────────────────────────────────────────────
    //  방향 전환
    // ─────────────────────────────────────────────
    private void Flip()
    {
        if (flipCooldown > 0f) return;
        flipCooldown = 0.2f;

        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // 쿨다운 무시하고 강제로 특정 방향으로 반전 (추적 방향 맞추기용)
    private void ForceFlip(bool toRight)
    {
        if (movingRight == toRight) return;

        movingRight = toRight;
        Vector3 scaler = transform.localScale;
        scaler.x = Mathf.Abs(scaler.x) * (toRight ? 1f : -1f);
        transform.localScale = scaler;
    }

    // ─────────────────────────────────────────────
    //  디버그용 기즈모
    // ─────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.DetectionRange * 2f, 1f, 0f));

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2f, monsterData.AttackRangeY * 2f, 0f));
    }
#endif
}
