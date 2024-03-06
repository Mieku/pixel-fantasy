using Systems.Buildings.Scripts;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class WallsSubCategoryBtn : SubCategoryBtn
    {
        [SerializeField] private WallOptionBtn _wallOptionPrefab;

        private WallSettings _wallSettings;
        private WallsCategoryBtn _categoryBtn;

        public void Init(WallSettings wallSettings, WallsCategoryBtn categoryBtn)
        {
            _wallSettings = wallSettings;
            _categoryBtn = categoryBtn;
            _icon.sprite = _wallSettings.WallOptions[0].OptionIcon;
        }
        
        protected override void PopulateOptions()
        {
            foreach (var option in _wallSettings.WallOptions)
            {
                var optionDisplay = Instantiate(_wallOptionPrefab, _optionsLayout.transform);
                optionDisplay.Init(option, _wallSettings, this);
                _displayedOptions.Add(optionDisplay);
            }
        }

        protected override void ButtonActivated()
        {
            base.ButtonActivated();

            _categoryBtn.SelectSubCategoryBtn(this);
            _displayedOptions[0].OnPressed();
        }

        protected override void ButtonDeactivated()
        {
            base.ButtonDeactivated();
            
            _categoryBtn.UnselectSubCategoryBtn();
        }
    }
}
