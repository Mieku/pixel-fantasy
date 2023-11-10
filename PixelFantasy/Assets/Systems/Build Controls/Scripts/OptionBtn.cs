using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public abstract class OptionBtn : MonoBehaviour
    {
        [SerializeField] protected Image _icon;

        protected bool _isOpen;
        protected CategoryBtn _ownerCategoryBtn;
        
        protected abstract void ToggledOn();
        protected abstract void ToggledOff();
        protected abstract void TriggerOptionEffect();
        
        public void OnPressed()
        {
            if (_isOpen)
            {
                _isOpen = false;
                ToggledOff();
                _ownerCategoryBtn.UnassignOptionSelected(this);
            }

            else
            {
                _isOpen = true;
                ToggledOn();
                _ownerCategoryBtn.AssignOptionSelected(this);
                TriggerOptionEffect();
            }
        }
        
        public virtual void Cancel()
        {
            if (_isOpen)
            {
                _isOpen = false;
                ToggledOff();
                _ownerCategoryBtn.UnassignOptionSelected(this);
            }
            else
            {
                Debug.LogError("Attempted to Cancel an unopen option");
            }
        }
    }
}
