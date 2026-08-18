using UnityEngine;
using BasePlatformer.Terrain;

public class MonsterProjectile : MonoBehaviour, IBarrierConsumable
{
    [Header("Settings")]
    public float lifetime = 5f;

    private Vector2 direction;
    private float speed;
    private int damage;
    private LayerMask groundLayer;
    private bool isInitialized = false;

    public void Initialize(Vector2 dir, float moveSpeed, int attackDamage, LayerMask groundMask)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        damage = attackDamage;
        groundLayer = groundMask;
        isInitialized = true;

        // 일정 시간 후 자동으로 투사체 파괴
        Destroy(gameObject, lifetime);

        // 방향에 따라 스프라이트/오브젝트 Flip 처리
        if (direction.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (direction.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ground 레이어와 충돌 시 삭제
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // Player (또는 플레이어 관련 충돌체)와 충돌 시 데미지 처리
        if (collision.CompareTag("Player") || collision.name.Contains("Player"))
        {
            var playerHealth = collision.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = collision.GetComponent<BasePlatformer.Player.PlayerHealth>();
            }

            if (playerHealth != null)
            {
                Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, knockbackDir);
            }

            Destroy(gameObject);
        }
    }
}
