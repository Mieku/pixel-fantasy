using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public abstract class OptionBtn : MonoBehaviour
    {
        [SerializeField] protected Image _icon;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        [SerializeField] protected TextMeshProUGUI _optionDetails;
        
        private bool _isOpen;
        protected CatergoryBtn _ownerCatergoryBtn;
        
        protected abstract void ShowDetails();
        protected abstract void HideDetails();
        protected abstract void TriggerOptionEffect();
        
        public void OnPressed()
        {
            if (_isOpen)
            {
                _isOpen = false;
                HideDetails();
                _ownerCatergoryBtn.UnassignOptionSelected(this);
            }

            else
            {
                _isOpen = true;
                ShowDetails();
                _ownerCatergoryBtn.AssignOptionSelected(this);
                TriggerOptionEffect();
            }
        }
        
        public void Cancel()
        {
            if (_isOpen)
            {
                _isOpen = false;
                HideDetails();
                _ownerCatergoryBtn.UnassignOptionSelected(this);
            }
            else
            {
                Debug.LogError("Attempted to Cancel an unopen option");
            }
        }
    }
}
