using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 기본 정령 및 서브 정령들의 체인(줄서기) 추적, 정령 등록/장착, 정령 선택을 관리하는 매니저.
    /// </summary>
    public class FairyManager : MonoBehaviour
    {
        public static FairyManager Instance { get; private set; }

        [Header("Player Target")]
        [SerializeField] private Transform playerTransform;

        [Header("Base Fairy")]
        [SerializeField] private FairyMovement baseFairy;

        [Header("Chain Settings")]
        [SerializeField] private Vector3 baseFairyOffset = new Vector3(-1.5f, 1.5f, 0f);
        [SerializeField] private Vector3 subFairyOffsetStep = new Vector3(-0.8f, 0f, 0f);
        [Tooltip("방향 전환/추적 시 이동 부드러움 (값이 클수록 더 천천히/부드럽게 이동)")]
        [SerializeField] private float followSmoothTime = 0.45f;
        [Tooltip("정령 이동 최대 속도 (값이 작을수록 느리게 이동)")]
        [SerializeField] private float followMaxSpeed = 10f;

        [Header("Sub Fairies Test (Prefabs or Scene Instances)")]
        [Tooltip("인스펙터에서 프리팹(Assets/_Project/Prefabs/Entities/Fairies)을 직접 등록하면 시작할 때 자동으로 생성합니다.")]
        [SerializeField] private List<GameObject> testSubFairyPrefabs = new List<GameObject>();

        [SerializeField] private List<FairyMovement> subFairies = new List<FairyMovement>();
        private int selectedIndex = 0; // 0: 기본 정령, 1~N: 서브 정령

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }
        }

        private SpriteRenderer playerSpriteRenderer;
        private bool isFacingRight = true;

        private void Start()
        {
            if (playerTransform != null)
            {
                playerSpriteRenderer = playerTransform.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer == null)
                    playerSpriteRenderer = playerTransform.GetComponentInChildren<SpriteRenderer>();
            }

            if (baseFairy == null)
            {
                baseFairy = FindAnyObjectByType<FairyMovement>();
            }

            // 인스펙터에 등록된 서브 정령 프리팹들을 동적으로 생성(Instantiate)하여 체인에 등록
            InstantiateTestSubFairies();

            UpdateChain();
        }

        public event System.Action<FairyMovement, Color> OnFairySelected;

        private void Update()
        {
            HandleSelectionInput();
            UpdatePlayerFacing();
            UpdateChain();
        }

        private void HandleSelectionInput()
        {
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                // 마우스가 올려진 대상(몬스터 / 플랫폼)에 따라 지능적으로 다음 정령 선택
                if (IsAnyMonsterHovered())
                {
                    SelectNextAttackFairy();
                }
                else if (IsAnyPlatformHovered())
                {
                    SelectNextPlatformFairy();
                }
                else
                {
                    SelectNextFairy();
                }
            }
        }

        private bool IsAnyMonsterHovered()
        {
            MonsterClickMarker[] monsterMarkers = FindObjectsByType<MonsterClickMarker>(FindObjectsInactive.Exclude);
            foreach (var marker in monsterMarkers)
            {
                if (marker != null && marker.IsHovering) return true;
            }
            return false;
        }

        private bool IsAnyPlatformHovered()
        {
            PlatformClickMarker[] platformMarkers = FindObjectsByType<PlatformClickMarker>(FindObjectsInactive.Exclude);
            foreach (var marker in platformMarkers)
            {
                if (marker != null && marker.IsHovering) return true;
            }
            return false;
        }

        private void UpdatePlayerFacing()
        {
            if (playerSpriteRenderer != null)
            {
                // SpriteRenderer.flipX가 true면 왼쪽을 바라봄 -> 오른쪽이 등(뒤)
                isFacingRight = !playerSpriteRenderer.flipX;
            }
            else if (playerTransform != null)
            {
                isFacingRight = playerTransform.localScale.x >= 0;
            }
        }

        private void InstantiateTestSubFairies()
        {
            if (testSubFairyPrefabs == null || testSubFairyPrefabs.Count == 0)
                return;

            Vector3 spawnOrigin = (baseFairy != null) ? baseFairy.transform.position : (playerTransform != null ? playerTransform.position : Vector3.zero);

            foreach (var prefab in testSubFairyPrefabs)
            {
                if (prefab == null) continue;

                // 씬 오브젝트가 아닌 프리팹 에셋인 경우 Instantiate 실행
                GameObject instance = Instantiate(prefab, spawnOrigin, Quaternion.identity);
                FairyMovement fairyMovement = instance.GetComponent<FairyMovement>();
                if (fairyMovement != null)
                {
                    if (!subFairies.Contains(fairyMovement))
                    {
                        subFairies.Add(fairyMovement);
                    }
                }
                else
                {
                    Debug.LogWarning($"[FairyManager] 생성된 서브 정령 프리팹({prefab.name})에 FairyMovement 컴포넌트가 없습니다.");
                }
            }
        }

        /// <summary>
        /// 새로운 서브 정령을 등록하고 체인 타겟을 업데이트합니다.
        /// </summary>
        public void RegisterSubFairy(FairyMovement subFairy)
        {
            if (subFairy == null || subFairies.Contains(subFairy))
                return;

            subFairies.Add(subFairy);
            UpdateChain();
        }

        /// <summary>
        /// 서브 정령을 해제/제거하고 체인 타겟을 업데이트합니다.
        /// </summary>
        public void UnregisterSubFairy(FairyMovement subFairy)
        {
            if (subFairies.Remove(subFairy))
            {
                UpdateChain();
            }
        }

        /// <summary>
        /// 모든 정령의 따라가기 대상(FollowTarget)과 오프셋, 호버 Phase를 일괄 갱신합니다.
        /// 플레이어 시선 방향(오른쪽/왼쪽)에 따라 항상 플레이어 등(뒤) 방향으로 오프셋 X를 반전하며,
        /// 중간 정령이 공격/변신으로 이탈(LockFollow)하면 남아있는 정령들이 빈자리를 당겨 채웁니다.
        /// </summary>
        public void UpdateChain()
        {
            if (playerTransform == null) return;

            float directionSign = isFacingRight ? 1f : -1f;

            // 현재 '줄을 서서 따라다니고 있는(IsFollowing)' 정령들의 순번(activeSlotIndex)
            int activeSlotIndex = 0;

            if (baseFairy != null)
            {
                baseFairy.smoothTime = followSmoothTime;
                baseFairy.maxSpeed = followMaxSpeed;

                if (baseFairy.IsFollowing)
                {
                    Vector3 currentBaseOffset = new Vector3(baseFairyOffset.x * directionSign, baseFairyOffset.y, baseFairyOffset.z);
                    baseFairy.SetTarget(playerTransform, currentBaseOffset, 0f);
                    activeSlotIndex = 1;
                }
            }

            for (int i = 0; i < subFairies.Count; i++)
            {
                if (subFairies[i] == null) continue;

                subFairies[i].smoothTime = followSmoothTime;
                subFairies[i].maxSpeed = followMaxSpeed;

                if (!subFairies[i].IsFollowing)
                    continue; // 공격/변신으로 이탈 중인 서브 정령은 슬롯을 차지하지 않고 건너뜀

                // 남아있는 정령의 activeSlotIndex 기반으로 연쇄 오프셋 계산 (빈자리 땡겨짐)
                // activeSlotIndex가 0이면(기본정령도 이탈 시): 첫 번째 오프셋 자리 차지
                float stepX;
                float stepY;
                if (activeSlotIndex == 0)
                {
                    stepX = baseFairyOffset.x * directionSign;
                    stepY = baseFairyOffset.y;
                }
                else
                {
                    stepX = (baseFairyOffset.x + subFairyOffsetStep.x * activeSlotIndex) * directionSign;
                    stepY = baseFairyOffset.y + subFairyOffsetStep.y * activeSlotIndex;
                }

                Vector3 targetOffset = new Vector3(stepX, stepY, 0f);
                // 앞선 정령의 파동을 일정 시간차(위상 지연)로 전달받아 물결(Wave)치도록 음수 phaseOffset 적용
                float phaseOffset = -activeSlotIndex * 0.8f;

                subFairies[i].SetTarget(playerTransform, targetOffset, phaseOffset);

                activeSlotIndex++;
            }
        }

        /// <summary>
        /// 기본 정령을 반환합니다.
        /// </summary>
        public FairyMovement GetBaseFairy() => baseFairy;

        /// <summary>
        /// 현재 활성화된 서브 정령 목록을 반환합니다.
        /// </summary>
        public IReadOnlyList<FairyMovement> GetSubFairies() => subFairies;

        /// <summary>
        /// 현재 선택된 정령(FairyMovement)을 반환합니다.
        /// </summary>
        public FairyMovement GetSelectedFairy()
        {
            if (selectedIndex == 0)
                return baseFairy;

            int subIndex = selectedIndex - 1;
            if (subIndex >= 0 && subIndex < subFairies.Count)
                return subFairies[subIndex];

            return baseFairy;
        }

        /// <summary>
        /// 현재 선택된 정령의 SpriteRenderer 색상을 반환합니다. (SpriteRenderer가 없으면 Color.white)
        /// </summary>
        public Color GetSelectedFairyColor()
        {
            FairyMovement fairy = GetSelectedFairy();
            return GetFairyColor(fairy);
        }

        /// <summary>
        /// 특정 정령 오브젝트/컴포넌트의 SpriteRenderer 색상을 반환합니다. (null이거나 없으면 선택된 정령 색상 또는 흰색)
        /// </summary>
        public Color GetFairyColor(object fairyObj)
        {
            if (fairyObj is Component comp)
            {
                SpriteRenderer sr = comp.GetComponent<SpriteRenderer>();
                if (sr == null) sr = comp.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) return sr.color;
            }
            else if (fairyObj is GameObject go)
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) return sr.color;
            }
            return Color.white;
        }

        /// <summary>
        /// 지정한 인덱스(0: 기본 정령, 1~N: 서브 정령)의 FairyMovement를 반환합니다.
        /// </summary>
        public FairyMovement GetFairyAtIndex(int index)
        {
            if (index == 0) return baseFairy;
            int subIdx = index - 1;
            if (subIdx >= 0 && subIdx < subFairies.Count) return subFairies[subIdx];
            return null;
        }

        /// <summary>
        /// 전체 등록된 정령(기본 정령 + 서브 정령)의 총 개수를 반환합니다.
        /// </summary>
        public int TotalFairyCount => 1 + (subFairies != null ? subFairies.Count : 0);

        /// <summary>
        /// 특정 인덱스의 정령을 명시적으로 선택합니다.
        /// </summary>
        public void SelectFairyAtIndex(int index)
        {
            int total = TotalFairyCount;
            if (total == 0) return;

            int clampedIndex = Mathf.Clamp(index, 0, total - 1);
            if (selectedIndex != clampedIndex)
            {
                selectedIndex = clampedIndex;
                FairyMovement selected = GetSelectedFairy();
                Color color = GetSelectedFairyColor();

                Debug.Log($"[FairyManager] 대상 조준으로 선택 정령 자동 변경: Index {selectedIndex} (Name: {selected?.gameObject.name}, Color: {color})");
                OnFairySelected?.Invoke(selected, color);
            }
        }

        /// <summary>
        /// 몬스터를 조준했을 때, 현재 선택된 정령 인덱스로부터 가장 가까운 '공격 가능(IFairyAttack)'한 정령을 찾아 반환합니다.
        /// (유저가 선택한 정령 인덱스는 변경되지 않고 그대로 유지됩니다)
        /// </summary>
        public IFairyAttack GetBestFairyForAttack()
        {
            int count = TotalFairyCount;
            if (count == 0) return null;

            // 1. 선택된 인덱스부터 순환하면서 IFairyAttack을 가지고 있고 CanAttack인 정령 탐색
            for (int i = 0; i < count; i++)
            {
                int checkIndex = (selectedIndex + i) % count;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                IFairyAttack attackCtrl = fairy.GetComponent<IFairyAttack>();
                if (attackCtrl != null && attackCtrl.CanAttack)
                {
                    return attackCtrl;
                }
            }

            // 2. 모든 정령이 쿨타임/이탈 중이어도 IFairyAttack 컴포넌트를 가진 가장 가까운 정령 반환
            for (int i = 0; i < count; i++)
            {
                int checkIndex = (selectedIndex + i) % count;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                IFairyAttack attackCtrl = fairy.GetComponent<IFairyAttack>();
                if (attackCtrl != null)
                {
                    return attackCtrl;
                }
            }

            return null;
        }

        /// <summary>
        /// 플랫폼 마커를 조준했을 때, 현재 선택된 정령 인덱스로부터 가장 가까운 '플랫폼 변신 가능(IFairyPlatform)'한 정령을 찾아 반환합니다.
        /// (유저가 선택한 정령 인덱스는 변경되지 않고 그대로 유지됩니다)
        /// </summary>
        public IFairyPlatform GetBestFairyForPlatform()
        {
            int count = TotalFairyCount;
            if (count == 0) return null;

            // 1. 선택된 인덱스부터 순환하면서 IFairyPlatform을 가지고 있고 CanTransform인 정령 탐색
            for (int i = 0; i < count; i++)
            {
                int checkIndex = (selectedIndex + i) % count;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                IFairyPlatform platCtrl = fairy.GetComponent<IFairyPlatform>();
                if (platCtrl != null && platCtrl.CanTransform)
                {
                    return platCtrl;
                }
            }

            // 2. 이미 플랫폼을 설치한 상태에서 다른 마커로 재변신(RequestTransformOrReplace)하기 위해 활성 플랫폼 보유 정령 탐색
            for (int i = 0; i < count; i++)
            {
                int checkIndex = (selectedIndex + i) % count;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                IFairyPlatform platCtrl = fairy.GetComponent<IFairyPlatform>();
                if (platCtrl != null && platCtrl.HasActivePlatform)
                {
                    return platCtrl;
                }
            }

            // 3. 그 외 IFairyPlatform을 가진 가장 가까운 정령 반환
            for (int i = 0; i < count; i++)
            {
                int checkIndex = (selectedIndex + i) % count;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                IFairyPlatform platCtrl = fairy.GetComponent<IFairyPlatform>();
                if (platCtrl != null)
                {
                    return platCtrl;
                }
            }

            return null;
        }

        /// <summary>
        /// 활성화된 모든 플랫폼 정령의 플랫폼을 해제합니다. (몬스터 클릭 시 사용)
        /// </summary>
        public void RevertAllActivePlatforms()
        {
            int count = TotalFairyCount;
            for (int i = 0; i < count; i++)
            {
                FairyMovement fairy = GetFairyAtIndex(i);
                if (fairy == null) continue;

                IFairyPlatform platCtrl = fairy.GetComponent<IFairyPlatform>();
                if (platCtrl != null && platCtrl.HasActivePlatform)
                {
                    platCtrl.MarkClickHandled();
                    platCtrl.RevertTransform();
                }
            }
        }

        /// <summary>
        /// 다음 정령을 선택합니다 (기본 정령 -> 서브 정령 1 -> 서브 정령 2 ... 순환)
        /// </summary>
        public void SelectNextFairy()
        {
            int total = TotalFairyCount;
            if (total == 0) return;

            selectedIndex = (selectedIndex + 1) % total;

            FairyMovement selected = GetSelectedFairy();
            Color color = GetSelectedFairyColor();

            Debug.Log($"[FairyManager] 선택된 정령 변경: Index {selectedIndex} (Name: {selected?.gameObject.name}, Color: {color})");
            OnFairySelected?.Invoke(selected, color);
        }

        /// <summary>
        /// 몬스터 호버 상태에서 우클릭 시, 현재 조준/선택된 공격 정령의 다음 공격 정령(IFairyAttack)으로 즉시 전환합니다.
        /// </summary>
        public void SelectNextAttackFairy()
        {
            int total = TotalFairyCount;
            if (total <= 1)
            {
                SelectNextFairy();
                return;
            }

            // 현재 이 대상에게 실제로 조준되고 있는 공격 정령을 기준 시작점으로 잡음
            IFairyAttack currentBest = GetBestFairyForAttack();
            int baseIndex = selectedIndex;
            if (currentBest is Component comp)
            {
                int found = GetFairyIndex(comp.GetComponent<FairyMovement>());
                if (found != -1) baseIndex = found;
            }

            // 기준 정령 다음 번호부터 1바퀴 순환하며 IFairyAttack을 가진 첫 번째 정령을 선택
            for (int i = 1; i <= total; i++)
            {
                int checkIndex = (baseIndex + i) % total;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                if (fairy.GetComponent<IFairyAttack>() != null)
                {
                    SelectFairyAtIndex(checkIndex);
                    return;
                }
            }

            // 공격 정령을 별도로 찾지 못한 경우 일반 다음 정령 선택
            SelectNextFairy();
        }

        /// <summary>
        /// 플랫폼 호버 상태에서 우클릭 시, 현재 조준/선택된 플랫폼 정령의 다음 플랫폼 정령(IFairyPlatform)으로 즉시 전환합니다.
        /// </summary>
        public void SelectNextPlatformFairy()
        {
            int total = TotalFairyCount;
            if (total <= 1)
            {
                SelectNextFairy();
                return;
            }

            // 현재 이 대상에게 실제로 조준되고 있는 플랫폼 정령을 기준 시작점으로 잡음
            IFairyPlatform currentBest = GetBestFairyForPlatform();
            int baseIndex = selectedIndex;
            if (currentBest is Component comp)
            {
                int found = GetFairyIndex(comp.GetComponent<FairyMovement>());
                if (found != -1) baseIndex = found;
            }

            // 기준 정령 다음 번호부터 1바퀴 순환하며 IFairyPlatform을 가진 첫 번째 정령을 선택
            for (int i = 1; i <= total; i++)
            {
                int checkIndex = (baseIndex + i) % total;
                FairyMovement fairy = GetFairyAtIndex(checkIndex);
                if (fairy == null) continue;

                if (fairy.GetComponent<IFairyPlatform>() != null)
                {
                    SelectFairyAtIndex(checkIndex);
                    return;
                }
            }

            // 플랫폼 정령을 별도로 찾지 못한 경우 일반 다음 정령 선택
            SelectNextFairy();
        }

        /// <summary>
        /// 특정 FairyMovement 인스턴스의 인덱스를 반환합니다 (0: 기본 정령, 1~N: 서브 정령, 미등록: -1)
        /// </summary>
        public int GetFairyIndex(FairyMovement fairy)
        {
            if (fairy == null) return -1;
            if (fairy == baseFairy) return 0;
            if (subFairies != null)
            {
                int subIdx = subFairies.IndexOf(fairy);
                if (subIdx != -1) return subIdx + 1;
            }
            return -1;
        }
    }
}
