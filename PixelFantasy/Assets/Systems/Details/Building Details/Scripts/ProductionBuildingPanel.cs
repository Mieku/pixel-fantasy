using System.Collections.Generic;
using Buildings;
using UnityEngine;

namespace Systems.Details.Building_Details.Scripts
{
    public class ProductionBuildingPanel : BuildingPanel
    {
        [SerializeField] private ProductionOption _productionOptionPrefab;
        [SerializeField] private Transform _productionOptionsParent;

        private List<ProductionOption> _displayedOptions = new List<ProductionOption>();
        private ProductionBuilding _prodBuilding => _building as ProductionBuilding;
        
        protected override void Show()
        {
            CreateOptions();
        }

        protected override void Refresh()
        {
            // Refreshes values
            foreach (var option in _displayedOptions)
            {
                option.RefreshValues(DetermineProductionProgress(option.Settings));
            }
        }

        private float DetermineProductionProgress(ProductionSettings settings)
        {
            return _prodBuilding.GetProductionProgress(settings.CraftedItem);
        }

        private void CreateOptions()
        {
            ClearDisplayedOptions();
            int priority = 1;
            foreach (var productionSetting in _prodBuilding.ProductionSettings)
            {
                ProductionOption prodDisplay = Instantiate(_productionOptionPrefab, _productionOptionsParent);
                prodDisplay.Init(productionSetting, priority, this);
                _displayedOptions.Add(prodDisplay);
                priority++;
            }
            Refresh();
        }

        private void ClearDisplayedOptions()
        {
            foreach (var option in _displayedOptions)
            {
                Destroy(option.gameObject);
            }
            _displayedOptions.Clear();
        }

        public void IncreaseProductionPriority(ProductionSettings settings)
        {
            int index = _prodBuilding.ProductionSettings.IndexOf(settings);
            if (index == 0)
            {
                Debug.LogError("Attempted to increase priority of top priority option");
                return;
            }
            
            index = Mathf.Clamp(index - 1, 0, _prodBuilding.ProductionSettings.Count - 1);
            _prodBuilding.ProductionSettings.Remove(settings);
            _prodBuilding.ProductionSettings.Insert(index, settings);
            
            CreateOptions();
        }
        
        public void DecreaseProductionPriority(ProductionSettings settings)
        {
            int index = _prodBuilding.ProductionSettings.IndexOf(settings);
            if (index >= _prodBuilding.ProductionSettings.Count - 1)
            {
                Debug.LogError("Attempted to decrease priority of the last priority option");
                return;
            }
            
            index = Mathf.Clamp(index + 1, 0, _prodBuilding.ProductionSettings.Count - 1);
            _prodBuilding.ProductionSettings.Remove(settings);
            _prodBuilding.ProductionSettings.Insert(index, settings);
            
            CreateOptions();
        }

        public bool IsSettingLastPriority(ProductionSettings settings)
        {
            return _prodBuilding.ProductionSettings.IndexOf(settings) == _prodBuilding.ProductionSettings.Count - 1;
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}
