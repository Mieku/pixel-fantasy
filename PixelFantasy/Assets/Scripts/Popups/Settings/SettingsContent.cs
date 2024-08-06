using UnityEngine;

namespace Popups.Settings
{
    public abstract class SettingsContent : MonoBehaviour
    {
        public SettingsPopup.EContentState ContentState;

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void OnShow();
    }
}
