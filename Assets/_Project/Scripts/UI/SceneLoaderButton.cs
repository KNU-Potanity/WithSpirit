using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BasePlatformer.UI
{
    /// <summary>
    /// 버튼을 클릭하면 인스펙터에 드래그 앤 드롭으로 등록한 씬 에셋을 로드하는 스크립트입니다.
    /// 버튼 컴포넌트가 있는 오브젝트에 부착 시 자동으로 OnClick 리스너가 연결됩니다.
    /// </summary>
    public class SceneLoaderButton : MonoBehaviour
    {
        [Header("씬 에셋 등록")]
#if UNITY_EDITOR
        [Tooltip("프로젝트 창에서 씬(.unity) 에셋을 이곳으로 직접 끌어다 놓으세요.")]
        [SerializeField] private SceneAsset sceneAsset;
#endif

        [HideInInspector]
        [SerializeField] private string sceneName;

        [Header("로드 모드")]
        [Tooltip("씬 로드 모드 (Single: 기존 씬 닫고 전환, Additive: 현재 씬 유지한 채 추가)")]
        [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

        [Header("선택 사항")]
        [Tooltip("연결할 버튼 (비워두면 자기 자신에서 자동 탐색)")]
        [SerializeField] private Button triggerButton;

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

        private void Awake()
        {
            if (triggerButton == null)
            {
                triggerButton = GetComponent<Button>();
            }

            if (triggerButton != null)
            {
                triggerButton.onClick.AddListener(LoadScene);
            }
        }

        private void OnDestroy()
        {
            if (triggerButton != null)
            {
                triggerButton.onClick.RemoveListener(LoadScene);
            }
        }

        /// <summary>
        /// 등록된 씬을 로드합니다.
        /// </summary>
        public void LoadScene()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning($"[{nameof(SceneLoaderButton)}] 등록된 씬이 없습니다. 인스펙터에 Scene Asset을 끌어다 놓아주세요.", this);
                return;
            }

            SceneManager.LoadScene(sceneName, loadMode);
        }

        /// <summary>
        /// 특정 씬 이름을 인자로 받아 동적으로 로드합니다.
        /// </summary>
        /// <param name="customSceneName">로드할 씬 이름</param>
        public void LoadSceneByName(string customSceneName)
        {
            if (string.IsNullOrWhiteSpace(customSceneName))
            {
                Debug.LogWarning($"[{nameof(SceneLoaderButton)}] 유효하지 않은 씬 이름입니다.", this);
                return;
            }

            SceneManager.LoadScene(customSceneName, loadMode);
        }
    }
}
