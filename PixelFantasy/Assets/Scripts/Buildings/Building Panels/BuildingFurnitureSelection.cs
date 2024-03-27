// using System.Collections.Generic;
// using Controllers;
// using Managers;
// using ScriptableObjects;
// using TMPro;
// using UnityEngine;
//
// namespace Buildings.Building_Panels
// {
//     public class BuildingFurnitureSelection : MonoBehaviour
//     {
//         [SerializeField] private TextMeshProUGUI _detailsText;
//         [SerializeField] private TextMeshProUGUI _productName;
//         [SerializeField] private Transform _varientsParent;
//         [SerializeField] private FurnitureVarientOption _furnitureVarientPrefab;
//         [SerializeField] private Transform _costsParent;
//         [SerializeField] private ResourceCost _costPrefab;
//
//         private FurnitureSettings _defaultFurnitureSettings;
//         private List<FurnitureVarientOption> _displayedVarientOptions = new List<FurnitureVarientOption>();
//         private FurnitureVarientOption _curSelectedVarientOption;
//         private List<ResourceCost> _displayedCosts = new List<ResourceCost>();
//         
//         public void Init(FurnitureSettings furnitureSettings)
//         {
//             _defaultFurnitureSettings = furnitureSettings;
//             DisplayVarientOptions();
//             
//             // Start by pressing the default
//             _displayedVarientOptions[0].VarientSelected();
//         }
//
//         private void DisplayVarientOptions()
//         {
//             foreach (var varientOption in _displayedVarientOptions)
//             {
//                 Destroy(varientOption.gameObject);
//             }
//             _displayedVarientOptions.Clear();
//             
//             // Start with the default
//             var option = Instantiate(_furnitureVarientPrefab, _varientsParent);
//             option.Init(_defaultFurnitureSettings, OnVarientSelected);
//             _displayedVarientOptions.Add(option);
//
//             // Then the rest
//             // foreach (var varient in _defaultFurnitureItemData.Varients)
//             // {
//             //     var varientOption = Instantiate(_furnitureVarientPrefab, _varientsParent);
//             //     varientOption.Init(varient, OnVarientSelected);
//             //     _displayedVarientOptions.Add(varientOption);
//             // }
//         }
//
//         private void OnVarientSelected(FurnitureSettings furnitureSettings, FurnitureVarientOption varientOption)
//         {
//             if (_curSelectedVarientOption != null)
//             {
//                 _curSelectedVarientOption.DisplaySelected(false);
//             }
//             _curSelectedVarientOption = varientOption;
//             _curSelectedVarientOption.DisplaySelected(true);
//             
//             DisplaySelectedOptionDetails(furnitureSettings);
//         }
//
//         private void DisplaySelectedOptionDetails(FurnitureSettings furnitureSettings)
//         {
//             _productName.text = furnitureSettings.ItemName;
//             RefreshDetails(furnitureSettings);
//             RefreshCosts(furnitureSettings);
//             StartPlanningFurniture(furnitureSettings);
//         }
//
//         private void RefreshDetails(FurnitureSettings furnitureSettings)
//         {
//             // string craftedWith = "";
//             // foreach (var option in furnitureItemData.RequiredCraftingTableOptions)
//             // {
//             //     craftedWith += $"{option.ItemName} ";
//             // }
//             //
//             // string details = $"Crafted with: <color=blue>{craftedWith}</color>";
//             // _detailsText.text = details;
//         }
//
//         private void RefreshCosts(FurnitureSettings furnitureSettings)
//         {
//             foreach (var displayedCost in _displayedCosts)
//             {
//                 Destroy(displayedCost.gameObject);
//             }
//             _displayedCosts.Clear();
//
//             var costs = furnitureSettings.CraftRequirements.GetMaterialCosts();
//             foreach (var costAmount in costs)
//             {
//                 var cost = Instantiate(_costPrefab, _costsParent);
//                 cost.Init(costAmount);
//                 _displayedCosts.Add(cost);
//             }
//         }
//
//         private void StartPlanningFurniture(FurnitureSettings furnitureSettings)
//         {
//             // Spawner.Instance.CancelInput();
//             // PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, furnitureItemData.ItemName);
//             // Spawner.Instance.PlanFurniture(furnitureItemData);
//         }
//     }
// }
