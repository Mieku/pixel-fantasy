using Databrain;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class StockpileZonesSubMenu : ControlsMenu
    {
        public DataLibrary DataLibrary;
        [SerializeField] private ControlsBtn _genericBtn;
        [DataObjectDropdown("DataLibrary", true)] [SerializeField] private StockpileZoneSettings _genericStockpileSettings;
        
        public override void Show()
        {
            base.Show();

            _genericBtn.OnPressed += GenericZonePressed;
        }

        public override void Hide()
        {
            base.Hide();
            
            SetBtnActive(null);
            
            _genericBtn.OnPressed -= GenericZonePressed;
        }

        private void GenericZonePressed(ControlsBtn btn)
        {
            SetBtnActive(btn);
            ZoneManager.Instance.BeginPlanningZone(_genericStockpileSettings, OnZoneCreated);
        }

        private void OnZoneCreated()
        {
            SetBtnActive(null);
        }
    }
}
