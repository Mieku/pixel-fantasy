// using System;
// using Buildings;
// using Controllers;
// using HUD.Tooltip;
// using Systems.Currency.Scripts;
// using Systems.Notifications.Scripts;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class FooterBuildingPanel : BuildingPanel
//     {
//         [SerializeField] private Button _buildBtn;
//         [SerializeField] private Button _deconstructBtn;
//         [SerializeField] private Button _repairBtn;
//         [SerializeField] private Button _moveBtn;
//         [SerializeField] private Button _selectBtn;
//         [SerializeField] private Button _pauseBtn;
//         [SerializeField] private TextMeshProUGUI _buildCostText;
//         [SerializeField] private TooltipTrigger _buildTooltip;
//         [SerializeField] private TooltipTrigger _deconstructTooltip;
//
//         [SerializeField] private Image _deconstructBtnImg;
//         [SerializeField] private Image _moveBtnImg;
//         [SerializeField] private Image _repairBtnImg;
//         [SerializeField] private Sprite _redBtnDefault, _redBtnActive;
//         [SerializeField] private Sprite _purpleBtnDefault, _purpleBtnActive;
//
//         private EBuildingDetailsState _detailsState;
//         
//         protected override void Show()
//         {
//             DetermineBuildingState();
//         }
//
//         protected override void Refresh()
//         {
//             DetermineBuildingState();
//         }
//
//         private void DetermineBuildingState()
//         {
//             // Being Moved?
//             if (_building.IsBuildingMoving)
//             {
//                 _detailsState = EBuildingDetailsState.Moving;
//             }
//             // Deconstructing?
//             else if (_building.IsDeconstructing)
//             {
//                 _detailsState = EBuildingDetailsState.Deconstructing;
//             }
//             // Being Planned?
//             else if (_building.State == Building.BuildingState.Planning)
//             {
//                 _detailsState = EBuildingDetailsState.Planning;
//             }
//             // In Construction?
//             else if (_building.State == Building.BuildingState.Construction)
//             {
//                 _detailsState = EBuildingDetailsState.InConstruction;
//             }
//             // If none of above, is built
//             else
//             {
//                 _detailsState = EBuildingDetailsState.Built;
//             }
//             
//             DetermineButtonsToShow();
//         }
//
//         private void DetermineButtonsToShow()
//         {
//             // Probably going to remove these btns
//             _pauseBtn.gameObject.SetActive(false);
//             _moveBtn.gameObject.SetActive(false);
//             
//             _moveBtnImg.sprite = _purpleBtnDefault;
//             _deconstructBtnImg.sprite = _redBtnDefault;
//
//             switch (_detailsState)
//             {
//                 case EBuildingDetailsState.Planning:
//                     _buildBtn.gameObject.SetActive(true);
//                     CheckCanBuild();
//                     
//                     _deconstructBtn.gameObject.SetActive(true);
//                     _deconstructBtn.interactable = true;
//                     RefreshDeconstruct();
//                     
//                     _repairBtn.gameObject.SetActive(false);
//                     
//                     //_moveBtn.gameObject.SetActive(true);
//                     
//                     _selectBtn.gameObject.SetActive(true);
//                     break;
//                 case EBuildingDetailsState.Built:
//                     _buildBtn.gameObject.SetActive(false);
//                     
//                     _deconstructBtn.gameObject.SetActive(true);
//                     _deconstructBtn.interactable = true;
//                     RefreshDeconstruct();
//                     
//                     _repairBtn.gameObject.SetActive(true);
//                     CheckAllowRepair();
//                     
//                     //_moveBtn.gameObject.SetActive(true);
//                     
//                     _selectBtn.gameObject.SetActive(true);
//                     break;
//                 case EBuildingDetailsState.Moving:
//                     _buildBtn.gameObject.SetActive(false);
//                     
//                     _deconstructBtn.gameObject.SetActive(true);
//                     _deconstructBtn.interactable = false;
//                     RefreshDeconstruct();
//                     
//                     _repairBtn.gameObject.SetActive(false);
//                     
//                     // _moveBtn.gameObject.SetActive(true);
//                     // _moveBtnImg.sprite = _purpleBtnActive;
//                     
//                     _selectBtn.gameObject.SetActive(true);
//                     break;
//                 case EBuildingDetailsState.Deconstructing:
//                     _buildBtn.gameObject.SetActive(false);
//                     
//                     _deconstructBtn.gameObject.SetActive(true);
//                     _deconstructBtn.interactable = true;
//                     _deconstructBtnImg.sprite = _redBtnActive;
//                     RefreshDeconstruct();
//                     
//                     _repairBtn.gameObject.SetActive(false);
//                     
//                     // _moveBtn.gameObject.SetActive(false);
//                     
//                     _selectBtn.gameObject.SetActive(true);
//                     break;
//                 case EBuildingDetailsState.InConstruction:
//                     _buildBtn.gameObject.SetActive(false);
//                     
//                     _deconstructBtn.gameObject.SetActive(true);
//                     _deconstructBtn.interactable = true;
//                     _deconstructBtnImg.sprite = _redBtnDefault;
//                     RefreshDeconstruct();
//                     
//                     _repairBtn.gameObject.SetActive(false);
//                     
//                     // _moveBtn.gameObject.SetActive(false);
//                     
//                     _selectBtn.gameObject.SetActive(true);
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//         }
//
//         private void CheckAllowRepair()
//         {
//             var curDurability = _building.CurrentDurability;
//             var maxDurability = _building.BuildingData.MaxDurability;
//             bool canRepair = Math.Abs(curDurability - maxDurability) > 0.01;
//             _repairBtn.interactable = canRepair;
//
//             if (_building.RepairsRequested)
//             {
//                 _repairBtnImg.sprite = _purpleBtnActive;
//             }
//             else
//             {
//                 _repairBtnImg.sprite = _purpleBtnDefault;
//             }
//         }
//
//         private void CheckCanBuild()
//         {
//             var price = _building.BuildingData.Price;
//             bool buildReqsMet = _building.AreBuildRequirementsMet;
//             bool canAfford = CurrencyManager.Instance.CanAfford(price);
//             
//             // Price Label
//             if (canAfford)
//             {
//                 _buildCostText.text = $"<sprite name=\"Coins\">{price}";
//             }
//             else
//             {
//                 _buildCostText.text = $"<sprite name=\"Coins\"><color=#FF6056>{price}</color>";
//             }
//
//             if (!buildReqsMet)
//             {
//                 _buildBtn.interactable = false;
//                 _buildTooltip.Header = "Build requirements are not met";
//             } 
//             else if (!canAfford)
//             {
//                 _buildBtn.interactable = false;
//                 _buildTooltip.Header = "Can't afford";
//             }
//             else
//             {
//                 _buildBtn.interactable = true;
//                 _buildTooltip.Header = "Build";
//             }
//         }
//
//         private void RefreshDeconstruct()
//         {
//             switch (_detailsState)
//             {
//                 case EBuildingDetailsState.Planning:
//                     _deconstructTooltip.Header = "Cancel planning";
//                     _deconstructTooltip.Content = "";
//                     break;
//                 case EBuildingDetailsState.Built:
//                     _deconstructTooltip.Header = "Deconstruct";
//                     _deconstructTooltip.Content = "";
//                     break;
//                 case EBuildingDetailsState.Moving:
//                     _deconstructTooltip.Header = "Can't deconstruct while moving";
//                     _deconstructTooltip.Content = "";
//                     break;
//                 case EBuildingDetailsState.Deconstructing:
//                     _deconstructTooltip.Header = "Cancel deconstructing";
//                     _deconstructTooltip.Content = "";
//                     break;
//                 case EBuildingDetailsState.InConstruction:
//                     _deconstructTooltip.Header = "Cancel Construction";
//                     _deconstructTooltip.Content = "Refunds coins, not materials";
//                     break;
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//         }
//
//         public enum EBuildingDetailsState
//         {
//             Planning,
//             Built,
//             Moving,
//             Deconstructing,
//             InConstruction,
//         }
//
//         #region Button Hooks
//
//         public void OnBuildPressed()
//         {
//             var buildAccepted = _building.SetBuild();
//             if (!buildAccepted)
//             {
//                 NotificationManager.Instance.Toast("Could Not Afford Building");
//             }
//             
//             DetermineBuildingState();
//         }
//
//         public void OnDeconstructPressed()
//         {
//             if (_building.State is Building.BuildingState.Planning or Building.BuildingState.Construction)
//             {
//                 // Cancel Construction
//                 _building.CancelBuilding();
//             }
//             else if (_building.State == Building.BuildingState.Built)
//             {
//                 _building.ToggleDeconstruct();
//                 DetermineBuildingState();
//             }
//         }
//
//         public void OnRepairPressed()
//         {
//             _building.RequestRepairs();
//             
//             DetermineBuildingState();
//         }
//
//         public void OnMovePressed()
//         {
//             _building.ToggleMoveBuilding();
//             
//             DetermineBuildingState();
//         }
//         
//         public void OnSelectPressed()
//         {
//             var lookPos = _building.UseagePosition(Vector2.zero);
//             if (lookPos != null && Camera.main != null)
//             {
//                 CameraManager.Instance.LookAtPosition((Vector2)lookPos);
//             }
//         }
//
//         public void OnPausePressed()
//         {
//             
//         }
//
//         #endregion
//         
//     }
// }
