using System;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class NewGameSection : GameSetupSection
    {
        public enum ENewGameSetupStage
        {
            ChooseWorldSettings,
            ChooseKinlings,
        }

        [SerializeField] private ChooseWorldPanel _chooseWorldPanel;
        [SerializeField] private ChooseKinlingsPanel _chooseKinlingsPanel;

        private ENewGameSetupStage _stage;
        
        public override void Show()
        {
            base.Show();
            
            ChangeSetupStage(ENewGameSetupStage.ChooseWorldSettings);
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
                case ENewGameSetupStage.ChooseWorldSettings:
                    GameSetupManager.Instance.ChangeSection(GameSetupManager.ESetupSection.Intro);
                    break;
                case ENewGameSetupStage.ChooseKinlings:
                    ChangeSetupStage(ENewGameSetupStage.ChooseWorldSettings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnContinue()
        {
            switch (_stage)
            {
                case ENewGameSetupStage.ChooseWorldSettings:
                    ChangeSetupStage(ENewGameSetupStage.ChooseKinlings);
                    break;
                case ENewGameSetupStage.ChooseKinlings:
                    StartNewGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartNewGame()
        {
            GameManager.Instance.StartNewGame(_chooseKinlingsPanel.PlayersKinlings);
        }

        private void ChangeSetupStage(ENewGameSetupStage stage)
        {
            HideAllStages();

            _stage = stage;
            switch (stage)
            {
                case ENewGameSetupStage.ChooseWorldSettings:
                    _chooseWorldPanel.Show();
                    break;
                case ENewGameSetupStage.ChooseKinlings:
                    _chooseKinlingsPanel.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }

        private void HideAllStages()
        {
            _chooseWorldPanel.Hide();
            _chooseKinlingsPanel.Hide();
        }
    }
}
