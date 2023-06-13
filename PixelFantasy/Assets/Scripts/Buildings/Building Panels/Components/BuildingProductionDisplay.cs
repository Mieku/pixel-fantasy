using System;
using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Buildings.Building_Panels.Components
{
    public class BuildingProductionDisplay : MonoBehaviour
    {
        [SerializeField] private BuildingProductionControl _controlPreset;
        [SerializeField] private BuildingProductionOption _optionPreset;
        [SerializeField] private GameObject _mainPanelHandle;
        [SerializeField] private GameObject _optionsPanelHandle;
        [SerializeField] private Transform _optionsParent;
        [SerializeField] private Transform _controlsParent;

        private BuildingPanel _buildingPanel;
        private bool _isOpen;
        private bool _isShowingOptions;
        private ProductionBuilding _building => _buildingPanel.Building;
        private List<GameObject> _displayedOptions = new List<GameObject>();
        private List<GameObject> _displayedControls = new List<GameObject>();

        private void Start()
        {
            _controlPreset.gameObject.SetActive(false);
            _optionPreset.gameObject.SetActive(false);
            _mainPanelHandle.SetActive(false);
            _optionsPanelHandle.SetActive(false);
            _buildingPanel = gameObject.GetComponentInParent<BuildingPanel>();
        }

        public void RefreshProductionControls()
        {
            // foreach (var control in _displayedControls)
            // {
            //     Destroy(control.gameObject);
            // }
            // _displayedControls.Clear();
            //
            // var allOrders = _building.OrderQueue.AllOrders;
            // int index = 0;
            // foreach (var order in allOrders)
            // {
            //     var control = Instantiate(_controlPreset, _controlsParent);
            //     control.Init(order, _building, OnOrderValueChanged, index);
            //     control.gameObject.SetActive(true);
            //     _displayedControls.Add(control.gameObject);
            //     index++;
            // }
        }

        private void OnOrderValueChanged(ProductOrder order)
        {
            RefreshProductionControls();
        }

        public void RefreshProductionOptions()
        {
            // foreach (var displayedOption in _displayedOptions)
            // {
            //     Destroy(displayedOption.gameObject);
            // }
            // _displayedOptions.Clear();
            //
            // var allCraftable = Librarian.Instance.GetAllCraftedItemDatas();
            // List<CraftedItemData> options = new List<CraftedItemData>();
            // foreach (var item in allCraftable)
            // {
            //     ProfessionData profession = item.CraftersProfession;
            //     if (_building.BuildingData.WorkersProfession == profession)
            //     {
            //         FurnitureItemData requiredTable = item.RequiredCraftingTable;
            //         if (_building.GetFurniture(requiredTable) != null)
            //         {
            //             options.Add(item);
            //         }
            //     }
            // }
            //
            // foreach (var option in options)
            // {
            //     BuildingProductionOption prodOption = Instantiate(_optionPreset, _optionsParent);
            //     prodOption.Init(option, OnProductSelected);
            //     prodOption.gameObject.SetActive(true);
            //     _displayedOptions.Add(prodOption.gameObject);
            // }
        }

        // private void OnProductSelected(CraftedItemData itemData)
        // {
        //     // Create order
        //     ProductOrder order = new ProductOrder(itemData);
        //     
        //     // Add order to building
        //     _building.OrderQueue.AddOrder(order);
        //     
        //     // Refresh Controls Display
        //     RefreshProductionControls();
        // }

        #region Button Hooks

        public void ExitPressed()
        {
            _mainPanelHandle.SetActive(false);
            _optionsPanelHandle.SetActive(false);
            _isOpen = false;
            _isShowingOptions = false;
        }

        public void ShowProductionPressed()
        {
            if (!_isOpen)
            {
                _isOpen = true;
                _mainPanelHandle.SetActive(true);
                RefreshProductionControls();
            }
            else
            {
                _mainPanelHandle.SetActive(false);
                _optionsPanelHandle.SetActive(false);
                _isOpen = false;
                _isShowingOptions = false;
            }
        }

        public void NewProductionPressed()
        {
            if (!_isShowingOptions)
            {
                _isShowingOptions = true;
                _optionsPanelHandle.SetActive(true);
                RefreshProductionOptions();
            }
            else
            {
                _isShowingOptions = false;
                _optionsPanelHandle.SetActive(false);
            }
        }

        #endregion
    }
}
