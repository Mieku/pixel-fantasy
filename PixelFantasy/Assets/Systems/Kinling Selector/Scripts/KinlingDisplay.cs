using System;
using Characters;
using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Kinling_Selector.Scripts
{
    public class KinlingDisplay : MonoBehaviour
    {
        [SerializeField] private Image _avatar;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private TextMeshProUGUI _actionText;
        [SerializeField] private Image _moodTint;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f; // Time in seconds

        public KinlingData KinlingData;

        public void Init(KinlingData kinlingData)
        {
            KinlingData = kinlingData;

            _avatar.sprite = KinlingData.Avatar.GetBaseAvatarSprite();
        }


        private void Update()
        {
            if (KinlingData != null)
            {
                _nicknameText.text = KinlingData.Nickname;
                _actionText.text = KinlingData.GetKinling().TaskHandler.GetCurrentTaskDisplay();
                
                _moodTint.color = Color.Lerp(_negativeColour, _positiveColour, Mathf.Clamp(KinlingData.GetKinling().MoodData.OverallMood / 100f, 0.0f, 1.0f));
            }
        }
        
        public void OnPressed()
        {
            float timeSinceLastClick = Time.time - _lastClickTime;
    
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                // Double click detected
                CameraManager.Instance.LookAtPosition(KinlingData.Position);

                // Reset the last click time to avoid unnecessary checks
                _lastClickTime = 0;
            }
            else
            {
                if (timeSinceLastClick > DOUBLE_CLICK_TIME * 2) // Adjust as needed
                {
                    // Reset the timer if the time since the last click is significantly more than the double click threshold
                    _lastClickTime = Time.time;
                }

                // Single click logic, only if not detected as double click
                PlayerInputController.Instance.SelectUnit(KinlingData.GetKinling());
            }
        }

    }
}
