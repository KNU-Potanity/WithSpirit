using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BasePlatformer.UI
{
    /// <summary>
    /// 버튼을 클릭하면 게임을 종료하는 스크립트입니다.
    /// 에디터 환경에서는 플레이 모드를 정지하고, 빌드된 게임 환경에서는 어플리케이션을 종료합니다.
    /// </summary>
    public class GameQuitButton : MonoBehaviour
    {
        [Header("선택 사항")]
        [Tooltip("연결할 버튼 (비워두면 자기 자신에서 자동 탐색)")]
        [SerializeField] private Button triggerButton;

        private void Awake()
        {
            if (triggerButton == null)
            {
                triggerButton = GetComponent<Button>();
            }

            if (triggerButton != null)
            {
                triggerButton.onClick.AddListener(QuitGame);
            }
        }

        private void OnDestroy()
        {
            if (triggerButton != null)
            {
                triggerButton.onClick.RemoveListener(QuitGame);
            }
        }

        /// <summary>
        /// 게임을 종료합니다. (Unity Event 등 외부에서도 호출 가능)
        /// </summary>
        public void QuitGame()
        {
            Debug.Log($"[{nameof(GameQuitButton)}] 게임 종료 요청");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
