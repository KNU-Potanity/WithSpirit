using UnityEngine;

namespace BasePlatformer.Monsters
{
    /// <summary>
    /// 몬스터 스프라이트 좌우 반전 처리.
    /// 모든 몬스터 애니메이션 클립은 오른쪽을 바라보는 것만 사용하므로,
    /// 왼쪽으로 이동할 때는 별도 클립 없이 SpriteRenderer.flipX로 좌우 반전합니다.
    /// (PlayerAnimatorController.cs와 동일한 방식 — 스크립트_설명서.md 참고)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MonsterFacing : MonoBehaviour
    {
        [Tooltip("Rigidbody2D의 x축 속도를 자동으로 읽어 반전할지 여부. " +
                 "false면 SetFacingFromDirection()을 AI 스크립트에서 직접 호출해야 합니다.")]
        [SerializeField] private bool autoDetectFromRigidbody = true;

        [Tooltip("속도의 절댓값이 이 값보다 작으면 방향을 바꾸지 않고 마지막 방향을 유지합니다.")]
        [SerializeField] private float velocityThreshold = 0.01f;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (!autoDetectFromRigidbody || rb == null) return;
            SetFacingFromDirection(rb.linearVelocity.x);
        }

        /// <summary>
        /// AI 스크립트에서 직접 방향을 넘겨줄 때 사용합니다 (예: Rigidbody2D가 없는 경우).
        /// x가 양수면 오른쪽, 음수면 왼쪽. 절댓값이 threshold보다 작으면 방향을 바꾸지 않습니다.
        /// </summary>
        public void SetFacingFromDirection(float x)
        {
            if (Mathf.Abs(x) < velocityThreshold) return; // 정지 시 마지막 방향 유지

            // 기본 클립이 오른쪽을 보고 있으므로, 왼쪽으로 갈 때만 반전(flipX = true)
            spriteRenderer.flipX = x < 0f;
        }
    }
}
