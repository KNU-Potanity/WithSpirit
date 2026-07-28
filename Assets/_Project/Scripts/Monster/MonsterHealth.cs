using System.Collections;
using UnityEngine;

namespace BasePlatformer.Monsters
{
    /// <summary>
    /// 몬스터 공통 체력/피격/사망 처리.
    /// Animator는 멈춤/걷기/공격만 담당하고, 피격/사망 연출은 이 스크립트가 별도로 처리합니다.
    /// (몬스터_기획서.md 참고 — Idle/Walk/Attack은 Animator State, Hurt/Death는 스크립트 연출)
    /// </summary>
    public class MonsterHealth : MonoBehaviour
    {
        [Header("체력")]
        [SerializeField] protected int maxHealth = 10;
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

        protected virtual void Awake()
        {
            // SpriteRenderer는 자식 오브젝트에 있을 수 있으므로 GetComponentInChildren 사용
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
            else
                Debug.LogWarning($"[MonsterHealth] {gameObject.name} 에서 SpriteRenderer를 찾을 수 없습니다!");

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
            if (spriteRenderer == null) yield break;
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(hurtFlashDuration);
            spriteRenderer.color = originalColor;
            hurtRoutine = null;
            Debug.Log("HurtFlash 실행");
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Rigidbody2D를 kinematic으로 고정해 추락 방지 (collider를 끄면 지면 충돌이 사라져 몬스터가 떨어짐)
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // AI 이동 중지
            var groundMovement = GetComponent<GroundMonsterMovement>();
            if (groundMovement != null) groundMovement.enabled = false;

            // 마지막 프레임에서 애니메이션 정지
            var animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.enabled = false;

            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
            Debug.Log("FadeOutAndDestroy 실행");
            if (spriteRenderer == null)
            {
                Destroy(gameObject);
                yield break;
            }

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
