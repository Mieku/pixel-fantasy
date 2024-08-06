using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Settings
{
    public class SettingsMenuOption : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _bg;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _activeBG;
        [SerializeField] private Color _defaultColour;
        [SerializeField] private Color _activeColour;
        [SerializeField] private Color _hoverColour;
        [SerializeField] private SettingsPopup _settingsPopup;

        public SettingsPopup.EContentState ContentState;

        private bool _isActive;

        public void SetActive(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _bg.sprite = _activeBG;
                _header.color = _activeColour;
                _icon.color = _activeColour;
            }
            else
            {
                _bg.sprite = _defaultBG;
                _header.color = _defaultColour;
                _icon.color = _defaultColour;
            }
        }
        
        public void OnPressed()
        {
            _settingsPopup.SetContent(ContentState);
        }

        public void OnHoverStart()
        {
            _header.color = _hoverColour;
            _icon.color = _hoverColour;
        }

        public void OnHoverEnd()
        {
            if (_isActive)
            {
                _header.color = _activeColour;
                _icon.color = _activeColour;
            }
            else
            {
                _header.color = _defaultColour;
                _icon.color = _defaultColour;
            }
        }
    }
}
