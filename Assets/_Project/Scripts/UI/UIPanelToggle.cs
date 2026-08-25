using UnityEngine;
using UnityEngine.UI;

namespace BasePlatformer.UI
{
    public enum PanelToggleAction
    {
        Toggle,   // 켜져 있으면 끄고, 꺼져 있으면 켬
        Open,     // 켜기만 함
        Close     // 끄기만 함
    }

    /// <summary>
    /// 버튼 클릭 시 특정 UI 패널을 토글(켜기/끄기)하거나 열기/닫기 동작을 수행하는 컴포넌트입니다.
    /// 버튼 오브젝트에 붙이면 자동으로 OnClick 이벤트에 등록됩니다.
    /// </summary>
    public class UIPanelToggle : MonoBehaviour
    {
        [Header("대상 패널")]
        [Tooltip("켜고 끌 대상 UI 패널 오브젝트")]
        [SerializeField] private GameObject targetPanel;

        [Header("동작 모드")]
        [Tooltip("버튼 클릭 시 수행할 동작 (Toggle: 토글, Open: 열기, Close: 닫기)")]
        [SerializeField] private PanelToggleAction action = PanelToggleAction.Toggle;

        [Header("선택 사항")]
        [Tooltip("연결할 버튼 (비워두면 자기 자신에서 자동 탐색)")]
        [SerializeField] private Button triggerButton;

        private void Awake()
        {
            if (triggerButton == null)
                triggerButton = GetComponent<Button>();

            if (triggerButton != null)
            {
                triggerButton.onClick.AddListener(Execute);
            }
        }

        private void OnDestroy()
        {
            if (triggerButton != null)
            {
                triggerButton.onClick.RemoveListener(Execute);
            }
        }

        /// <summary>
        /// 설정된 동작 모드에 따라 패널을 제어합니다.
        /// </summary>
        public void Execute()
        {
            if (targetPanel == null)
            {
                Debug.LogWarning($"[UIPanelToggle] {gameObject.name}에 대상 패널(targetPanel)이 지정되지 않았습니다.");
                return;
            }

            switch (action)
            {
                case PanelToggleAction.Toggle:
                    targetPanel.SetActive(!targetPanel.activeSelf);
                    break;
                case PanelToggleAction.Open:
                    targetPanel.SetActive(true);
                    break;
                case PanelToggleAction.Close:
                    targetPanel.SetActive(false);
                    break;
            }
        }

        /// <summary>
        /// 패널의 활성화 상태를 토글합니다.
        /// </summary>
        public void TogglePanel()
        {
            if (targetPanel != null)
                targetPanel.SetActive(!targetPanel.activeSelf);
        }

        /// <summary>
        /// 패널을 엽니다.
        /// </summary>
        public void OpenPanel()
        {
            if (targetPanel != null)
                targetPanel.SetActive(true);
        }

        /// <summary>
        /// 패널을 닫습니다.
        /// </summary>
        public void ClosePanel()
        {
            if (targetPanel != null)
                targetPanel.SetActive(false);
        }
    }
}
