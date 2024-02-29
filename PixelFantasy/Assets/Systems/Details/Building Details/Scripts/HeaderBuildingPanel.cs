// using HUD.Tooltip;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class HeaderBuildingPanel : BuildingPanel
//     {
//         [SerializeField] private TextMeshProUGUI _buildingNameText;
//         [SerializeField] private TextMeshProUGUI _detailsText;
//         [SerializeField] private Color _positiveGreen;
//         [SerializeField] private Color _negativeRed;
//         [SerializeField] private GameObject _upkeepIncomeHandle;
//         [SerializeField] private TextMeshProUGUI _upkeepIncomeText;
//         [SerializeField] private Image _upkeepIncomeIndicator;
//         [SerializeField] private Sprite _upkeepIcon;
//         [SerializeField] private Sprite _IncomeIcon;
//         [SerializeField] private Image _durabilityFillbar;
//         [SerializeField] private TextMeshProUGUI _durabilityAmountText;
//         [SerializeField] private TooltipTrigger _upkeepIncomeTooltip;
//          
//         protected override void Show()
//         {
//             _buildingNameText.text = _building.BuildingName;
//             Refresh();
//         }
//
//         protected override void Refresh()
//         {
//             RefreshDurability();
//             RefreshIncomeUpkeep();
//             RefreshDetails();
//         }
//
//         private void RefreshDurability()
//         {
//             var curDurability = _building.CurrentDurability;
//             var maxDurability = _building.BuildingData.MaxDurability;
//             float percentage = curDurability / maxDurability;
//             _durabilityFillbar.fillAmount = percentage;
//             _durabilityAmountText.text = $"{curDurability} / {maxDurability}";
//         }
//
//         private void RefreshIncomeUpkeep()
//         {
//             var upkeep = _building.DailyUpkeep();
//             if (upkeep > 0)
//             {
//                 // Show as upkeep
//                 _upkeepIncomeHandle.SetActive(true);
//                 _upkeepIncomeText.color = _negativeRed;
//                 _upkeepIncomeIndicator.color = _negativeRed;
//                 _upkeepIncomeIndicator.sprite = _upkeepIcon;
//                 _upkeepIncomeText.text = $"{upkeep}";
//                 _upkeepIncomeTooltip.Header = $"{upkeep} Upkeep Daily";
//             }
//             else if (upkeep < 0)
//             {
//                 // Show as income
//                 _upkeepIncomeHandle.SetActive(true);
//                 _upkeepIncomeText.color = _positiveGreen;
//                 _upkeepIncomeIndicator.color = _positiveGreen;
//                 _upkeepIncomeIndicator.sprite = _IncomeIcon;
//                 _upkeepIncomeText.text = $"{Mathf.Abs(upkeep)}";
//                 _upkeepIncomeTooltip.Header = $"{Mathf.Abs(upkeep)} Income Daily";
//             }
//             else
//             {
//                 // Don't Show
//                 _upkeepIncomeHandle.SetActive(false);
//             }
//         }
//
//         private void RefreshDetails()
//         {
//             if (_building.BuildingNotes.Count == 0)
//             {
//                 _detailsText.text = _building.BuildingData.ConstructionDetails;
//             }
//             else
//             {
//                 string details = "";
//                 foreach (var note in _building.BuildingNotes)
//                 {
//                     if (note.IsPositive)
//                     {
//                         details += $"<color={Helper.ColorToHex(_positiveGreen)}>{note.Note}</color>";
//                     }
//                     else
//                     {
//                         details += $"<color={Helper.ColorToHex(_negativeRed)}>{note.Note}</color>";
//                     }
//                     
//                     if (note != _building.BuildingNotes[^1])
//                     {
//                         details += "\n";
//                     }
//                 }
//
//                 _detailsText.text = details;
//             }
//         }
//     }
// }
