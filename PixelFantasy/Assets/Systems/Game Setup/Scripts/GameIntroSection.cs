using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class GameIntroSection : GameSetupSection
    {
        [SerializeField] private GameObject _resumeButton;
        
        public override void Show()
        {
            base.Show();
            
            CheckIfResumeAvailable();
        }

        private void CheckIfResumeAvailable()
        {
            // TODO: Check if there is a saved game, if so show the resume button
            _resumeButton.SetActive(false);
        }

        public void OnResumePressed()
        {
            Debug.Log("Resume Pressed");
        }

        public void OnNewGamePressed()
        {
            GameSetupManager.Instance.ChangeSection(GameSetupManager.ESetupSection.NewGame);
        }

        public void OnLoadPressed()
        {
            Debug.Log("Load Pressed");
            GameManager.Instance.StartLoadedGame("", true);
        }
        
        public void OnSettingsPressed()
        {
            Debug.Log("Settings Pressed");
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
