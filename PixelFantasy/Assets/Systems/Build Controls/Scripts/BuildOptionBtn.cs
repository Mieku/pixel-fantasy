using System.Collections.Generic;
using Buildings;
using Controllers;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class BuildOptionBtn : OptionBtn
    {
        [SerializeField] protected Transform _costsLayout;
        [SerializeField] protected BuildControlCostDisplay _costDisplayPrefab;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        [SerializeField] protected TextMeshProUGUI _optionDetails;

        private BuildingData _buildingData;
        private List<BuildControlCostDisplay> _displayedCosts = new List<BuildControlCostDisplay>();

        public void Init(BuildingData buildingData, CategoryBtn categoryBtn)
        {
            _ownerCategoryBtn = categoryBtn;
            _buildingData = buildingData;
            _icon.sprite = _buildingData.Icon;
        }

        protected override void ToggledOn()
        {
            _detailsPanel.SetActive(true);
            _optionName.text = _buildingData.ConstructionName;
            _optionDetails.text = _buildingData.ConstructionDetails;

            foreach (var cost in _buildingData.GetResourceCosts())
            {
                var costDisplay = Instantiate(_costDisplayPrefab, _costsLayout);
                costDisplay.Init(cost);
                _displayedCosts.Add(costDisplay);
            }
        }

        protected override void ToggledOff()
        {
            foreach (var displayedCost in _displayedCosts)
            {
                Destroy(displayedCost.gameObject);
            }
            _displayedCosts.Clear();
            
            _detailsPanel.SetActive(false);
        }

        protected override void TriggerOptionEffect()
        {
            PlanBuilding();
        }
        
        private Building _plannedBuilding;
        private void PlanBuilding()
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildBuilding, _buildingData.ConstructionName);
            Spawner.Instance.PlanBuilding(_buildingData.LinkedBuilding, PlanBuilding);
        }

    }
}
