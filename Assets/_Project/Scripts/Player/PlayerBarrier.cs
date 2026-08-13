using UnityEngine;
using System.Collections;

namespace BasePlatformer.Player
{
    /// <summary>
    /// 플레이어에게 장착되는 장막 컴포넌트.
    /// 데미지를 1회 무효화하고, 소모되면 자신을 제거합니다.
    /// </summary>
    public class PlayerBarrier : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("장막 이펙트로 사용할 자식 오브젝트 (SpriteRenderer 또는 ParticleSystem 등)")]
        [SerializeField] private GameObject barrierVisual;

        [Tooltip("장막 소모 시 재생할 파티클 프리팹 (선택)")]
        [SerializeField] private GameObject consumeEffectPrefab;

        private bool isConsumed = false;

        private void Awake()
        {
            // barrierVisual이 Inspector에서 지정되지 않은 경우 자동으로 탐색
            if (barrierVisual == null && transform.childCount > 0)
            {
                barrierVisual = transform.GetChild(0).gameObject;
            }
        }

        /// <summary>
        /// BarrierPlatform이 런타임에 장막을 부여할 때 비주얼 오브젝트를 주입합니다.
        /// </summary>
        public void SetVisualAtRuntime(GameObject visual, GameObject consumeEffect)
        {
            barrierVisual = visual;
            consumeEffectPrefab = consumeEffect;

            if (barrierVisual != null)
                barrierVisual.SetActive(true);
        }

        private void OnEnable()
        {
            isConsumed = false;
            if (barrierVisual != null)
                barrierVisual.SetActive(true);
        }

        /// <summary>
        /// PlayerHealth.TakeDamage()에서 호출.
        /// true를 반환하면 데미지를 막았다는 의미 → PlayerHealth 측에서 데미지를 무시합니다.
        /// </summary>
        public bool TryAbsorbDamage()
        {
            if (isConsumed) return false;

            isConsumed = true;
            StartCoroutine(ConsumeRoutine());
            return true;
        }

        private IEnumerator ConsumeRoutine()
        {
            // 소모 이펙트 생성 (선택)
            if (consumeEffectPrefab != null)
            {
                Instantiate(consumeEffectPrefab, transform.position, Quaternion.identity);
            }

            // 비주얼 비활성화
            if (barrierVisual != null)
                barrierVisual.SetActive(false);

            // 한 프레임 뒤에 컴포넌트 제거 (ConsumeRoutine이 정상 종료되도록)
            yield return null;

            Destroy(this);
        }

        /// <summary>
        /// 장막이 아직 활성 상태인지 외부에서 확인할 때 사용.
        /// </summary>
        public bool IsActive => !isConsumed;
    }
}
