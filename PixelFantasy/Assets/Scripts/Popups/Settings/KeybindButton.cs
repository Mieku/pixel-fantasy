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

        private bool _isListening;
        private Button _btn;

        private void Awake()
        {
            _btn = GetComponent<Button>();
        }

        public void OnPressed()
        {
            _isListening = true;
            _buttonText.text = "Listening...";
            _btn.interactable = false;
            _btnBG.sprite = _activeBG;
            
            PlayerSettings.KeyBindings.BeginListeningForKeyBind(_keyBindAction, _isAlternate, OnKeyBindSet);
        }

        private void OnKeyBindSet(InputActionRebindingExtensions.RebindingOperation operation)
        {
            _isListening = false;
            _btn.interactable = true;

            // Update the UI with the new keybind
            UpdateKeyBindText();

            _btnBG.sprite = _defaultBG;
            operation.Dispose(); // Clean up the rebinding operation
            PlayerSettings.SavePlayerSettings();
        }

        public void OnCancel()
        {
            _isListening = false;
            _btn.interactable = true;
            UpdateKeyBindText();
            _btnBG.sprite = _defaultBG;
        }

        public void UpdateKeyBindText()
        {
            _buttonText.text = PlayerSettings.KeyBindings.GetKeyBindText(_keyBindAction, _isAlternate);
        }
    }
}
