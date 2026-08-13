using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// CrumblingPlatform의 내부 헬퍼 컴포넌트.
    ///
    /// PlacedPlatform 프리팹의 자식 오브젝트(Platform)에 런타임에 자동 추가됩니다.
    /// 자식의 OnCollisionEnter2D / OnCollisionStay2D 이벤트를 부모의 CrumblingPlatform으로 전달합니다.
    ///
    /// ※ 직접 Inspector에서 붙이지 마세요. CrumblingPlatform.Start()가 자동으로 관리합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public class CrumblingPlatformTrigger : MonoBehaviour
    {
        private CrumblingPlatform owner;

        /// <summary>
        /// CrumblingPlatform.Start()에서 호출하여 부모 참조를 주입합니다.
        /// </summary>
        public void Initialize(CrumblingPlatform crumblingPlatform)
        {
            owner = crumblingPlatform;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (owner == null) return;

            // 플레이어가 발판 위에서 접촉 시작
            if (IsPlayerAbove(collision))
            {
                owner.OnPlayerLanded();
                return;
            }

            // 낙하 중 다른 충돌체와 접촉 → 즉시 삭제
            owner.OnHitWhileFalling();
        }

        // ─── Helper ───────────────────────────────────────────────────────

        /// <summary>
        /// 충돌 접점이 플랫폼 상단에서 발생했는지(플레이어가 위에서 밟았는지) 판정합니다.
        /// </summary>
        private bool IsPlayerAbove(Collision2D collision)
        {
            // PlayerMovement 또는 PlayerHealth가 붙어있으면 플레이어로 간주
            if (collision.gameObject.GetComponent<Player.PlayerMovement>() == null &&
                collision.gameObject.GetComponent<Player.PlayerHealth>() == null)
                return false;

            // 접점 노멀이 위쪽을 향하면(플랫폼 상단 충돌) 플레이어가 위에서 밟은 것
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 플랫폼 입장에서 노멀이 아래쪽이면 플레이어가 위에서 누르는 것
                if (contact.normal.y < -0.5f)
                    return true;
            }

            return false;
        }
    }
}
