using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class RoadsCategoryBtn : CategoryBtn
    {
        [SerializeField] private RoadsSubCategoryBtn _roadSubCatBtnPrefab;
        [SerializeField] private DirtSubCategoryBtn _dirtSubCatBtnPrefab;
        [SerializeField] private List<RoadSettings> _roadOptions = new List<RoadSettings>();

        private List<SubCategoryBtn> _displayedOptions = new List<SubCategoryBtn>();
        private SubCategoryBtn _selectedSubCategoryBtn;

        public override void Cancel()
        {
            if (_selectedSubCategoryBtn != null)
            {
                _selectedSubCategoryBtn.Cancel();
            }
            else
            {
                ButtonDeactivated();
            }
        }

        protected override void DisplayOptions()
        {
            base.DisplayOptions();
            
            var dirtOption = Instantiate(_dirtSubCatBtnPrefab, _optionsLayout.transform);
            dirtOption.Init(this);
            _displayedOptions.Add(dirtOption);

            foreach (var roadSO in _roadOptions)
            {
                var option = Instantiate(_roadSubCatBtnPrefab, _optionsLayout.transform);
                option.Init(roadSO, this);
                _displayedOptions.Add(option);
            }
        }

        protected override void HideOptions()
        {
            base.HideOptions();

            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }

        public void SelectSubCategoryBtn(SubCategoryBtn subCategoryBtnToSelect)
        {
            _selectedSubCategoryBtn = subCategoryBtnToSelect;
            HideOtherSubCategoryBtn(subCategoryBtnToSelect);
        }
        
        public void UnselectSubCategoryBtn()
        {
            ShowAllSubCategoryBtns();
            _selectedSubCategoryBtn = null;
        }
        
        private void HideOtherSubCategoryBtn(SubCategoryBtn subCategoryBtnToShow)
        {
            foreach (var displayedOption in _displayedOptions)
            {
                if (displayedOption != subCategoryBtnToShow)
                {
                    displayedOption.gameObject.SetActive(false);
                }
            }
        }

        private void ShowAllSubCategoryBtns()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                displayedOption.gameObject.SetActive(true);
            }
        }
    }
}
