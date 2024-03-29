using Characters;
using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Kinling_Selector.Scripts
{
    public class KinlingDisplay : MonoBehaviour
    {
        [SerializeField] private Image _statusIcon;
        [SerializeField] private GameObject _statusHandle;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f; // Time in seconds

        public KinlingData KinlingData;

        public void Init(KinlingData kinlingData)
        {
            KinlingData = kinlingData;
            _nicknameText.text = KinlingData.GetNickname();
            CheckStatus(KinlingData);
        }

        private void CheckStatus(KinlingData kinlingData)
        {
            if(kinlingData != KinlingData) return;
            
            // TODO: Set up a game event when a kinling's status changes and subscribe this.
            // For now just deactivated
            _statusHandle.SetActive(false);
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
                PlayerInputController.Instance.SelectUnit(KinlingData.Kinling);
            }
        }

    }
}
