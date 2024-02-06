using System;
using System.Collections.Generic;
using Interfaces;
using TMPro;
using UnityEngine;

namespace Systems.CursorHandler.Scripts
{
    public class CommandControls : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private CommandButton _commandBtnPrefab;
        [SerializeField] private Transform _commandBtnParent;
        
        private bool _isActive;
        private Transform _targetTransform;
        private Vector2 _targetPosition;

        private List<CommandButton> _displayedCmdBtns = new List<CommandButton>();

        private void Start()
        {
            _commandBtnPrefab.gameObject.SetActive(false);
            HideControls();
        }

        private void ClearDisplayedBtns()
        {
            foreach (var cmdBtn in _displayedCmdBtns)
            {
                Destroy(cmdBtn.gameObject);
            }
            _displayedCmdBtns.Clear();
        }
        
        public void ShowControls(Transform followTransform, string header, List<Command> commands, Command inProgressCmd, List<Command> invalidCommands, Action<Command> onCommandPressed)
        {
            _canvasGroup.alpha = 1;
            _targetTransform = followTransform;
            _isActive = true;
            _headerText.text = header;
            DisplayCommands(commands, inProgressCmd, invalidCommands, onCommandPressed);
        }

        public void ShowControls(Vector2 worldPosition, string header, List<Command> commands, Command inProgressCmd, List<Command> invalidCommands, Action<Command> onCommandPressed)
        {
            _canvasGroup.alpha = 1;
            _targetPosition = worldPosition;
            _targetTransform = null;
            _isActive = true;
            _headerText.text = header;
            DisplayCommands(commands, inProgressCmd, invalidCommands, onCommandPressed);
        }

        private void DisplayCommands(List<Command> commands, Command inProgressCmd, List<Command> invalidCommands, Action<Command> onCommandPressed)
        {
            ClearDisplayedBtns();

            foreach (var command in commands)
            {
                var commandBtn = Instantiate(_commandBtnPrefab, _commandBtnParent);
                commandBtn.Init(command, command == inProgressCmd, invalidCommands.Contains(command), onCommandPressed);
                commandBtn.gameObject.SetActive(true);
                _displayedCmdBtns.Add(commandBtn);
            }
        }

        public void HideControls()
        {
            _canvasGroup.alpha = 0;
            _isActive = false;
            ClearDisplayedBtns();
        }

        private void Update()
        {
            if (_isActive)
            {
                if (_targetTransform != null)
                {
                    transform.position = _targetTransform.position;
                }
                else
                {
                    transform.position = _targetPosition;
                }
            }
        }
    }
}
