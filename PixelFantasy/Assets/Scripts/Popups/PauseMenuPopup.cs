using DataPersistence;
using Managers;
using Systems.Game_Setup.Scripts;

namespace Popups
{
    public class PauseMenuPopup : Popup<PauseMenuPopup>
    {
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
        
        }

        public void ResumePressed()
        {
            Hide();
        }

        public void SavePressed()
        {
            StartCoroutine(DataPersistenceManager.Instance.SaveGameCoroutine());
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
    }
}
