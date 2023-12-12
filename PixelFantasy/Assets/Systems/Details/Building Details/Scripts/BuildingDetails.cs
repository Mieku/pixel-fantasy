using System;
using Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class BuildingDetails : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private HeaderBuildingPanel _headerPanel;
        [SerializeField] private OccupantsBuildingPanel _occupantsPanel;
        [SerializeField] private ProductionBuildingPanel _productionPanel;
        [SerializeField] private CraftingOrdersBuildingPanel _craftingOrdersPanel;
        [SerializeField] private StorageBuildingPanel _storagePanel;
        [SerializeField] private StockpileBuildingPanel _stockpilePanel;
        [SerializeField] private UnderConstructionBuildingPanel _underConstructionPanel;
        [SerializeField] private FurnitureBuildingPanel _furniturePanel;
        [SerializeField] private ZoningBuildingPanel _zoningPanel;
        [SerializeField] private FooterBuildingPanel _footerPanel;

        [Header("Top Buttons")] 
        [SerializeField] private Image _generalBtnImg;
        [SerializeField] private Image _constructionBtnImg;
        [SerializeField] private Sprite _activeBtnSpr;
        [SerializeField] private Sprite _defaultBtnSpr;

        private Building _building;
        private EDetailsTab _tab;
        private Building.BuildingState? _curBuildingState = null;

        private void DeterminePanelsToShow()
        {
            // Header and Footer are always shown
            _headerPanel.Init(_building, this);
            _footerPanel.Init(_building, this);

            // Occupants Panel
            if (_building.BuildingData.MaxOccupants > 0)
            {
                _occupantsPanel.Init(_building, this);
            }
            
            // Under Construction Panel
            if (_building.State is Building.BuildingState.Construction or Building.BuildingState.Planning)
            {
                _underConstructionPanel.Init(_building, this);
            }
            
            // Stockpile Building Panels
            var stockpileBuilding = _building as StockpileBuilding;
            if (stockpileBuilding != null && _building.State == Building.BuildingState.Built)
            {
                _stockpilePanel.Init(_building, this);
            }
            
            // Storage Building Panel
            if (stockpileBuilding == null 
                && _building.State == Building.BuildingState.Built 
                && _building.GetBuildingStorages().Count > 0)
            {
                _storagePanel.Init(_building, this);
            }
            
            // Crafting Orders Panel
            var craftingBuilding = _building as CraftingBuilding;
            if (craftingBuilding != null && _building.State == Building.BuildingState.Built)
            {
                _craftingOrdersPanel.Init(_building, this);
            }

            // Production Panel
            var productionBuilding = _building as ProductionBuilding;
            if (productionBuilding != null && _building.State == Building.BuildingState.Built)
            {
                _productionPanel.Init(_building, this);
            }
        }

        private void HideAllPanels()
        {
            _headerPanel.Hide();
            _occupantsPanel.Hide();
            _productionPanel.Hide();
            _craftingOrdersPanel.Hide();
            _storagePanel.Hide();
            _stockpilePanel.Hide();
            _underConstructionPanel.Hide();
            _furniturePanel.Hide();
            _zoningPanel.Hide();
            _footerPanel.Hide();
        }

        private void SetTab(EDetailsTab tab)
        {
            _tab = tab;

            _generalBtnImg.sprite = _defaultBtnSpr;
            _constructionBtnImg.sprite = _defaultBtnSpr;
            switch (_tab)
            {
                case EDetailsTab.General:
                    _generalBtnImg.sprite = _activeBtnSpr;
                    break;
                case EDetailsTab.Construction:
                    _constructionBtnImg.sprite = _activeBtnSpr;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tab), tab, null);
            }
            
            DeterminePanelsToShow();
        }

        public void Show(Building building, EDetailsTab tab = EDetailsTab.General)
        {
            GameEvents.OnBuildingChanged += GameEvent_OnBuildingChanged;
            _building = building;
            _mainPanel.SetActive(true);
            HideAllPanels();
            SetTab(tab);
            DeterminePanelsToShow();
        }

        public void Hide()
        {
            GameEvents.OnBuildingChanged -= GameEvent_OnBuildingChanged;
            _building = null;
            _mainPanel.SetActive(false);
            HideAllPanels();
        }

        private void GameEvent_OnBuildingChanged(Building changedBuilding)
        {
            if (changedBuilding != _building) return;

            if (_curBuildingState != _building.State)
            {
                _curBuildingState = _building.State;
                HideAllPanels();
                DeterminePanelsToShow();
            }
        }

        public void GeneralTabPressed()
        {
            SetTab(EDetailsTab.General);
        }

        public void ConstructionTabPressed()
        {
            SetTab(EDetailsTab.Construction);
        }
        
        public void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        public enum EDetailsTab
        {
            General,
            Construction,
        }
    }
}
