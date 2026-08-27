using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BasePlatformer.UI
{
    /// <summary>
    /// 감시 대상 오브젝트(Target Object)의 활성화/비활성화 상태(activeInHierarchy)에 따라
    /// 자신의 색상을 자동으로 변경하고 복구하는 스크립트입니다.
    /// Image, SpriteRenderer, TMP_Text, Text, Renderer 컴포넌트를 모두 지원합니다.
    /// </summary>
    public class ObjectStateColorSync : MonoBehaviour
    {
        [Header("감시 대상 오브젝트")]
        [Tooltip("활성/비활성 상태를 체크할 대상 게임오브젝트")]
        [SerializeField] private GameObject targetObject;

        [Header("색상 설정")]
        [Tooltip("타겟이 비활성화(Deactive) 상태일 때 적용할 색상")]
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("타겟이 활성화(Active) 상태일 때 사용할 원래 색상 (체크 시 Awake 시점의 기존 컴포넌트 색상을 자동 사용)")]
        [SerializeField] private bool useCurrentColorAsActiveColor = true;

        [Tooltip("useCurrentColorAsActiveColor가 false일 때 사용할 활성 상태 색상")]
        [SerializeField] private Color activeColor = Color.white;

        // 색상을 적용할 수 있는 지원 컴포넌트 캐싱
        private Graphic uiGraphic;                // Image, RawImage, Text 등 uGUI
        private SpriteRenderer spriteRenderer;    // 2D SpriteRenderer
        private TMP_Text tmpText;                 // TextMeshPro
        private Renderer meshRenderer;            // 일반 3D Renderer / Material

        private bool? lastActiveState = null;

        private void Awake()
        {
            // 컴포넌트 탐색 및 캐싱
            uiGraphic = GetComponent<Graphic>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            tmpText = GetComponent<TMP_Text>();
            meshRenderer = GetComponent<Renderer>();

            if (useCurrentColorAsActiveColor)
            {
                activeColor = GetCurrentColor();
            }
        }

        private void OnEnable()
        {
            lastActiveState = null; // 활성화될 때 즉시 재평가
            CheckAndUpdateColor();
        }

        private void Update()
        {
            CheckAndUpdateColor();
        }

        private void CheckAndUpdateColor()
        {
            if (targetObject == null) return;

            // hierarchy 상에서 실제로 활성화되어 있는지 여부
            bool isTargetActive = targetObject.activeInHierarchy;

            // 상태 변화가 있을 때만 색상 갱신
            if (lastActiveState == null || lastActiveState != isTargetActive)
            {
                lastActiveState = isTargetActive;
                ApplyColor(isTargetActive ? activeColor : disabledColor);
            }
        }

        private Color GetCurrentColor()
        {
            if (tmpText != null) return tmpText.color;
            if (uiGraphic != null) return uiGraphic.color;
            if (spriteRenderer != null) return spriteRenderer.color;
            if (meshRenderer != null && meshRenderer.material != null) return meshRenderer.material.color;
            return Color.white;
        }

        private void ApplyColor(Color color)
        {
            if (tmpText != null)
            {
                tmpText.color = color;
                return;
            }

            if (uiGraphic != null)
            {
                uiGraphic.color = color;
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                return;
            }

            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.color = color;
            }
        }
    }
}
