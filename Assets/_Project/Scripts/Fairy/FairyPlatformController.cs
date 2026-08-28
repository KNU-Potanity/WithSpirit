using System.Collections;
using BasePlatformer.Fairy;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 정령의 플랫폼 변신/해제를 제어하는 컨트롤러.
/// 기획서 2.2(플랫폼 변신) 및 2.3(플랫폼 해제) 스펙에 맞추어 연동됩니다.
/// </summary>
public class FairyPlatformController : MonoBehaviour, IFairyPlatform
{
    [Header("Movement & Anim Settings")]
    [SerializeField] private float moveSpeed = 18f;
    [SerializeField] private float arriveDistance = 0.15f;
    [SerializeField] private float interactAnimDuration = 0.5f;

    [Header("Off-Screen Revert Settings")]
    [Tooltip("y축 화면 밖 감지 시 부여할 추가 마진 (예: 0.2면 화면 위아래로 20% 여유 제공)")]
    [SerializeField] private float yMargin = 0.2f;
    [Tooltip("x축 화면 밖 감지 시 부여할 추가 마진")]
    [SerializeField] private float xMargin = 0.05f;

    [Header("Fairy Data")]
    public MainFairyData fairyData;

    [Header("Platform Activation Physics")]
    [Tooltip("플랫폼 활성화 시 영역 안 오브젝트들을 위로 밀쳐내는 힘")]
    [SerializeField] private float pushUpForce = 8f;

    private FairyMovement fairyMovement;
    private FairyAttackController fairyAttack;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D fairyCollider;

    private Coroutine transformRoutine;
    private bool isTransformed = false;
    private bool isTransforming = false;

    // RevertTransform() 직후 1프레임 동안 재변신 요청을 차단하는 플래그
    private bool wasJustReverted = false;

    // PlatformClickMarker가 이 프레임의 클릭을 소비했을 때 true → LateUpdate의 빈공간 해제 억제
    private bool clickHandledThisFrame = false;

    private GameObject activePlatformInstance;   // 현재 씬에 생성된 플랫폼 인스턴스
    private Collider2D activePlatformCollider;
    private PlatformClickMarker activeMarker;    // 현재 플랫폼을 요청한 마커 (마커별 상태 판별용)

    private void Awake()
    {
        fairyMovement = GetComponent<FairyMovement>();
        fairyAttack = GetComponent<FairyAttackController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fairyCollider = GetComponent<Collider2D>();
    }

    /// <summary>마커에서 클릭을 처리했을 때 호출. LateUpdate의 빈공간 해제를 억제합니다.</summary>
    public void MarkClickHandled() => clickHandledThisFrame = true;

    private void Update()
    {
        if (!isTransformed) return;

        // 2.3 변신 해제 조건: 플랫폼이 카메라 화면 밖으로 벗어났을 때
        if (IsPlatformOffScreen())
        {
            RevertTransform();
        }
    }

    private void LateUpdate()
    {
        // 2.3 변신 해제 조건: 마커·몬스터가 아닌 빈 공간 클릭 시 해제
        // PlatformClickMarker.Update()에서 MarkClickHandled()를 호출했으면 실행하지 않음
        if (isTransformed && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !clickHandledThisFrame)
        {
            RevertTransform();
        }
        clickHandledThisFrame = false;
    }

    private bool IsPlatformOffScreen()
    {
        if (activePlatformInstance == null || Camera.main == null) return false;

        float minX = 0f - xMargin;
        float maxX = 1f + xMargin;
        float minY = 0f - yMargin;
        float maxY = 1f + yMargin;

        Bounds bounds;
        if (activePlatformCollider != null)
        {
            bounds = activePlatformCollider.bounds;
        }
        else if (activePlatformInstance.TryGetComponent<Renderer>(out var renderer))
        {
            bounds = renderer.bounds;
        }
        else
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(activePlatformInstance.transform.position);
            return viewportPos.x < minX || viewportPos.x > maxX || viewportPos.y < minY || viewportPos.y > maxY;
        }

        Vector3 min = Camera.main.WorldToViewportPoint(bounds.min);
        Vector3 max = Camera.main.WorldToViewportPoint(bounds.max);

        bool isOffScreen = max.x < minX || min.x > maxX || max.y < minY || min.y > maxY;
        return isOffScreen;
    }

    public bool CanTransform => !isTransformed && !isTransforming && !wasJustReverted && (fairyAttack == null || fairyAttack.CanAttack);
    public bool HasActivePlatform => activePlatformInstance != null || isTransformed || isTransforming;

    /// <summary>
    /// 지정한 플랫폼 오브젝트가 현재 이 정령에 의해 사용 중(이동 중 or 활성화 중)인지 반환.
    /// PlatformClickMarker가 자신의 플랫폼 상태만 판별하기 위해 사용합니다.
    /// </summary>
    public bool IsActivePlatformAlive => activePlatformInstance != null || isTransforming;

    /// <summary>
    /// 지정한 마커가 현재 활성(이동 중 or 플랫폼 생성 완료) 상태인지 반환.
    /// 다른 마커는 false를 받아 자신의 마커 표시를 정상적으로 유지합니다.
    /// </summary>
    public bool IsMyMarkerActive(PlatformClickMarker marker) => activeMarker == marker && IsActivePlatformAlive;

    /// <param name="platformPrefab">설치할 플랫폼 프리팹</param>
    public void RequestTransform(Vector3 targetPosition, PlatformClickMarker marker = null)
    {
        if (!CanTransform) return;

        if (transformRoutine != null)
            StopCoroutine(transformRoutine);

        activeMarker = marker;
        transformRoutine = StartCoroutine(TransformRoutine(targetPosition));
    }

    private IEnumerator TransformRoutine(Vector3 targetPosition)
    {
        isTransforming = true;
        GameObject platformPrefab = fairyData != null ? fairyData.PlatformPrefab : null;

        if (fairyMovement != null)
            fairyMovement.LockFollow();

        // 1. 애니메이션 트리거: Interact & Move 속도 설정
        if (animator != null)
        {
            animator.SetTrigger("Interact");
            animator.SetFloat("Speed", 1.0f);
        }

        // 2. 마커 위치로 이동 (InteractMove)
        while (Vector3.Distance(transform.position, targetPosition) > arriveDistance)
        {
            if (fairyMovement != null)
                fairyMovement.FaceTarget(targetPosition);

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;

        // 3. 도착 즉시 플랫폼 활성화 및 Interact 애니메이션 바로 재생
        if (platformPrefab != null)
        {
            // 프리팹을 마커 위치 + 오프셋에 인스턴스로 생성
            Vector3 spawnPos = targetPosition + (Vector3)(fairyData != null ? fairyData.PlatformSpawnOffset : Vector2.zero);
            activePlatformInstance = Instantiate(platformPrefab, spawnPos, Quaternion.identity);
            activePlatformCollider = activePlatformInstance.GetComponent<Collider2D>();

            // 인스턴스 또는 자식에 FairyPlatformEntityPusher가 없으면 자동 추가하여 밀쳐내기 실행
            var pusher = activePlatformInstance.GetComponentInChildren<FairyPlatformEntityPusher>();
            if (pusher == null)
            {
                pusher = activePlatformInstance.AddComponent<FairyPlatformEntityPusher>();
            }
            pusher.pushUpForce = pushUpForce;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);

            // [Fix 1] 이동 시작 시 SetTrigger("Interact")로 큐에 남은 트리거를 제거.
            // 제거하지 않으면 Play() 직후 다음 프레임에 트리거가 소비되어
            // 애니메이션이 0프레임으로 강제 리셋 → 깜빡임 발생.
            animator.ResetTrigger("Interact");
            animator.Play("Interact", 0, 0f);

            // [Fix 2] Play() 직후 Animator 스테이트 정보는 바로 반영되지 않음.
            // 1프레임이 아닌 2프레임 대기하여 확실히 반영.
            yield return null;
            yield return null;

            // [Fix 3] WaitForSeconds(고정 시간) 대신 normalizedTime 폴링으로
            // 실제 애니메이션 완료 시점을 정확하게 감지.
            // normalizedTime >= 1 이 되는 순간 루프 탈출 → Idle 첫 프레임이
            // 렌더링되기 전에 SetFairyVisible(false)가 호출됨.
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName("Interact") && stateInfo.normalizedTime < 0.7f)
            {
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
        else
        {
            yield return new WaitForSeconds(interactAnimDuration);
        }

        // 5. 애니메이션 완전히 끝난 후 정령 비활성화
        SetFairyVisible(false);

        isTransforming = false;
        isTransformed = true;
        transformRoutine = null;
    }

    /// <summary>
    /// 변신 상태 해제의 공통 내부 처리. wasJustReverted 쿨다운은 적용하지 않습니다.
    /// </summary>
    private void DoRevertCleanup()
    {
        if (transformRoutine != null)
        {
            StopCoroutine(transformRoutine);
            transformRoutine = null;
        }

        if (activePlatformInstance != null)
        {
            Destroy(activePlatformInstance);
            activePlatformInstance = null;
            activePlatformCollider = null;
        }

        activeMarker = null;

        SetFairyVisible(true);

        if (fairyMovement != null)
        {
            fairyMovement.UnlockFollow();
            fairyMovement.SyncFollowState(transform.position);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }

        isTransformed = false;
        isTransforming = false;
    }

    public void RevertTransform()
    {
        if (!isTransformed && !isTransforming) return;

        DoRevertCleanup();

        // 해제 직후 1프레임 동안 재변신 요청을 차단
        // (빈 공간 클릭 후 동일 프레임 내 RequestTransform 재호출 방지)
        wasJustReverted = true;
        StartCoroutine(ClearRevertFlag());
    }

    /// <summary>
    /// 현재 변신 상태와 관계없이 지정 플랫폼으로 즉시 전환 요청.
    /// 이미 변신 중이면 쿨다운 없이 해제 후 바로 재변신을 시작합니다.
    /// PlatformClickMarker에서 다른 마커 클릭 시 사용합니다.
    /// </summary>
    /// <param name="platformPrefab">설치할 플랫폼 프리팹</param>
    public void RequestTransformOrReplace(Vector3 targetPosition, PlatformClickMarker marker = null)
    {
        // 이미 변신/변신 중이면 쿨다운 없이 즉시 해제
        if (isTransformed || isTransforming)
            DoRevertCleanup();

        // 공격 중이면 차단
        if (fairyAttack != null && !fairyAttack.CanAttack) return;

        activeMarker = marker;
        transformRoutine = StartCoroutine(TransformRoutine(targetPosition));
    }

    private IEnumerator ClearRevertFlag()
    {
        yield return null;
        wasJustReverted = false;
    }

    private void SetFairyVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (fairyCollider != null) fairyCollider.enabled = visible;
    }
}
