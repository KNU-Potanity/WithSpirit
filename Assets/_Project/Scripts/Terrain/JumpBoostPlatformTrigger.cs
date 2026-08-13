using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// JumpBoostPlatform의 내부 헬퍼 컴포넌트.
    ///
    /// 자식 Platform 오브젝트에 런타임에 자동 추가됩니다.
    /// OnCollisionEnter2D로 플레이어 착지를 감지하여 JumpBoostPlatform에 전달합니다.
    ///
    /// ※ 직접 Inspector에서 붙이지 마세요. JumpBoostPlatform.Start()가 자동으로 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class JumpBoostPlatformTrigger : MonoBehaviour
    {
        private JumpBoostPlatform owner;

        public void Initialize(JumpBoostPlatform platform)
        {
            owner = platform;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (owner == null) return;

            if (!IsPlayerAbove(collision)) return;

            var playerJump = collision.gameObject.GetComponent<Player.PlayerJump>();
            if (playerJump != null)
                owner.OnPlayerEnter(playerJump);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (owner == null) return;

            // 플레이어인지 확인 (이탈 시는 노멀 방향 판정 불가능하므로 커폴넷 타입만 체크)
            if (collision.gameObject.GetComponent<Player.PlayerMovement>() == null &&
                collision.gameObject.GetComponent<Player.PlayerHealth>() == null)
                return;

            var playerJump = collision.gameObject.GetComponent<Player.PlayerJump>();
            if (playerJump != null)
                owner.OnPlayerExit(playerJump);
        }

        // ─── Helper ───────────────────────────────────────────────────────

        /// <summary>
        /// 충돌 접점이 발판 상단인지(플레이어가 위에서 밟았는지) 판정합니다.
        /// </summary>
        private bool IsPlayerAbove(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<Player.PlayerMovement>() == null &&
                collision.gameObject.GetComponent<Player.PlayerHealth>() == null)
                return false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 플랫폼 입장에서 노멀이 아래쪽 → 플레이어가 위에서 누르는 것
                if (contact.normal.y < -0.5f)
                    return true;
            }

            return false;
        }
    }
}
