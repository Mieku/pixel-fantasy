using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class BuildCatergoryBtn : CatergoryBtn
    {
        [SerializeField] private GameObject _arrowHandle;
        [SerializeField] private GameObject _optionsLayout;
        [SerializeField] private BuildOptionBtn _buildOptionBtnPrefab;
        [SerializeField] private List<BuildingData> _buildOptions = new List<BuildingData>();

        private List<BuildOptionBtn> _displayedBuildOptions = new List<BuildOptionBtn>();

        private void DisplayOptions()
        {
            _arrowHandle.SetActive(true);
            _optionsLayout.SetActive(true);

            foreach (var buildingData in _buildOptions)
            {
                var buildOption = Instantiate(_buildOptionBtnPrefab, _optionsLayout.transform);
                buildOption.Init(buildingData, this);
                _displayedBuildOptions.Add(buildOption);
            }
            
            _buildController.HideOtherOptions(this);
        }

        private void HideOptions()
        {
            _arrowHandle.SetActive(false);
            _optionsLayout.SetActive(false);

            foreach (var displayedBuildOption in _displayedBuildOptions)
            {
                Destroy(displayedBuildOption.gameObject);
            }
            _displayedBuildOptions.Clear();
            
            _buildController.ShowAllOptions();
        }

        protected override void ButtonActivated()
        {
            base.ButtonActivated();
            DisplayOptions();
        }

        protected override void ButtonDeactivated()
        {
            base.ButtonDeactivated();
            HideOptions();
        }
    }
}
