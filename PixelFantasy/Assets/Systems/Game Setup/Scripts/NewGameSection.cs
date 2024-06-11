using System;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class NewGameSection : GameSetupSection
    {
        public enum ENewGameSetupStage
        {
            ChooseKinlings,
        }
        
        
        [SerializeField] private ChooseKinlingsPanel _chooseKinlingsPanel;

        private ENewGameSetupStage _stage;
        
        public override void Show()
        {
            base.Show();
            
            ChangeSetupStage(ENewGameSetupStage.ChooseKinlings);
        }

        public override void Hide()
        {
            base.Hide();
            
            HideAllStages();
        }

        public void OnBack()
        {
            switch (_stage)
            {
                case ENewGameSetupStage.ChooseKinlings:
                    GameSetupManager.Instance.ChangeSection(GameSetupManager.ESetupSection.Intro);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnContinue()
        {
            switch (_stage)
            {
                case ENewGameSetupStage.ChooseKinlings:
                    Debug.LogError("Not built yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeSetupStage(ENewGameSetupStage stage)
        {
            HideAllStages();

            _stage = stage;
            switch (stage)
            {
                case ENewGameSetupStage.ChooseKinlings:
                    _chooseKinlingsPanel.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        private void HideAllStages()
        {
            _chooseKinlingsPanel.Hide();
        }
    }
}
