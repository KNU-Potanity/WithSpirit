using System.Collections;
using System.Collections.Generic;
using BasePlatformer.Monsters;
using BasePlatformer.Fairy;
using UnityEngine;

namespace BasePlatformer.Fairy
{
    /// <summary>
    /// 폭발 정령(주황색)의 공격 특화 컨트롤러.
    /// 목표 몬스터로 돌진하여 타격 시 기준 몬스터 및 주변 반경(explosionRadius) 내 모든 몬스터에게 광역 데미지를 입힙니다.
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

        [Tooltip("폭발 피해 대상 레이어 (설정되지 않으면 모든 2D 콜라이더 탐색 후 MonsterHealth 필터링)")]
        [SerializeField] private LayerMask monsterLayer = ~0;

        [Header("Animation & Visual Effects")]
        [Tooltip("폭발 시 재생할 애니메이션 클립 (선택 사항)")]
        [SerializeField] private AnimationClip explosionAnimationClip;

        [Tooltip("폭발 위치에서 사용할 별도 폭발 Animator (등록 시 트리거 또는 상태 재생)")]
        [SerializeField] private Animator explosionAnimator;

        [Tooltip("폭발 애니메이션 상태 이름 (AC_FX_Explosion의 상태 이름 'Explosion')")]
        [SerializeField] private string explodeStateName = "Explosion";

        [Tooltip("정령 본체의 Animator (돌진/공격 모션용, 없으면 자동 검색)")]
        [SerializeField] private Animator fairyAnimator;

        [Tooltip("정령 본체의 공격 시작 트리거 이름 (컨트롤러에 있을 때만 실행, 비어있으면 무시)")]
        [SerializeField] private string attackTriggerName = "";

        private int attackDamage = 1;
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

            // 정령 본체 Animator 자동 탐색
            if (fairyAnimator == null)
            {
                fairyAnimator = GetComponent<Animator>();
            }

            // 자식 오브젝트(예: Explosion 자식 오브젝트)의 Animator 자동 탐색
            if (explosionAnimator == null)
            {
                Animator[] animators = GetComponentsInChildren<Animator>(true);
                foreach (var anim in animators)
                {
                    if (anim != fairyAnimator)
                    {
                        explosionAnimator = anim;
                        break;
                    }
                }
            }

            // 폭발 오브젝트 초기 비활성화 (필요 시)
            if (explosionAnimator != null && explosionAnimator.gameObject != gameObject)
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

            // 1. 공격 시작 애니메이션 트리거
            PlayAttackAnimation();

            // 2. 목표 몬스터로 돌진
            yield return MoveToTarget(target);

            // 3. 목표 지점 도달 시 폭발 애니메이션 및 광역 데미지 적용
            Vector3 impactPoint = target != null ? target.transform.position : transform.position;
            TriggerExplosion(impactPoint, target);

            // 4. 원래 위치로 복귀
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
            // 폭발 애니메이션 재생
            PlayExplodeAnimation(centerPoint);

            // 중심 몬스터 및 주변 몬스터 탐색하여 광역 데미지 적용
            Collider2D[] hits = Physics2D.OverlapCircleAll(centerPoint, explosionRadius, monsterLayer);
            HashSet<MonsterHealth> hitMonsters = new HashSet<MonsterHealth>();

            // 직격 대상 우선 포함
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

            Debug.Log($"[FairyExplosionAttackController] 폭발 발생! 위치: {centerPoint}, 반경: {explosionRadius}, 적중 대상 수: {hitMonsters.Count}, 데미지: {attackDamage}");
        }

        #region Animation Callbacks & Helpers

        /// <summary>
        /// 공격 돌진 시작 시 정령 본체 애니메이션 발동
        /// </summary>
        protected virtual void PlayAttackAnimation()
        {
            if (fairyAnimator != null && !string.IsNullOrEmpty(attackTriggerName))
            {
                if (HasParameter(fairyAnimator, attackTriggerName))
                {
                    fairyAnimator.SetTrigger(attackTriggerName);
                }
            }
        }

        /// <summary>
        /// 목표 도달 폭발 시점 애니메이션 발동
        /// </summary>
        protected virtual void PlayExplodeAnimation(Vector3 position)
        {
            // 1. 등록된 폭발 전용 Animator가 있는 경우 재생
            if (explosionAnimator != null)
            {
                explosionAnimator.transform.position = position;
                explosionAnimator.gameObject.SetActive(true);

                string targetState = !string.IsNullOrEmpty(explodeStateName) ? explodeStateName : "Explosion";
                if (HasState(explosionAnimator, targetState))
                {
                    explosionAnimator.Play(targetState, 0, 0f);
                }
                else
                {
                    // 기본 첫 번째 상태 재생
                    explosionAnimator.Play(0, 0, 0f);
                }
            }
            // 2. 등록된 AnimationClip이 있는 경우 (동적 오브젝트를 생성하여 재생 후 자동 파기)
            else if (explosionAnimationClip != null)
            {
                StartCoroutine(PlayAnimationClipAt(explosionAnimationClip, position));
            }
            // 3. 정령 본체 Animator에 폭발 트리거가 존재하는 경우 전달
            else if (fairyAnimator != null && !string.IsNullOrEmpty(explodeStateName))
            {
                if (HasParameter(fairyAnimator, explodeStateName))
                {
                    fairyAnimator.SetTrigger(explodeStateName);
                }
            }
        }

        private bool HasParameter(Animator anim, string paramName)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return false;
            foreach (var param in anim.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }

        private bool HasState(Animator anim, string stateName)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return false;
            return anim.HasState(0, Animator.StringToHash(stateName));
        }

        private IEnumerator PlayAnimationClipAt(AnimationClip clip, Vector3 position)
        {
            GameObject tempObj = new GameObject("TempExplosionEffect");
            tempObj.transform.position = position;

            SpriteRenderer sr = tempObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            Animation anim = tempObj.AddComponent<Animation>();
            anim.AddClip(clip, clip.name);
            anim.Play(clip.name);

            yield return new WaitForSeconds(clip.length);
            Destroy(tempObj);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        public bool CanAttack => !isAttacking && coolTimer <= 0f;
    }
}
