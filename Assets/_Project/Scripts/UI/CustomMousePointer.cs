using UnityEngine;
using UnityEngine.InputSystem;

namespace BasePlatformer.UI
{
    /// <summary>
    /// 마우스 포인터를 따라다니는 커스텀 포인터 UI/월드 스크립트.
    /// 플랫폼 마커(PlatformClickMarker)나 몬스터 마커(MonsterClickMarker) 위에 올라가면
    /// 마커가 켜지는 동안 자신의 비주얼을 자동으로 비활성화(숨김)합니다.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public class CustomMousePointer : MonoBehaviour
    {
        [Header("Pointer Visual")]
        [Tooltip("마우스 포인터 비주얼 (SpriteRenderer 또는 UI Image 오브젝트)")]
        [SerializeField] private GameObject pointerVisual;

        [Header("Cursor Settings")]
        [Tooltip("시스템 마우스 커서를 숨길지 여부")]
        [SerializeField] private bool hideSystemCursor = true;

        [Tooltip("선택된 정령의 색상에 맞춰 커서 색상을 변경할지 여부")]
        [SerializeField] private bool syncColorWithSelectedFairy = true;

        private Camera mainCamera;

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private Renderer pointerRenderer;
        private CanvasGroup pointerCanvasGroup;
        private SpriteRenderer pointerSpriteRenderer;
        private UnityEngine.UI.Image pointerUIImage;

        private void Start()
        {
            mainCamera = Camera.main;
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();

            if (hideSystemCursor)
            {
                Cursor.visible = false;
            }

            if (pointerVisual == null)
            {
                pointerVisual = gameObject;
            }

            pointerRenderer = pointerVisual.GetComponent<Renderer>();
            pointerCanvasGroup = pointerVisual.GetComponent<CanvasGroup>();
            pointerSpriteRenderer = pointerVisual.GetComponent<SpriteRenderer>();
            pointerUIImage = pointerVisual.GetComponent<UnityEngine.UI.Image>();

            if (Fairy.FairyManager.Instance != null && syncColorWithSelectedFairy)
            {
                SetPointerColor(Fairy.FairyManager.Instance.GetSelectedFairyColor());
                Fairy.FairyManager.Instance.OnFairySelected += HandleFairySelected;
            }
        }

        private void HandleFairySelected(FairyMovement fairy, Color fairyColor)
        {
            if (syncColorWithSelectedFairy)
            {
                SetPointerColor(fairyColor);
            }
        }

        /// <summary>
        /// 커서(SpriteRenderer 또는 UI Image)의 색상을 설정합니다.
        /// </summary>
        public void SetPointerColor(Color color)
        {
            if (pointerSpriteRenderer != null)
            {
                pointerSpriteRenderer.color = color;
            }
            else if (pointerUIImage != null)
            {
                pointerUIImage.color = color;
            }
            else if (pointerRenderer != null && pointerRenderer.material != null)
            {
                pointerRenderer.material.color = color;
            }
        }

        private void OnDestroy()
        {
            if (Fairy.FairyManager.Instance != null)
            {
                Fairy.FairyManager.Instance.OnFairySelected -= HandleFairySelected;
            }

            if (hideSystemCursor)
            {
                Cursor.visible = true;
            }
        }

        private void OnDisable()
        {
            if (hideSystemCursor)
                Cursor.visible = true;
        }

        // Update after the player and camera have finished moving this frame.
        private void LateUpdate()
        {
            if (Mouse.current == null) return;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            // 1. 마우스 위치 추적
            // 1-A. UI Canvas (RectTransform) 기반 포인터일 경우: 화면 좌표를 RectTransform에 직접 대입 (카메라 이동/물리와 완전 분리되어 떨림 0)
            if (rectTransform != null && parentCanvas != null)
            {
                if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    rectTransform.position = mouseScreenPos;
                }
                else
                {
                    Camera canvasCamera = parentCanvas.worldCamera != null
                        ? parentCanvas.worldCamera
                        : mainCamera;

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentCanvas.transform as RectTransform,
                        mouseScreenPos,
                        canvasCamera,
                        out Vector2 localPoint
                    );
                    rectTransform.anchoredPosition = localPoint;
                }
            }
            // 1-B. 2D World Space SpriteRenderer 포인터일 경우
            else if (mainCamera != null || Camera.main != null)
            {
                if (mainCamera == null) mainCamera = Camera.main;
                float distanceToPointerPlane =
                    (0f - mainCamera.transform.position.z) / mainCamera.transform.forward.z;
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
                    new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceToPointerPlane));
                pointerVisual.transform.position = mouseWorldPos;
            }

            // 2. 마커 감지 (몬스터 마커 또는 플랫폼 마커가 Hover 상태인지 확인)
            bool isMarkerActive = IsAnyMarkerHovered();

            // 3. 마커가 활성화되면 커스텀 포인터 비주얼 숨김
            if (pointerVisual != null)
            {
                if (pointerRenderer != null)
                {
                    pointerRenderer.enabled = !isMarkerActive;
                }
                else if (pointerCanvasGroup != null)
                {
                    pointerCanvasGroup.alpha = isMarkerActive ? 0f : 1f;
                }
                else if (pointerVisual != gameObject)
                {
                    pointerVisual.SetActive(!isMarkerActive);
                }
            }
        }

        /// <summary>
        /// 씬에 존재하는 몬스터 마커나 플랫폼 마커 중 마우스가 올려진 것이 있는지 확인합니다.
        /// </summary>
        private bool IsAnyMarkerHovered()
        {
            // 몬스터 마커 확인
            MonsterClickMarker[] monsterMarkers = FindObjectsByType<MonsterClickMarker>(FindObjectsInactive.Exclude);
            foreach (var monsterMarker in monsterMarkers)
            {
                if (monsterMarker != null && monsterMarker.IsHovering)
                    return true;
            }

            // 플랫폼 마커 확인
            PlatformClickMarker[] platformMarkers = FindObjectsByType<PlatformClickMarker>(FindObjectsInactive.Exclude);
            foreach (var platformMarker in platformMarkers)
            {
                if (platformMarker != null && platformMarker.IsHovering)
                    return true;
            }

            return false;
        }
    }
}
