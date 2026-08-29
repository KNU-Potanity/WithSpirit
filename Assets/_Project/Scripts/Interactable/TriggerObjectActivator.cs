using System.Collections.Generic;
using UnityEngine;

namespace BasePlatformer.Interactables
{
    public class TriggerObjectActivator : MonoBehaviour
    {
        [Header("활성화할 오브젝트 리스트")]
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();

        [Header("감지할 레이어")]
        [SerializeField] private LayerMask targetLayer;

        [Header("1회만 발동")]
        [SerializeField] private bool triggerOnlyOnce = true;

        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            CheckAndActivate(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CheckAndActivate(collision.gameObject);
        }

        private void CheckAndActivate(GameObject obj)
        {
            if (hasTriggered && triggerOnlyOnce) return;

            // 닿은 오브젝트의 레이어가 targetLayer에 포함되어 있는지 확인
            if ((targetLayer.value & (1 << obj.layer)) != 0)
            {
                if (triggerOnlyOnce)
                {
                    hasTriggered = true;
                }

                foreach (var target in targetObjects)
                {
                    if (target != null)
                    {
                        target.SetActive(true);
                    }
                }
            }
        }
    }
}
