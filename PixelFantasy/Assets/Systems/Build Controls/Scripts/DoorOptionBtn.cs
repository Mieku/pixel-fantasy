using System.Collections.Generic;
using Buildings;
using Controllers;
using Managers;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using Systems.CursorHandler.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class DoorOptionBtn : OptionBtn
    {
        [SerializeField] protected Transform _costsLayout;
        [SerializeField] protected BuildControlCostDisplay _costDisplayPrefab;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        [SerializeField] protected TextMeshProUGUI _optionDetails;

        private DoorSettings _doorSettings;
        private List<BuildControlCostDisplay> _displayedCosts = new List<BuildControlCostDisplay>();

        public void Init(DoorSettings doorSettings, CategoryBtn categoryBtn)
        {
            _ownerCategoryBtn = categoryBtn;
            _doorSettings = doorSettings;
            _icon.sprite = doorSettings.Icon;
        }

        protected override void ToggledOn()
        {
            _detailsPanel.SetActive(true);
            _optionName.text = _doorSettings.DoorName;
            _optionDetails.text = "Not implemented yet";
            
            foreach (var cost in _doorSettings.GetResourceCosts())
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
            PlanDoor();
        }
        
        private Door _plannedDoor;
        private void PlanDoor()
        {
            // Spawner.Instance.CancelInput();
            // PlayerInputController.Instance.ChangeState(PlayerInputState.BuildDoor, _doorSettings.DoorName);
            // Spawner.Instance.PlanDoor(_doorSettings, ToggledOff);
        }
    }
}
