// using System;
// using Buildings;
// using Buildings.Building_Panels;
// using UnityEngine;
//
// namespace HUD
// {
//     public class BuildingDetailsUI : MonoBehaviour
//     {
//         [SerializeField] private BuildingPanel _panel;
//
//         private Building _building;
//
//         private void Start()
//         {
//             Hide();
//         }
//
//         public void Show(Building building)
//         {
//             this._building = building;
//             
//             _building.OnShowDetails();
//             _panel.gameObject.SetActive(true);
//             _panel.Init(_building);
//         }
//
//         public void Hide()
//         {
//             if (_building != null)
//             {
//                 _building.OnHideDetails();
//                 _building = null;
//             }
//             
//             _panel.Hide();
//             _panel.gameObject.SetActive(false);
//         }
//     }
// }
