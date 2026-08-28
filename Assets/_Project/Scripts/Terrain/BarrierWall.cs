using UnityEngine;
using System.Collections;
using BasePlatformer.Monsters;

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

        [Header("몬스터 공격 연출")]
        [Tooltip("근접/디버프 몬스터가 배리어를 때릴 때 공격 애니메이션 후 소멸까지의 딜레이(초)")]
        [SerializeField] private float monsterAttackDelay = 0.4f;

        /// <summary>이미 공격 모션을 시작한 몬스터를 추적하여 중복 트리거 방지.</summary>
        private readonly System.Collections.Generic.HashSet<GameObject> attackingMonsters
            = new System.Collections.Generic.HashSet<GameObject>();


        /// <summary>장막이 아직 살아있는지 외부에서 확인할 때 사용.</summary>
        public bool IsActive => !isConsumed && gameObject.activeSelf;

        private void Awake()
        {
            myCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            isConsumed = false;
            attackingMonsters.Clear();
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

            // IBarrierConsumable 인터페이스를 구현한 대상(접촉 데미지 몬스터, 투사체 등)이 Barrier에 닿으면 장막 소멸
            if (other.GetComponent<IBarrierConsumable>() != null)
            {
                Consume();
                return;
            }

            // 근접/디버프 몬스터(IMonsterMovement는 있지만 IBarrierConsumable이 아닌)가
            // 배리어에 닿으면 → 공격 애니메이션 재생 후 장막 소멸
            if (other.GetComponent<IMonsterMovement>() != null
                && !attackingMonsters.Contains(other))
            {
                attackingMonsters.Add(other);
                StartCoroutine(MonsterAttackBarrierCoroutine(other));
            }
        }
        // ─── 근접/디버프 몬스터의 배리어 공격 코루틴 ────────────────────

        /// <summary>
        /// 근접/디버프 몬스터가 배리어에 닿았을 때:
        ///  1) 몬스터의 이동을 잠깐 멈추고 배리어를 바라보게 함
        ///  2) Attack 애니메이션 트리거 재생
        ///  3) monsterAttackDelay 후 장막 소멸
        /// </summary>
        private IEnumerator MonsterAttackBarrierCoroutine(GameObject monster)
        {
            if (monster == null) yield break;

            // 몬스터의 이동 스크립트(MonoBehaviour)를 비활성화하여
            // FixedUpdate가 속도를 덮어쓰지 못하게 함
            var movement = monster.GetComponent<IMonsterMovement>() as MonoBehaviour;
            if (movement != null)
                movement.enabled = false;

            // 몬스터의 Rigidbody X 속도를 0으로 정지
            Rigidbody2D monsterRb = monster.GetComponent<Rigidbody2D>();
            if (monsterRb != null)
                monsterRb.linearVelocity = new Vector2(0f, monsterRb.linearVelocity.y);

            // 몬스터가 배리어를 바라보도록 방향 조정
            float dirToBarrier = transform.position.x - monster.transform.position.x;
            if (dirToBarrier != 0f)
            {
                Vector3 scale = monster.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (dirToBarrier > 0f ? 1f : -1f);
                monster.transform.localScale = scale;
            }

            // Attack 애니메이션 트리거 재생
            Animator monsterAnim = monster.GetComponentInChildren<Animator>();
            if (monsterAnim != null)
                monsterAnim.SetTrigger("Attack");

            // 공격 딜레이 대기
            yield return new WaitForSeconds(monsterAttackDelay);

            // 딜레이 후 장막 소멸
            Consume();

            // 몬스터의 이동 스크립트 다시 활성화
            if (movement != null)
                movement.enabled = true;
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

            // 배리어 비활성화 시 모든 코루틴이 중단되므로,
            // 공격 중인 몬스터들의 이동 스크립트를 먼저 복구
            foreach (var monster in attackingMonsters)
            {
                if (monster == null) continue;
                var movement = monster.GetComponent<IMonsterMovement>() as MonoBehaviour;
                if (movement != null)
                    movement.enabled = true;
            }
            attackingMonsters.Clear();

            Debug.Log("[BarrierWall] 장막 소멸");
            gameObject.SetActive(false);
        }
    }
}
