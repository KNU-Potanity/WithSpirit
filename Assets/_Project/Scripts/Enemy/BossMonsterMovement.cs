using System.Collections;
using UnityEngine;
using BasePlatformer.Monsters;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BossMonsterMovement : MonoBehaviour, IMonsterMovement
{
    private enum State { Idle, Chase, MeleeAttack, ChargePreDelay, Charging }
    private State currentState = State.Idle;

    [Header("Monster Data")]
    public BossMonsterData monsterData;

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

    // 돌진 관련 변수
    private float chargeSpeed;
    private float chargePreDelay;
    private float chargeCooldown;
    private float chargeKnockback;
    private float chargeTimer = 0f;
    private Vector2 chargeDirection;

    // 소환 패턴 관련 타이머 배열
    private float[] patternTimers;

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

            chargeSpeed = moveSpeed * monsterData.chargeSpeedMultiplier;
            chargePreDelay = monsterData.chargePreDelay;
            chargeCooldown = monsterData.chargeCooldown;
            chargeKnockback = monsterData.chargeKnockback;

            if (monsterData.spawnPatterns != null && monsterData.spawnPatterns.Count > 0)
            {
                patternTimers = new float[monsterData.spawnPatterns.Count];
                for (int i = 0; i < patternTimers.Length; i++)
                {
                    // 씬 시작 후 지정된 spawnInterval 시간 뒤 첫 소환되도록 초기화
                    patternTimers[i] = monsterData.spawnPatterns[i].spawnInterval;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        if (chargeTimer > 0f)
            chargeTimer -= Time.fixedDeltaTime;

        UpdateSpawnPatterns();

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

    private void UpdateSpawnPatterns()
    {
        if (monsterData == null || monsterData.spawnPatterns == null || patternTimers == null) return;

        for (int i = 0; i < monsterData.spawnPatterns.Count; i++)
        {
            if (i >= patternTimers.Length) break;

            patternTimers[i] -= Time.fixedDeltaTime;

            if (patternTimers[i] <= 0f)
            {
                SpawnPattern(monsterData.spawnPatterns[i]);
                patternTimers[i] = monsterData.spawnPatterns[i].spawnInterval;
            }
        }
    }

    private void SpawnPattern(PatternSpawnData pattern)
    {
        if (pattern.patternPrefab == null) return;

        if (animator != null && HasParameter(animator, "Attack"))
        {
            animator.SetTrigger("Attack");
        }

        StartCoroutine(SpawnPatternCoroutine(pattern, attackDelay));
    }

    private IEnumerator SpawnPatternCoroutine(PatternSpawnData pattern, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        int spawnCount = Mathf.Max(1, pattern.count);

        for (int k = 0; k < spawnCount; k++)
        {
            Vector3 basePosition = transform.position + pattern.spawnOffset;
            bool hasValidSpawnPoint = false;

            // 1. k번째 스폰 포인트 이름이 명시되어 있는지 확인
            if (pattern.spawnPointNames != null && pattern.spawnPointNames.Count > 0)
            {
                // k가 스폰 포인트 개수 이상이면 유효한 마지막 스폰 포인트를 기준점으로 사용
                int nameIndex = Mathf.Min(k, pattern.spawnPointNames.Count - 1);
                string pointName = pattern.spawnPointNames[nameIndex];

                if (!string.IsNullOrEmpty(pointName))
                {
                    Transform targetPoint = FindChildByName(transform, pointName);
                    if (targetPoint != null)
                    {
                        basePosition = targetPoint.position;
                        hasValidSpawnPoint = true;
                    }
                }
            }

            Vector3 spacingOffset = pattern.spawnSpacing * k;
            Vector3 spawnPosition;

            if (hasValidSpawnPoint)
            {
                // 스폰 포인트가 존재하는 경우: 스폰 포인트 위치에 (해당 포인트 이후 추가 소환분 index차이만큼 spacing) 추가
                int extraIndex = (pattern.spawnPointNames != null && k >= pattern.spawnPointNames.Count) ? (k - (pattern.spawnPointNames.Count - 1)) : 0;
                Vector3 extraSpacing = pattern.spawnSpacing * extraIndex;
                if (!movingRight) extraSpacing.x *= -1f;
                spawnPosition = basePosition + extraSpacing;
            }
            else
            {
                // 스폰 포인트가 없는 경우: 보스 위치 + Offset 기준 spacing 계산
                if (!movingRight)
                {
                    spacingOffset.x *= -1f;
                    Vector3 adjustedOffset = pattern.spawnOffset;
                    adjustedOffset.x *= -1f;
                    basePosition = transform.position + adjustedOffset;
                }
                spawnPosition = basePosition + spacingOffset;
            }

            Instantiate(pattern.patternPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName) return child;
        }
        return null;
    }

    private void UpdateState()
    {
        // 돌진 예비동작 중이거나 돌진 중일 때는 외부 State 전환을 차단
        if (currentState == State.ChargePreDelay || currentState == State.Charging)
            return;

        if (playerTransform == null)
        {
            currentState = State.Idle;
            return;
        }

        float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

        // 1. 돌진 쿨타임이 끝났고 플레이어가 감지 범위 내에 있으면 돌진 발동!
        if (chargeTimer <= 0f && distX <= detectionRange)
        {
            StartChargePattern();
            return;
        }

        // 2. 일반 상태 머신
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
                    currentState = State.MeleeAttack;
                }
                break;

            case State.MeleeAttack:
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
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

            case State.MeleeAttack:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                if (attackTimer <= 0f)
                {
                    if (animator != null)
                    {
                        animator.SetTrigger("Attack");
                    }
                    StartCoroutine(DealMeleeDamageCoroutine(attackDelay));
                    attackTimer = attackCooldown;
                }
                break;

            case State.ChargePreDelay:
                // 돌진 준비 중에는 제자리 정지
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;

            case State.Charging:
                // 지정된 방향(돌진 목표 지점/벽까지)으로 고속 이동
                rb.linearVelocity = new Vector2(chargeDirection.x * chargeSpeed, rb.linearVelocity.y);
                break;
        }
    }

    private void StartChargePattern()
    {
        currentState = State.ChargePreDelay;
        chargeTimer = chargeCooldown;

        // 돌진할 방향 고정
        bool playerIsRight = playerTransform.position.x > transform.position.x;
        ForceFlip(playerIsRight);
        chargeDirection = movingRight ? Vector2.right : Vector2.left;

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        // 1. 돌진 전 예비동작 (경고/텔레그래프 - Idle 모션으로 멈춤)
        yield return new WaitForSeconds(chargePreDelay);

        // 2. 돌진 시작
        currentState = State.Charging;

        // 돌진 시작 시 정령의 플랫폼 강제 해제
        RevertFairyPlatform();

        // 돌진 모션 트리거가 필요하면 애니메이터 실행 (Walk 또는 Attack 트리를 재활용)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 3. 벽에 부딪힐 때까지 또는 일정 시간 동안 돌진 진행
        float maxChargeDuration = 3.0f; // 최대 돌진 시간 (안전장치)
        float elapsed = 0f;

        while (currentState == State.Charging && elapsed < maxChargeDuration)
        {
            elapsed += Time.deltaTime;

            // 벽에 부딪혔는지 확인
            if (IsHittingWall())
            {
                break;
            }

            yield return null;
        }

        // 4. 돌진 종료 ➔ Idle로 복귀
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        currentState = State.Idle;
    }

    private bool IsHittingWall()
    {
        Vector2 checkPos = transform.position;
        float dir = movingRight ? 1f : -1f;

        // 몬스터 전방 레이캐스트로 지형/벽 충돌 탐지
        RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.right * dir, 1.0f, whatIsGround | obstacleLayer);
        return hit.collider != null;
    }

    private IEnumerator DealMeleeDamageCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 딜레이 시간 동안 돌진 패턴으로 상태가 변경되지 않고 펀치(MeleeAttack) 상태를 유지할 때만 데미지 적용
        if (currentState == State.MeleeAttack && playerTransform != null)
        {
            float distX = Mathf.Abs(playerTransform.position.x - transform.position.x);
            float distY = Mathf.Abs(playerTransform.position.y - transform.position.y);

            if (distX <= attackRangeX && distY <= attackRangeY)
            {
                DealDamageToTarget(playerTransform.gameObject, knockback);
            }
        }
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
        // 돌진 중에 플레이어와 부딪히면 강력한 넉백과 데미지 부여
        if (currentState == State.Charging)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Player"))
            {
                DealDamageToTarget(collision.gameObject, chargeKnockback);
            }
            else if (((1 << collision.gameObject.layer) & (whatIsGround | obstacleLayer)) != 0)
            {
                // 벽에 충돌 시 돌진 중단
                currentState = State.Idle;
            }
        }
        else
        {
            // 일반 상태에서 접촉 시 기본 데미지
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Player"))
            {
                DealDamageToTarget(collision.gameObject, knockback);
            }
        }
    }

    private void RevertFairyPlatform()
    {
        var fairyControllers = FindObjectsByType<FairyPlatformController>();
        foreach (var controller in fairyControllers)
        {
            if (controller.HasActivePlatform)
            {
                controller.RevertTransform();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 돌진 중이거나 일반 이동 중 정령의 플랫폼과 부딪히면 해제
        if (collision.name.Contains("Platform") || collision.gameObject.name.Contains("Platform"))
        {
            RevertFairyPlatform();
        }
    }

    private void DealDamageToTarget(GameObject target, float customKnockback)
    {
        var playerHealth = target.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
        if (playerHealth == null) playerHealth = target.GetComponent<BasePlatformer.Player.PlayerHealth>();

        if (playerHealth != null)
        {
            float dirX = (target.transform.position.x > transform.position.x) ? 1f : -1f;
            Vector2 knockbackDir = new Vector2(dirX, 1.5f).normalized * customKnockback;
            playerHealth.TakeDamage((int)attackDamage, knockbackDir);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;

        // 감지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.DetectionRange * 2f, 1f, 0f));

        // 근접 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(monsterData.AttackRangeX * 2f, monsterData.AttackRangeY * 2f, 0f));
    }
#endif
}
