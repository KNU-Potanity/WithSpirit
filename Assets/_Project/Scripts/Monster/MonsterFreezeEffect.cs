using System.Collections;
using System.Collections.Generic;
using BasePlatformer.Monsters;
using UnityEngine;

namespace BasePlatformer.Effects
{
    /// <summary>
    /// 몬스터에게 부여되는 빙결(Freeze) 상태이상 컴포넌트.
    /// 지정된 지속시간(duration) 동안 몬스터의 이동, 공격(AI 컴포넌트들), 물리 속도, 애니메이션을 정지시키고
    /// 스프라이트를 차가운 하늘색(Ice Blue)으로 틴트합니다.
    /// </summary>
    public class MonsterFreezeEffect : MonoBehaviour
    {
        [Header("Freeze Settings")]
        [SerializeField] private float duration = 3f;
        [SerializeField] private Color freezeColor = new Color(0.4f, 0.8f, 1f, 1f); // 차가운 하늘색

        private MonsterHealth monsterHealth;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Rigidbody2D rb;
        private Color originalColor;
        private Coroutine freezeRoutine;
        private bool isEffectActive = false;

        // 빙결 동안 비활성화할 몬스터 이동/공격 스크립트 목록
        private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
        private float savedAnimatorSpeed = 1f;

        private void Awake()
        {
            monsterHealth = GetComponent<MonsterHealth>();
            if (monsterHealth == null)
                monsterHealth = GetComponentInParent<MonsterHealth>();

            rb = GetComponent<Rigidbody2D>();

            Transform visualRoot = transform.childCount > 0 ? transform.GetChild(0) : transform;
            spriteRenderer = visualRoot.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

            if (spriteRenderer != null)
            {
                // 피격(빨간색) 또는 화상/빙결 중에 컴포넌트가 붙었을 경우 빨간색이 originalColor로 오염되는 것을 방지
                originalColor = Color.white;
            }

            animator = visualRoot.GetComponentInChildren<Animator>(true);
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        public void Initialize(float freezeDuration = 3f)
        {
            duration = freezeDuration;

            if (freezeRoutine != null)
                StopCoroutine(freezeRoutine);

            freezeRoutine = StartCoroutine(FreezeCoroutine());
        }

        private void Start()
        {
            if (freezeRoutine == null)
            {
                freezeRoutine = StartCoroutine(FreezeCoroutine());
            }
        }

        private void LateUpdate()
        {
            // 빙결 중에는 피격 flash나 다른 요소에 덮어씌워지지 않게 하늘색 유지
            if (isEffectActive && spriteRenderer != null && (monsterHealth == null || !monsterHealth.IsDead))
            {
                spriteRenderer.color = freezeColor;
            }
        }

        private IEnumerator FreezeCoroutine()
        {
            isEffectActive = true;

            // 1. 이동 및 공격(IMonsterMovement 및 관련 AI) 컴포넌트 비활성화
            disabledComponents.Clear();
            var movements = GetComponents<IMonsterMovement>();
            foreach (var movement in movements)
            {
                if (movement is MonoBehaviour mb && mb.enabled)
                {
                    mb.enabled = false;
                    disabledComponents.Add(mb);
                }
            }

            // 2. 물리 속도 정지
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // 3. 애니메이션 일시 정지 (Speed = 0)
            if (animator != null)
            {
                savedAnimatorSpeed = animator.speed;
                animator.speed = 0f;
            }

            // 4. 스프라이트 하늘색 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.color = freezeColor;
            }

            // 5. 지속 시간 동안 대기
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (monsterHealth != null && monsterHealth.IsDead)
                    break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 빙결 해제 및 원복
            isEffectActive = false;
            RestoreMonster();
            Destroy(this);
        }

        private void RestoreMonster()
        {
            // 몬스터가 이미 죽었다면 원복하지 않음
            if (monsterHealth != null && monsterHealth.IsDead)
                return;

            // 1. 비활성화했던 이동/공격 스크립트 복원
            foreach (var mb in disabledComponents)
            {
                if (mb != null)
                {
                    mb.enabled = true;
                }
            }
            disabledComponents.Clear();

            // 2. 애니메이션 속도 복원
            if (animator != null)
            {
                animator.speed = savedAnimatorSpeed;
            }

            // 3. 색상 원래대로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        private void OnDestroy()
        {
            if (isEffectActive)
            {
                isEffectActive = false;
                RestoreMonster();
            }
        }
    }
}
