using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 점프력 증가 발판 컴포넌트.
    ///
    /// PlacedPlatform과 동일한 구조의 프리팹 루트 오브젝트에 부착합니다.
    /// 구조: 루트 (Grid, 이 컴포넌트) → 자식 Platform (Tilemap, TilemapCollider2D, CompositeCollider2D, Rigidbody2D)
    ///
    /// 동작:
    ///   - 플레이어가 발판 위에 올라서면 즉시 점프력 버프를 부여합니다.
    ///   - 버프 지속 시간이 지나면 점프력이 원래대로 돌아옵니다.
    ///   - 버프 도중 다시 밟으면 지속 시간이 갱신됩니다.
    /// </summary>
    public class JumpBoostPlatform : MonoBehaviour
    {
        [Header("Jump Boost Settings")]
        [Tooltip("점프 속도 배율 (1.5 = 점프 높이 약 1.5배)")]
        [SerializeField] private float jumpMultiplier = 1.5f;

        // ─── Internal ─────────────────────────────────────────────────────

        private void Start()
        {
            // 자식 Platform 오브젝트에 충돌 이벤트 브릿지 추가
            var rb = GetComponentInChildren<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning("[JumpBoostPlatform] 자식 오브젝트에서 Rigidbody2D를 찾을 수 없습니다!", this);
                return;
            }

            var bridge = rb.GetComponent<JumpBoostPlatformTrigger>();
            if (bridge == null)
                bridge = rb.gameObject.AddComponent<JumpBoostPlatformTrigger>();

            bridge.Initialize(this);
        }

        // ─── Public API (called by JumpBoostPlatformTrigger) ──────────────

        /// <summary>
        /// 플레이어가 발판에 올라섰을 때 호출됩니다.
        /// </summary>
        public void OnPlayerEnter(Player.PlayerJump playerJump)
        {
            playerJump.SetJumpBuff(jumpMultiplier);
        }

        /// <summary>
        /// 플레이어가 발판에서 이탈했을 때 호출됩니다.
        /// </summary>
        public void OnPlayerExit(Player.PlayerJump playerJump)
        {
            playerJump.ClearJumpBuff();
        }
    }
}
