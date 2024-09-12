using System;
using System.Collections.Generic;
using Managers;
using Systems.Input_Management;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : Singleton<SelectionManager>
{
    [SerializeField] private InputActionReference _includeInput;
    
    private List<PlayerInteractable> _selectedObjects = new List<PlayerInteractable>();
    private Command _pendingCommand;
    private Action _onCompleted;
    private bool _includeIsActive;
    
    public List<PlayerInteractable> SelectedObjects => _selectedObjects;

    protected override void Awake()
    {
        base.Awake();

        _includeInput.action.performed += OnIncludePressed;
        _includeInput.action.canceled += OnIncludeReleased;
    }

    private void OnDestroy()
    {
        _includeInput.action.performed -= OnIncludePressed;
        _includeInput.action.canceled -= OnIncludeReleased;
    }

    private void OnIncludePressed(InputAction.CallbackContext context)
    {
        _includeIsActive = true;
    }
    
    private void OnIncludeReleased(InputAction.CallbackContext context)
    {
        _includeIsActive = false;
    }

    public void UnSelect(List<PlayerInteractable> unselectedPIs)
    {
        foreach (var pi in unselectedPIs)
        {
            _selectedObjects.Remove(pi);
            pi.IsSelected = false;
        }

        if (_selectedObjects.Count > 1)
        {
            HUDController.Instance.ShowMultipleDetails(_selectedObjects);
        }
        else if (_selectedObjects.Count == 0)
        {
            HUDController.Instance.HideDetails();
        }
        else
        {
            HUDController.Instance.ShowItemDetails(_selectedObjects[0]);
        }
    }

    public void Select(PlayerInteractable playerInteractable)
    {
        _selectedObjects.Add(playerInteractable);
        playerInteractable.IsSelected = true;
        HUDController.Instance.ShowItemDetails(playerInteractable);
    }

    public void Select(List<PlayerInteractable> playerInteractables)
    {
        if (!_includeIsActive)
        {
            ClearSelection();
        }
        
        foreach (var pi in playerInteractables)
        {
            _selectedObjects.Add(pi);
            pi.IsSelected = true;
        }
        
        if (_selectedObjects.Count > 1)
        {
            HUDController.Instance.ShowMultipleDetails(_selectedObjects);
        }
        else if (_selectedObjects.Count == 0)
        {
            HUDController.Instance.HideDetails();
        }
        else
        {
            HUDController.Instance.ShowItemDetails(playerInteractables[0]);
        }
    }

    public void Highlight(PlayerInteractable playerInteractable)
    {
        _selectedObjects.Add(playerInteractable);
        playerInteractable.IsSelected = true;
    }

    public void ClearHighlight(PlayerInteractable playerInteractable)
    {
        _selectedObjects.Remove(playerInteractable);
        playerInteractable.IsSelected = false;
    }

    public void SelectAdditional(PlayerInteractable playerInteractable, bool removeExisting)
    {
        if (_selectedObjects.Contains(playerInteractable))
        {
            if (removeExisting)
            {
                _selectedObjects.Remove(playerInteractable);
                playerInteractable.IsSelected = false;
            }
        }
        else
        {
            _selectedObjects.Add(playerInteractable);
            playerInteractable.IsSelected = true;
        }

        if (_selectedObjects.Count > 1)
        {
            HUDController.Instance.ShowMultipleDetails(_selectedObjects);
        }
        else if (_selectedObjects.Count == 0)
        {
            HUDController.Instance.HideDetails();
        }
        else
        {
            HUDController.Instance.ShowItemDetails(playerInteractable);
        }
    }
    
    public void ClearSelection()
    {
        bool hadObjects = _selectedObjects.Count > 0;
        foreach (var selectedObject in _selectedObjects)
        {
            if (selectedObject != null)
            {
                selectedObject.IsSelected = false;
            }
        }
        _selectedObjects.Clear();
        
        ZonesDatabase.Instance.UnselectZone();

        if (hadObjects)
        {
            HUDController.Instance.HideDetails();
        }
    }

    public void BeginCommandSelectionBox(Command command, Action onCompleted)
    {
        _pendingCommand = command;
        _onCompleted = onCompleted;
        
        var mode = (CommandSelectionInputHandler) InputManager.Instance.SetInputMode(InputMode.CommandSelection);
        mode.PendingCommand = _pendingCommand;
    }

    public void CancelCommandSelectionBox()
    {
        _onCompleted?.Invoke();
        _pendingCommand = null;
        InputManager.Instance.ReturnToDefault();
    }

    public void OnSelectionComplete(List<PlayerInteractable> selectedPIs)
    {
        // Assign commands to all selected objects
        foreach (var selectedObject in selectedPIs)
        {
            if (selectedObject.ObjectValidForCommandSelection(_pendingCommand))
            {
                selectedObject.AssignCommand(_pendingCommand);
            }
        }

        BeginCommandSelectionBox(_pendingCommand, _onCompleted);
    }
}
