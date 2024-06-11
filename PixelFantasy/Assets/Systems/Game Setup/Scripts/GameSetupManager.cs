using System;
using Managers;
using TMPro;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class GameSetupManager : Singleton<GameSetupManager>
    {
        public enum ESetupSection
        {
            Intro,
            NewGame,
            Load,
            Settings,
        }
        
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private GameIntroSection _introSection;
        [SerializeField] private NewGameSection _newGameSection;

        private void Start()
        {
            DisplayGameVersion();
            ChangeSection(ESetupSection.Intro);
        }

        private void DisplayGameVersion()
        {
            var version = Application.version;
            _versionText.text = $"Version: {version}";
        }

        public void ChangeSection(ESetupSection section)
        {
            HideAllSections();
            
            switch (section)
            {
                case ESetupSection.Intro:
                    _introSection.Show();
                    break;
                case ESetupSection.NewGame:
                    _newGameSection.Show();
                    break;
                case ESetupSection.Load:
                case ESetupSection.Settings:
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }
        }

        private void HideAllSections()
        {
            _introSection.Hide();
            _newGameSection.Hide();
        }
    }
}
