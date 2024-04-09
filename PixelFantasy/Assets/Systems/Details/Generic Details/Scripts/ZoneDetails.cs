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

        [BoxGroup("Generid")] [SerializeField] private TextMeshProUGUI _genericDetailsText;

        [BoxGroup("Buttons")] [SerializeField] private Sprite _defaultBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Sprite _enabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Sprite _disabledBtnSprite;
        [BoxGroup("Buttons")] [SerializeField] private Image _enableBtnBG;
        [BoxGroup("Buttons")] [SerializeField] private Image _expandBtnBG;
        [BoxGroup("Buttons")] [SerializeField] private Image _shrinkBtnBG;
        [BoxGroup("Buttons")] [SerializeField] private Button _editSettingsBtn;
        [BoxGroup("Buttons")] [SerializeField] private Image _editSettingsBtnBG;

        private ZoneData _zoneData;
        private bool _isEditingSettings;
        private Action<bool> _onEditSettingsPressed;
        
        public void Show(ZoneData zoneData)
        {
            _zoneData = zoneData;
            _zoneData.OnZoneChanged += OnZoneChanged;
            _panelHandle.SetActive(true);
            
            ResetEditBtn();

            UpdateDisplayedDetails();
        }

        private void UpdateDisplayedDetails()
        {
            DisplayGenericZoneDetails();
            
            switch (_zoneData.ZoneType)
            {
                case EZoneType.Stockpile:
                    DisplayStockpileZoneDetails(_zoneData as StockpileZoneData);
                    break;
                case EZoneType.Farm:
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
            _genericDetailsText.text = $"Size: {_zoneData.NumCells} Cells";
        }

        private void DisplayStockpileZoneDetails(StockpileZoneData data)
        {
            _panelTitle.text = data.ZoneName;
            
            ShowEditSettingsBtn(DisplayStockpileSettings);
        }

        private void DisplayStockpileSettings(bool isShown)
        {
            Debug.Log("Displaying Stockpile settings: " + isShown);
        }

        private void DisplayFarmingZoneDetails(FarmingZoneData data)
        {
            _panelTitle.text = data.ZoneName;
            
            ShowEditSettingsBtn(DisplayFarmSettings);
        }
        
        private void DisplayFarmSettings(bool isShown)
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
            
            ZoneManager.Instance.BeginPlanningZoneExpansion(_zoneData, () =>
            {
                _expandBtnBG.sprite = _defaultBtnSprite;
            });
        }

        public void ShrinkBtnPressed()
        {
            _shrinkBtnBG.sprite = _enabledBtnSprite;
            
            ZoneManager.Instance.BeginPlanningZoneShrinking(() =>
            {
                _shrinkBtnBG.sprite = _defaultBtnSprite;
            });
        }

        public void DeleteBtnPressed()
        {
            ZoneManager.Instance.DeleteZone(_zoneData);
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

        private void ShowEditSettingsBtn(Action<bool> onEditSettingsPressed)
        {
            _onEditSettingsPressed = onEditSettingsPressed;
            _editSettingsBtn.gameObject.SetActive(true);
        }

        private void HideEditSettingsBtn()
        {
            _onEditSettingsPressed = null;
            _editSettingsBtn.gameObject.SetActive(false);
        }

        public void EditBtnToggled()
        {
            _isEditingSettings = !_isEditingSettings;
            RefreshEditBtnDisplay();

            _onEditSettingsPressed.Invoke(_isEditingSettings);
        }

        private void ResetEditBtn()
        {
            _isEditingSettings = false;
            RefreshEditBtnDisplay();
            
            _onEditSettingsPressed?.Invoke(false);
        }

        private void RefreshEditBtnDisplay()
        {
            if (_isEditingSettings)
            {
                _editSettingsBtnBG.sprite = _enabledBtnSprite;
            }
            else
            {
                _editSettingsBtnBG.sprite = _defaultBtnSprite;
            }
        }

        #endregion
    }
}
