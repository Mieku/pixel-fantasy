using DataPersistence;
using Popups;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class GameIntroSection : GameSetupSection
    {
        [SerializeField] private GameObject _resumeButton;
        [SerializeField] private GameObject _loadButton;

        public override void Show()
        {
            base.Show();
            
            CheckIfResumeAvailable();
        }

        private void CheckIfResumeAvailable()
        {
            var hasSaves = DataPersistenceManager.Instance.AreSavesAvailable();
            
            _resumeButton.SetActive(hasSaves);
            _loadButton.SetActive(hasSaves);
        }

        public void OnResumePressed()
        {
            var recentSave = DataPersistenceManager.Instance.GetMostRecentSave();
            GameManager.Instance.StartLoadedGame(recentSave, true);
        }

        public void OnNewGamePressed()
        {
            GameSetupManager.Instance.ChangeSection(GameSetupManager.ESetupSection.NewGame);
        }

        public void OnLoadPressed()
        {
            LoadGamePopup.Show(data =>
            {
                GameManager.Instance.StartLoadedGame(data, true);
            }, CheckIfResumeAvailable);
        }
        
        public void OnSettingsPressed()
        {
            SettingsPopup.Show();
        }

        public void OnExitPressed()
        {
            Application.Quit();

            // If you are running the game in the editor, this line will stop playing the scene
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
