using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class DirtSubCategoryBtn : SubCategoryBtn
    {
        [SerializeField] private DirtOptionBtn _dirtOptionPrefab;
        
        private RoadsCategoryBtn _categoryBtn;
        
        public void Init(RoadsCategoryBtn categoryBtn)
        {
            _categoryBtn = categoryBtn;
        }
        
        protected override void PopulateOptions()
        {
            var optionDisplay = Instantiate(_dirtOptionPrefab, _optionsLayout.transform);
            optionDisplay.Init(this);
            _displayedOptions.Add(optionDisplay);
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
