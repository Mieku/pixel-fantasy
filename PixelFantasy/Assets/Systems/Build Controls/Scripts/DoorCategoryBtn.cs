// using System.Collections.Generic;
// using Systems.Buildings.Scripts;
// using UnityEngine;
//
// namespace Systems.Build_Controls.Scripts
// {
//     public class DoorCategoryBtn : CategoryBtn
//     {
//         [SerializeField] private DoorOptionBtn _doorOptionBtnPrefab;
//         [SerializeField] private List<DoorSettings> _buildOptions = new List<DoorSettings>();
//
//         private List<DoorOptionBtn> _displayedBuildOptions = new List<DoorOptionBtn>();
//         
//         protected override void DisplayOptions()
//         {
//             base.DisplayOptions();
//             
//             foreach (var buildingData in _buildOptions)
//             {
//                 var buildOption = Instantiate(_doorOptionBtnPrefab, _optionsLayout.transform);
//                 buildOption.Init(buildingData, this);
//                 _displayedBuildOptions.Add(buildOption);
//             }
//         }
//
//         protected override void HideOptions()
//         {
//             base.HideOptions();
//             
//             foreach (var displayedBuildOption in _displayedBuildOptions)
//             {
//                 Destroy(displayedBuildOption.gameObject);
//             }
//             _displayedBuildOptions.Clear();
//         }
//     }
// }
