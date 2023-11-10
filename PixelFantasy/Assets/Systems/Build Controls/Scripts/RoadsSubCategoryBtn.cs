using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class RoadsSubCategoryBtn : SubCategoryBtn
    {
        [SerializeField] private RoadOptionBtn _roadOptionPrefab;

        private RoadSO _roadSo;
        private RoadsCategoryBtn _categoryBtn;

        public void Init(RoadSO roadSo, RoadsCategoryBtn categoryBtn)
        {
            _roadSo = roadSo;
            _categoryBtn = categoryBtn;
            _icon.sprite = _roadSo.RoadOptions[0].OptionIcon;
        }
        
        protected override void PopulateOptions()
        {
            foreach (var option in _roadSo.RoadOptions)
            {
                var optionDisplay = Instantiate(_roadOptionPrefab, _optionsLayout.transform);
                optionDisplay.Init(option, _roadSo, this);
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
