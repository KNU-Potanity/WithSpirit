using UnityEngine;
using UnityEngine.UI;
using BasePlatformer.Player;

namespace BasePlatformer.UI
{
    public class HeartUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("하트 아이콘들을 정렬할 부모 컨테이너 (HorizontalLayoutGroup 권장)")]
        [SerializeField] private Transform heartContainer;
        [Tooltip("생성할 하트 아이콘 프리팹 (Image 컴포넌트 포함)")]
        [SerializeField] private GameObject heartPrefab;
        
        [Header("Color Settings")]
        [Tooltip("체력이 있을 때의 하트 색상")]
        [SerializeField] private Color fullColor = Color.white;
        [Tooltip("체력이 없을 때의 빈 하트 색상 (예: 검정색, 흑백, 반투명 등)")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 1f); // 기본값: 어두운 회색

        private PlayerHealth playerHealth;
        private Image[] heartImages;

        private void Start()
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                InitializeHearts(playerHealth.MaxHearts);
                
                // 체력 변경 이벤트 구독
                playerHealth.OnHealthChanged += UpdateHearts;
                
                // 초기 UI 갱신
                UpdateHearts(playerHealth.CurrentHearts);
            }
            else
            {
                Debug.LogWarning("[HeartUI] PlayerHealth 컴포넌트를 씬에서 찾을 수 없습니다.");
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                // 메모리 누수 방지를 위한 구독 해제
                playerHealth.OnHealthChanged -= UpdateHearts;
            }
        }

        private void InitializeHearts(int maxHearts)
        {
            // 기존에 있던 임시 하트 지우기
            foreach (Transform child in heartContainer)
            {
                Destroy(child.gameObject);
            }

            heartImages = new Image[maxHearts];

            // 최대 체력만큼 하트 이미지 생성
            for (int i = 0; i < maxHearts; i++)
            {
                GameObject heartObj = Instantiate(heartPrefab, heartContainer);
                heartObj.transform.localScale = Vector3.one; // 스케일 뻥튀기 방지
                heartImages[i] = heartObj.GetComponent<Image>();
            }
        }

        private void UpdateHearts(int currentHearts)
        {
            if (heartImages == null) return;

            for (int i = 0; i < heartImages.Length; i++)
            {
                // 현재 체력보다 인덱스가 작으면 채워진 색상, 아니면 빈 색상
                if (i < currentHearts)
                {
                    heartImages[i].color = fullColor;
                }
                else
                {
                    heartImages[i].color = emptyColor;
                }
            }
        }
    }
}
