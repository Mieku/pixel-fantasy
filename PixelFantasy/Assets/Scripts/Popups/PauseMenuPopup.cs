using DataPersistence;
using Managers;
using Systems.Game_Setup.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups
{
    public class PauseMenuPopup : Popup<PauseMenuPopup>
    {
        [SerializeField] private TextMeshProUGUI _version;
        [SerializeField] private TextMeshProUGUI _saveText;
        
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
        }

        public void ResumePressed()
        {
            Hide();
        }

        public void SavePressed()
        {
            StartCoroutine(DataPersistenceManager.Instance.SaveGameCoroutine(OnSaveProgress));
        }

        public void LoadPressed()
        {
            Hide();
            GameManager.Instance.StartLoadedGame("", false);
        }

        public void SettingsPressed()
        {
            // TODO: Build Me!
        }

        public void MainMenuPressed()
        {
            // TODO: Build Me!
        }

        public void QuitPressed()
        {
            // TODO: Build Me!
        }

        public void ReportBugPressed()
        {
            // TODO: Build Me!
            StartCoroutine(DataPersistenceManager.Instance.ClearWorld());
        }

        private void OnSaveProgress(float progress)
        {
            string progressString = (progress * 100).ToString("F0") + "%"; // "F0" formats the number with 0 decimal places

            _saveText.text = $"Saving ({progressString})";

            if (progress >= 0.999)
            {
                _saveText.text = "Saved!";
            }
        }
    }
}
