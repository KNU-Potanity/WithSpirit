using UnityEngine;
using UnityEngine.SceneManagement;

namespace BasePlatformer.Respawn
{
    /// <summary>
    /// 리스폰 지점 관리자.
    ///
    /// 기술기획서_ver4.md 4.3장:
    ///   "RespawnManager 같은 별도 컴포넌트를 두고, 현재 리스폰 지점이 어디인지를 관리.
    ///    MVP에서는 항상 스테이지 시작 지점을 반환하지만, 추후 체크포인트가 추가되면
    ///    이 매니저 내부 로직만 바꾸면 되도록 분리."
    ///
    /// MVP 동작:
    ///   - 즉사 장애물(HazardTrigger) 접촉 또는 낭떠러지 추락(FallRespawnDetector) 시 호출된다.
    ///   - 현재 씬을 다시 로드해 게임이 새로 시작된 것처럼 처리한다.
    ///   (추후 체크포인트 기능 추가 시 이 메서드 내부 로직만 교체하면 됨)
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        public void RespawnPlayer()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
