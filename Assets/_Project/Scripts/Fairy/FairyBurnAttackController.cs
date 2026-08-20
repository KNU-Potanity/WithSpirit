using System.Collections;
using BasePlatformer.Monsters;
using BasePlatformer.Effects;
using UnityEngine;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 화상 정령(빨간색/주황색)의 공격 특화 컨트롤러.
    /// 목표 몬스터로 돌진하여 타격 시 기본 데미지와 함께 화상(2초마다 1 데미지 지속 및 불꽃색 틴트) 상태이상을 부여합니다.
    /// </summary>
    public class FairyBurnAttackController : MonoBehaviour, IFairyAttack
    {
        [Header("Fairy Data")]
        public MainFairyData fairyData;

        [Header("Attack Motion")]
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float returnSpeed = 18f;
        [SerializeField] private float hitDistance = 0.15f;
        [SerializeField] private float returnDistance = 0.05f;

        [Header("Burn Settings")]
        [Tooltip("화상 데미지 주기 (초)")]
        [SerializeField] private float burnInterval = 2f;
        [Tooltip("화상 지속 시간 (0이면 사망 시까지 지속)")]
        [SerializeField] private float burnDuration = 0f;

        private int burnTickDamage = 1;
        private float coolTime = 0.5f;
        private float coolTimer;

        private FairyMovement fairyMovement;
        private FairyPlatformController fairyPlatform;
        private Coroutine attackRoutine;
        private bool isAttacking;

        private void Awake()
        {
            fairyMovement = GetComponent<FairyMovement>();
            fairyPlatform = GetComponent<FairyPlatformController>();

            if (fairyData != null)
            {
                // fairyData의 Damage를 화상 틱 데미지로 적용
                burnTickDamage = fairyData.Damage;
                coolTime = fairyData.CoolTime;
            }
            else
            {
                Debug.LogWarning("[FairyBurnAttackController] fairyData가 할당되지 않았습니다! 기본값을 사용합니다.");
            }
        }

        private void Update()
        {
            if (coolTimer > 0f)
                coolTimer -= Time.deltaTime;
        }

        public void RequestAttack(MonsterHealth target)
        {
            if (isAttacking)
                return;

            if (coolTimer > 0f)
            {
                Debug.Log($"[FairyBurnAttackController] 쿨타임 중 ({coolTimer:F1}초 남음)");
                return;
            }

            if (fairyPlatform != null && fairyPlatform.HasActivePlatform)
            {
                Debug.Log("[FairyBurnAttackController] 정령이 플랫폼 변신 중이라 공격을 시작할 수 없습니다.");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("[FairyBurnAttackController] 공격 대상이 없습니다.");
                return;
            }

            if (attackRoutine != null)
                StopCoroutine(attackRoutine);

            attackRoutine = StartCoroutine(AttackRoutine(target));
        }

        private IEnumerator AttackRoutine(MonsterHealth target)
        {
            isAttacking = true;
            coolTimer = coolTime;

            Vector3 homePosition = transform.position;

            if (fairyMovement != null)
                fairyMovement.LockFollow();

            // 1. 목표 몬스터로 돌진
            yield return MoveToTarget(target);

            // 2. 목표 지점 도달 시 즉발 데미지 없이 화상(지속 피해) 상태이상만 부여
            if (target != null)
            {
                // 기존에 빙결 효과가 걸려있다면 즉시 해제(오버라이드)
                MonsterFreezeEffect existingFreeze = target.GetComponent<MonsterFreezeEffect>();
                if (existingFreeze != null)
                {
                    Destroy(existingFreeze);
                }

                // 화상 효과 부여 (기존에 이미 붙어있으면 초기화/갱신)
                MonsterBurnEffect burnEffect = target.GetComponent<MonsterBurnEffect>();
                if (burnEffect == null)
                {
                    burnEffect = target.gameObject.AddComponent<MonsterBurnEffect>();
                }
                burnEffect.Initialize(burnTickDamage, burnInterval, burnDuration);

                Debug.Log($"[FairyBurnAttackController] {target.gameObject.name} 화상 부여! (틱 데미지: {burnTickDamage}, 주기: {burnInterval}초)");
            }

            // 3. 원래 위치로 복귀
            yield return ReturnHome(homePosition);

            if (fairyMovement != null)
                fairyMovement.UnlockFollow();

            attackRoutine = null;
            isAttacking = false;
        }

        private IEnumerator MoveToTarget(MonsterHealth target)
        {
            while (target != null)
            {
                Vector3 targetPos = target.transform.position;
                if (fairyMovement != null)
                    fairyMovement.FaceTarget(targetPos);

                Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
                transform.position = newPos;

                if (Vector3.Distance(transform.position, targetPos) <= hitDistance)
                {
                    if (fairyMovement != null)
                        fairyMovement.FaceTarget(targetPos);

                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator ReturnHome(Vector3 homePosition)
        {
            while (Vector3.Distance(transform.position, homePosition) > returnDistance)
            {
                if (fairyMovement != null)
                    fairyMovement.FaceTarget(homePosition);

                transform.position = Vector3.MoveTowards(transform.position, homePosition, returnSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = homePosition;

            if (fairyMovement != null)
                fairyMovement.SyncFollowState(homePosition);
        }

        public bool CanAttack => !isAttacking && coolTimer <= 0f;
    }
}
