// using System.Collections.Generic;
// using System.Linq;
// using ScriptableObjects;
// using UnityEngine;
//
// namespace Buildings.Building_Panels
// {
//     public class FurnitureBuildingPanel : MonoBehaviour
//     {
//         [SerializeField] private Transform _furnitureOptionParent;
//         [SerializeField] private BuildingFurnitureOption _optionPrefab;
//         [SerializeField] private Transform _furnitureCatergoryParent;
//         [SerializeField] private BuildingFurnitureCatergoryOption _catergoryOptionPrefab;
//         [SerializeField] private BuildingFurnitureSelection _buildingFurnitureSelection;
//
//         private Building _building;
//         private Dictionary<FurnitureCatergory, List<FurnitureItemData>> _furnitureDictionary =
//             new Dictionary<FurnitureCatergory, List<FurnitureItemData>>();
//         private FurnitureCatergory _curCatergory;
//         private List<BuildingFurnitureCatergoryOption> _displayedCatergoryOptions =
//             new List<BuildingFurnitureCatergoryOption>();
//         private List<BuildingFurnitureOption> _displayedFurnitureOptions = new List<BuildingFurnitureOption>();
//         private BuildingFurnitureOption _curBuildingFurnitureOption;
//
//         public void Init(Building building)
//         {
//             _building = building;
//             
//             RefreshFurnitureDictionary();
//             RefreshCatergories();
//         }
//
//         private void RefreshCatergories()
//         {
//             foreach (var option in _displayedCatergoryOptions)
//             {
//                 Destroy(option.gameObject);
//             }
//             _displayedCatergoryOptions.Clear();
//             
//             var catergories = _furnitureDictionary.Keys.ToList();
//             foreach (var catergory in catergories)
//             {
//                 var catOption = Instantiate(_catergoryOptionPrefab, _furnitureCatergoryParent);
//                 catOption.Init(catergory, OnCatergoryPressed);
//                 _displayedCatergoryOptions.Add(catOption);
//             }
//             
//             AssignCatergory(catergories[0]);
//         }
//
//         private void OnCatergoryPressed(FurnitureCatergory catergory, BuildingFurnitureCatergoryOption catergoryOptionBtn)
//         {
//             AssignCatergory(catergory);
//         }
//
//         private void AssignCatergory(FurnitureCatergory newCatergory)
//         {
//             foreach (var displayedCatergory in _displayedCatergoryOptions)
//             {
//                 if (displayedCatergory.Catergory == _curCatergory)
//                 {
//                     displayedCatergory.DisplaySelected(false);
//                 }
//
//                 if (displayedCatergory.Catergory == newCatergory)
//                 {
//                     displayedCatergory.DisplaySelected(true);
//                 }
//             }
//
//             _curCatergory = newCatergory;
//             RefreshFurnitureOptions(_curCatergory);
//         }
//
//         private void RefreshFurnitureOptions(FurnitureCatergory catergory)
//         {
//             foreach (var furnitureOption in _displayedFurnitureOptions)
//             {
//                 Destroy(furnitureOption.gameObject);
//             }
//             _displayedFurnitureOptions.Clear();
//
//             List<FurnitureItemData> furnitureOptions = new List<FurnitureItemData>();
//             foreach (var kvp in _furnitureDictionary)
//             {
//                 if (kvp.Key == catergory)
//                 {
//                     foreach (var option in kvp.Value)
//                     {
//                         furnitureOptions.Add(option);
//                     }
//                 }
//             }
//
//             foreach (var furnitureOption in furnitureOptions)
//             {
//                 var option = Instantiate(_optionPrefab, _furnitureOptionParent);
//                 option.Init(furnitureOption, FurnitureOptionSelected);
//                 _displayedFurnitureOptions.Add(option);
//             }
//             
//             _displayedFurnitureOptions[0].OnOptionPressed();
//         }
//
//         private void FurnitureOptionSelected(FurnitureItemData furnitureItemData, BuildingFurnitureOption optionPressed)
//         {
//             if (_curBuildingFurnitureOption != null)
//             {
//                 _curBuildingFurnitureOption.DisplaySelector(false);
//             }
//
//             _curBuildingFurnitureOption = optionPressed;
//             _curBuildingFurnitureOption.DisplaySelector(true);
//
//             _buildingFurnitureSelection.Init(furnitureItemData);
//         }
//
//         private void RefreshFurnitureDictionary()
//         {
//             // _furnitureDictionary.Clear();
//             // var allFurniture = _building.BuildingData.AllowedFurniture;
//             // foreach (var furniture in allFurniture)
//             // {
//             //     if (_furnitureDictionary.ContainsKey(furniture.Catergory))
//             //     {
//             //         _furnitureDictionary[furniture.Catergory].Add(furniture);
//             //     }
//             //     else
//             //     {
//             //         List<FurnitureItemData> newFurnList = new List<FurnitureItemData> { furniture };
//             //         _furnitureDictionary.Add(furniture.Catergory, newFurnList);
//             //     }
//             // }
//         }
//
//         public void Close()
//         {
//             gameObject.SetActive(false);
//         }
//     }
// }
