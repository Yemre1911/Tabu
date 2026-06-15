using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TabuOyunu.UI.MainMenu
{
    public class SettingsUIController : MonoBehaviour
    {
        [Header("Sliders")]
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Slider tabooPenaltySlider;
        [SerializeField] private Slider passSlider;
        [SerializeField] private Slider targetScoreSlider;

        [Header("Value Texts")]
        [SerializeField] private TMP_Text timeValueText;
        [SerializeField] private TMP_Text tabooPenaltyValueText;
        [SerializeField] private TMP_Text passValueText;
        [SerializeField] private TMP_Text targetScoreValueText;

        public int RoundTimeSeconds => Mathf.RoundToInt(timeSlider.value);
        public int TabooPenalty => Mathf.RoundToInt(tabooPenaltySlider.value);
        public int PassCount => Mathf.RoundToInt(passSlider.value);
        public int TargetScore => Mathf.RoundToInt(targetScoreSlider.value);

        private void Awake()
        {
            SetupSlider(timeSlider, 30, 240, 90);
            SetupSlider(tabooPenaltySlider, 1, 3, 2);
            SetupSlider(passSlider, 2, 6, 3);
            SetupSlider(targetScoreSlider, 25, 100, 30);

            timeSlider.onValueChanged.AddListener(_ => UpdateTexts());
            tabooPenaltySlider.onValueChanged.AddListener(_ => UpdateTexts());
            passSlider.onValueChanged.AddListener(_ => UpdateTexts());
            targetScoreSlider.onValueChanged.AddListener(_ => UpdateTexts());

            UpdateTexts();
        }

        private void SetupSlider(Slider slider, int min, int max, int defaultValue)
        {
            if (slider == null)
            {
                Debug.LogError($"{nameof(SettingsUIController)}: Slider reference is missing.");
                return;
            }

            slider.wholeNumbers = true;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = defaultValue;
        }

        private void UpdateTexts()
        {
            if (timeValueText != null)
                timeValueText.text = $"{RoundTimeSeconds} sn";

            if (tabooPenaltyValueText != null)
                tabooPenaltyValueText.text = $"{TabooPenalty}";

            if (passValueText != null)
                passValueText.text = $"{PassCount}";

            if (targetScoreValueText != null)
                targetScoreValueText.text = $"{TargetScore}";
        }
    }
}