using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

namespace Popups.Settings
{
    public class ControlsSettingsContent : SettingsContent
    {
        [SerializeField] private List<KeybindButton> _keyBindButtons;
        
        public override void OnShow()
        {
            _keyBindButtons = GetComponentsInChildren<KeybindButton>().ToList();
            
            foreach (var kbBtn in _keyBindButtons)
            {
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
    }
}
