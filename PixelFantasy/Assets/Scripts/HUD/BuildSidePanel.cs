using System;
using UnityEngine;

namespace HUD
{
    public class BuildSidePanel : MonoBehaviour
    {
        [Header("Options")] 
        [SerializeField] private GameObject StructureOpt;
        [SerializeField] private GameObject StorageOpt;
        [SerializeField] private GameObject FloorOpt;
        [SerializeField] private GameObject FurnitureOpt;

        private PanelState _panelState;
        
        private enum PanelState
        {
            None,
            Structure,
            Storage,
            Floor,
            Furniture
        }

        private void OnEnable()
        {
            ChangeState(PanelState.None);
        }

        private void HideOptions()
        {
            StructureOpt.SetActive(false);
            StorageOpt.SetActive(false);
            FloorOpt.SetActive(false);
            FurnitureOpt.SetActive(false);
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
                case PanelState.Floor:
                    SetStateFloor();
                    break;
                case PanelState.Furniture:
                    SetStateFurniture();
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

        private void SetStateFloor()
        {
            HideOptions();
            
            FloorOpt.SetActive(true);
        }

        private void SetStateFurniture()
        {
            HideOptions();

            FurnitureOpt.SetActive(true);
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

        public void FloorOptionPressed()
        {
            OptionPressed(PanelState.Floor);
        }

        public void FurnitureOptionPressed()
        {
            OptionPressed(PanelState.Furniture);
        }
        
        #endregion
    }
}
