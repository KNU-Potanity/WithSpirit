using System.Collections;
using BasePlatformer.Monsters;
using UnityEngine;

namespace BasePlatformer.Effects
{
    /// <summary>
    /// 몬스터에게 부여되는 화상 상태이상 컴포넌트.
    /// 일정 주기마다 지속 데미지를 입히며 몬스터의 스프라이트를 불꽃 주황색으로 틴트합니다.
    /// </summary>
    public class MonsterBurnEffect : MonoBehaviour
    {
        [Header("Burn Settings")]
        [SerializeField] private int burnDamage = 1;
        [SerializeField] private float burnInterval = 2f;
        [SerializeField] private float duration = 0f; // 0이면 몬스터 사망 시까지 지속
        [SerializeField] private Color burnColor = new Color(1f, 0.45f, 0.1f, 1f); // 타오르는 주황 불꽃색

        private MonsterHealth monsterHealth;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine burnRoutine;

        private void Awake()
        {
            monsterHealth = GetComponent<MonsterHealth>();
            if (monsterHealth == null)
                monsterHealth = GetComponentInParent<MonsterHealth>();

            Transform visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            spriteRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer != null)
            {
                originalColor = Color.white;
            }
        }

        public void Initialize(int damage = 1, float interval = 2f, float totalDuration = 0f)
        {
            burnDamage = damage;
            burnInterval = interval;
            duration = totalDuration;

            if (burnRoutine != null)
                StopCoroutine(burnRoutine);

            burnRoutine = StartCoroutine(BurnCoroutine());
        }

        private bool isEffectActive = false;

        private void Start()
        {
            if (burnRoutine == null)
            {
                burnRoutine = StartCoroutine(BurnCoroutine());
            }
        }

        private void LateUpdate()
        {
            // 화상 상태가 활성화되어 있고 몬스터가 살아있을 때, 스프라이트 색상을 항상 불꽃 주황색으로 유지
            if (isEffectActive && spriteRenderer != null && (monsterHealth == null || !monsterHealth.IsDead))
            {
                spriteRenderer.color = burnColor;
            }
        }

        private IEnumerator BurnCoroutine()
        {
            isEffectActive = true;
            float elapsed = 0f;

            while (monsterHealth != null && !monsterHealth.IsDead)
            {
                yield return new WaitForSeconds(burnInterval);
                elapsed += burnInterval;

                if (monsterHealth == null || monsterHealth.IsDead)
                    break;

                // 주기마다 화상 데미지 부여
                monsterHealth.TakeDamage(burnDamage);

                if (duration > 0f && elapsed >= duration)
                    break;
            }

            isEffectActive = false;
            RemoveEffect();
        }

        public void RemoveEffect()
        {
            if (burnRoutine != null)
            {
                StopCoroutine(burnRoutine);
                burnRoutine = null;
            }

            if (spriteRenderer != null && (monsterHealth == null || !monsterHealth.IsDead))
            {
                spriteRenderer.color = originalColor;
            }

            Destroy(this);
        }

        private void OnDestroy()
        {
            if (spriteRenderer != null && (monsterHealth == null || !monsterHealth.IsDead))
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}
