using System.Collections.Generic;
using UnityEngine;

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

        private void Update()
        {
            UpdatePlayerFacing();
            UpdateChain();
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

                float wiggleY = (activeSlotIndex % 2 == 0) ? -0.2f : 0.2f;
                Vector3 targetOffset = new Vector3(stepX, stepY + wiggleY, 0f);
                float phaseOffset = activeSlotIndex * 0.5f;

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
        /// 다음 정령을 선택합니다 (기본 정령 -> 서브 정령 1 -> 서브 정령 2 ... 순환)
        /// </summary>
        public void SelectNextFairy()
        {
            int total = 1 + subFairies.Count;
            selectedIndex = (selectedIndex + 1) % total;
            Debug.Log($"[FairyManager] 선택된 정령 변경: Index {selectedIndex}");
        }
    }
}
