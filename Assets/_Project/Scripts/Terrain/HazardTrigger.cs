using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 즉사 장애물 트리거 - 접촉 시 RespawnManager를 통해 플레이어를 리스폰시킨다.
    ///
    /// 기술기획서_ver4.md 4.2장:
    ///   "즉사 장애물(가시)은 Tilemap이 아닌 개별 GameObject + 트리거 콜라이더로 별도 구현"
    ///
    /// 주의: 리스폰 직후 Rigidbody2D.position 대입으로 위치를 이동해도 Physics2D는 같은 FixedUpdate
    /// 스텝 안에서 이미 Overlap 상태를 유지하고 있어, OnTriggerStay2D가 계속 발생할 수 있다.
    /// 이를 막기 위해 리스폰 후 쿨다운(isOnCooldown) 동안은 추가 트리거를 무시한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardTrigger : MonoBehaviour
    {
        [Tooltip("리스폰 직후 같은 트리거에서 중복 반응을 막는 쿨다운 시간(초)")]
        [SerializeField] private float respawnCooldown = 0.5f;

        private Respawn.RespawnManager respawnManager;
        private float cooldownTimer = 0f;

        private void Awake()
        {
            respawnManager = FindAnyObjectByType<Respawn.RespawnManager>();
            if (respawnManager == null)
                Debug.LogWarning("[HazardTrigger] RespawnManager를 씬에서 찾을 수 없습니다: " + gameObject.name);
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (cooldownTimer > 0f) return;
            if (other.GetComponent<Player.PlayerMovement>() == null) return;

            cooldownTimer = respawnCooldown;
            respawnManager?.RespawnPlayer();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Enter와 동일하게 처리: 리스폰 후 물리 overlap이 유지될 경우 Stay에서도 감지
            if (cooldownTimer > 0f) return;
            if (other.GetComponent<Player.PlayerMovement>() == null) return;

            cooldownTimer = respawnCooldown;
            respawnManager?.RespawnPlayer();
        }
    }
}
