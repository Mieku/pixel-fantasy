using HUD.Tooltip;
using Systems.Mood.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class EmotionDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _emotionName;
        [SerializeField] private TextMeshProUGUI _value;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        [SerializeField] private GameObject _bg;
        [SerializeField] private TooltipTrigger _tooltip;

        private EmotionState _emotionState;

        public void Init(EmotionState emotionState, int index)
        {
            _emotionState = emotionState;
            _emotionName.text = emotionState.LinkedEmotionSettings.DisplayName;
            _value.text = emotionState.MoodModifierDisplay;

            if (emotionState.MoodModifier >= 0)
            {
                _value.color = _positiveColour;
            }
            else
            {
                _value.color = _negativeColour;
            }

            int modulus = index % 2;
            _bg.SetActive(modulus == 1);

            _tooltip.Header = emotionState.LinkedEmotionSettings.DisplayName;
            _tooltip.Content = emotionState.LinkedEmotionSettings.Description;
        }
    }
}
