using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Buildings.Building_Panels
{
    public class BuildingFurnitureSelection : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _detailsText;
        [SerializeField] private TextMeshProUGUI _productName;
        [SerializeField] private Transform _varientsParent;
        [SerializeField] private FurnitureVarientOption _furnitureVarientPrefab;
        [SerializeField] private Transform _costsParent;
        [SerializeField] private ResourceCost _costPrefab;

        private FurnitureItemData _defaultFurnitureItemData;
        private List<FurnitureVarientOption> _displayedVarientOptions = new List<FurnitureVarientOption>();
        private FurnitureVarientOption _curSelectedVarientOption;
        private List<ResourceCost> _displayedCosts = new List<ResourceCost>();
        
        public void Init(FurnitureItemData furnitureItemData)
        {
            _defaultFurnitureItemData = furnitureItemData;
            DisplayVarientOptions();
            
            // Start by pressing the default
            _displayedVarientOptions[0].VarientSelected();
        }

        private void DisplayVarientOptions()
        {
            foreach (var varientOption in _displayedVarientOptions)
            {
                Destroy(varientOption.gameObject);
            }
            _displayedVarientOptions.Clear();
            
            // Start with the default
            var option = Instantiate(_furnitureVarientPrefab, _varientsParent);
            option.Init(_defaultFurnitureItemData, OnVarientSelected);
            _displayedVarientOptions.Add(option);

            // Then the rest
            foreach (var varient in _defaultFurnitureItemData.Varients)
            {
                var varientOption = Instantiate(_furnitureVarientPrefab, _varientsParent);
                varientOption.Init(varient, OnVarientSelected);
                _displayedVarientOptions.Add(varientOption);
            }
        }

        private void OnVarientSelected(FurnitureItemData furnitureItemData, FurnitureVarientOption varientOption)
        {
            if (_curSelectedVarientOption != null)
            {
                _curSelectedVarientOption.DisplaySelected(false);
            }
            _curSelectedVarientOption = varientOption;
            _curSelectedVarientOption.DisplaySelected(true);
            
            DisplaySelectedOptionDetails(furnitureItemData);
        }

        private void DisplaySelectedOptionDetails(FurnitureItemData furnitureItemData)
        {
            _productName.text = furnitureItemData.ItemName;
            RefreshDetails(furnitureItemData);
            RefreshCosts(furnitureItemData);
            StartPlanningFurniture(furnitureItemData);
        }

        private void RefreshDetails(FurnitureItemData furnitureItemData)
        {
            string craftedWith = furnitureItemData.RequiredCraftingTable.ItemName;

            string details = $"Crafted with: <color=blue>{craftedWith}</color>";
            _detailsText.text = details;
        }

        private void RefreshCosts(FurnitureItemData furnitureItemData)
        {
            foreach (var displayedCost in _displayedCosts)
            {
                Destroy(displayedCost.gameObject);
            }
            _displayedCosts.Clear();

            var costs = furnitureItemData.GetResourceCosts();
            foreach (var costAmount in costs)
            {
                var cost = Instantiate(_costPrefab, _costsParent);
                cost.Init(costAmount);
                _displayedCosts.Add(cost);
            }
        }

        private void StartPlanningFurniture(FurnitureItemData furnitureItemData)
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, furnitureItemData.ItemName);
            Spawner.Instance.PlanFurniture(furnitureItemData);
        }
    }
}
