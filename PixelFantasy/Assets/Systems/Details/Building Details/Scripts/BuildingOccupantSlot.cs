// using System;
// using System.Collections.Generic;
// using Buildings;
// using Characters;
// using HUD.Tooltip;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Systems.Details.Building_Details.Scripts
// {
//     public class BuildingOccupantSlot : MonoBehaviour
//     {
//         [SerializeField] private Image _portraitImage;
//         [SerializeField] private TooltipTrigger _tooltip;
//         [SerializeField] private Canvas _occupantSelectCanvas;
//         [SerializeField] private Color _defaultTextColour;
//         
//         [Header("Remove Current")]
//         [SerializeField] private GameObject _removeCurrentHandle;
//         [SerializeField] private TextMeshProUGUI _removeCurrentName;
//         [SerializeField] private Color _removeCurrentHoverColour;
//         [SerializeField] private TooltipTrigger _removeCurrentTooltip;
//         
//         [Header("Options")]
//         [SerializeField] private OccupantOption _occupantOptionPrefab;
//         [SerializeField] private Transform _occupantOptionParent;
//         [SerializeField] private GameObject _noneAvailableHandle;
//
//         private Kinling _assignedKinling;
//         private Building _linkedBuilding;
//         private Action<Kinling, Kinling> _onKinlingSelected;
//         private List<OccupantOption> _displayedOptions = new List<OccupantOption>();
//
//         public void Init(Kinling assignedKinling, Building building, Action<Kinling, Kinling> onKinlingSelected)
//         {
//             _assignedKinling = assignedKinling;
//             _linkedBuilding = building;
//             _onKinlingSelected = onKinlingSelected;
//
//             if (_assignedKinling != null)
//             {
//                 _portraitImage.gameObject.SetActive(true);
//                 _tooltip.Header = _assignedKinling.FullName;
//                 _tooltip.Content =
//                     _assignedKinling.Skills.GetSkillList(_linkedBuilding.BuildingData.RelevantSkillType);
//             }
//             else
//             {
//                 _portraitImage.gameObject.SetActive(false);
//                 _tooltip.Header = "Assign Kinling";
//                 _tooltip.Content = "";
//             }
//         }
//
//         private void ShowSelect()
//         {
//             _occupantSelectCanvas.gameObject.SetActive(true);
//             ClearDisplayedOptions();
//             
//             if (_assignedKinling != null)
//             {
//                 _removeCurrentHandle.SetActive(true);
//                 _removeCurrentName.text = _assignedKinling.FullName;
//                 _removeCurrentTooltip.Header = _assignedKinling.FullName;
//                 _removeCurrentTooltip.Content = _assignedKinling.Skills.GetSkillList(_linkedBuilding.BuildingData.RelevantSkillType);
//             }
//             else
//             {
//                 _removeCurrentHandle.SetActive(false);
//             }
//
//             List<Kinling> potentialOccupants = _linkedBuilding.GetPotentialOccupants();
//             if (potentialOccupants.Count == 0)
//             {
//                 _noneAvailableHandle.SetActive(true);
//             }
//             else
//             {
//                 _noneAvailableHandle.SetActive(false);
//                 foreach (var potentialOccupant in potentialOccupants)
//                 {
//                     var option = Instantiate(_occupantOptionPrefab, _occupantOptionParent);
//                     option.Init(potentialOccupant, _linkedBuilding.BuildingData.RelevantSkillType, OnOptionSelected);
//                     _displayedOptions.Add(option);
//                 }
//             }
//         }
//
//         private void HideSelect()
//         {
//             ClearDisplayedOptions();
//             _occupantSelectCanvas.gameObject.SetActive(false);
//         }
//
//         private void ClearDisplayedOptions()
//         {
//             foreach (var displayedOption in _displayedOptions)
//             {
//                 Destroy(displayedOption.gameObject);
//             }
//             _displayedOptions.Clear();
//         }
//
//         public void OnSlotPressed()
//         {
//             ShowSelect();
//         }
//
//         public void OnBlockerPressed()
//         {
//             HideSelect();
//         }
//
//         public void OnRemoveCurrentPressed()
//         {
//             HideSelect();
//             _onKinlingSelected.Invoke(null, _assignedKinling);
//         }
//
//         private void OnOptionSelected(Kinling selectedKinling)
//         {
//             HideSelect();
//             _onKinlingSelected.Invoke(selectedKinling, _assignedKinling);
//         }
//
//         public void OnRemoveCurrentHover(bool isHovering)
//         {
//             if (isHovering)
//             {
//                 _removeCurrentName.color = _removeCurrentHoverColour;
//             }
//             else
//             {
//                 _removeCurrentName.color = _defaultTextColour;
//             }
//         }
//     }
// }
