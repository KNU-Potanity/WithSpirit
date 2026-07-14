using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// 즉사 장애물(가시) 트리거. 
    /// 플레이어의 자체 무적 시간(invincibilityDuration)에 맞춰 데미지가 들어갑니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            ApplyDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            ApplyDamage(other);
        }

        private void ApplyDamage(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerMovement>() == null) return;

            var playerHealth = other.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                // 플레이어가 가시의 어느 쪽에 있는지에 따라 넉백 방향 결정
                float dirX = (other.transform.position.x > transform.position.x) ? 1f : -1f;
                Vector2 knockbackDir = new Vector2(dirX, 1f).normalized;
                
                playerHealth.TakeDamage(1, knockbackDir);
            }
        }
    }
}
