using System;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class BuildingPanel : MonoBehaviour
    {
        private enum PanelState
        {
            General,
            Zoning,
            Furniture,
            Production,
            Exterior,
        }
        
        [Header("Control Buttons")]
        [SerializeField] private Button _generalBtn;
        [SerializeField] private Button _deconstructBtn;
        [SerializeField] private Button _moveBtn;
        [SerializeField] private Button _zoningBtn;
        [SerializeField] private Button _furnitureBtn;
        [SerializeField] private Button _productionBtn;
        [SerializeField] private Button _exteriorBtn;
        
        [Header("Content Panels")]
        [SerializeField] private GeneralBuildingPanel _generalPanel;
        [SerializeField] private ZoningBuildingPanel _zoningPanel;
        [SerializeField] private FurnitureBuildingPanel _furniturePanel;
        [SerializeField] private ProductionBuildingPanel _productionPanel;
        [SerializeField] private ExteriorBuildingPanel _exteriorPanel;

        [Header("Misc")]
        [SerializeField] private TMP_InputField _buildingNameIF;
        [SerializeField] private Color _btnIconColourSelected;
        [SerializeField] private Color _btnIconColourUnselected;
        
        private Building _building;
        private PanelState _state;
        
        public void Init(Building building)
        {
            _building = building;
            ShowReleventBtns();
            _buildingNameIF.text = _building.BuildingName;
            SetState(PanelState.General);
        }

        public void Hide()
        {
            // if (_building != null)
            // {
            //     _building.HideCraftableFurniture();
            // }
        }

        public void OnBuildingNameChanged()
        {
            string newValue = _buildingNameIF.text;
            if (!newValue.IsNullOrWhitespace())
            {
                _building.BuildingName = newValue;
            } 
        }

        private void SetState(PanelState state)
        {
            DeactivateAllControlBtns();

            HideAllContent();
            
            _state = state;
            switch (_state)
            {
                case PanelState.General:
                    ChangeBtnIconColour(_generalBtn, _btnIconColourSelected);
                    _generalPanel.gameObject.SetActive(true);
                    _generalPanel.Init(_building);
                    break;
                case PanelState.Zoning:
                    ChangeBtnIconColour(_zoningBtn, _btnIconColourSelected);
                    _zoningPanel.gameObject.SetActive(true);
                    _zoningPanel.Init(_building);
                    break;
                case PanelState.Furniture:
                    ChangeBtnIconColour(_furnitureBtn, _btnIconColourSelected);
                    _furniturePanel.gameObject.SetActive(true);
                    _furniturePanel.Init(_building);
                    break;
                case PanelState.Production:
                    ChangeBtnIconColour(_productionBtn, _btnIconColourSelected);
                    _productionPanel.Init(_building);
                    break;
                case PanelState.Exterior:
                    ChangeBtnIconColour(_exteriorBtn, _btnIconColourSelected);
                    _exteriorPanel.gameObject.SetActive(true);
                    _exteriorPanel.Init(_building);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HideAllContent()
        {
            _generalPanel.Close();
            _furniturePanel.Close();
            _productionPanel.Close();
            
            _zoningPanel.gameObject.SetActive(false);
            _exteriorPanel.gameObject.SetActive(false);
        }

        private void ShowReleventBtns()
        {
            // TODO: Detect which ones are actually needed
            
            // Show all btns
            _generalBtn.gameObject.SetActive(true);
            _zoningBtn.gameObject.SetActive(true);
            
            _exteriorBtn.gameObject.SetActive(true);

            // Production buildings only show the production options
            var prodBuilding = _building as ProductionBuilding;
            if (prodBuilding != null)
            {
                _productionBtn.gameObject.SetActive(true);
            }
            else
            {
                _productionBtn.gameObject.SetActive(false);
            }

            // Don't show furniture if there are none
            if (_building.BuildingData.AllowedFurniture.Count > 0)
            {
                _furnitureBtn.gameObject.SetActive(true);
            }
            else
            {
                _furnitureBtn.gameObject.SetActive(false);
            }
        }

        private void DeactivateAllControlBtns()
        {
            ChangeBtnIconColour(_generalBtn, _btnIconColourUnselected);
            ChangeBtnIconColour(_zoningBtn, _btnIconColourUnselected);
            ChangeBtnIconColour(_furnitureBtn, _btnIconColourUnselected);
            ChangeBtnIconColour(_productionBtn, _btnIconColourUnselected);
            ChangeBtnIconColour(_exteriorBtn, _btnIconColourUnselected);
        }

        private void ChangeBtnIconColour(Button button, Color newColour)
        {
            var icon = button.transform.GetChild(0).gameObject.GetComponent<Image>();
            if (icon != null)
            {
                icon.color = newColour;
            }
        }
        
        #region Button Hooks

        public void OnMovePressed()
        {
            Debug.LogError("Move Not Implemented Yet!");
        }

        public void OnDeconstructPressed()
        {
            Debug.LogError("Deconstruct Not Implemented Yet!");
        }

        public void OnGeneralPressed()
        {
            SetState(PanelState.General);
        }

        public void OnZoningPressed()
        {
            SetState(PanelState.Zoning);
        }

        public void OnFurniturePressed()
        {
            //SetState(PanelState.Furniture);
            var isToggled = _building.ToggleShowCraftableFurniture();
            if (isToggled)
            {
                ChangeBtnIconColour(_furnitureBtn, _btnIconColourSelected);
            }
            else
            {
                ChangeBtnIconColour(_furnitureBtn, _btnIconColourUnselected);
            }
        }

        public void OnProductionPressed()
        {
            SetState(PanelState.Production);
        }

        public void OnExteriorAppearancePressed()
        {
            SetState(PanelState.Exterior);
        }

        #endregion
    }
}
