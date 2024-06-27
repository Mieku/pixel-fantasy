using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class BuildStructureDetailsUI : MonoBehaviour
    {
        public enum EDetailsState
        {
            None,
            Wall,
            Door,
            Floor
        }
        
        [SerializeField] private List<DyeSettings> _colourOptions;
        [SerializeField] private List<WallSettings> _wallOptions;
        [SerializeField] private List<DoorSettings> _doorOptions;
        [SerializeField] private List<FloorSettings> _floorOptions;
        
        [SerializeField] private WallBuilder _wallBuilder;
        [SerializeField] private FloorBuilder _floorBuilder;
        
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private SelectionStructureDetails _currentSelection;
        [SerializeField] private GameObject _optionGroupSeperator;
        
        [SerializeField] private GameObject _colourOptionGroup;
        [SerializeField] private Transform _colourLayoutParent;
        [SerializeField] private TextMeshProUGUI _colourGroupHeader;
        [SerializeField] private ColourOptionBtn _colourOptionBtnPrefab;
        
        [SerializeField] private GameObject _materialOptionGroup;
        [SerializeField] private Transform _materialLayoutParent;
        [SerializeField] private MaterialOptionBtn _materialOptionBtnPrefab;
        
        [SerializeField] private GameObject _styleOptionGroup;
        [SerializeField] private Transform _styleLayoutParent;
        [SerializeField] private TextMeshProUGUI _styleGroupHeader;
        [SerializeField] private StyleOptionBtn _styleOptionBtnPrefab;
        
        private List<ColourOptionBtn> _displayedColourOptions = new List<ColourOptionBtn>();
        private List<MaterialOptionBtn> _displayedMaterialOptions = new List<MaterialOptionBtn>();
        private List<StyleOptionBtn> _displayedStyleOptions = new List<StyleOptionBtn>();

        private EDetailsState _state;
        private DyeSettings _currentColour;
        private StyleOption _currentStyleOption;

        public void Show()
        {
            gameObject.SetActive(true);
            HideCurrentSelection();
            RefreshLayout();
        }

        public void ShowForSpecificBuild(DoorSettings doorSettings)
        {
            Show();
            OnDoorPressed();
            DisplayCurrentDoorSelection(doorSettings);
        }
        
        public void ShowForSpecificBuild(WallSettings wallSettings)
        {
            Show();
            OnWallPressed();
            DisplayCurrentWallSelection(wallSettings);
        }
        
        public void ShowForSpecificBuild(FloorSettings floorSettings)
        {
            Show();
            OnFloorPressed();
            DisplayCurrentFloorSelection(floorSettings);
        }

        public void Hide()
        {
            _wallBuilder.CancelWallBuild();
            gameObject.SetActive(false);
            HideCurrentSelection();
        }

        #region Button Controls

        public void OnWallPressed()
        {
            Show();
            ShowCurrentSelection(EDetailsState.Wall);
        }
        
        public void OnDoorPressed()
        {
            Show();
            ShowCurrentSelection(EDetailsState.Door);
        }
        
        public void OnFloorPressed()
        {
            Show();
            ShowCurrentSelection(EDetailsState.Floor);
        }

        #endregion

        private void HideCurrentSelection()
        {
            _currentSelection.gameObject.SetActive(false);
            _state = EDetailsState.None;
        }

        private void ShowCurrentSelection(EDetailsState detailsState)
        {
            _currentSelection.gameObject.SetActive(true);
            _state = detailsState;
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            _wallBuilder.CancelWallBuild();
            _floorBuilder.CancelFloorBuild();
            
            switch (_state)
            {
                case EDetailsState.None:
                    HideCurrentSelection();
                    break;
                case EDetailsState.Wall:
                    _colourOptionGroup.SetActive(true);
                    _optionGroupSeperator.SetActive(true);
                    _styleOptionGroup.SetActive(false);
                    ShowColourOptions("Interior Wall", WallColourSelected);
                    ShowWallMaterialOptions(WallMaterialSelected);
                    break;
                case EDetailsState.Door:
                    _colourOptionGroup.SetActive(true);
                    _optionGroupSeperator.SetActive(true);
                    _styleOptionGroup.SetActive(false);
                    ShowColourOptions("Mat", DoorColourSelected);
                    ShowDoorMaterialOptions(DoorMaterialSelected);
                    break;
                case EDetailsState.Floor:
                    HideColourOptions();
                    _colourOptionGroup.SetActive(false);
                    _optionGroupSeperator.SetActive(true);
                    _styleOptionGroup.SetActive(true);
                    ShowFloorMaterialOptions(FloorMaterialSelected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ShowColourOptions(string header, Action<DyeSettings> onSelectedCallback)
        {
            _colourOptionBtnPrefab.gameObject.SetActive(false);
            
            HideColourOptions();

            _colourGroupHeader.text = header;

            foreach (var colourOption in _colourOptions)
            {
                var colourBtn = Instantiate(_colourOptionBtnPrefab, _colourLayoutParent);
                colourBtn.Init(colourOption, (btn, colour) =>
                {
                    HighlightColourBtn(btn);
                    onSelectedCallback.Invoke(colour);
                });
                colourBtn.gameObject.SetActive(true);
                _displayedColourOptions.Add(colourBtn);
            }
        }

        private void HideColourOptions()
        {
            foreach (var displayedColourOption in _displayedColourOptions)
            {
                Destroy(displayedColourOption.gameObject);
            }
            _displayedColourOptions.Clear();
        }

        private void HighlightColourBtn(ColourOptionBtn colourBtn)
        {
            foreach (var colourOption in _displayedColourOptions)
            {
                colourOption.RemoveHighlight();
            }
            colourBtn.ShowHighlight();
        }

        private void WallColourSelected(DyeSettings colour)
        {
            _currentColour = colour;
        }
        
        private void DoorColourSelected(DyeSettings colour)
        {
            
        }

        private void ShowWallMaterialOptions(Action<WallSettings> onSelectedCallback)
        {
            HideMaterialOptions();
            _currentColour = null;
            _materialOptionBtnPrefab.gameObject.SetActive(false);
            
            foreach (var wallOption in _wallOptions)
            {
                var optionBtn = Instantiate(_materialOptionBtnPrefab, _materialLayoutParent);
                optionBtn.Init(wallOption, wallOption.OptionIcon, wallOption.CraftRequirements, (btn, s) =>
                {
                    HighlightMaterialBtn(btn);
                    onSelectedCallback.Invoke((WallSettings) s);
                });
                optionBtn.gameObject.SetActive(true);
                _displayedMaterialOptions.Add(optionBtn);
            }
            
            // Select the first
            _displayedMaterialOptions[0].OnPressed();
        }

        private void HideMaterialOptions()
        {
            foreach (var option in _displayedMaterialOptions)
            {
                Destroy(option.gameObject);
            }
            _displayedMaterialOptions.Clear();
        }
        
        private void HighlightMaterialBtn(MaterialOptionBtn matBtn)
        {
            foreach (var option in _displayedMaterialOptions)
            {
                option.RemoveHighlight();
            }
            matBtn.ShowHighlight();
        }

        private void WallMaterialSelected(WallSettings settings)
        {
            if (_currentColour == null)
            {
                _displayedColourOptions[0].OnPressed();
            }
            
            DisplayCurrentWallSelection(settings);
        }

        private void DisplayCurrentWallSelection(WallSettings settings)
        {
            _currentSelection.ShowWallSelection(settings, _currentColour);
            RefreshLayout();
            
            _wallBuilder.BeginWallBuild(settings, _currentColour);
        }
        
        private void ShowDoorMaterialOptions(Action<DoorSettings> onSelectedCallback)
        {
            HideMaterialOptions();
            _currentColour = null;
            _materialOptionBtnPrefab.gameObject.SetActive(false);
            
            foreach (var doorOption in _doorOptions)
            {
                var optionBtn = Instantiate(_materialOptionBtnPrefab, _materialLayoutParent);
                optionBtn.Init(doorOption, doorOption.OptionIcon, doorOption.CraftRequirements, (btn, s) =>
                {
                    HighlightMaterialBtn(btn);
                    onSelectedCallback.Invoke((DoorSettings) s);
                });
                optionBtn.gameObject.SetActive(true);
                _displayedMaterialOptions.Add(optionBtn);
            }
            
            // Select the first
            _displayedMaterialOptions[0].OnPressed();
        }
        
        private void DoorMaterialSelected(DoorSettings settings)
        {
            if (_currentColour == null)
            {
                _displayedColourOptions[0].OnPressed();
            }
            
            DisplayCurrentDoorSelection(settings);
        }
        
        private void DisplayCurrentDoorSelection(DoorSettings settings)
        {
            _currentSelection.ShowDoorSelection(settings, _currentColour);
            RefreshLayout();
            
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildDoor, settings.DoorName);
            Spawner.Instance.PlanDoor(settings, _currentColour, () =>
            {
                DisplayCurrentDoorSelection(settings);
            });
        }
        
        private void ShowFloorMaterialOptions(Action<FloorSettings> onSelectedCallback)
        {
            HideMaterialOptions();
            _currentColour = null;
            _currentStyleOption = null;
            _materialOptionBtnPrefab.gameObject.SetActive(false);
            
            foreach (var floorOption in _floorOptions)
            {
                var optionBtn = Instantiate(_materialOptionBtnPrefab, _materialLayoutParent);
                optionBtn.Init(floorOption, floorOption.MaterialIcon, floorOption.CraftRequirements, (btn, s) =>
                {
                    HighlightMaterialBtn(btn);
                    onSelectedCallback.Invoke((FloorSettings) s);
                });
                optionBtn.gameObject.SetActive(true);
                _displayedMaterialOptions.Add(optionBtn);
            }
            
            // Select the first
            _displayedMaterialOptions[0].OnPressed();
        }
        
        private void FloorMaterialSelected(FloorSettings settings)
        {
            ShowStyleOptions("Styles", settings.StyleOptions.OfType<StyleOption>().ToList(), FloorStyleSelected);

            if (_currentStyleOption == null)
            {
                _displayedStyleOptions[0].OnPressed();
            }
            
            DisplayCurrentFloorSelection(settings);
        }
        
        private void DisplayCurrentFloorSelection(FloorSettings settings)
        {
            _currentSelection.ShowFloorSelection(settings, _currentStyleOption);
            RefreshLayout();
            
            _floorBuilder.BeginFloorBuild(settings, _currentStyleOption);
        }
        
        private void ShowStyleOptions(string header, List<StyleOption> options, Action<StyleOption> onSelectedCallback)
        {
            _styleOptionBtnPrefab.gameObject.SetActive(false);
            
            HideStyleOptions();

            _styleGroupHeader.text = header;

            foreach (var option in options)
            {
                var styleBtn = Instantiate(_styleOptionBtnPrefab, _styleLayoutParent);
                styleBtn.Init(option, (btn, pressedBtn) =>
                {
                    HighlightStyleBtn(btn);
                    onSelectedCallback.Invoke(pressedBtn);
                });
                styleBtn.gameObject.SetActive(true);
                _displayedStyleOptions.Add(styleBtn);
            }
        }

        private void HideStyleOptions()
        {
            foreach (var displayedStyleOption in _displayedStyleOptions)
            {
                Destroy(displayedStyleOption.gameObject);
            }
            _displayedStyleOptions.Clear();
        }

        private void FloorStyleSelected(StyleOption option)
        {
            _currentStyleOption = option;

            _floorBuilder.UpdateStyle(option);
        }
        
        private void HighlightStyleBtn(StyleOptionBtn styleBtn)
        {
            foreach (var option in _displayedStyleOptions)
            {
                option.RemoveHighlight();
            }
            styleBtn.ShowHighlight();
        }
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }
    }
}
