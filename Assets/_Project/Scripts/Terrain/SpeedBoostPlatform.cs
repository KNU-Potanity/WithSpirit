using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 이속 증가 발판 컴포넌트.
    ///
    /// PlacedPlatform과 동일한 구조의 프리팹 루트 오브젝트에 부착합니다.
    /// 구조: 루트 (Grid, 이 컴포넌트) → 자식 Platform (Tilemap, TilemapCollider2D, CompositeCollider2D, Rigidbody2D)
    ///
    /// 동작:
    ///   - 플레이어가 발판 위에 올라서면 즉시 이동 속도 버프를 부여합니다.
    ///   - 버프 지속 시간이 지나면 속도가 원래대로 돌아옵니다.
    ///   - 버프 도중 다시 밟으면 지속 시간이 갱신됩니다.
    /// </summary>
    public class SpeedBoostPlatform : MonoBehaviour
    {
        [Header("Speed Boost Settings")]
        [Tooltip("이동 속도 배율 (1.5 = 1.5배)")]
        [SerializeField] private float speedMultiplier = 1.5f;

        [Tooltip("버프 지속 시간 (초)")]
        [SerializeField] private float buffDuration = 3f;

        // ─── Internal ─────────────────────────────────────────────────────

        private void Start()
        {
            // 자식 Platform 오브젝트에 충돌 이벤트 브릿지 추가
            var rb = GetComponentInChildren<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("[SpeedBoostPlatform] 자식 오브젝트에서 Rigidbody2D를 찾을 수 없습니다!", this);
                return;
            }

            var bridge = rb.GetComponent<SpeedBoostPlatformTrigger>();
            if (bridge == null)
                bridge = rb.gameObject.AddComponent<SpeedBoostPlatformTrigger>();

            bridge.Initialize(this);
        }

        // ─── Public API (called by SpeedBoostPlatformTrigger) ─────────────

        /// <summary>
        /// 플레이어가 발판에 올라섰을 때 헬퍼 컴포넌트가 호출합니다.
        /// </summary>
        public void OnPlayerContact(Player.PlayerMovement playerMovement)
        {
            playerMovement.ApplySpeedBuff(speedMultiplier, buffDuration);
        }
    }
}
