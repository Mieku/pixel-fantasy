// using ScriptableObjects;
// using UnityEngine;
//
// namespace Systems.Build_Controls.Scripts
// {
//     public class RoadsSubCategoryBtn : SubCategoryBtn
//     {
//         [SerializeField] private RoadOptionBtn _roadOptionPrefab;
//
//         private RoadSettings _roadSettings;
//         private RoadsCategoryBtn _categoryBtn;
//
//         public void Init(RoadSettings roadSettings, RoadsCategoryBtn categoryBtn)
//         {
//             _roadSettings = roadSettings;
//             _categoryBtn = categoryBtn;
//             _icon.sprite = _roadSettings.RoadOptions[0].OptionIcon;
//         }
//         
//         protected override void PopulateOptions()
//         {
//             foreach (var option in _roadSettings.RoadOptions)
//             {
//                 var optionDisplay = Instantiate(_roadOptionPrefab, _optionsLayout.transform);
//                 optionDisplay.Init(option, _roadSettings, this);
//                 _displayedOptions.Add(optionDisplay);
//             }
//         }
//
//         protected override void ButtonActivated()
//         {
//             base.ButtonActivated();
//
//             _categoryBtn.SelectSubCategoryBtn(this);
//             _displayedOptions[0].OnPressed();
//         }
//
//         protected override void ButtonDeactivated()
//         {
//             base.ButtonDeactivated();
//             
//             _categoryBtn.UnselectSubCategoryBtn();
//         }
//     }
// }
