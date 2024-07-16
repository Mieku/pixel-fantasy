using System;
using Controllers;
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
        [SerializeField] private TMP_InputField _panelTitle;

        [BoxGroup("Generic")] [SerializeField] private TextMeshProUGUI _genericDetailsText;

        [BoxGroup("Storage Settings")] [SerializeField] private StorageSettingsDetails _storageDetails;

        [BoxGroup("Buttons")] [SerializeField] private Sprite _defaultBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Sprite _enabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Sprite _disabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Image _enableBtnBG;
        [BoxGroup("Buttons")] [SerializeField] private Image _expandBtnBG;
        [BoxGroup("Buttons")] [SerializeField] private Image _shrinkBtnBG;

        private ZoneData _zoneData;
        
        public void Show(ZoneData zoneData)
        {
            _zoneData = zoneData;
            _zoneData.OnZoneChanged += OnZoneChanged;
            _panelHandle.SetActive(true);
            
            UpdateDisplayedDetails();
        }

        private void UpdateDisplayedDetails()
        {
            DisplayGenericZoneDetails();
            
            switch (_zoneData.ZoneType)
            {
                case ZoneSettings.EZoneType.Stockpile:
                    DisplayStockpileZoneDetails(_zoneData as StockpileZoneData);
                    break;
                case ZoneSettings.EZoneType.Farm:
                    DisplayFarmingZoneDetails(_zoneData as FarmingZoneData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            RefreshEnableBtnDisplay();
            
            RefreshLayout();
        }

        private void OnZoneChanged()
        {
            UpdateDisplayedDetails();
        }

        private void DisplayGenericZoneDetails()
        {
            //_genericDetailsText.text = $"Size: {_zoneData.NumCells} Cells";
        }

        private void DisplayStockpileZoneDetails(StockpileZoneData data)
        {
            _panelTitle.SetTextWithoutNotify(data.ZoneName);
            
            DisplayStockpileSettings();
        }

        private void DisplayStockpileSettings()
        {
            _storageDetails.Show(_zoneData as StockpileZoneData);
        }

        private void DisplayFarmingZoneDetails(FarmingZoneData data)
        {
            _panelTitle.text = data.ZoneName;
            
            DisplayFarmSettings();
        }
        
        private void DisplayFarmSettings()
        {
            
        }

        public void Hide()
        {
            _panelHandle.SetActive(false);

            if (_zoneData != null)
            {
                _zoneData.OnZoneChanged -= OnZoneChanged;
            }
        }
        
        private void RefreshLayout()
        {
            _layoutRebuilder.RefreshLayout();
        }

        #region Buttons

        public void ExpandBtnPressed()
        {
            _expandBtnBG.sprite = _enabledBtnSprite;
            
            ZonesDatabase.Instance.BeginPlanningZoneExpansion(_zoneData, () =>
            {
                _expandBtnBG.sprite = _defaultBtnSprite;
            });
        }

        public void ShrinkBtnPressed()
        {
            _shrinkBtnBG.sprite = _enabledBtnSprite;
            
            ZonesDatabase.Instance.BeginPlanningZoneShrinking(() =>
            {
                _shrinkBtnBG.sprite = _defaultBtnSprite;
            });
        }

        public void DeleteBtnPressed()
        {
            ZonesDatabase.Instance.DeleteZone(_zoneData);
        }

        public void LookAtBtnPressed()
        {
            var centerPoint = ZonesDatabase.Instance.ZoneCenter(_zoneData.Cells);
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

        public void OnZoneNameChanged(string value)
        {
            _zoneData.ChangeZoneName(value);
        }

        #endregion
    }
}
