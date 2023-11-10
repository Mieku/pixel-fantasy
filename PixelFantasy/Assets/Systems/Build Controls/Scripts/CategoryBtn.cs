using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class CategoryBtn : MonoBehaviour
    {
        [SerializeField] protected BuildController _buildController;
        [SerializeField] protected GameObject _arrowHandle;
        [SerializeField] protected GameObject _optionsLayout;
        
        protected bool _isActive;
        protected OptionBtn _selectedOption;

        public virtual void Cancel()
        {
            if (_selectedOption != null)
            {
                _selectedOption.Cancel();
            }
            else
            {
                ButtonDeactivated();
            }
        }
        
        public void UnselectButton()
        {
            ButtonDeactivated();
        }
        
        public virtual void OnPressed()
        {
            if (_isActive)
            {
                _buildController.AssignUnselected(this);
                ButtonDeactivated();
            }
            else
            {
                _buildController.AssignSelected(this);
                ButtonActivated();
            }
        }

        public void AssignOptionSelected(OptionBtn selectedOption)
        {
            if (_selectedOption != null)
            {
                _selectedOption.Cancel();
            }

            _selectedOption = selectedOption;
        }

        public void UnassignOptionSelected(OptionBtn unselectedOption)
        {
            if (_selectedOption != unselectedOption)
            {
                Debug.LogError("Tried to assign an option unselected, when the selected option isn't the one requesting");
                return;
            }

            _selectedOption = null;
        }

        protected virtual void ButtonActivated()
        {
            _isActive = true;
            DisplayOptions();
        }

        protected virtual void ButtonDeactivated()
        {
            _isActive = false;
            HideOptions();
        }

        protected virtual void DisplayOptions()
        {
            _arrowHandle.SetActive(true);
            _optionsLayout.SetActive(true);

            _buildController.HideOtherOptions(this);
        }
        
        protected virtual void HideOptions()
        {
            _arrowHandle.SetActive(false);
            _optionsLayout.SetActive(false);

            _buildController.ShowAllOptions();
        }
    }
}
