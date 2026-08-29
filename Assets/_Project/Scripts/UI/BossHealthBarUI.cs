using UnityEngine;
using UnityEngine.UI;
using BasePlatformer.Monsters;

namespace BasePlatformer.UI
{
    /// <summary>
    /// 보스 체력바 UI.
    /// 지정된 MonsterHealth를 추적하여 Slider 기반 체력바를 표시합니다.
    /// 보스가 없으면 자동으로 숨기고, 보스가 등장하면 자동으로 표시합니다.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("체력바 Slider (Image Fill 방식)")]
        [SerializeField] private Slider healthSlider;

        [Tooltip("보스 이름 텍스트 (선택사항)")]
        [SerializeField] private TMPro.TextMeshProUGUI bossNameText;

        [Header("Settings")]
        [Tooltip("추적할 보스의 태그 (비워두면 수동 연결만 사용)")]
        [SerializeField] private string bossTag = "Boss";

        [Tooltip("체력바 감소 시 부드럽게 보간할 속도 (0이면 즉시 반영)")]
        [SerializeField] private float lerpSpeed = 5f;

        [Header("Auto Hide")]
        [Tooltip("보스가 없을 때 체력바 전체 패널을 숨길 것인지")]
        [SerializeField] private bool autoHide = true;

        private MonsterHealth trackedBoss;
        private GameObject panelRoot;
        private float targetValue = 1f;

        private void Awake()
        {
            // 체력바 패널의 루트 (이 컴포넌트가 붙은 오브젝트)
            panelRoot = gameObject;

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = 1f;
            }
        }

        private void Start()
        {
            // 보스 자동 탐색
            if (trackedBoss == null)
            {
                TryFindBoss();
            }

            // 보스가 아직 없으면 숨기기
            if (trackedBoss == null && autoHide)
            {
                panelRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // 보스가 아직 연결되지 않았으면 주기적으로 탐색
            if (trackedBoss == null)
            {
                TryFindBoss();
                return;
            }

            // 보스가 파괴되었으면 숨기기
            if (trackedBoss.IsDead)
            {
                if (autoHide)
                    panelRoot.SetActive(false);
                trackedBoss = null;
                return;
            }

            // 부드러운 체력바 보간
            if (healthSlider != null && lerpSpeed > 0f)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * lerpSpeed);
            }
        }

        /// <summary>
        /// 태그로 보스를 자동 탐색하여 연결합니다.
        /// </summary>
        private void TryFindBoss()
        {
            if (string.IsNullOrEmpty(bossTag)) return;

            GameObject bossObj = GameObject.FindWithTag(bossTag);
            if (bossObj != null)
            {
                var health = bossObj.GetComponent<MonsterHealth>();
                if (health != null)
                {
                    SetBoss(health);
                }
            }
        }

        /// <summary>
        /// 보스를 수동으로 연결합니다.
        /// 외부 스크립트에서 보스 스폰 시 호출할 수 있습니다.
        /// </summary>
        public void SetBoss(MonsterHealth bossHealth)
        {
            // 기존 보스 구독 해제
            if (trackedBoss != null)
            {
                trackedBoss.OnHealthChanged -= OnBossHealthChanged;
            }

            trackedBoss = bossHealth;

            if (trackedBoss != null)
            {
                // 이벤트 구독
                trackedBoss.OnHealthChanged += OnBossHealthChanged;

                // 이름 표시
                if (bossNameText != null)
                {
                    bossNameText.text = trackedBoss.gameObject.name;
                }

                // 초기값 설정
                targetValue = 1f;
                if (healthSlider != null)
                {
                    healthSlider.value = 1f;
                }

                // 패널 표시
                if (autoHide)
                {
                    panelRoot.SetActive(true);
                }
            }
        }

        private void OnBossHealthChanged(int currentHealth, int maxHealth)
        {
            if (maxHealth <= 0) return;

            targetValue = (float)currentHealth / maxHealth;

            // lerpSpeed가 0이면 즉시 반영
            if (healthSlider != null && lerpSpeed <= 0f)
            {
                healthSlider.value = targetValue;
            }
        }

        private void OnDestroy()
        {
            if (trackedBoss != null)
            {
                trackedBoss.OnHealthChanged -= OnBossHealthChanged;
            }
        }
    }
}
