using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// BarrierPlatform 프리팹의 자식 오브젝트 "Barrier"에 붙이는 스크립트.
    ///
    /// ■ 동작 방식:
    ///   - 몬스터(Monster 레이어)와 화살(MonsterProjectile 컴포넌트 보유)은 Barrier 콜라이더에 막힘
    ///   - 위 두 종류가 Barrier에 닿거나, 근접 공격으로 TryAbsorb()가 호출되면 → 장막(자신) 비활성화
    ///   - 플레이어(Player 태그)는 물리 충돌 무시하여 통과
    ///
    /// ■ 근접 공격 차단:
    ///   몬스터는 공격 범위 안에서 PlayerHealth.TakeDamage()를 직접 호출합니다.
    ///   PlayerHealth.TakeDamage()는 데미지 적용 전 BarrierWall.TryAbsorb()를 먼저 확인합니다.
    ///   장막이 활성 상태이면 데미지를 무효화하고 장막을 소멸시킵니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BarrierWall : MonoBehaviour
    {
        [Header("플레이어 통과 태그")]
        [Tooltip("이 태그를 가진 오브젝트는 Barrier를 물리적으로 통과합니다.")]
        [SerializeField] private string playerTag = "Player";

        private Collider2D myCollider;
        private bool isConsumed = false;

        /// <summary>장막이 아직 살아있는지 외부에서 확인할 때 사용.</summary>
        public bool IsActive => !isConsumed && gameObject.activeSelf;

        private void Awake()
        {
            myCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            isConsumed = false;
        }

        // ─── 물리 충돌 (isTrigger = false) ───────────────────────────────

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleContact(collision.gameObject, collision.collider);
        }

        // ─── 트리거 충돌 (isTrigger = true) ─────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleContact(other.gameObject, other);
        }

        // ─── 공통 접촉 처리 ───────────────────────────────────────────────

        private void HandleContact(GameObject other, Collider2D otherCollider)
        {
            if (isConsumed) return;

            // 플레이어는 물리 충돌 무시 (통과)
            if (other.CompareTag(playerTag))
            {
                if (!myCollider.isTrigger)
                    Physics2D.IgnoreCollision(myCollider, otherCollider, true);
                return;
            }

            // 접촉 데미지 몬스터 또는 투사체가 Barrier에 닿으면 장막 소멸
            // - ChestnutMonsterMovement : 점프해서 몸으로 부딪히는 몬스터
            // - RangedMonsterMovement   : 원거리지만 몸에 닿아도 데미지를 주는 몬스터
            // - MonsterProjectile       : 화살 등 투사체
            bool isContactDamageMonster = other.GetComponent<ChestnutMonsterMovement>() != null
                                       || other.GetComponent<RangedMonsterMovement>() != null;
            bool isProjectile = other.GetComponent<MonsterProjectile>() != null;

            if (isContactDamageMonster || isProjectile)
            {
                Consume();
            }
        }

        // ─── 근접 공격 차단 (PlayerHealth.TakeDamage에서 호출) ───────────

        /// <summary>
        /// 장막이 활성 상태이면 데미지를 1회 흡수하고 자신을 비활성화합니다.
        /// true를 반환하면 PlayerHealth 측에서 데미지를 무시합니다.
        /// </summary>
        public bool TryAbsorb()
        {
            if (isConsumed || !gameObject.activeSelf) return false;

            Consume();
            return true;
        }

        // ─── 장막 소멸 ────────────────────────────────────────────────────

        private void Consume()
        {
            if (isConsumed) return;
            isConsumed = true;

            Debug.Log("[BarrierWall] 장막 소멸");
            gameObject.SetActive(false);
        }
    }
}
