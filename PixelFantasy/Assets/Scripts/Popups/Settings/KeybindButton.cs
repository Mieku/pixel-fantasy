using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Popups.Settings
{
    [RequireComponent(typeof(Button))]
    public class KeybindButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _activeBG;

        [SerializeField] private EKeyBindAction _keyBindAction;
        [SerializeField] private bool _isAlternate;
        
        private Button _btn;
        private ControlsSettingsContent _content;

        private void Awake()
        {
            _btn = GetComponent<Button>();
        }

        public void Init(ControlsSettingsContent content)
        {
            _content = content;
        }

        public void OnPressed()
        {
            _buttonText.text = "Listening...";
            _btn.interactable = false;
            _btnBG.sprite = _activeBG;
            
            PlayerSettings.KeyBindings.BeginListeningForKeyBind(_keyBindAction, _isAlternate, OnKeyBindSet, OnKeyBindCancel, OnKeyBindExists);
        }

        private void OnKeyBindExists(InputAction conflictingAction)
        {
            _buttonText.text = "Exists, Try Again...";
        }

        private void OnKeyBindSet(InputActionRebindingExtensions.RebindingOperation operation)
        {
            _btn.interactable = true;

            // Update the UI with the new keybind
            UpdateKeyBindText();

            _btnBG.sprite = _defaultBG;
            operation.Dispose(); // Clean up the rebinding operation
            PlayerSettings.SavePlayerSettings();
            
            operation.action.Disable();
            operation.action.Enable();
        }

        private void OnKeyBindCancel(InputActionRebindingExtensions.RebindingOperation operation)
        {
            _btn.interactable = true;
            UpdateKeyBindText();
            _btnBG.sprite = _defaultBG;
        }

        public void UpdateKeyBindText()
        {
            _buttonText.text = PlayerSettings.KeyBindings.GetKeyBindText(_keyBindAction, _isAlternate);
            
            _content.UpdateCancelNote();
        }
    }
}
