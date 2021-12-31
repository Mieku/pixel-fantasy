using System;
using UnityEngine;

namespace HUD
{
    public class BuildSidePanel : MonoBehaviour
    {
        [Header("Options")] 
        [SerializeField] private GameObject StructureOpt;
        [SerializeField] private GameObject StorageOpt;

        private PanelState _panelState;
        
        private enum PanelState
        {
            None,
            Structure,
            Storage,
        }

        private void OnEnable()
        {
            ChangeState(PanelState.None);
        }

        private void HideOptions()
        {
            StructureOpt.SetActive(false);
            StorageOpt.SetActive(false);
        }
        
        private void ChangeState (PanelState panelState)
        {
            _panelState = panelState;
            switch (panelState)
            {
                case PanelState.None:
                    SetStateNone();
                    break;
                case PanelState.Structure:
                    SetStateStructure();
                    break;
                case PanelState.Storage:
                    SetStateStorage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(panelState), panelState, null);
            }
        }

        private void OptionPressed(PanelState panelState)
        {
            var pendingState = panelState;
            if (_panelState == pendingState)
            {
                pendingState = PanelState.None;
            }

            ChangeState(pendingState);
        }

        #region States

        private void SetStateNone()
        {
            HideOptions();
        }
        
        private void SetStateStructure()
        {
            HideOptions();
            
            StructureOpt.SetActive(true);
        }

        private void SetStateStorage()
        {
            HideOptions();
            
            StorageOpt.SetActive(true);
        }

        #endregion

        #region Option Buttons

        public void StructureOptionPressed()
        {
            OptionPressed(PanelState.Structure);
        }

        public void StorageOptionPressed()
        {
            OptionPressed(PanelState.Storage);
        }
        
        #endregion
    }
}
