using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.CursorHandler.Scripts
{
    [RequireComponent(typeof(Button))]
    public class CommandButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _commandText;
        [SerializeField] private Image _btnBGImage;
        [SerializeField] private Color _activeTextColour;
        [SerializeField] private Color _inactiveTextColour;
        [SerializeField] private Sprite _activeBG;
        [SerializeField] private Sprite _activeBGHover;
        [SerializeField] private Sprite _inactiveBG;
        [SerializeField] private Sprite _inactiveBGHover;

        private bool _isActive;
        private bool _isInvalid;
        private bool _isHovered;
        private Command _command;
        private Action<Command> _onCommandBtnPressed;
        private Button _button;

        public void Init(Command command, bool isActive, bool isInvalid, Action<Command> onCommandBtnPressed)
        {
            _button = GetComponent<Button>();
            _command = command;
            _isActive = isActive;
            _isInvalid = isInvalid;
            _onCommandBtnPressed = onCommandBtnPressed;

            _commandText.text = command.Name;
            
            Refresh();
        }

        private void Refresh()
        {
            _button.interactable = !_isInvalid;
            if (_isInvalid)
            {
                _commandText.color = _activeTextColour;
                return;
            }
            
            if (_isActive)
            {
                _commandText.color = _activeTextColour;
                if (_isHovered)
                {
                    _btnBGImage.sprite = _activeBGHover;
                }
                else
                {
                    _btnBGImage.sprite = _activeBG;
                }
            }
            else
            {
                _commandText.color = _inactiveTextColour;
                if (_isHovered)
                {
                    _btnBGImage.sprite = _inactiveBGHover;
                }
                else
                {
                    _btnBGImage.sprite = _inactiveBG;
                }
            }
        }

        public void OnBtnPressed()
        {
            if (_isInvalid) return;
            
            _onCommandBtnPressed?.Invoke(_command);
        }

        public void OnHoverStart()
        {
            _isHovered = true;
            Refresh();
        }

        public void OnHoverEnd()
        {
            _isHovered = false;
            Refresh();
        }
    }
}
