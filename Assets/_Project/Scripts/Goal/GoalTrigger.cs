using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BasePlatformer.Goal
{
    /// <summary>
    /// 목표 지점(도착) 판정 — 플레이어가 닿으면 스테이지 클리어 처리 및 지정된 씬 로드.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoalTrigger : MonoBehaviour
    {
        [Header("클리어 후 씬 전환 설정")]
#if UNITY_EDITOR
        [Tooltip("도착 시 이동할 씬 에셋을 프로젝트 창에서 끌어다 놓으세요.")]
        [SerializeField] private SceneAsset sceneAsset;
#endif

        [HideInInspector]
        [SerializeField] private string sceneName;

        [Tooltip("씬 로드 모드 (Single: 기존 씬 닫고 전환, Additive: 현재 씬 유지한 채 추가)")]
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

        private bool cleared = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneAsset != null)
            {
                sceneName = sceneAsset.name;
            }
            else
            {
                sceneName = string.Empty;
            }
        }
#endif

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (cleared) return;
            if (other.GetComponent<Player.PlayerMovement>() == null) return;

            cleared = true;
            OnClear();
        }

        private void OnClear()
        {
            Debug.Log("Clear");

            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                SceneManager.LoadScene(sceneName, loadMode);
            }
            else
            {
                Debug.LogWarning($"[{nameof(GoalTrigger)}] 이동할 씬이 지정되지 않았습니다. 인스펙터에 Scene Asset을 등록해주세요.", this);
            }
        }
    }
}
