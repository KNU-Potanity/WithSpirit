using System;
using System.Collections;
using UnityEngine;

namespace BasePlatformer.Monsters
{
    /// <summary>
    /// Common monster health / hit / death handling.
    /// Uses the first child as the visual root.
    /// </summary>
    public class MonsterHealth : MonoBehaviour
    {
        [Header("Monster Data")]
        public MonsterData monsterData;
        
        private int currentHealth;

        
        private Color hurtColor = Color.red;
        private float hurtFlashDuration;
        private float deathFadeDuration;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Color originalColor;
        private Coroutine hurtRoutine;
        private bool isDead;

        public bool IsDead => isDead;

        /// <summary>
        /// MonsterData에서 설정된 최대 체력값.
        /// </summary>
        public int MaxHealth => monsterData != null ? monsterData.Health : 0;

        /// <summary>
        /// 현재 체력값.
        /// </summary>
        public int CurrentHealthValue => currentHealth;

        /// <summary>
        /// 체력이 변경될 때 발생하는 이벤트. (현재 체력, 최대 체력)
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        protected virtual void Awake()
        {
            if (monsterData != null)
            {
                currentHealth = monsterData.Health;
                hurtFlashDuration = monsterData.hurtFlashDuration;
                deathFadeDuration = monsterData.deathFadeDuration;
            }
            else
            {
                Debug.LogWarning($"[GroundMonsterHealth] {gameObject.name} has no monsterData assigned. Using default maxHealth.");
            }
            
            Transform visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;

            spriteRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            else
            {
                Debug.LogWarning($"[MonsterHealth] {gameObject.name} could not find a SpriteRenderer on the first child.");
            }

            animator = visualRoot.GetComponentInChildren<Animator>(true);
        }

        public void TakeDamage(int amount)
        {
            if (isDead) return;

            currentHealth -= amount;

            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            if (hurtRoutine != null)
                StopCoroutine(hurtRoutine);

            hurtRoutine = StartCoroutine(HurtFlash());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HurtFlash()
        {
            if (spriteRenderer == null)
                yield break;

            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(hurtFlashDuration);
            spriteRenderer.color = originalColor;
            hurtRoutine = null;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // IMonsterMovement를 구현한 모든 몬스터 로직/이동 컴포넌트를 일괄 비활성화
            var movements = GetComponents<IMonsterMovement>();
            foreach (var movement in movements)
            {
                if (movement is MonoBehaviour mb)
                {
                    mb.enabled = false;
                }
            }

            // 클릭 마커 비활성화
            var clickMarkers = GetComponentsInChildren<MonsterClickMarker>(true);
            foreach (var marker in clickMarkers)
            {
                marker.HideMarker();
                marker.enabled = false;
            }

            if (animator != null)
                animator.enabled = false;

            StartCoroutine(FadeOutAndDestroy());
        }

        private IEnumerator FadeOutAndDestroy()
        {
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
