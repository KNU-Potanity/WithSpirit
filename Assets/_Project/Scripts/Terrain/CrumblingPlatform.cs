using System.Collections;
using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 무너지는 발판 컴포넌트.
    ///
    /// PlacedPlatform 프리팹(루트 오브젝트)에 부착합니다.
    /// 구조: PlacedPlatform (Grid, 이 컴포넌트) → 자식 Platform (Tilemap, TilemapCollider2D, CompositeCollider2D, Rigidbody2D)
    ///
    /// 동작:
    ///   1. 플레이어가 발판 위에 올라서면 대기 중 흔들림 효과 재생.
    ///   2. fallDelay 초 뒤에 낙하 시작 (대기 시간이 지날수록 진폭 증가).
    ///   3. 낙하 시작 시 자식 Platform의 Rigidbody2D를 Kinematic → Dynamic으로 전환해 중력 적용.
    ///   4. 낙하 중 다른 콜라이더와 충돌하거나 Y좌표가 destroyBelowY 미만이면 오브젝트 삭제.
    /// </summary>
    public class CrumblingPlatform : MonoBehaviour
    {
        [Header("Crumbling Settings")]
        [Tooltip("플레이어가 밟은 뒤 낙하까지의 대기 시간(초)")]
        [SerializeField] private float fallDelay = 1f;

        [Tooltip("이 Y좌표 미만으로 내려가면 오브젝트를 삭제합니다")]
        [SerializeField] private float destroyBelowY = -20f;

        [Tooltip("낙하 시 적용할 중력 스케일")]
        [SerializeField] private float fallGravityScale = 2f;

        [Header("Shake Settings")]
        [Tooltip("흔들림 최대 진폭 (X축 픽셀 단위)")]
        [SerializeField] private float shakeMaxAmplitude = 0.12f;

        [Tooltip("흔들림 주파수 (초당 왕복 횟수)")]
        [SerializeField] private float shakeFrequency = 18f;

        // ─── Internal state ───────────────────────────────────────────────
        private Rigidbody2D platformRb;
        private Transform platformTransform;   // 자식 Platform Transform (흔들림 대상)
        private Vector3 platformLocalOrigin;   // 자식 Platform 원래 로컬 위치
        private bool isFalling = false;
        private bool triggerStarted = false;

        // ─── Unity lifecycle ──────────────────────────────────────────────

        private void Awake()
        {
            // 자식 오브젝트(Platform)의 Rigidbody2D를 탐색합니다.
            platformRb = GetComponentInChildren<Rigidbody2D>();

            if (platformRb == null)
            {
                Debug.LogWarning("[CrumblingPlatform] 자식 오브젝트에서 Rigidbody2D를 찾을 수 없습니다!", this);
            }
            else
            {
                platformTransform = platformRb.transform;
                platformLocalOrigin = platformTransform.localPosition;
            }
        }

        private void Start()
        {
            // 자식 Platform 오브젝트에 헬퍼 컴포넌트가 없으면 자동으로 추가합니다.
            // 자식의 충돌/스테이 이벤트를 루트 컴포넌트로 전달하기 위한 브릿지입니다.
            if (platformRb != null)
            {
                var trigger = platformRb.GetComponent<CrumblingPlatformTrigger>();
                if (trigger == null)
                    trigger = platformRb.gameObject.AddComponent<CrumblingPlatformTrigger>();

                trigger.Initialize(this);
            }
        }

        private void Update()
        {
            if (!isFalling) return;

            // Y좌표 기준 삭제 체크
            if (transform.position.y < destroyBelowY)
                DestroySelf();
        }

        // ─── Public API (called by CrumblingPlatformTrigger) ─────────────

        /// <summary>
        /// 플레이어가 발판에 올라섰을 때 헬퍼 컴포넌트가 호출합니다.
        /// </summary>
        public void OnPlayerLanded()
        {
            if (triggerStarted || isFalling) return;
            triggerStarted = true;
            StartCoroutine(FallAfterDelay());
        }

        /// <summary>
        /// 낙하 중 다른 콜라이더와 충돌했을 때 헬퍼 컴포넌트가 호출합니다.
        /// </summary>
        public void OnHitWhileFalling()
        {
            if (isFalling)
                DestroySelf();
        }

        // ─── Internal logic ───────────────────────────────────────────────

        private IEnumerator FallAfterDelay()
        {
            // 흔들림과 낙하 대기를 병렬로 실행합니다.
            StartCoroutine(ShakeRoutine(fallDelay));
            yield return new WaitForSeconds(fallDelay);
            StartFall();
        }

        /// <summary>
        /// duration 동안 자식 Platform을 X축으로 진동시킵니다.
        /// 시간이 지날수록 진폭이 선형으로 증가하여 붕괴 직전의 긴장감을 표현합니다.
        /// </summary>
        private IEnumerator ShakeRoutine(float duration)
        {
            if (platformTransform == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;  // 0 → 1 (진행률)

                // 진폭: 초반엔 작게, 낙하 직전엔 최대로
                float amplitude = Mathf.Lerp(0f, shakeMaxAmplitude, t);

                // 사인파로 좌우 진동
                float offsetX = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f) * amplitude;
                platformTransform.localPosition = platformLocalOrigin + new Vector3(offsetX, 0f, 0f);

                yield return null;
            }

            // 낙하 전 로컬 위치 복구
            platformTransform.localPosition = platformLocalOrigin;
        }

        private void StartFall()
        {
            if (isFalling || platformRb == null) return;
            isFalling = true;

            // Kinematic → Dynamic으로 전환하여 중력 적용
            platformRb.bodyType = RigidbodyType2D.Dynamic;
            platformRb.gravityScale = fallGravityScale;
            platformRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
