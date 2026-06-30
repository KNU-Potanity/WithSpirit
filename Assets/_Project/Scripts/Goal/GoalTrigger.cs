using UnityEngine;
namespace BasePlatformer.Goal
{
    /// <summary>
    /// 목표 지점(도착) 판정 — 플레이어가 닿으면 스테이지 클리어 처리.
    ///
    /// 스펙 참조:
    ///   - 플랫포머_게임_기획서_ver8.md 2장: "스테이지 끝에 도달하면 클리어 처리되는 단순한 도착 지점 오브젝트 1종"
    ///   - 일정_마일스톤 6번: "도착 지점 접촉 시 클리어 판정 (디버그 로그나 임시 텍스트로 확인)" <- 지금 이 단계
    ///   - QA_테스트_계획서 T-07: "도착 지점 접촉 → 클리어 판정 발생"
    ///
    /// MVP 클리어 처리:
    ///   콘솔에 "Clear"를 출력한다 (일정_마일스톤 6번 기준).
    ///   추후 클리어 연출(UI, 페이드 아웃, 다음 씬 전환 등) 추가 시 OnClear() 내부만 교체.
    ///
    /// GoalPoint 오브젝트에는 이미 BoxCollider2D(isTrigger=true)가 세팅되어 있고,
    /// 플레이어(PlayerStartMarker)는 Rigidbody2D를 가지고 있으므로 OnTriggerEnter2D가 정상 발생한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalTrigger : MonoBehaviour
    {
        private bool cleared = false;

        private void Awake()
        {
            // GoalPoint는 Player 레이어와 충돌해야 하므로 Hazard와 동일하게 별도 레이어 필요
            // 현재는 Default 레이어 상태 — Player 레이어(9)와 Default 레이어는 기본 충돌 허용이므로
            // 레이어 매트릭스 추가 설정 없이도 동작한다.
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (cleared) return;
            if (other.GetComponent<Player.PlayerMovement>() == null) return;

            cleared = true;
            OnClear();
        }

        private void OnClear()
        {
            // MVP: 콘솔 출력 (일정_마일스톤 6번 — "디버그 로그나 임시 텍스트로 확인")
            // 추후 클리어 UI, 페이드 아웃, 다음 씬 전환 등으로 교체 예정
            Debug.Log("Clear");
        }
    }
}
