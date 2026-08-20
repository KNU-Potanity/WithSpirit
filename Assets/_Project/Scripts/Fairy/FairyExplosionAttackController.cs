using System.Collections;
using System.Collections.Generic;
using BasePlatformer.Monsters;
using BasePlatformer.Fairy;
using UnityEngine;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 폭발 정령(주황색)의 공격 특화 컨트롤러.
    /// 목표 몬스터로 돌진하여 타격 시 폭발 애니메이션 재생 및 반경(explosionRadius) 내 모든 몬스터에게 광역 데미지를 입힙니다.
    /// </summary>
    public class FairyExplosionAttackController : MonoBehaviour, IFairyAttack
    {
        [Header("Fairy Data")]
        public MainFairyData fairyData;

        [Header("Attack Motion")]
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float returnSpeed = 18f;
        [SerializeField] private float hitDistance = 0.15f;
        [SerializeField] private float returnDistance = 0.05f;

        [Header("Explosion Settings")]
        [Tooltip("폭발 광역 피해 반경")]
        [SerializeField] private float explosionRadius = 2.5f;

        [Tooltip("폭발 피해 대상 레이어")]
        [SerializeField] private LayerMask monsterLayer = ~0;

        [Header("Visual Effects")]
        [Tooltip("폭발 Animator (비워둘 경우 자식 오브젝트에서 자동 검색)")]
        [SerializeField] private Animator explosionAnimator;

        private int attackDamage = 1;
        private float coolTime = 0.5f;
        private float coolTimer;

        private FairyMovement fairyMovement;
        private FairyPlatformController fairyPlatform;
        private Coroutine attackRoutine;
        private Coroutine explosionRoutine;
        private bool isAttacking;

        private void Awake()
        {
            fairyMovement = GetComponent<FairyMovement>();
            fairyPlatform = GetComponent<FairyPlatformController>();

            // 자식 오브젝트의 폭발 Animator 자동 탐색
            if (explosionAnimator == null)
            {
                Animator[] animators = GetComponentsInChildren<Animator>(true);
                foreach (var anim in animators)
                {
                    if (anim.gameObject != gameObject)
                    {
                        explosionAnimator = anim;
                        break;
                    }
                }
            }

            // 시작 시 폭발 오브젝트 무조건 비활성화 (평상시 보이지 않도록)
            if (explosionAnimator != null)
            {
                explosionAnimator.gameObject.SetActive(false);
            }

            if (fairyData != null)
            {
                attackDamage = fairyData.Damage;
                coolTime = fairyData.CoolTime;
            }
            else
            {
                Debug.LogWarning("[FairyExplosionAttackController] fairyData가 할당되지 않았습니다! 기본값을 사용합니다.");
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
                Debug.Log($"[FairyExplosionAttackController] 쿨타임 중 ({coolTimer:F1}초 남음)");
                return;
            }

            if (fairyPlatform != null && fairyPlatform.HasActivePlatform)
            {
                Debug.Log("[FairyExplosionAttackController] 정령이 플랫폼 변신 중이라 공격을 시작할 수 없습니다.");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("[FairyExplosionAttackController] 공격 대상이 없습니다.");
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

            // 2. 목표 지점 도달 시 폭발 애니메이션 및 광역 데미지 적용
            Vector3 impactPoint = target != null ? target.transform.position : transform.position;
            TriggerExplosion(impactPoint, target);

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

        /// <summary>
        /// 폭발 처리: 애니메이션 재생 및 광역 데미지 부여
        /// </summary>
        private void TriggerExplosion(Vector3 centerPoint, MonsterHealth directTarget)
        {
            // 1. 공격 포인트 위치에 독립적인 폭발 이펙트 생성 및 재생 후 자동 파기
            SpawnExplosionEffect(centerPoint);

            // 2. 중심 몬스터 및 주변 몬스터 탐색하여 광역 데미지 적용
            Collider2D[] hits = Physics2D.OverlapCircleAll(centerPoint, explosionRadius, monsterLayer);
            HashSet<MonsterHealth> hitMonsters = new HashSet<MonsterHealth>();

            if (directTarget != null)
            {
                hitMonsters.Add(directTarget);
            }

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                MonsterHealth mh = hit.GetComponent<MonsterHealth>();
                if (mh == null) mh = hit.GetComponentInParent<MonsterHealth>();

                if (mh != null)
                {
                    hitMonsters.Add(mh);
                }
            }

            foreach (var monster in hitMonsters)
            {
                monster.TakeDamage(attackDamage);
            }

            Debug.Log($"[FairyExplosionAttackController] 폭발 발생! 위치: {centerPoint}, 적중 대상 수: {hitMonsters.Count}, 데미지: {attackDamage}");
        }

        private void SpawnExplosionEffect(Vector3 position)
        {
            if (explosionAnimator == null) return;

            // 자식 폭발 오브젝트를 템플릿으로 사용하여 해당 위치에 월드 오브젝트로 인스턴스화
            GameObject explosionInstance = Instantiate(explosionAnimator.gameObject, position, Quaternion.identity);
            explosionInstance.SetActive(true);

            StartCoroutine(DestroyAfterAnimation(explosionInstance));
        }

        private IEnumerator DestroyAfterAnimation(GameObject effectObj)
        {
            if (effectObj == null) yield break;

            Animator anim = effectObj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(0, 0, 0f);
                yield return null;

                float animDuration = 1.0f;
                if (anim != null)
                {
                    var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                    if (stateInfo.length > 0f)
                    {
                        animDuration = stateInfo.length;
                    }
                }

                yield return new WaitForSeconds(animDuration);
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }

            if (effectObj != null)
            {
                Destroy(effectObj);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        public bool CanAttack => !isAttacking && coolTimer <= 0f;
    }
}
