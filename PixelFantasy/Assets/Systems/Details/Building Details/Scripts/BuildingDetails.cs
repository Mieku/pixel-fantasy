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

        private void DeterminePanelsToShow()
        {
            // Header and Footer are always shown
            _headerPanel.Init(_building);
            _footerPanel.Init(_building);

            // Occupants Panel
            if (_building.BuildingData.MaxOccupants > 0)
            {
                _occupantsPanel.Init(_building);
            }
            
            // Under Construction Panel
            if (_building.State is Building.BuildingState.Construction or Building.BuildingState.Planning)
            {
                _underConstructionPanel.Init(_building);
            }
            
            // Stockpile Building Panels
            var stockpileBuilding = _building as StockpileBuilding;
            if (stockpileBuilding != null && _building.State == Building.BuildingState.Built)
            {
                _stockpilePanel.Init(_building);
            }
        }

        private void HideAllPanels()
        {
            _headerPanel.Hide();
            _occupantsPanel.Hide();
            _productionPanel.Hide();
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
            HideAllPanels();
            DeterminePanelsToShow();
        }

        public void GeneralTabPressed()
        {
            SetTab(EDetailsTab.General);
        }

        public void ConstructionTabPressed()
        {
            SetTab(EDetailsTab.Construction);
        }

        public enum EDetailsTab
        {
            General,
            Construction,
        }
    }
}
