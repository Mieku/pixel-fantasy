using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public abstract class SubCategoryBtn : CategoryBtn
    {
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBtnSpr;
        [SerializeField] private Sprite _activeBtnSpr;
        [SerializeField] protected Image _icon;

        protected List<OptionBtn> _displayedOptions = new List<OptionBtn>();
        protected abstract void PopulateOptions();

        public override void OnPressed()
        {
            if (_isActive)
            {
                ButtonDeactivated();
            }
            else
            {
                ButtonActivated();
            }
        }

        protected override void ButtonActivated()
        {
            base.ButtonActivated();
            
            _btnBG.sprite = _activeBtnSpr;
        }

        protected override void ButtonDeactivated()
        {
            base.ButtonDeactivated();

            _btnBG.sprite = _defaultBtnSpr;
        }

        protected override void DisplayOptions()
        {
            _arrowHandle.SetActive(true);
            _optionsLayout.SetActive(true);
            
            PopulateOptions();
        }

        protected override void HideOptions()
        {
            _arrowHandle.SetActive(false);
            _optionsLayout.SetActive(false);
            
            ClearDisplayedOptions();
        }
        

        private void ClearDisplayedOptions()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }
    }
}
