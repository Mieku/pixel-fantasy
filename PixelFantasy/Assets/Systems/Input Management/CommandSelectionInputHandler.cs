using System.Collections.Generic;
using CodeMonkey.Utils;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class CommandSelectionInputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private Transform _selectionBox;

        private bool _selectBoxActive;
        private bool _startedSelecting;
        private Vector2 _selectBoxStartPos;
        private List<PlayerInteractable> _selectedObjects = new List<PlayerInteractable>();
        private Vector2 _lowerLeft;
        private Vector2 _upperRight;

        public Command PendingCommand;
        
        private void Awake()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.CommandSelection, this);
            _selectionBox.gameObject.SetActive(false);
        }
        
        public void HandleInput()
        {
            UpdateSelectionBox();
        }
        
        public void UpdateSelectionBox()
        {
            if (!_selectBoxActive) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                
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

            if (Input.GetMouseButtonDown(1) && _startedSelecting)
            {
                DeactivateSelectionBox();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                SelectionManager.Instance.CancelCommandSelectionBox();
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
                if (playerInteractable != null && playerInteractable.ObjectValidForCommandSelection(PendingCommand))
                {
                    _selectedObjects.Add(playerInteractable);
                    SelectionManager.Instance.Highlight(playerInteractable);
                }
            }
        }
        
        private void ReleaseSelectionBox()
        {
            _startedSelecting = false;
            _selectBoxActive = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.localScale = Vector3.zero;
            
            SelectionManager.Instance.OnSelectionComplete(_selectedObjects);
            ClearSelection();
        }
        
        public void ClearSelection()
        {
            foreach (var selectedObject in _selectedObjects)
            {
                if (selectedObject != null)
                {
                    SelectionManager.Instance.ClearHighlight(selectedObject);
                }
            }
            _selectedObjects.Clear();
        }

        public void OnEnter()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            _selectBoxActive = true;
        }

        public void OnExit()
        {
            DeactivateSelectionBox();
        }
        
        public void DeactivateSelectionBox()
        {
            _startedSelecting = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.localScale = Vector3.zero;
            ClearSelection();
        }
    }
}