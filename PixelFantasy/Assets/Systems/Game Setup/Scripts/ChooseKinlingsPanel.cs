using System;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class ChooseKinlingsPanel : MonoBehaviour
    {

        [SerializeField] private NewGameSection _newGameSection;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #region Button Hooks

        public void OnBackPressed()
        {
            _newGameSection.OnBack();
        }

        public void OnContinuePressed()
        {
            _newGameSection.OnContinue();
        }

        public void OnRerollColonyPressed()
        {
            
        }

        public void OnRerollKinlingPressed()
        {
            
        }

        public void OnFirstnameChanged(string value)
        {
            
        }

        public void OnLastnameChanged(string value)
        {
            
        }

        public void OnNicknameChanged(string value)
        {
            
        }

        public void OnRefreshNicknamePressed()
        {
            
        }

        #endregion
    }
}
