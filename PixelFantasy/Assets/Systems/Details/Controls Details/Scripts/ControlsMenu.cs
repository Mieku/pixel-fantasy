using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.Details.Controls_Details.Scripts
{
    public abstract class ControlsMenu : MonoBehaviour
    {
        protected ControlsBtn _activeBtn;

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        
        protected void SetBtnActive(ControlsBtn btn)
        {
            if (_activeBtn != null)
            {
                _activeBtn.SetActive(false);
            }

            _activeBtn = btn;
            
            if (_activeBtn != null)
            {
                _activeBtn.SetActive(true);
            }
        }
    }
}
