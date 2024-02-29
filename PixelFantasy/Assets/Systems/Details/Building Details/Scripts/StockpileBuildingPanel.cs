// using System.Collections.Generic;
// using ScriptableObjects;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class StockpileBuildingPanel : BuildingPanel
//     {
//         [SerializeField] private StockpileCategoryDetails _categoryDetailsPrefab;
//
//         private List<StockpileCategoryDetails> _displayedCategories = new List<StockpileCategoryDetails>();
//         
//         protected override void Show()
//         {
//             ClearDisplayedCategories();
//             
//             var validCategories = GetValidCategories();
//             foreach (var category in validCategories)
//             {
//                 var categoryDisplay = Instantiate(_categoryDetailsPrefab, transform);
//                 categoryDisplay.Init(category, _building, this);
//                 _displayedCategories.Add(categoryDisplay);
//             }
//         }
//         
//         protected override void Refresh()
//         {
//             foreach (var category in _displayedCategories)
//             {
//                 category.Refresh();
//             }
//         }
//         
//         public override void Hide()
//         {
//             base.Hide();
//             ClearDisplayedCategories();
//         }
//
//         private void ClearDisplayedCategories()
//         {
//             foreach (var displayedCategory in _displayedCategories)
//             {
//                 Destroy(displayedCategory.gameObject);
//             }
//             _displayedCategories.Clear();
//         }
//         
//         private List<EItemCategory> GetValidCategories()
//         {
//             var storages = _building.GetBuildingStorages();
//             List<EItemCategory> validCategories = new List<EItemCategory>();
//             foreach (var storage in storages)
//             {
//                 foreach (var category in storage.AcceptedCategories)
//                 {
//                     if (!validCategories.Contains(category))
//                     {
//                         validCategories.Add(category);
//                     }
//                 }
//             }
//
//             return validCategories;
//         }
//     }
// }
