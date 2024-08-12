using System.Collections.Generic;
using System.Linq;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Popups.Settings
{
    public class ControlsSettingsContent : SettingsContent
    {
        [SerializeField] private List<KeybindButton> _keyBindButtons;
        [SerializeField] private TextMeshProUGUI _cancelText;
        
        public override void OnShow()
        {
            UpdateCancelNote();
            
            _keyBindButtons = GetComponentsInChildren<KeybindButton>().ToList();
            
            foreach (var kbBtn in _keyBindButtons)
            {
                kbBtn.Init(this);
                kbBtn.UpdateKeyBindText();
            }
        }

        public void OnResetPressed()
        {
            PlayerSettings.KeyBindings.ResetKeyBinds();

            foreach (var kbBtn in _keyBindButtons)
            {
                kbBtn.UpdateKeyBindText();
            }
        }

        public void UpdateCancelNote()
        {
            var cancelKey = PlayerSettings.KeyBindings.GetKeyBindText(EKeyBindAction.Cancel, false);
            _cancelText.text = $"Note: Press {cancelKey} to cancel listening";
        }
    }
}
