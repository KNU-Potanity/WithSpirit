using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 정령 선택지 UI 카드 단위 클래스.
    /// 카드 루트 오브젝트 하나만 지정하면 자식 오브젝트 이름으로 컴포넌트를 자동 탐색합니다.
    /// - FairyCategoryText : 카테고리 TextMeshProUGUI
    /// - FairyTitleText    : 정령 이름 TextMeshProUGUI
    /// - FairyImageBackGround/FairyImage : 정령 이미지 Image
    /// - FairyContentText  : 설명 TextMeshProUGUI
    /// - Button (루트)      : 선택 버튼
    /// </summary>
    [System.Serializable]
    public class FairySelectCardUI
    {
        public GameObject cardRoot;

        // 런타임에 자동 탐색되는 참조 (직접 지정 불필요)
        private Button selectButton;
        private TextMeshProUGUI categoryText;
        private TextMeshProUGUI titleText;
        private Image fairyImage;
        private TextMeshProUGUI contentText;
        private bool isInitialized;

        private void Initialize()
        {
            if (isInitialized || cardRoot == null) return;
            isInitialized = true;

            // 루트에서 Button 탐색
            selectButton = cardRoot.GetComponent<Button>();

            // 자식 이름으로 TMP / Image 탐색
            var allTMPs = cardRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in allTMPs)
            {
                switch (tmp.gameObject.name)
                {
                    case "FairyCategoryText": categoryText = tmp; break;
                    case "FairyTitleText":    titleText    = tmp; break;
                    case "FairyContentText":  contentText  = tmp; break;
                }
            }

            // FairyImageBackGround 아래의 SpiritImage Image 탐색
            var imgBackGround = cardRoot.transform.Find("FairyImageBackGround");
            if (imgBackGround != null)
            {
                var imgChild = imgBackGround.Find("SpiritImage");
                if (imgChild != null)
                    fairyImage = imgChild.GetComponent<Image>();
                // SpiritImage가 없으면 BackGround 자체의 Image를 사용
                if (fairyImage == null)
                    fairyImage = imgBackGround.GetComponent<Image>();
            }
        }

        public void Bind(FairyInfo info, System.Action onClickCallback)
        {
            if (cardRoot == null || info == null) return;
            Initialize();

            cardRoot.SetActive(true);

            if (categoryText != null)
                categoryText.text = info.GetCategory();

            if (titleText != null)
                titleText.text = info.GetFairyName();

            if (contentText != null)
                contentText.text = info.GetDescription();

            if (fairyImage != null)
            {
                // 1. FairyPool에서 직접 지정한 스프라이트 우선
                Sprite spriteToShow = info.fairySprite;

                // 2. 없으면 프리팹의 SpriteRenderer에서 자동 추출
                if (spriteToShow == null && info.fairyPrefab != null)
                {
                    var sr = info.fairyPrefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (sr != null) spriteToShow = sr.sprite;
                }

                if (spriteToShow != null)
                    fairyImage.sprite = spriteToShow;

                // 색은 항상 적용 (Color.white면 원본 색 유지)
                Color fairyColor = info.GetColor();
                fairyImage.color = fairyColor;
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => onClickCallback?.Invoke());
            }
        }

        public void Hide()
        {
            if (cardRoot != null)
                cardRoot.SetActive(false);
        }
    }

    /// <summary>
    /// 정령 선택 패널(FairySelectPanel)을 관리하는 UI 스크립트.
    /// 보유하지 않은 정령 중 랜덤으로 최대 3개를 추출하여 UI에 표시하고 선택 시 등록합니다.
    /// </summary>
    public class FairySelectUI : MonoBehaviour
    {
        public static FairySelectUI Instance { get; private set; }

        [Header("Panel Root")]
        [SerializeField] private GameObject selectPanel;

        [Header("Selection Cards")]
        [Tooltip("카드 루트 오브젝트만 드래그하면 자식 컴포넌트를 자동 탐색합니다.")]
        [SerializeField] private List<FairySelectCardUI> cardUIs = new List<FairySelectCardUI>();

        [Header("All Fairy Pool")]
        [SerializeField] private FairyPool fairyPool;

        [Header("Settings")]
        [Tooltip("선택지 오픈 시 게임 일시정지 여부")]
        [SerializeField] private bool pauseGameOnOpen = true;
        private float timeScaleBeforeOpen = 1f;
        private bool isOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // selectPanel이 미지정이면 자기 자신으로 설정
            if (selectPanel == null)
                selectPanel = gameObject;

            // 자기 자신이 selectPanel인 경우: SetActive(false)하면 이 컴포넌트도 꺼지므로 건너뜀
            // → 이 경우 씬에서 비활성화 상태로 배치해두거나, selectPanel을 별도 자식 오브젝트로 분리해야 합니다.
            if (selectPanel != gameObject)
            {
                selectPanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[FairySelectUI] FairySelectUI 컴포넌트가 selectPanel과 같은 오브젝트에 있습니다. " +
                    "selectPanel을 별도 자식 오브젝트로 분리하거나, 씬에서 이 오브젝트를 시작 시 비활성화 해두세요.");
            }
        }

        private void Start()
        {
            // 인스펙터에 카드가 등록되어 있지 않다면 자식에서 자동 탐색 시도
            if (cardUIs == null || cardUIs.Count == 0)
            {
                AutoFindCards();
            }
        }

        private void AutoFindCards()
        {
            if (cardUIs == null)
                cardUIs = new List<FairySelectCardUI>();

            for (int i = 1; i <= 3; i++)
            {
                Transform cardTransform = transform.Find($"FairySelectCard_{i}");
                if (cardTransform != null)
                {
                    cardUIs.Add(new FairySelectCardUI { cardRoot = cardTransform.gameObject });
                }
            }
        }

        /// <summary>
        /// 선택지 UI를 오픈합니다.
        /// </summary>
        public bool OpenSelection()
        {
            if (FairyManager.Instance == null)
            {
                Debug.LogWarning("[FairySelectUI] FairyManager 인스턴스를 찾을 수 없습니다.");
                return false;
            }

            if (fairyPool == null || fairyPool.fairies == null || fairyPool.fairies.Count == 0)
            {
                Debug.LogWarning("[FairySelectUI] FairyPool이 비어 있거나 연결되지 않았습니다.");
                return false;
            }

            // 1. 현재 유저가 가지고 있지 않은 정령 목록 필터링
            List<FairyInfo> availablePool = GetUnownedFairies();

            if (availablePool.Count == 0)
            {
                Debug.Log("[FairySelectUI] 유저가 이미 모든 정령을 보유 중이어서 선택지를 표시하지 않습니다.");
                return false;
            }

            // cardUIs가 비어있으면 패널을 열 수 없음
            Debug.Log($"[FairySelectUI][DEBUG] cardUIs.Count={cardUIs.Count}, selectPanel={(selectPanel != null ? selectPanel.name : "NULL")}");
            if (cardUIs == null || cardUIs.Count == 0)
            {
                Debug.LogWarning("[FairySelectUI] cardUIs가 비어 있습니다. 인스펙터에서 카드 루트 오브젝트를 연결해 주세요.");
                return false;
            }

            // 2. 랜덤 셔플 후 최대 3개(카드 수만큼) 추출
            List<FairyInfo> chosenFairies = PickRandomFairies(availablePool, cardUIs.Count);

            // 3. 각 카드에 바인딩
            for (int i = 0; i < cardUIs.Count; i++)
            {
                if (i < chosenFairies.Count)
                {
                    FairyInfo info = chosenFairies[i];
                    cardUIs[i].Bind(info, () => OnFairySelected(info));
                }
                else
                {
                    cardUIs[i].Hide();
                }
            }

            // 4. 패널 활성화
            if (selectPanel != null)
            {
                selectPanel.SetActive(true);
                Debug.Log($"[FairySelectUI][DEBUG] selectPanel.SetActive(true) 호출됨. activeSelf={selectPanel.activeSelf}, activeInHierarchy={selectPanel.activeInHierarchy}");
                // cardRoot null 체크
                for (int i = 0; i < cardUIs.Count; i++)
                    Debug.Log($"[FairySelectUI][DEBUG] cardUIs[{i}].cardRoot={(cardUIs[i].cardRoot != null ? cardUIs[i].cardRoot.name : "NULL")}");
            }
            else
                Debug.LogWarning("[FairySelectUI] selectPanel이 null입니다!");

            if (pauseGameOnOpen)
            {
                if (!isOpen) timeScaleBeforeOpen = Time.timeScale;
                Time.timeScale = 0f;
            }

            isOpen = true;
            return true;
        }


        private void OnFairySelected(FairyInfo selectedInfo)
        {
            if (selectedInfo == null) return;

            // 1. 정령 생성 및 FairyManager에 등록
            if (FairyManager.Instance != null && selectedInfo.fairyPrefab != null)
            {
                Transform playerT = FairyManager.Instance.GetBaseFairy() != null
                    ? FairyManager.Instance.GetBaseFairy().transform
                    : FairyManager.Instance.transform;

                GameObject newFairyObj = Instantiate(selectedInfo.fairyPrefab, playerT.position, Quaternion.identity);
                FairyMovement fairyMovement = newFairyObj.GetComponent<FairyMovement>();
                if (fairyMovement != null)
                {
                    FairyManager.Instance.RegisterSubFairy(fairyMovement);
                    Debug.Log($"[FairySelectUI] 새 정령 획득: {selectedInfo.GetFairyName()}");
                }
            }

            // 2. UI 닫기 및 게임 재개
            CloseSelection();
        }

        public void CloseSelection()
        {
            if (pauseGameOnOpen && isOpen)
            {
                Time.timeScale = timeScaleBeforeOpen;
            }
            isOpen = false;

            if (selectPanel != null)
            {
                selectPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 유저가 현재 보유하고 있지 않은 정령 목록을 반환합니다.
        /// MainFairyData ScriptableObject 레퍼런스를 우선적으로 비교하여 정확하고 안전하게 판정합니다.
        /// </summary>
        private List<FairyInfo> GetUnownedFairies()
        {
            List<FairyInfo> unowned = new List<FairyInfo>();
            if (FairyManager.Instance == null || fairyPool == null || fairyPool.fairies == null) return unowned;

            HashSet<MainFairyData> ownedData = new HashSet<MainFairyData>();
            HashSet<string> ownedPrefabNames = new HashSet<string>();

            // 기본 정령 확인
            var baseFairy = FairyManager.Instance.GetBaseFairy();
            if (baseFairy != null)
            {
                MainFairyData data = FairyInfo.ExtractFairyData(baseFairy.gameObject);
                if (data != null) ownedData.Add(data);
                string nameEntry = baseFairy.gameObject.name.Replace("(Clone)", "").Trim();
                ownedPrefabNames.Add(nameEntry);
                Debug.Log($"[FairySelectUI][DEBUG] BaseFairy: '{nameEntry}', fairyData: {(data != null ? data.name : "null")}");
            }

            // 현재 장착된 서브 정령들 확인
            var currentFairies = FairyManager.Instance.GetSubFairies();
            if (currentFairies != null)
            {
                foreach (var fairy in currentFairies)
                {
                    if (fairy != null)
                    {
                        MainFairyData data = FairyInfo.ExtractFairyData(fairy.gameObject);
                        if (data != null) ownedData.Add(data);
                        string nameEntry = fairy.gameObject.name.Replace("(Clone)", "").Trim();
                        ownedPrefabNames.Add(nameEntry);
                        Debug.Log($"[FairySelectUI][DEBUG] SubFairy: '{nameEntry}', fairyData: {(data != null ? data.name : "null")}");
                    }
                }
            }

            Debug.Log($"[FairySelectUI][DEBUG] 보유 정령 수: 이름={ownedPrefabNames.Count}, 데이터={ownedData.Count} | FairyPool 전체 수: {fairyPool.fairies.Count}");

            // 풀 내의 정령들과 비교
            foreach (var info in fairyPool.fairies)
            {
                if (info == null) continue;

                MainFairyData poolFairyData = info.GetFairyData();
                bool isOwned = false;

                // 1. ScriptableObject 레퍼런스로 직접 비교
                if (poolFairyData != null && ownedData.Contains(poolFairyData))
                {
                    isOwned = true;
                    Debug.Log($"[FairySelectUI][DEBUG] Pool[{info.fairyType}] → 보유 중 (데이터 일치: {poolFairyData.name})");
                }
                // 2. fairyData가 없는 경우 프리팹 이름 기반 fallback 비교 (완전 일치만)
                else if (info.fairyPrefab != null)
                {
                    string prefabName = info.fairyPrefab.name.Trim();
                    foreach (var ownedName in ownedPrefabNames)
                    {
                        if (string.Equals(ownedName, prefabName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            isOwned = true;
                            Debug.Log($"[FairySelectUI][DEBUG] Pool[{info.fairyType}] → 보유 중 (이름 완전 일치: '{prefabName}')");
                            break;
                        }
                    }
                }

                if (!isOwned)
                {
                    Debug.Log($"[FairySelectUI][DEBUG] Pool[{info.fairyType}] → 미보유 (선택지 추가)");
                    unowned.Add(info);
                }
            }

            return unowned;
        }


        private List<FairyInfo> PickRandomFairies(List<FairyInfo> pool, int count)
        {
            List<FairyInfo> copy = new List<FairyInfo>(pool);
            List<FairyInfo> result = new List<FairyInfo>();

            int pickCount = Mathf.Min(count, copy.Count);
            for (int i = 0; i < pickCount; i++)
            {
                int randomIndex = Random.Range(0, copy.Count);
                result.Add(copy[randomIndex]);
                copy.RemoveAt(randomIndex);
            }

            return result;
        }
    }
}
