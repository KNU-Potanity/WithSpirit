using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 정령의 플랫폼 변신/해제를 제어하는 컨트롤러.
/// 기획서 2.2(플랫폼 변신) 및 2.3(플랫폼 해제) 스펙에 맞추어 연동됩니다.
/// </summary>
public class FairyPlatformController : MonoBehaviour
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

    private GameObject activePlatformChild;
    private Collider2D activePlatformCollider;

    private void Awake()
    {
        fairyMovement = GetComponent<FairyMovement>();
        fairyAttack = GetComponent<FairyAttackController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fairyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!isTransformed) return;

        // 2.3 변신 해제 조건 1: 플레이어가 공격 키를 눌렀을 때
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            RevertTransform();
            return;
        }

        // 2.3 변신 해제 조건 2: 플랫폼이 카메라 화면 밖으로 벗어났을 때
        if (IsPlatformOffScreen())
        {
            RevertTransform();
            return;
        }
    }

    private bool IsPlatformOffScreen()
    {
        if (activePlatformChild == null || Camera.main == null) return false;

        float minX = 0f - xMargin;
        float maxX = 1f + xMargin;
        float minY = 0f - yMargin;
        float maxY = 1f + yMargin;

        Bounds bounds;
        if (activePlatformCollider != null)
        {
            bounds = activePlatformCollider.bounds;
        }
        else if (activePlatformChild.TryGetComponent<Renderer>(out var renderer))
        {
            bounds = renderer.bounds;
        }
        else
        {
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(activePlatformChild.transform.position);
            return viewportPos.x < minX || viewportPos.x > maxX || viewportPos.y < minY || viewportPos.y > maxY;
        }

        Vector3 min = Camera.main.WorldToViewportPoint(bounds.min);
        Vector3 max = Camera.main.WorldToViewportPoint(bounds.max);

        bool isOffScreen = max.x < minX || min.x > maxX || max.y < minY || min.y > maxY;
        return isOffScreen;
    }

    public bool CanTransform => !isTransformed && !isTransforming && (fairyAttack == null || fairyAttack.CanAttack);
    public bool HasActivePlatform => activePlatformChild != null || isTransformed || isTransforming;

    public void RequestTransform(Vector3 targetPosition, GameObject platformChild)
    {
        if (!CanTransform) return;

        if (transformRoutine != null)
            StopCoroutine(transformRoutine);

        transformRoutine = StartCoroutine(TransformRoutine(targetPosition, platformChild));
    }

    private IEnumerator TransformRoutine(Vector3 targetPosition, GameObject platformChild)
    {
        isTransforming = true;

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
        if (platformChild != null)
        {
            platformChild.SetActive(true);
            activePlatformChild = platformChild;
            activePlatformCollider = platformChild.GetComponent<Collider2D>();

            // 자식 플랫폼에 FairyPlatformEntityPusher가 없으면 자동 추가하여 자체적 밀쳐내기 실행
            if (!platformChild.TryGetComponent<FairyPlatformEntityPusher>(out var pusher))
            {
                pusher = platformChild.AddComponent<FairyPlatformEntityPusher>();
            }
            pusher.pushUpForce = pushUpForce;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            // Interact State를 직접 즉시 재생
            animator.Play("Interact", 0, 0f);

            // 한 프레임 대기하여 State 정보 업데이트 반영
            yield return null;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animLength = stateInfo.IsName("Interact") ? stateInfo.length : interactAnimDuration;
            yield return new WaitForSeconds(animLength);
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

    public void RevertTransform()
    {
        if (!isTransformed && !isTransforming) return;

        if (transformRoutine != null)
        {
            StopCoroutine(transformRoutine);
            transformRoutine = null;
        }

        if (activePlatformChild != null)
        {
            activePlatformChild.SetActive(false);
            activePlatformChild = null;
            activePlatformCollider = null;
        }

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

    private void SetFairyVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (fairyCollider != null) fairyCollider.enabled = visible;
    }
}
