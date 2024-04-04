using System;
using Controllers;
using Data.Zones;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using Systems.Zones.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class ZoneDetails : MonoBehaviour
    {
        [SerializeField] private PanelLayoutRebuilder _layoutRebuilder;
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private TextMeshProUGUI _panelTitle;

        [BoxGroup("Buttons")] [SerializeField] private Sprite _enabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Sprite _disabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Image _enableBtnBG;

        private ZoneData _zoneData;
        
        public void Show(ZoneData zoneData)
        {
            _zoneData = zoneData;
            _panelHandle.SetActive(true);

            switch (zoneData.ZoneType)
            {
                case EZoneType.Stockpile:
                    DisplayStockpileZoneDetails(zoneData as StockpileZoneData);
                    break;
                case EZoneType.Farm:
                    DisplayFarmingZoneDetails(zoneData as FarmingZoneData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            RefreshEnableBtnDisplay();
            
            RefreshLayout();
        }

        private void DisplayStockpileZoneDetails(StockpileZoneData data)
        {
            _panelTitle.text = data.ZoneName;
        }

        private void DisplayFarmingZoneDetails(FarmingZoneData data)
        {
            _panelTitle.text = data.ZoneName;
        }

        public void Hide()
        {
            _panelHandle.SetActive(false);
        }
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }

        #region Buttons

        public void ExpandBtnPressed()
        {
            
        }

        public void ShrinkBtnPressed()
        {
            
        }

        public void DeleteBtnPressed()
        {
            ZoneManager.Instance.DeleteZone(_zoneData);
            Hide();
        }

        public void LookAtBtnPressed()
        {
            var centerPoint = ZoneManager.Instance.ZoneCenter(_zoneData.Cells);
            CameraManager.Instance.LookAtPosition(centerPoint);
        }

        public void EnableBtnToggled()
        {
            _zoneData.IsEnabled = !_zoneData.IsEnabled;
            RefreshEnableBtnDisplay();
        }

        private void RefreshEnableBtnDisplay()
        {
            if (_zoneData.IsEnabled)
            {
                _enableBtnBG.sprite = _enabledBtnSprite;
            }
            else
            {
                _enableBtnBG.sprite = _disabledBtnSprite;
            }
        }

        #endregion
    }
}
