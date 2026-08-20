using UnityEngine;
using BasePlatformer.Fairy;

namespace BasePlatformer.Interactables
{
    /// <summary>
    /// 플레이어와 접촉(OnTriggerEnter2D)하면 정령 선택지 UI를 띄우는 상호작용 트리거 오브젝트.
    /// 1회 사용 후 다시 활성화되지 않도록 콜라이더 또는 오브젝트 자체를 비활성화합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FairySelectTrigger : MonoBehaviour
    {
        [Header("UI 연결")]
        [Tooltip("FairySelectPanel에 붙어있는 FairySelectUI 컴포넌트를 직접 연결하세요 (비워두면 자동 탐색).")]
        [SerializeField] private FairySelectUI fairySelectUI;

        [Header("Trigger Settings")]
        [Tooltip("트리거 후 오브젝트 자체를 비활성화할지 여부 (false면 Collider만 꺼짐)")]
        [SerializeField] private bool disableGameObjectOnTrigger = false;

        [Tooltip("선택지 오픈 시 함께 숨길 이펙트/비주얼 오브젝트 (선택 사항)")]
        [SerializeField] private GameObject visualFeedback;

        private Collider2D triggerCol;
        private bool isTriggered = false;

        private void Awake()
        {
            triggerCol = GetComponent<Collider2D>();
            if (triggerCol != null)
            {
                triggerCol.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            // 플레이어 태그 또는 PlayerMovement 컴포넌트로 플레이어 감지
            if (other.CompareTag("Player") || other.GetComponent<BasePlatformer.Player.PlayerMovement>() != null)
            {
                TriggerSelection();
            }
        }

        private void TriggerSelection()
        {
            FairySelectUI ui = fairySelectUI
                ?? FairySelectUI.Instance
                ?? FindAnyObjectByType<FairySelectUI>();

            if (ui == null || !ui.OpenSelection())
            {
                Debug.LogWarning("[FairySelectTrigger] FairySelectUI를 열 수 없습니다.");
                return;
            }

            isTriggered = true;
            if (triggerCol != null)
                triggerCol.enabled = false;
            if (visualFeedback != null)
                visualFeedback.SetActive(false);
            if (disableGameObjectOnTrigger)
                gameObject.SetActive(false);
        }
    }
}
