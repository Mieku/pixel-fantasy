using Systems.Mood.Scripts;
using TMPro;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class EmotionDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeLeftText;
        [SerializeField] private TextMeshProUGUI _emotionNameText;
        [SerializeField] private TextMeshProUGUI _moodModifierText;

        private EmotionState _emotionState;
        public EmotionState EmotionState => _emotionState;

        public void Init(EmotionState emotionState)
        {
            _emotionState = emotionState;
            _emotionNameText.text = _emotionState.LinkedEmotionSettings.DisplayName;
            
            Refresh();
        }
        
        public void Refresh()
        {
            _moodModifierText.text = _emotionState.MoodModifierDisplay;
            _timeLeftText.text = _emotionState.TimeLeftHoursDisplay;
        }
    }
}
