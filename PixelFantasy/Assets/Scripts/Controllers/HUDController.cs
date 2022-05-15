using System;
using System.Collections;
using System.Collections.Generic;
using Gods;
using HUD;
using UnityEngine;

namespace Controllers
{
    public class HUDController : God<HUDController>
    {
        [Header("Side Panels")] 
        [SerializeField] private GameObject BuildSP;
        [SerializeField] private GameObject CheatSP;

        [SerializeField] private SelectedItemInfoPanel _selectedItemInfoPanel;

        private HUDState _hudState;
        
        public enum HUDState
        {
            None,
            Build,
            Cheat
        }

        private void Start()
        {
            ChangeState(HUDState.None);
        }

        public void ShowItemDetails(SelectionData selectionData)
        {
            ChangeState(HUDState.None);
            _selectedItemInfoPanel.ShowItemDetails(selectionData);
        }

        public void HideItemDetails()
        {
            if (_selectedItemInfoPanel != null)
            {
                _selectedItemInfoPanel.HideItemDetails();
            }
        }

        private void ChangeState(HUDState hudState)
        {
            HideItemDetails();
            _hudState = hudState;
            switch (hudState)
            {
                case HUDState.None:
                    SetStateNone();
                    break;
                case HUDState.Build:
                    SetStateBuild();
                    break;
                case HUDState.Cheat:
                    SetStateCheat();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hudState), hudState, null);
            }
        }

        private void ClearPanels()
        {
            // Side Panels
            BuildSP.SetActive(false);
            CheatSP.SetActive(false);
        }

        #region States

        private void SetStateNone()
        {
            ClearPanels();
        }
        
        private void SetStateBuild()
        {
            ClearPanels();
            BuildSP.SetActive(true);
        }
        
        private void SetStateCheat()
        {
            ClearPanels();
            CheatSP.SetActive(true);
        }

        #endregion

        #region Bottom HUD Buttons

        /// <summary>
        /// Changes the HUD state, unless the state is already the requested state, then change to None
        /// </summary>
        private void BottomHUDPressed(HUDState hudState)
        {
            var pendingState = hudState;
            if (_hudState == pendingState)
            {
                pendingState = HUDState.None;
            }

            ChangeState(pendingState);
        }

        public void BuildButtonPressed()
        {
            BottomHUDPressed(HUDState.Build);
        }
        
        public void CheatButtonPressed()
        {
            BottomHUDPressed(HUDState.Cheat);
        }

        #endregion
        
    }
    
    
}