// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class UnderConstructionBuildingPanel : BuildingPanel
//     {
//         [SerializeField] private RemainingItemsDisplay _remainingItemsDisplayPrefab;
//         [SerializeField] private Transform _remainingItemsParent;
//         [SerializeField] private Image _workbarFill;
//         [SerializeField] private TextMeshProUGUI _workbarProgressText;
//         [SerializeField] private Toggle _prioritizeToggle;
//
//         private List<RemainingItemsDisplay> _displayedItems = new List<RemainingItemsDisplay>();
//         
//         protected override void Show()
//         {
//             Refresh();
//         }
//
//         protected override void Refresh()
//         {
//             ClearDisplayedItems();
//
//             var maxWork = _building.BuildingData.WorkCost;
//             var remainingWork = _building.RemainingWork;
//             var workDone = maxWork - remainingWork;
//             var workPercent = workDone / maxWork;
//
//             _workbarFill.fillAmount = workPercent;
//             _workbarProgressText.text = $"{workDone} / {maxWork}";
//
//             var materialsCost = _building.BuildingData.GetResourceCosts();
//             var remainingMaterialsCost = _building.GetRemainingMissingItems();
//
//             foreach (var cost in materialsCost)
//             {
//                 float currentAmount;
//                 var remainingCost = remainingMaterialsCost.Find(c => c.Item == cost.Item);
//                 if (remainingCost == null)
//                 {
//                     currentAmount = cost.Quantity;
//                 }
//                 else
//                 {
//                     currentAmount = cost.Quantity - remainingCost.Quantity;
//                 }
//
//                 var itemDisplay = Instantiate(_remainingItemsDisplayPrefab, _remainingItemsParent);
//                 itemDisplay.Init(cost.Item, currentAmount, cost.Quantity);
//                 _displayedItems.Add(itemDisplay);
//             }
//         }
//
//         public void OnPrioritizeConstructionToggled(bool value)
//         {
//             Debug.LogError($"Prioritize Toggle Not Built, value: {value}");
//         }
//
//         private void ClearDisplayedItems()
//         {
//             foreach (var displayedItem in _displayedItems)
//             {
//                 Destroy(displayedItem.gameObject);
//             }
//             _displayedItems.Clear();
//         }
//     }
// }
