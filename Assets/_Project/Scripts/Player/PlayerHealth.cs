using UnityEngine;
using System;
using System.Collections;
using BasePlatformer.Respawn;
using BasePlatformer.Terrain;

namespace BasePlatformer.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        //[Tooltip("데미지 입은 후 무적 시간(초)")]
        private float invincibilityDuration => playerData.invincibilityDuration;
        //[Tooltip("무적 깜빡임 주기")]
        private float blinkInterval => playerData.blinkInterval;

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

            // 장막(BarrierWall)이 활성화 되어 있으면 데미지 1회 무효화
            // 씬 내 활성 BarrierWall을 탐색 (플레이어 근처에 있는 것)
            var barrierWall = FindActiveBarrierWall();
            if (barrierWall != null && barrierWall.TryAbsorb())
                return;

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

        /// <summary>
        /// 씬에서 활성화된 BarrierWall 중 이 플레이어와 가장 가까운 것을 반환합니다.
        /// 장막이 없거나 비활성 상태이면 null을 반환합니다.
        /// </summary>
        private BarrierWall FindActiveBarrierWall()
        {
            // 씬에 장막이 많지 않으므로 FindObjectsByType 사용
            var barriers = FindObjectsByType<BarrierWall>(FindObjectsInactive.Exclude);
            if (barriers.Length == 0) return null;

            BarrierWall closest = null;
            float minDist = float.MaxValue;
            Vector2 myPos = transform.position;

            foreach (var b in barriers)
            {
                float dist = Vector2.Distance(myPos, b.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = b;
                }
            }

            // 너무 멀리 있는 장막은 무시 (같은 플랫폼 위가 아닌 경우)
            return minDist <= 8f ? closest : null;
        }
    }
}
