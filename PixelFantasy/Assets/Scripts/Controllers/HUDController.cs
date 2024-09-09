using System;
using System.Collections.Generic;
using Characters;
using HUD;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : Singleton<HUDController>
{
    [SerializeField] private SelectedItemInfoPanel _selectedItemInfoPanel;
    [SerializeField] private Image _pause, _normalSpeed, _fastSpeed, _fastestSpeed;
    [SerializeField] private Color _defaultColour, _selectedColour;

    private void OnEnable()
    {
        GameEvents.OnGameSpeedChanged += OnGameSpeedChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnGameSpeedChanged -= OnGameSpeedChanged;
    }

    public void ShowMultipleDetails(List<PlayerInteractable> playerInteractables)
    {
        _selectedItemInfoPanel.ShowMultipleDetailed(playerInteractables);
    }

    public void ShowItemDetails(PlayerInteractable playerInteractable)
    {
        _selectedItemInfoPanel.ShowItemDetails(playerInteractable);
    }

    public void HideDetails()
    {
        _selectedItemInfoPanel.ShowControlsDetails();
    }
    
    public void ShowZoneDetails(ZoneData zoneData)
    {
        _selectedItemInfoPanel.ShowZoneDetails(zoneData);
    }
        
    public void ShowBuildStructureDetails(DoorSettings doorSettings)
    {
        _selectedItemInfoPanel.ShowBuildStructureDetails(doorSettings);
    }
        
    public void ShowBuildStructureDetails(WallSettings wallSettings)
    {
        _selectedItemInfoPanel.ShowBuildStructureDetails(wallSettings);
    }
        
    public void ShowBuildStructureDetails(FloorSettings floorSettings)
    {
        _selectedItemInfoPanel.ShowBuildStructureDetails(floorSettings);
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