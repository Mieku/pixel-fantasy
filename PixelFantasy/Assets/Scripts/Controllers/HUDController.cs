using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Characters;
using HUD;
using Managers;
using Popups;
using UnityEngine;

namespace Controllers
{
    public class HUDController : Singleton<HUDController>
    {
        [SerializeField] private SelectedItemInfoPanel _selectedItemInfoPanel;
        [SerializeField] private GameObject _pauseHighlight, _playHighlight, _fastHighlight, _fastestHighlight;

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

        public void ShowUnitDetails(Unit unit)
        {
            _selectedItemInfoPanel.ShowUnitDetails(unit);
        }

        public void ShowItemDetails(SelectionData selectionData)
        {
            _selectedItemInfoPanel.ShowItemDetails(selectionData);
        }

        public void ShowBuildingDetails(Building building)
        {
            _selectedItemInfoPanel.ShowBuildingDetails(building);
        }

        public void HideItemDetails()
        {
            if (_selectedItemInfoPanel != null)
            {
                _selectedItemInfoPanel.HideItemDetails();
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
            
            _pauseHighlight.SetActive(false);
            _playHighlight.SetActive(false);
            _fastHighlight.SetActive(false);
            _fastestHighlight.SetActive(false);
            
            switch (speed)
            {
                case GameSpeed.Paused:
                    _pauseHighlight.SetActive(true);
                    break;
                case GameSpeed.Play:
                    _playHighlight.SetActive(true);
                    break;
                case GameSpeed.Fast:
                    _fastHighlight.SetActive(true);
                    break;
                case GameSpeed.Fastest:
                    _fastestHighlight.SetActive(true);
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