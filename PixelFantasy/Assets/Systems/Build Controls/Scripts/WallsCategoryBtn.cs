using System.Collections.Generic;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class WallsCategoryBtn : CategoryBtn
    {
        [SerializeField] private WallsSubCategoryBtn _wallSubCatBtnPrefab;
        [SerializeField] private List<WallSO> _wallOptions = new List<WallSO>();

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

            foreach (var wallSO in _wallOptions)
            {
                var option = Instantiate(_wallSubCatBtnPrefab, _optionsLayout.transform);
                option.Init(wallSO, this);
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
