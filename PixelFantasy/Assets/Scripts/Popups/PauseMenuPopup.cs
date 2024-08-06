using DataPersistence;
using Systems.Game_Setup.Scripts;
using TMPro;
using UnityEngine;

namespace Popups
{
    public class PauseMenuPopup : Popup<PauseMenuPopup>
    {
        [SerializeField] private TextMeshProUGUI _version;
        [SerializeField] private TextMeshProUGUI _saveText;
        [SerializeField] private GameObject _loadButton;
        
        public static void Show()
        {
            Open(() => Instance.Refresh(), true);
        }
    
        public override void OnBackPressed()
        {
            Hide();
        }
    
        void AnimMoveOutHandler()
        {
            Close();
        }

        private void Refresh()
        {
            var version = Application.version;
            _version.text = $"Version: {version}";
            
            CheckIfLoadAvailable();
        }

        private void CheckIfLoadAvailable()
        {
            var hasSave = DataPersistenceManager.Instance.AreSavesAvailable();
            _loadButton.SetActive(hasSave);
        }

        public void ResumePressed()
        {
            Hide();
        }

        public void SavePressed()
        {
            SaveGamePopup.Show(CheckIfLoadAvailable);
        }

        public void LoadPressed()
        {
            LoadGamePopup.Show(data =>
            {
                Hide();
                GameManager.Instance.StartLoadedGame(data, false);
            }, CheckIfLoadAvailable);
        }

        public void SettingsPressed()
        {
            SettingsPopup.Show();
        }

        public void MainMenuPressed()
        {
            ConfirmationPopup.Show("Are you sure?\nAny unsaved changes will be lost", (confirmation) =>
            {
                if (confirmation)
                {
                    Hide();
            
                    GameManager.Instance.GoToMainMenu();
                }
            });
        }

        public void QuitPressed()
        {
            ConfirmationPopup.Show("Are you sure?\nAny unsaved changes will be lost", (confirmation) =>
            {
                if (confirmation)
                {
                    GameManager.Instance.QuitGame();
                }
            });
        }

        public void ReportBugPressed()
        {
            // TODO: Build Me!
            // TODO: Remove this!
            StartCoroutine(DataPersistenceManager.Instance.ClearWorld());
        }
    }
}
