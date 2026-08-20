using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace BasePlatformer.UI
{
    /// <summary>
    /// TMP 텍스트 안에 <link="...">로 표시된 상태이상 단어(화상/빙결 등)에 마우스를 올리면
    /// 해당 상태이상의 상세 설명을 보여주는 툴팁을 띄웁니다.
    /// 이 컴포넌트를 링크가 포함된 TMP_Text와 같은 오브젝트(또는 부모)에 붙입니다.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class StatusEffectTooltip : MonoBehaviour
    {
        [System.Serializable]
        public class LinkEntry
        {
            public string linkId;
            [TextArea(2, 4)]
            public string tooltipText;
        }

        [Header("링크 ID -> 툴팁 내용 매핑")]
        [SerializeField]
        private List<LinkEntry> linkEntries = new List<LinkEntry>
        {
            new LinkEntry { linkId = "slow",  tooltipText = "둔화\n이동 속도 50% 감소\n지속시간: 3초" },
            new LinkEntry { linkId = "burn",  tooltipText = "화상\n2초마다 1데미지\n지속시간: 6초 (총 3회)" },
            new LinkEntry { linkId = "freeze", tooltipText = "빙결\n이동 및 공격 정지\n지속시간: 3초" },
        };

        [Header("참조")]
        [Tooltip("화면에 표시할 툴팁 패널 오브젝트 (기본적으로 비활성화되어 있어야 함)")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private Vector2 offset = new Vector2(16f, -16f);

        private TMP_Text sourceText;
        private Canvas parentCanvas;
        private Dictionary<string, string> linkLookup;
        private int currentLinkIndex = -1;

        private void Awake()
        {
            sourceText = GetComponent<TMP_Text>();
            parentCanvas = GetComponentInParent<Canvas>();

            linkLookup = new Dictionary<string, string>();
            foreach (var entry in linkEntries)
            {
                if (!string.IsNullOrEmpty(entry.linkId) && !linkLookup.ContainsKey(entry.linkId))
                    linkLookup.Add(entry.linkId, entry.tooltipText);
            }

            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (sourceText == null || tooltipPanel == null) return;

            Vector2 mouseScreenPos = Input.mousePosition;
            Camera cam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? parentCanvas.worldCamera
                : null;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(sourceText, mouseScreenPos, cam);

            if (linkIndex != -1)
            {
                if (linkIndex != currentLinkIndex)
                {
                    currentLinkIndex = linkIndex;
                    var linkInfo = sourceText.textInfo.linkInfo[linkIndex];
                    string linkId = linkInfo.GetLinkID();

                    if (linkLookup.TryGetValue(linkId, out string content))
                    {
                        ShowTooltip(content);
                    }
                    else
                    {
                        HideTooltip();
                    }
                }

                if (tooltipPanel.activeSelf)
                {
                    PositionTooltip(mouseScreenPos, cam);
                }
            }
            else
            {
                if (currentLinkIndex != -1)
                {
                    currentLinkIndex = -1;
                    HideTooltip();
                }
            }
        }

        private void ShowTooltip(string content)
        {
            if (tooltipText != null) tooltipText.text = content;
            tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }

        private void PositionTooltip(Vector2 screenPos, Camera cam)
        {
            if (tooltipRect == null) return;

            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos + offset, cam, out localPoint);
            tooltipRect.anchoredPosition = localPoint;
        }
    }
}
