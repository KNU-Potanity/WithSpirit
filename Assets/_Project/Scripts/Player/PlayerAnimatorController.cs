using UnityEngine;

namespace BasePlatformer.Player
{
    /// <summary>
    /// PlayerMovement / PlayerJump의 상태를 읽어서 Animator 파라미터로 넘겨주는 연결 스크립트.
    ///
    /// ToDo 항목: "캐릭터 애니메이션 클립 제작(대기/이동/점프 상승/낙하) 및 Animator Controller 연결"
    /// 기술기획서_ver4.md 4.7절 캐릭터 에셋 경로(Idle/Run/Jump/Fall)를 그대로 4개 상태에 매핑했다.
    ///
    /// 물리/입력 로직(PlayerMovement, PlayerJump, Rigidbody2D)은 모두 부모(물리 루트) 오브젝트에 있고,
    /// 이 스크립트와 Animator, SpriteRenderer는 시각 전용 자식 오브젝트("Visual")에 있다.
    /// (캐릭터 스케일/피벗 문제 수정 때 분리해둔 구조를 그대로 활용)
    ///
    /// Animator 파라미터:
    ///   - Speed (float): 좌우 이동 속도의 절댓값 -> Idle/Run 전환
    ///   - Grounded (bool): 접지 여부 -> Jump/Fall 전환의 기준
    ///   - VerticalVelocity (float): 수직 속도 -> 상승(Jump)/하강(Fall) 구분
    ///
    /// 추가로, 이동 방향에 따라 SpriteRenderer.flipX로 좌우 반전을 처리한다 (애니메이션 자체에는
    /// 없는 기능이지만, 좌우 이동 캐릭터에 자연스럽게 필요해서 같이 처리).
    /// </summary>
    public class PlayerAnimatorController : MonoBehaviour
    {
        private Animator animator;
        private SpriteRenderer spriteRenderer;

        private PlayerMovement playerMovement;
        private PlayerJump playerJump;
        private Rigidbody2D rb;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 물리/입력 로직은 부모(물리 루트)에 있다.
            playerMovement = GetComponentInParent<PlayerMovement>();
            playerJump = GetComponentInParent<PlayerJump>();
            rb = GetComponentInParent<Rigidbody2D>();
        }

        private void Update()
        {
            if (playerMovement == null || playerJump == null || rb == null || animator == null)
            {
                return;
            }

            // 경계 클램프로 실제 속도가 0이 되어도 입력이 있으면 Run 애니메이션을 유지한다.
            // CurrentSpeed(실제 이동 속도) 대신 MoveInput(입력 의도)의 절댓값을 사용.
            float speed = Mathf.Abs(playerMovement.MoveInput);
            animator.SetFloat("Speed", speed);
            animator.SetBool("Grounded", playerJump.IsGrounded);
            animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);

            // 좌우 반전: 입력이 있을 때만 방향을 갱신 (정지 시 마지막 바라보던 방향 유지)
            if (spriteRenderer != null && Mathf.Abs(playerMovement.MoveInput) > 0.01f)
            {
                spriteRenderer.flipX = playerMovement.MoveInput < 0f;
            }
        }
    }
}
