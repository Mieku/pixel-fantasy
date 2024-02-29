// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Characters;
// using Managers;
// using ScriptableObjects;
// using UnityEngine;
//
// namespace Buildings.Building_Panels
// {
//     public class SelectKinlingSidePanel : MonoBehaviour
//     {
//         [SerializeField] private GameObject _noneAvailableMsg;
//         [SerializeField] private BuildingKinlingSelect _kinlingSelectPrefab;
//         [SerializeField] private Transform _kinlingSelectParent;
//         
//         private Building _building;
//         private Kinling _currentKinling;
//         private Action<Kinling, Kinling> _kinlingSelectedCallback;
//         private List<BuildingKinlingSelect> _displayedSelectors = new List<BuildingKinlingSelect>();
//         
//         public void Init(Building building, Kinling currentKinling, Action<Kinling, Kinling> kinlingSelectedCallback)
//         {
//             _building = building;
//             _currentKinling = currentKinling;
//             _kinlingSelectedCallback = kinlingSelectedCallback;
//             
//             DisplayAvailableKinlings();
//         }
//
//         private void DisplayAvailableKinlings()
//         {
//             RemoveCurrentlyDisplayedSelectors();
//             
//             if (_currentKinling != null)
//             {
//                 CreateKinlingSelector(_currentKinling);
//             }
//
//             if (_building.BuildingType == BuildingType.Home)
//             {
//                 var homelessKinlings = KinlingsManager.Instance.HomelessKinlings;
//                 foreach (var homelessKinling in homelessKinlings)
//                 {
//                     CreateKinlingSelector(homelessKinling);
//                 }
//             }
//             else
//             {
//                 var unemployedKinlings = KinlingsManager.Instance.UnemployedKinlings;
//                 foreach (var unemployedKinling in unemployedKinlings)
//                 {
//                     CreateKinlingSelector(unemployedKinling);
//                 }
//             }
//             
//             // Show no kinling available msg if there are none
//             _noneAvailableMsg.SetActive(_displayedSelectors.Count == 0);
//         }
//
//         private void RemoveCurrentlyDisplayedSelectors()
//         {
//             foreach (var displayedSelector in _displayedSelectors)
//             {
//                 Destroy(displayedSelector.gameObject);
//             }
//             _displayedSelectors.Clear();
//         }
//
//         private void CreateKinlingSelector(Kinling kinling)
//         {
//             var selector = Instantiate(_kinlingSelectPrefab, _kinlingSelectParent);
//             _displayedSelectors.Add(selector);
//
//             selector.Init(kinling, OnKinlingSelected);
//         }
//
//         private void OnKinlingSelected(Kinling selectedKinling)
//         {
//             _kinlingSelectedCallback.Invoke(selectedKinling, _currentKinling);
//         }
//
//         public void CloseBtnPressed()
//         {
//             _kinlingSelectedCallback.Invoke(null, _currentKinling);
//         }
//     }
// }
