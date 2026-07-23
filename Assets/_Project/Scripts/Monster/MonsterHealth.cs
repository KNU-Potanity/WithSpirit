using System.Collections;
using UnityEngine;

namespace BasePlatformer.Monsters
{
    /// <summary>
    /// 몬스터 공통 체력/피격/사망 처리.
    /// Animator는 멈춤/걷기/공격만 담당하고, 피격/사망 연출은 이 스크립트가 별도로 처리합니다.
    /// (몬스터_기획서.md 참고 — Idle/Walk/Attack은 Animator State, Hurt/Death는 스크립트 연출)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MonsterHealth : MonoBehaviour
    {
        [Header("체력")]
        [SerializeField] private int maxHealth = 10;
        private int currentHealth;

        [Header("피격 연출 (데미지 받음)")]
        [SerializeField] private Color hurtColor = Color.red;
        [SerializeField] private float hurtFlashDuration = 0.1f;

        [Header("사망 연출 (페이드 아웃)")]
        [SerializeField] private float deathFadeDuration = 0.5f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Coroutine hurtRoutine;
        private bool isDead;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            currentHealth = maxHealth;
        }

        /// <summary>
        /// 정령 공격 등 외부에서 데미지를 줄 때 호출합니다.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (isDead) return;

            currentHealth -= amount;

            // 맞을 때마다 색을 빨갛게 깜빡임 (Animator 상태와 무관하게 동작)
            if (hurtRoutine != null) StopCoroutine(hurtRoutine);
            hurtRoutine = StartCoroutine(HurtFlash());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HurtFlash()
        {
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(hurtFlashDuration);
            spriteRenderer.color = originalColor;
            hurtRoutine = null;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // 더 이상 움직이거나 공격하지 않도록 비활성화
            var collider = GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            var animator = GetComponent<Animator>();
            if (animator != null) animator.enabled = false; // 마지막 프레임에서 정지

            var monsterAI = GetComponent<MonoBehaviour>(); // 프로젝트의 실제 AI 스크립트로 교체 필요
            if (monsterAI != null && monsterAI != this) monsterAI.enabled = false;

            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
            float elapsed = 0f;
            Color startColor = spriteRenderer.color;

            while (elapsed < deathFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / deathFadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
