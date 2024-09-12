using System.Collections.Generic;
using CodeMonkey.Utils;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class CommandSelectionInputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private SpriteRenderer _selectionBox;

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

            // Calculate the width and height based on the difference between the current and start positions
            float width = currentMousePos.x - _selectBoxStartPos.x;
            float height = currentMousePos.y - _selectBoxStartPos.y;

            // Keep the position of the selection box fixed at the start position (no recalculating)
            _selectionBox.transform.position = _selectBoxStartPos;

            // Update the size of the SpriteRenderer (allowing negative width/height for flipped directions)
            _selectionBox.size = new Vector2(width, height);

            HighlightObjectsInSelectionBox();
        }
        
        private void HighlightObjectsInSelectionBox()
        {
            // Calculate the lower left and upper right corners from the selection box, handling negative values
            _lowerLeft = new Vector2(
                Mathf.Min(_selectBoxStartPos.x, _selectBoxStartPos.x + _selectionBox.size.x),
                Mathf.Min(_selectBoxStartPos.y, _selectBoxStartPos.y + _selectionBox.size.y)
            );
            _upperRight = new Vector2(
                Mathf.Max(_selectBoxStartPos.x, _selectBoxStartPos.x + _selectionBox.size.x),
                Mathf.Max(_selectBoxStartPos.y, _selectBoxStartPos.y + _selectionBox.size.y)
            );

            // Create a set of currently selected objects for quick lookup
            HashSet<PlayerInteractable> previousSelectedObjects = new HashSet<PlayerInteractable>(_selectedObjects);

            // Create a new list to store the currently selected objects
            List<PlayerInteractable> currentlySelectedObjects = new List<PlayerInteractable>();

            // Perform an overlap check within the calculated area
            var allItems = Physics2D.OverlapAreaAll(_lowerLeft, _upperRight);

            foreach (var itemOverlapped in allItems)
            {
                var playerInteractable = itemOverlapped.gameObject.GetComponent<PlayerInteractable>();
                if (playerInteractable != null && playerInteractable.ObjectValidForCommandSelection(PendingCommand))
                {
                    currentlySelectedObjects.Add(playerInteractable);
                    playerInteractable.IsSelected = true;
                }
            }

            // Unselect objects that are no longer in the selection box
            foreach (var previouslySelected in previousSelectedObjects)
            {
                if (!currentlySelectedObjects.Contains(previouslySelected))
                {
                    previouslySelected.IsSelected = false; // Unselect the object
                }
            }

            // Update the selected objects to the new selection
            _selectedObjects = currentlySelectedObjects;
        }
        
        private void ReleaseSelectionBox()
        {
            _startedSelecting = false;
            _selectBoxActive = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.size = Vector2.zero;
            
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
            _selectionBox.size = Vector2.zero;
            ClearSelection();
        }
    }
}