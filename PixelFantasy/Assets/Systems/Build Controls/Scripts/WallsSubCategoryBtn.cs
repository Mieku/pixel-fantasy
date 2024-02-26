using Systems.Buildings.Scripts;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class WallsSubCategoryBtn : SubCategoryBtn
    {
        [SerializeField] private WallOptionBtn _wallOptionPrefab;

        private WallSO _wallSo;
        private WallsCategoryBtn _categoryBtn;

        public void Init(WallSO wallSo, WallsCategoryBtn categoryBtn)
        {
            _wallSo = wallSo;
            _categoryBtn = categoryBtn;
            _icon.sprite = _wallSo.WallOptions[0].OptionIcon;
        }
        
        protected override void PopulateOptions()
        {
            foreach (var option in _wallSo.WallOptions)
            {
                var optionDisplay = Instantiate(_wallOptionPrefab, _optionsLayout.transform);
                optionDisplay.Init(option, _wallSo, this);
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
