using System.Collections.Generic;
using UnityEngine;

namespace BasePlatformer.Monsters
{
    /// <summary>
    /// 이 오브젝트가 파괴될 때 리스트에 등록된 오브젝트들을 함께 파괴합니다.
    /// BossEnt처럼 죽으면서 관련 오브젝트(소환물, 이펙트 등)를 정리할 때 사용합니다.
    /// </summary>
    public class DestroyLinkedObjects : MonoBehaviour
    {
        [Header("이 오브젝트가 파괴될 때 함께 삭제할 오브젝트 리스트")]
        [Tooltip("인스펙터에서 드래그하여 추가하거나, 런타임에 Register()로 등록할 수 있습니다.")]
        [SerializeField] private List<GameObject> linkedObjects = new List<GameObject>();

        /// <summary>
        /// 런타임에 오브젝트를 리스트에 등록합니다.
        /// (보스가 소환한 몬스터 등을 스크립트에서 추가할 때 사용)
        /// </summary>
        public void Register(GameObject obj)
        {
            if (obj != null && !linkedObjects.Contains(obj))
            {
                linkedObjects.Add(obj);
            }
        }

        /// <summary>
        /// 런타임에 오브젝트를 리스트에서 제거합니다.
        /// (이미 별도로 파괴된 오브젝트를 정리할 때 사용)
        /// </summary>
        public void Unregister(GameObject obj)
        {
            linkedObjects.Remove(obj);
        }

        private void OnDestroy()
        {
            for (int i = linkedObjects.Count - 1; i >= 0; i--)
            {
                if (linkedObjects[i] != null)
                {
                    Destroy(linkedObjects[i]);
                }
            }

            linkedObjects.Clear();
        }
    }
}
