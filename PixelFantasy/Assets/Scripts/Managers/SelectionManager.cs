using System;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Managers;
using Systems.CursorHandler.Scripts;
using Systems.Zones.Scripts;

public class SelectionManager : Singleton<SelectionManager>
{
    [SerializeField] private Transform _selectionBox;

    private bool _selectBoxActive;
    private bool _startedSelecting;
    private Vector2 _selectBoxStartPos;
    private List<PlayerInteractable> _selectedObjects = new List<PlayerInteractable>();
    private Vector2 _lowerLeft;
    private Vector2 _upperRight;
    private Command _pendingCommand;
    private Action _onCompleted;

    protected override void Awake()
    {
        base.Awake();
        _selectionBox.gameObject.SetActive(false);
    }

    // Method to handle single clicks (called from SelectionInputHandler)
    public void HandleClick(Vector3 mousePosition)
    {
        var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);
        var zone = Helper.GetObjectAtPosition<ZoneCell>(mousePosition);
        if (playerInteractable != null && !playerInteractable.IsClickDisabled)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                SelectAdditional(playerInteractable);
            }
            else
            {
                ClearSelection();
                Select(playerInteractable);
            }
        }
        else if (zone != null)
        {
            ClearSelection();
            zone.RuntimeData.SelectZone();
        }
        else
        {
            ClearSelection();
        }
    }

    public void SelectMultiple(List<PlayerInteractable> selectedPIs)
    {
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            ClearSelection();
        }

        foreach (var pi in selectedPIs)
        {
            _selectedObjects.Add(pi);
            pi.IsSelected = true;
        }

        HUDController.Instance.ShowMultipleDetails(_selectedObjects);
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

    public void SelectAdditional(PlayerInteractable playerInteractable)
    {
        if (_selectedObjects.Contains(playerInteractable))
        {
            _selectedObjects.Remove(playerInteractable);
            playerInteractable.IsSelected = false;
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

    public void Deselect(PlayerInteractable playerInteractable)
    {
        _selectedObjects.Remove(playerInteractable);
        playerInteractable.IsSelected = false;
        HUDController.Instance.HideDetails();
    }

    public void ClearSelection()
    {
        foreach (var selectedObject in _selectedObjects)
        {
            if (selectedObject != null)
            {
                selectedObject.IsSelected = false;
            }
        }
        _selectedObjects.Clear();
        HUDController.Instance.HideDetails();
    }

    public void DeactivateSelectionBox()
    {
        _startedSelecting = false;
        _selectionBox.gameObject.SetActive(false);
        _selectionBox.localScale = Vector3.zero;
    }

    public void BeginCommandSelectionBox(Command command, Action onCompleted)
    {
        _pendingCommand = command;
        _onCompleted = onCompleted;
        _selectBoxActive = true;
        CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
    }

    public void UpdateSelectionBox()
    {
        if (!_selectBoxActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            _startedSelecting = true;
            _selectBoxStartPos = UtilsClass.GetMouseWorldPosition();
            _selectionBox.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0) && _startedSelecting)
        {
            ResizeSelectionBox();
        }

        if (Input.GetMouseButtonUp(0) && _startedSelecting)
        {
            ReleaseSelectionBox();
        }
    }

    private void ResizeSelectionBox()
    {
        Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
        _lowerLeft = new Vector2(
            Mathf.Min(_selectBoxStartPos.x, currentMousePos.x),
            Mathf.Min(_selectBoxStartPos.y, currentMousePos.y)
        );
        _upperRight = new Vector2(
            Mathf.Max(_selectBoxStartPos.x, currentMousePos.x),
            Mathf.Max(_selectBoxStartPos.y, currentMousePos.y)
        );

        _selectionBox.position = _lowerLeft;
        _selectionBox.localScale = _upperRight - _lowerLeft;

        HighlightObjectsInSelectionBox();
    }

    private void HighlightObjectsInSelectionBox()
    {
        ClearSelection();

        var allItems = Physics2D.OverlapAreaAll(_lowerLeft, _upperRight);

        foreach (var itemOverlapped in allItems)
        {
            var playerInteractable = itemOverlapped.gameObject.GetComponent<PlayerInteractable>();
            if (playerInteractable != null && playerInteractable.ObjectValidForCommandSelection(_pendingCommand))
            {
                Select(playerInteractable);
            }
        }
    }

    private void ReleaseSelectionBox()
    {
        _startedSelecting = false;
        _selectBoxActive = false;
        _selectionBox.gameObject.SetActive(false);
        _selectionBox.localScale = Vector3.zero;

        // Assign commands to all selected objects
        foreach (var selectedObject in _selectedObjects)
        {
            selectedObject.AssignCommand(_pendingCommand);
        }

        _onCompleted?.Invoke();
        CursorManager.Instance.ChangeCursorState(ECursorState.Default);
    }
}
