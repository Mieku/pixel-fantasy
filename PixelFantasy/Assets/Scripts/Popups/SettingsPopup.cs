using System.Collections.Generic;
using Player;
using Popups.Settings;
using UnityEngine;

namespace Popups
{
    public class SettingsPopup : Popup<SettingsPopup>
    {
        [SerializeField] private List<SettingsMenuOption> _menuOptions;
        [SerializeField] private List<SettingsContent> _contents;
        
        private EContentState _contentState;
        
        public enum EContentState
        {
            General,
            Audio,
            Gameplay,
            Graphics,
            Controls,
        }
        
        public static void Show()
        {
            Open(() => Instance.Refresh(), true);
        }
        
        public override void OnBackPressed()
        {
            PlayerSettings.KeyBindings.CancelKeyBindListening();
            
            Hide();
        }

        private void Refresh()
        {
            SetContent(EContentState.General);
        }

        public void SetContent(EContentState contentState)
        {
            PlayerSettings.KeyBindings.CancelKeyBindListening();
            
            _contentState = contentState;
            RefreshMenu();

            foreach (var content in _contents)
            {
                content.SetActive(content.ContentState == _contentState);
            }
        }

        private void RefreshMenu()
        {
            foreach (var option in _menuOptions)
            {
                option.SetActive(option.ContentState == _contentState);
            }
        }

        public void OnOKPressed()
        {
            PlayerSettings.KeyBindings.CancelKeyBindListening();
            
            Hide();
        }
    }
}
