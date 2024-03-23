using System.Collections.Generic;
using Buildings.Building_Panels;
using Data.Dye;
using Data.Item;
using Data.Structure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class SelectionStructureDetails : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _statsListText;
        [SerializeField] private TextMeshProUGUI _requiredCraftingLvl;
        [SerializeField] private ResourceCost _resourceCostPrefab;
        [SerializeField] private Transform _resourceCostParent;

        private CraftRequirements _requirements;
        private List<ResourceCost> _displayedResourceCosts = new List<ResourceCost>();
        
        public void ShowWallSelection(WallSettings wallSettings, DyeData colour)
        {
            _requirements = wallSettings.CraftRequirements;
            _title.text = wallSettings.title;
            RefreshStatsDisplay(wallSettings.MaxDurability, wallSettings.GetStatsList());
            RefreshCraftingRequirements(_requirements);
        }
        
        public void ShowDoorSelection(DoorSettings doorSettings, DyeData matColour)
        {
            _requirements = doorSettings.CraftRequirements;
            _title.text = doorSettings.title;
            RefreshStatsDisplay(doorSettings.MaxDurability, doorSettings.GetStatsList());
            RefreshCraftingRequirements(_requirements);
        }
        
        private void RefreshStatsDisplay(int durability, List<string> statsList)
        {
            string msg = $"Durability: {durability}\n";
            foreach (var stat in statsList)
            {
                msg += stat + "/n";
            }
            _statsListText.text = msg;
        }
        
        private void RefreshCraftingRequirements(CraftRequirements craftRequirements)
        {
            if (craftRequirements.MinCraftingSkillLevel > 0)
            {
                string craftMsg = $"Required {craftRequirements.CraftingSkill.GetDescription()}: {craftRequirements.MinCraftingSkillLevel}";
                _requiredCraftingLvl.gameObject.SetActive(true);
                _requiredCraftingLvl.text = craftMsg;
            }
            else
            {
                _requiredCraftingLvl.gameObject.SetActive(false);
            }

            foreach (var displayedResourceCost in _displayedResourceCosts)
            {
                Destroy(displayedResourceCost.gameObject);
            }
            _displayedResourceCosts.Clear();
            
            _resourceCostPrefab.gameObject.SetActive(false);
            foreach (var costAmount in craftRequirements.GetMaterialCosts())
            {
                var cost = Instantiate(_resourceCostPrefab, _resourceCostParent);
                cost.gameObject.SetActive(true);
                cost.Init(costAmount);
                _displayedResourceCosts.Add(cost);
            }
        }
    }
}
