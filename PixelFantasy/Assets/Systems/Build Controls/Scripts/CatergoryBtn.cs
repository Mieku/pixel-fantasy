using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class CatergoryBtn : MonoBehaviour
    {
        [SerializeField] protected BuildController _buildController;
        
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
        
        public void OnPressed()
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
                Debug.LogError("Tried to assign an option selected, when there is one selected already");
                return;
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
        }

        protected virtual void ButtonDeactivated()
        {
            _isActive = false;
        }
    }
}
