using System;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Controls_Details.Scripts
{
    public class ControlsBtn : MonoBehaviour
    {
        public Action<ControlsBtn> OnPressed;
        
        [SerializeField] private Sprite _defaultBG, _activeBG;
        [SerializeField] private Image _btnBG;
        
        private bool _isActive;

        public void OnBtnPressed()
        {
            OnPressed?.Invoke(this);
        }

        public void SetActive(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _btnBG.sprite = _activeBG;
            }
            else
            {
                _btnBG.sprite = _defaultBG;
            }
        }
    }
}
