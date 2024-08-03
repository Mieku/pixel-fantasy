using System;
using Characters;
using DataPersistence;
using HUD;
using Interfaces;
using Managers;
using Popups;
using Systems.Game_Setup.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class HUDController : Singleton<HUDController>
    {
        [SerializeField] private SelectedItemInfoPanel _selectedItemInfoPanel;
        [SerializeField] private Image _pause, _normalSpeed, _fastSpeed, _fastestSpeed;
        [SerializeField] private Color _defaultColour, _selectedColour;

        protected override void Awake()
        {
            base.Awake();

            GameEvents.OnGameSpeedChanged += OnGameSpeedChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameSpeedChanged -= OnGameSpeedChanged;
        }

        private void Start()
        {
            RefreshSpeedDisplay();
        }

        public void ShowUnitDetails(Kinling kinling)
        {
            _selectedItemInfoPanel.ShowUnitDetails(kinling);
        }

        public void ShowItemDetails(IClickableObject clickableObject)
        {
            _selectedItemInfoPanel.ShowItemDetails(clickableObject);
        }

        public void ShowZoneDetails(ZoneData zoneData)
        {
            HideDetails();
            
            _selectedItemInfoPanel.ShowZoneDetails(zoneData);
        }
        
        public void ShowBuildStructureDetails(DoorSettings doorSettings)
        {
            HideDetails();
            
            _selectedItemInfoPanel.ShowBuildStructureDetails(doorSettings);
        }
        
        public void ShowBuildStructureDetails(WallSettings wallSettings)
        {
            HideDetails();
            
            _selectedItemInfoPanel.ShowBuildStructureDetails(wallSettings);
        }
        
        public void ShowBuildStructureDetails(FloorSettings floorSettings)
        {
            HideDetails();
            
            _selectedItemInfoPanel.ShowBuildStructureDetails(floorSettings);
        }
        
        public void HideDetails()
        {
            if (_selectedItemInfoPanel != null)
            {
                _selectedItemInfoPanel.HideAllDetails();
                _selectedItemInfoPanel.ShowControlsDetails();
            }
        }

        public void MenuPressed()
        {
            PauseMenuPopup.Show();
        }

        #region Game Speed Controls


        private void OnGameSpeedChanged(float speedMod)
        {
            RefreshSpeedDisplay();
        }

        private void RefreshSpeedDisplay()
        {
            var speed = TimeManager.Instance.GameSpeed;

            _pause.color = _defaultColour;
            _normalSpeed.color = _defaultColour;
            _fastSpeed.color = _defaultColour;
            _fastestSpeed.color = _defaultColour;
            
            switch (speed)
            {
                case GameSpeed.Paused:
                    _pause.color = _selectedColour;
                    break;
                case GameSpeed.Play:
                    _normalSpeed.color = _selectedColour;
                    break;
                case GameSpeed.Fast:
                    _fastSpeed.color = _selectedColour;
                    break;
                case GameSpeed.Fastest:
                    _fastestSpeed.color = _selectedColour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void PauseBtnPressed()
        {
            TimeManager.Instance.SetGameSpeed(GameSpeed.Paused);
        }

        public void PlayBtnPressed()
        {
            TimeManager.Instance.SetGameSpeed(GameSpeed.Play);
        }

        public void FastBtnPressed()
        {
            TimeManager.Instance.SetGameSpeed(GameSpeed.Fast);
        }

        public void FastestBtnPressed()
        {
            TimeManager.Instance.SetGameSpeed(GameSpeed.Fastest);
        }

        #endregion
    }
}