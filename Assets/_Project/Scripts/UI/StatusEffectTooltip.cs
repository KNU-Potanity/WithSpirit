using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace BasePlatformer.UI
{
    /// <summary>
    /// TMP 텍스트 안에 <link="...">로 표시된 상태이상 단어(화상/빙결 등)에 마우스를 올리면
    /// 해당 상태이상의 상세 설명을 보여주는 툴팁을 띄웁니다.
    /// 이 컴포넌트를 링크가 포함된 TMP_Text와 같은 오브젝트에 붙입니다.
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

        [Header("링크 ID -> 툴팁 내용 매핑 (기본값 fallback)")]
        [SerializeField]
        private List<LinkEntry> linkEntries = new List<LinkEntry>
        {
            new LinkEntry { linkId = "slow",  tooltipText = "둔화\n이동 속도 50% 감소\n지속시간: 3초" },
            new LinkEntry { linkId = "burn",  tooltipText = "화상\n2초마다 1데미지\n지속시간: 6초 (총 3회)" },
            new LinkEntry { linkId = "freeze", tooltipText = "빙결\n이동 및 공격 정지\n지속시간: 3초" },
        };

        [Header("ScriptableObject 데이터 (선택사항)")]
        [Tooltip("연결 시 FairySelectUIData의 상태이상 설명을 우선 적용합니다.")]
        [SerializeField] private FairySelectUIData uiData;

        [Header("참조 (비워둘 시 씬에서 자동 탐색)")]
        [Tooltip("화면에 표시할 툴팁 패널 오브젝트 (비워두면 StatusEffectTooltipPanel 자동 탐색)")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private Vector2 offset = new Vector2(16f, -16f);

        private TMP_Text sourceText;
        private Canvas parentCanvas;
        private Dictionary<string, string> linkLookup;
        private int currentLinkIndex = -1;

        public void SetUIData(FairySelectUIData data)
        {
            uiData = data;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            if (sourceText != null)
                sourceText.ForceMeshUpdate();
        }

        private void Initialize()
        {
            if (sourceText == null)
                sourceText = GetComponent<TMP_Text>();

            if (parentCanvas == null)
                parentCanvas = GetComponentInParent<Canvas>();

            // 링크 딕셔너리 초기화
            if (linkLookup == null)
            {
                linkLookup = new Dictionary<string, string>();
                foreach (var entry in linkEntries)
                {
                    if (!string.IsNullOrEmpty(entry.linkId) && !linkLookup.ContainsKey(entry.linkId))
                        linkLookup.Add(entry.linkId, entry.tooltipText);
                }
            }

            // 툴팁 패널 자동 탐색
            if (tooltipPanel == null)
            {
                AutoFindTooltipPanel();
            }

            if (tooltipPanel != null)
            {
                if (tooltipRect == null)
                    tooltipRect = tooltipPanel.GetComponent<RectTransform>();

                if (tooltipText == null)
                    tooltipText = tooltipPanel.GetComponentInChildren<TMP_Text>(true);

                // 툴팁 패널이 마우스 레이캐스트를 가로막지 않도록 RaycastTarget 비활성화
                var images = tooltipPanel.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                foreach (var img in images)
                    img.raycastTarget = false;

                var tmps = tooltipPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (var tmp in tmps)
                    tmp.raycastTarget = false;

                tooltipPanel.SetActive(false);
            }
        }

        private void AutoFindTooltipPanel()
        {
            // 1. 부모 Canvas 내에서 탐색
            if (parentCanvas != null)
            {
                var allTransforms = parentCanvas.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.name == "StatusEffectTooltipPanel")
                    {
                        tooltipPanel = t.gameObject;
                        return;
                    }
                }
            }

            // 2. 씬 전체 루트에서 탐색
            var roots = gameObject.scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var allTransforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t.gameObject.name == "StatusEffectTooltipPanel")
                    {
                        tooltipPanel = t.gameObject;
                        return;
                    }
                }
            }
        }

        private Vector2 GetMouseScreenPosition()
        {
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            return Vector2.zero;
        }

        private void Update()
        {
            if (sourceText == null)
                sourceText = GetComponent<TMP_Text>();

            if (tooltipPanel == null)
                AutoFindTooltipPanel();

            if (sourceText == null || tooltipPanel == null) return;

            Vector2 mouseScreenPos = GetMouseScreenPosition();
            Camera cam = null;
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = parentCanvas.worldCamera != null ? parentCanvas.worldCamera : Camera.main;
            }

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(sourceText, mouseScreenPos, cam);

            if (linkIndex != -1)
            {
                if (linkIndex != currentLinkIndex)
                {
                    currentLinkIndex = linkIndex;
                    var linkInfo = sourceText.textInfo.linkInfo[linkIndex];
                    string linkId = linkInfo.GetLinkID();

                    string content = GetContentForLinkId(linkId);
                    if (!string.IsNullOrEmpty(content))
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

        private void EnsureUIData()
        {
            if (uiData == null)
            {
                var allData = Resources.FindObjectsOfTypeAll<FairySelectUIData>();
                if (allData != null && allData.Length > 0)
                {
                    uiData = allData[0];
                }
#if UNITY_EDITOR
                if (uiData == null)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:FairySelectUIData");
                    if (guids != null && guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        uiData = UnityEditor.AssetDatabase.LoadAssetAtPath<FairySelectUIData>(path);
                    }
                }
#endif
            }
        }

        private string GetContentForLinkId(string linkId)
        {
            EnsureUIData();
            if (uiData != null)
            {
                string desc = uiData.GetStatusDescriptionByLinkId(linkId);
                if (!string.IsNullOrEmpty(desc))
                    return desc;
            }

            if (linkLookup != null && linkLookup.TryGetValue(linkId, out string content))
                return content;

            return null;
        }

        private void ShowTooltip(string content)
        {
            if (tooltipPanel == null) return;

            if (tooltipText != null) 
                tooltipText.text = content;

            tooltipPanel.transform.SetAsLastSibling();
            tooltipPanel.SetActive(true);
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void PositionTooltip(Vector2 screenPos, Camera cam)
        {
            if (tooltipRect == null)
            {
                if (tooltipPanel != null)
                    tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                if (tooltipRect == null) return;
            }

            if (parentCanvas == null)
                parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        tooltipRect.position = screenPos + offset;
                    }
                    else
                    {
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos + offset, cam, out Vector2 localPoint))
                        {
                            tooltipRect.anchoredPosition = localPoint;
                        }
                    }
                }
            }
            else
            {
                tooltipRect.position = screenPos + offset;
            }
        }
    }
}
