using System.Collections.Generic;
using Buildings.Building_Panels.Components;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class ProductionPanel : MonoBehaviour
    {
        [SerializeField] private BuildingProductionControl _controlPreset;
        [SerializeField] private BuildingProductionOption _optionPreset;
        [SerializeField] private Transform _controlParent;
        [SerializeField] private Transform _optionParent;
        
        private ProductionZone _zone;
        private List<BuildingProductionOption> _displayedOptions = new List<BuildingProductionOption>();
        private List<BuildingProductionControl> _displayedControls = new List<BuildingProductionControl>();

        public void Show(ProductionZone zone)
        {
            _zone = zone;
            gameObject.SetActive(true);
            
            RefreshProductionOptions();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void RefreshProductionOptions()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
            
            var options = _zone.GetProductionOptions();
            foreach (var craft in options)
            {
                var option = Instantiate(_optionPreset, _optionParent);
                option.Init(craft, CraftOptionSelected, _zone);
                _displayedOptions.Add(option);
            }
        }
        
        public void RefreshProductionControls()
        {
            foreach (var control in _displayedControls)
            {
                Destroy(control.gameObject);
            }
            _displayedControls.Clear();
            
            var allOrders = _zone.OrderQueue.AllOrders;
            int index = 0;
            foreach (var order in allOrders)
            {
                var control = Instantiate(_controlPreset, _controlParent);
                control.Init(order, _zone, OnOrderValueChanged, index);
                control.gameObject.SetActive(true);
                _displayedControls.Add(control);
                index++;
            }
        }
        
        private void OnOrderValueChanged(ProductOrder order)
        {
            RefreshProductionControls();
        }

        public void CraftOptionSelected(CraftedItemData craftedItemData)
        {
            // Create order
            ProductOrder order = new ProductOrder(craftedItemData);
            
            // Add order to room
            _zone.OrderQueue.AddOrder(order);
            
            // Refresh Controls Display
            RefreshProductionControls();
        }
    }
}
