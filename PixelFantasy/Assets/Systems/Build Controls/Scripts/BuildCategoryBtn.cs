// using System.Collections.Generic;
// using ScriptableObjects;
// using UnityEngine;
//
// namespace Systems.Build_Controls.Scripts
// {
//     public class BuildCategoryBtn : CategoryBtn
//     {
//         [SerializeField] private BuildOptionBtn _buildOptionBtnPrefab;
//         [SerializeField] private List<BuildingData> _buildOptions = new List<BuildingData>();
//
//         private List<BuildOptionBtn> _displayedBuildOptions = new List<BuildOptionBtn>();
//         
//         protected override void DisplayOptions()
//         {
//             base.DisplayOptions();
//             
//             foreach (var buildingData in _buildOptions)
//             {
//                 var buildOption = Instantiate(_buildOptionBtnPrefab, _optionsLayout.transform);
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
