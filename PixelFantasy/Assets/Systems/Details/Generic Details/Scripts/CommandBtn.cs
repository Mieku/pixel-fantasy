using System;
using HUD.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class CommandBtn : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _btnBGImage;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _activeBG;
        [SerializeField] private TooltipTrigger _tooltip;

        private bool _isActive;
        private Command _command;
        private Action<Command> _onCommandBtnPressed;

        public void Init(Command command, bool isActive, Action<Command> onCommandBtnPressed)
        {
            _command = command;
            _isActive = isActive;
            _onCommandBtnPressed = onCommandBtnPressed;

            _icon.sprite = _command.Icon;
            _tooltip.Header = _command.CommandID;
            
            Refresh();
        }

        public void OnBtnPressed()
        {
            _onCommandBtnPressed?.Invoke(_command);
        }

        private void Refresh()
        {
            if (_isActive)
            {
                _btnBGImage.sprite = _activeBG;
            }
            else
            {
                _btnBGImage.sprite = _defaultBG;
            }
        }
    }
}
