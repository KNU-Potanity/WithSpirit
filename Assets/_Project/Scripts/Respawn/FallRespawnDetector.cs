using UnityEngine;

namespace BasePlatformer.Respawn
{
    /// <summary>
    /// 낭떠러지 추락 판정 — 화면 하단 경계 이탈 시 즉시 리스폰.
    ///
    /// 스펙 참조: 캐릭터컨트롤_스펙_ver1.md
    ///   "낭떠러지(추락) 판정 | 화면 하단 경계 이탈 시 즉시 리스폰 처리"
    ///
    /// 동작 방식:
    ///   - 매 Update마다 플레이어의 월드 Y 좌표를 감시한다.
    ///   - 플레이어 위치가 killPlaneY 이하로 내려가면 RespawnManager.RespawnPlayer()를 호출한다.
    ///   - killPlaneY는 카메라 최저 경계(CameraBounds 하단)보다 살짝 아래로 설정하면
    ///     "화면 밖으로 사라진 직후" 리스폰되는 자연스러운 타이밍이 된다.
    ///   - HazardTrigger와 동일하게 쿨다운을 두어 중복 호출을 방지한다.
    /// </summary>
    public class FallRespawnDetector : MonoBehaviour
    {
        [Tooltip("이 Y 좌표 이하로 내려가면 즉시 리스폰 (CameraBounds 하단보다 살짝 아래 권장)")]
        [SerializeField] private float killPlaneY = -4.0f;

        [Tooltip("리스폰 후 중복 호출 방지 쿨다운(초)")]
        [SerializeField] private float respawnCooldown = 0.5f;

        private RespawnManager respawnManager;
        private float cooldownTimer = 0f;

        private void Awake()
        {
            respawnManager = FindAnyObjectByType<RespawnManager>();
            if (respawnManager == null)
                Debug.LogWarning("[FallRespawnDetector] RespawnManager를 씬에서 찾을 수 없습니다.");
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            if (transform.position.y < killPlaneY)
            {
                cooldownTimer = respawnCooldown;
                respawnManager?.RespawnPlayer();
            }
        }
    }
}
