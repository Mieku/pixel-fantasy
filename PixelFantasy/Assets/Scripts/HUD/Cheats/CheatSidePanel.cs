using System;
using UnityEngine;

namespace HUD.Cheats
{
    public class CheatSidePanel : MonoBehaviour
    {
        [Header("Options")] 
        [SerializeField] private GameObject _resourcesOpt;
        
        private PanelState _panelState;
        
        private enum PanelState
        {
            None,
            Resources,
        }
        
        private void OnEnable()
        {
            ChangeState(PanelState.None);
        }

        private void HideOptions()
        {
            _resourcesOpt.SetActive(false);
        }
        
        private void ChangeState (PanelState panelState)
        {
            _panelState = panelState;
            switch (panelState)
            {
                case PanelState.None:
                    SetStateNone();
                    break;
                case PanelState.Resources:
                    SetStateResources();
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

        private void SetStateResources()
        {
            HideOptions();
            
            _resourcesOpt.SetActive(true);
        }
        
        #endregion
        
        public void ResourceOptionPressed()
        {
            OptionPressed(PanelState.Resources);
        }
    }
}
