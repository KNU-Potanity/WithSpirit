using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BasePlatformer.UI
{
    /// <summary>
    /// Slider의 현재 값을 0% ~ 100% 형태(또는 사용자 지정 포맷)의 텍스트로 실시간 표시해 주는 컴포넌트입니다.
    /// Slider 오브젝트나 텍스트 오브젝트에 바로 붙여서 사용할 수 있습니다.
    /// </summary>
    public class SliderPercentText : MonoBehaviour
    {
        [Header("UI 참조 (비워둘 시 자동 탐색)")]
        [Tooltip("값을 감지할 슬라이더 (비워두면 자기 자신 또는 부모/자식에서 자동 탐색)")]
        [SerializeField] private Slider targetSlider;

        [Tooltip("퍼센트를 표시할 TMP 텍스트 (비워두면 자기 자신 또는 자식에서 자동 탐색)")]
        [SerializeField] private TextMeshProUGUI valueText;

        [Header("표시 형식 설정")]
        [Tooltip("슬라이더의 Min/Max 범위를 기준으로 0% ~ 100%로 환산할지 여부")]
        [SerializeField] private bool useNormalizedPercentage = true;

        [Tooltip("정수로 반올림하여 표시할지 여부 (예: 50%)")]
        [SerializeField] private bool roundToInt = true;

        [Tooltip("텍스트 포맷 (예: {0}% -> 50%, {0} -> 50)")]
        [SerializeField] private string format = "{0}%";

        private void Awake()
        {
            InitializeReferences();
        }

        private void OnEnable()
        {
            InitializeReferences();

            if (targetSlider != null)
            {
                targetSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
                targetSlider.onValueChanged.AddListener(OnSliderValueChanged);
                UpdateValueDisplay(targetSlider.value);
            }
        }

        private void OnDisable()
        {
            if (targetSlider != null)
            {
                targetSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        private void InitializeReferences()
        {
            // Slider 자동 탐색
            if (targetSlider == null)
            {
                targetSlider = GetComponent<Slider>();
                if (targetSlider == null)
                    targetSlider = GetComponentInParent<Slider>();
                if (targetSlider == null)
                    targetSlider = GetComponentInChildren<Slider>();
            }

            // TextMeshProUGUI 자동 탐색
            if (valueText == null)
            {
                valueText = GetComponent<TextMeshProUGUI>();
                if (valueText == null)
                    valueText = GetComponentInChildren<TextMeshProUGUI>();
                if (valueText == null && targetSlider != null)
                    valueText = targetSlider.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void OnSliderValueChanged(float value)
        {
            UpdateValueDisplay(value);
        }

        /// <summary>
        /// 슬라이더 값에 따라 텍스트를 갱신합니다.
        /// </summary>
        public void UpdateValueDisplay(float value)
        {
            if (valueText == null || targetSlider == null) return;

            float displayValue;

            if (useNormalizedPercentage)
            {
                float min = targetSlider.minValue;
                float max = targetSlider.maxValue;
                float range = max - min;

                // 0으로 나누기 방지
                float normalized = Mathf.Approximately(range, 0f) ? 0f : (value - min) / range;
                displayValue = normalized * 100f;
            }
            else
            {
                displayValue = value;
            }

            if (roundToInt)
            {
                int roundedValue = Mathf.RoundToInt(displayValue);
                valueText.text = string.Format(format, roundedValue);
            }
            else
            {
                valueText.text = string.Format(format, displayValue.ToString("F1"));
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (targetSlider != null && valueText != null)
            {
                UpdateValueDisplay(targetSlider.value);
            }
        }
#endif
    }
}
