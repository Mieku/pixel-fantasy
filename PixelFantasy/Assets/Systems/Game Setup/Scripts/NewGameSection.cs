using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Game_Setup.Scripts
{
    public class NewGameSection : GameSetupSection
    {
        public enum ENewGameSetupStage
        {
            ChooseBackgroundSettings,
            ChooseWorldSettings,
            ChooseKinlings,
        }

        [SerializeField] private ChooseWorldPanel _chooseWorldPanel;
        [SerializeField] private ChooseKinlingsPanel _chooseKinlingsPanel;
        [SerializeField] private ChooseBackgroundPanel _chooseBackgroundPanel;

        private ENewGameSetupStage _stage;
        
        public override void Show()
        {
            base.Show();
            
            ChangeSetupStage(ENewGameSetupStage.ChooseBackgroundSettings);
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
                case ENewGameSetupStage.ChooseBackgroundSettings:
                    GameSetupManager.Instance.ChangeSection(GameSetupManager.ESetupSection.Intro);
                    break;
                case ENewGameSetupStage.ChooseWorldSettings:
                    ChangeSetupStage(ENewGameSetupStage.ChooseBackgroundSettings);
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
                case ENewGameSetupStage.ChooseBackgroundSettings:
                    ChangeSetupStage(ENewGameSetupStage.ChooseWorldSettings);
                    break;
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
            GameManager.Instance.StartNewGame(_chooseBackgroundPanel.SettlementName, _chooseKinlingsPanel.PlayersKinlings, _chooseWorldPanel.BlueprintLayers);
        }

        private void ChangeSetupStage(ENewGameSetupStage stage)
        {
            HideAllStages();

            _stage = stage;
            switch (stage)
            {
                case ENewGameSetupStage.ChooseBackgroundSettings:
                    _chooseBackgroundPanel.Show();
                    break;
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
            _chooseBackgroundPanel.Hide();
        }
    }
}
