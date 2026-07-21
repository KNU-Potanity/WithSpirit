using UnityEngine;
using System;
using System.Collections;
using BasePlatformer.Respawn;

namespace BasePlatformer.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [Tooltip("데미지 입은 후 무적 시간(초)")]
        [SerializeField] private float invincibilityDuration = 0.5f;
        [Tooltip("무적 깜빡임 주기")]
        [SerializeField] private float blinkInterval = 0.15f;

        private RespawnManager respawnManager;
        private SpriteRenderer spriteRenderer;
        private bool isInvincible = false;
        
        public int CurrentHearts { get; private set; }
        public int MaxHearts => playerData != null ? playerData.Health : 4;

        public event Action<int> OnHealthChanged;

        private void Awake()
        {
            respawnManager = FindAnyObjectByType<RespawnManager>();
            
            // 플레이어의 그래픽을 담당하는 SpriteRenderer 찾기
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Start()
        {
            CurrentHearts = MaxHearts;
            OnHealthChanged?.Invoke(CurrentHearts);
        }

        public void TakeDamage(int damage, Vector2 knockbackDir = default)
        {
            if (isInvincible) return; // 무적 상태면 데미지 무시

            CurrentHearts -= damage;
            if (CurrentHearts < 0) CurrentHearts = 0;
            OnHealthChanged?.Invoke(CurrentHearts);

            if (CurrentHearts > 0)
            {
                // 생존 시: 넉백 적용 (위치 강제 초기화 X)
                var pm = GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    // 기본 넉백이 없으면 살짝 위로 튕기게 설정
                    if (knockbackDir == default) knockbackDir = new Vector2(-1f, 1f).normalized;
                    
                    // 넉백 힘과 지속시간 적용
                    pm.ApplyKnockback(knockbackDir * 3f, 0.4f); 
                }

                // 무적 및 깜빡임 코루틴 시작
                StartCoroutine(InvincibilityRoutine());
            }
            else
            {
                // 사망 시: 씬 재시작 등
                respawnManager?.RespawnPlayer();
            }
        }

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;
            float endTime = Time.time + invincibilityDuration;
            bool isVisible = true;

            if (spriteRenderer != null)
            {
                while (Time.time < endTime)
                {
                    isVisible = !isVisible;
                    
                    Color color = spriteRenderer.color;
                    color.a = isVisible ? 1f : 0.5f; // 알파값으로 반투명/불투명 반복
                    spriteRenderer.color = color;

                    yield return new WaitForSeconds(blinkInterval);
                }

                // 무적 종료 시 불투명 상태로 복구
                Color finalColor = spriteRenderer.color;
                finalColor.a = 1f;
                spriteRenderer.color = finalColor;
            }
            else
            {
                // SpriteRenderer가 없는 경우 시간만 대기
                yield return new WaitForSeconds(invincibilityDuration);
            }

            isInvincible = false;
        }

        public void SetHealth(int health)
        {
            CurrentHearts = health;
            if (CurrentHearts < 0) CurrentHearts = 0;
            OnHealthChanged?.Invoke(CurrentHearts);

            respawnManager?.RespawnPlayer();
        }
    }
}
