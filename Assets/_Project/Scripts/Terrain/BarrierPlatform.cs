using UnityEngine;

namespace BasePlatformer.Terrain
{
    /// <summary>
    /// BarrierPlatform 프리팹의 루트에 붙이는 스크립트.
    /// 
    /// 프리팹 구조 (권장):
    ///   BarrierPlatform          ← 이 스크립트 + Collider2D(isTrigger=true, 발판 전체 영역)
    ///   ├─ Platform              ← Tilemap 발판
    ///   └─ Barrier               ← Collider2D(isTrigger=false) + "Barrier" 레이어
    ///                               몬스터는 막고, 화살은 통과하게 레이어/태그 설정
    ///
    /// ■ 플레이어가 발판 위 트리거 영역에 진입하면 자식 Barrier 오브젝트를 활성화합니다.
    /// ■ barrierObject가 Inspector에서 지정되지 않으면 "Barrier"라는 이름의 자식을 자동 탐색합니다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BarrierPlatform : MonoBehaviour
    {
        [Header("Barrier Object (자식 장막 오브젝트)")]
        [Tooltip("활성화할 자식 Barrier 오브젝트. 비워두면 이름이 'Barrier'인 자식을 자동 탐색합니다.")]
        [SerializeField] private GameObject barrierObject;

        private bool hasActivated = false;

        private void Awake()
        {
            // 이 오브젝트의 Collider2D는 반드시 트리거여야 합니다.
            var col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning("[BarrierPlatform] Collider2D가 isTrigger=false였습니다. 자동으로 true로 설정했습니다.");
            }

            // barrierObject가 지정되지 않았으면 "Barrier" 이름의 자식을 자동 탐색
            if (barrierObject == null)
            {
                Transform found = transform.Find("Barrier");
                if (found != null)
                    barrierObject = found.gameObject;
                else
                    Debug.LogWarning("[BarrierPlatform] 자식 오브젝트 'Barrier'를 찾을 수 없습니다. Inspector에서 직접 지정하거나, 자식 이름을 'Barrier'로 설정하세요.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryActivateBarrier(other);
        }

        private void TryActivateBarrier(Collider2D other)
        {
            // 이미 한 번 활성화된 적 있으면 무시 (재소환 불가)
            if (hasActivated) return;

            // 플레이어인지 확인
            var playerHealth = other.GetComponentInParent<BasePlatformer.Player.PlayerHealth>();
            if (playerHealth == null)
                playerHealth = other.GetComponent<BasePlatformer.Player.PlayerHealth>();

            if (playerHealth == null) return;

            // 자식 Barrier 오브젝트 활성화
            if (barrierObject != null)
            {
                hasActivated = true;
                barrierObject.SetActive(true);
                Debug.Log("[BarrierPlatform] Barrier 오브젝트 활성화 (1회 한정)");
            }
        }
    }
}
