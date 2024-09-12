using System.Collections.Generic;
using System.Linq;
using Characters;
using CodeMonkey.Utils;
using Managers;
using Player;
using Systems.CursorHandler.Scripts;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class DefaultInputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private SpriteRenderer _selectionBox;
        [SerializeField] private float dragThreshold = 0.1f; // Threshold to differentiate drag and click

        private bool _selectBoxActive;
        private bool _startedSelecting;
        private Vector2 _selectBoxStartPos;
        private List<PlayerInteractable> _selectedObjects = new List<PlayerInteractable>();
        private Vector2 _lowerLeft;
        private Vector2 _upperRight;
        
        private float _lastClickTime = 0f;
        private const float DOUBLE_CLICK_TIME = 0.3f; // Time in seconds
        private PlayerInteractable _currentlyHoveredPI;
        
        private Vector2 mouseStartPos;
        private bool isDragging;

        private bool _startedRightDrag;
        private List<KinlingPositionPreview> _displayedKinlingPreviews = new List<KinlingPositionPreview>();
        
        private void Awake()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.Default, this);
        }

        public void HandleInput()
        {
            // Mouse down - start the potential drag
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;

                _startedSelecting = true;
                mouseStartPos = UtilsClass.GetMouseWorldPosition();
                _selectBoxStartPos = mouseStartPos;
                _selectionBox.gameObject.SetActive(true);
            }

            // Mouse held down - check for drag
            if (Input.GetMouseButton(0))
            {
                if (_startedSelecting && !isDragging)
                {
                    Vector2 currentMousePos = UtilsClass.GetMouseWorldPosition();
                    float distance = Vector2.Distance(mouseStartPos, currentMousePos);

                    if (distance > dragThreshold)
                    {
                        isDragging = true; // Drag started
                        ResizeSelectionBox();
                    }
                }
                else if (isDragging)
                {
                    ResizeSelectionBox();
                }
            }

            // Mouse up - handle click or drag release
            if (Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    // Drag release logic
                    ReleaseSelectionBox();
                }
                else
                {
                    // Click logic
                    HandleLeftClick();
                }

                // Reset the selection state
                isDragging = false;
                _startedSelecting = false;
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;

                if (CheckIfAnyDrafted())
                {
                    _startedRightDrag = true;
                    OnRightDragStarted();
                }
                else
                {
                    SelectionManager.Instance.ClearSelection();
                    HUDController.Instance.HideDetails();
                }
            }

            if (Input.GetMouseButton(1) && _startedRightDrag)
            {
                OnRightDrag();
            }

            if (Input.GetMouseButtonUp(1) && _startedRightDrag)
            {
                _startedRightDrag = false;
                ReleaseRightDrag();
            }

            HandleHover();
        }

        private void HandleHover()
        {
            if (_startedSelecting) return;
            if (PlayerSettings.OutlineOnHover == false) return;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (_currentlyHoveredPI != null)
                {
                    _currentlyHoveredPI.OnHoverEnd();
                    _currentlyHoveredPI = null;
                }
                return;
            }
            
            // Get mouse position in world coordinates
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);

            // Handle hover end if we are no longer hovering over the previous object
            if (_currentlyHoveredPI != null && _currentlyHoveredPI != playerInteractable)
            {
                _currentlyHoveredPI.OnHoverEnd();
                _currentlyHoveredPI = null;
            }

            // Handle hover start if we're hovering a new object
            if (playerInteractable != null && playerInteractable != _currentlyHoveredPI)
            {
                playerInteractable.OnHoverStart();
                _currentlyHoveredPI = playerInteractable;
            }
        }
        
        public void OnEnter()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
        }

        public void OnExit()
        {
            if (_currentlyHoveredPI != null)
            {
                _currentlyHoveredPI.OnHoverEnd();
                _currentlyHoveredPI = null;
            }
        }
        
        private void HandleLeftClick()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
            
            float timeSinceLastClick = Time.time - _lastClickTime;
    
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                // Double click detected
                DoubleLeftClickDown(mousePosition);

                // Reset the last click time to avoid unnecessary checks
                _lastClickTime = 0;
            }
            else
            {
                if (timeSinceLastClick > DOUBLE_CLICK_TIME * 2) // Adjust as needed
                {
                    // Reset the timer if the time since the last click is significantly more than the double click threshold
                    _lastClickTime = Time.time;
                }

                // Single click logic, only if not detected as double click
                HandleClick(mousePosition);
            }
        }
        
        private void DoubleLeftClickDown(Vector3 mousePosition)
        {
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);
            
            if (playerInteractable != null)
            {
                var visiblePIs = PlayerInteractableDatabase.Instance.GetAllSimilarVisiblePIs(playerInteractable);
                
                if (Input.GetKey(KeyCode.LeftShift)) {}
                
                SelectMultiple(visiblePIs);
            }
        }
        
        public void HandleClick(Vector3 mousePosition)
        {
            var playerInteractable = Helper.GetObjectAtPosition<PlayerInteractable>(mousePosition);
            var zone = Helper.GetObjectAtPosition<ZoneCell>(mousePosition);
            if (playerInteractable != null)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    SelectionManager.Instance.SelectAdditional(playerInteractable, true);
                }
                else
                {
                    SelectionManager.Instance.ClearSelection();
                    SelectionManager.Instance.Select(playerInteractable);
                }
            }
            else if (zone != null)
            {
                SelectionManager.Instance.ClearSelection();
                zone.RuntimeData.SelectZone();
            }
            else
            {
                SelectionManager.Instance.ClearSelection();
            }
        }
        
        public void SelectMultiple(List<PlayerInteractable> selectedPIs)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var pi in selectedPIs)
                {
                    SelectionManager.Instance.SelectAdditional(pi, false);
                }
            }
            else
            {
                SelectionManager.Instance.ClearSelection();
                
                foreach (var pi in selectedPIs)
                {
                    SelectionManager.Instance.Highlight(pi);
                
                }
        
                HUDController.Instance.ShowMultipleDetails(selectedPIs);
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
        
        private void ReleaseSelectionBox()
        {
            _startedSelecting = false;
            _selectBoxActive = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.size = Vector2.zero;

            if (_selectedObjects.Count == 1)
            {
                SelectionManager.Instance.Select(_selectedObjects[0]);
            } 
            else if (_selectedObjects.Count > 1)
            {
                SelectionManager.Instance.Select(_selectedObjects);
            }
            else
            {
                SelectionManager.Instance.ClearSelection();
            }
            
            _selectedObjects.Clear();
        }
        
        public void ClearSelection()
        {
            foreach (var pi in _selectedObjects)
            {
                pi.IsSelected = false;
            }
            
            _selectedObjects.Clear();
            SelectionManager.Instance.ClearSelection();
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
                if (playerInteractable != null && ObjectValidForSelection(playerInteractable))
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
                    if (SelectionManager.Instance.IncludeIsActive)
                    {
                        if (!SelectionManager.Instance.SelectedObjects.Contains(previouslySelected))
                        {
                            previouslySelected.IsSelected = false; // Unselect the object
                        }
                    }
                    else
                    {
                        previouslySelected.IsSelected = false; // Unselect the object
                    }
                }
            }

            if (SelectionManager.Instance.IncludeIsActive)
            {
                currentlySelectedObjects.AddRange(SelectionManager.Instance.SelectedObjects);
                currentlySelectedObjects = currentlySelectedObjects.Distinct().ToList();
            }
            
            // Update the selected objects to the new selection
            _selectedObjects = currentlySelectedObjects;
        }

        private bool ObjectValidForSelection(PlayerInteractable playerInteractable)
        {
            var currentlySelected = SelectionManager.Instance.SelectedObjects;
            
            // If nothing is currently selected, only select Kinlings
            if (currentlySelected.Count == 0)
            {
                return playerInteractable is Kinling;
            }
            
            // If something is already selected, check IsSimilar to all the currently selected, if any replies true it is valid
            foreach (var selectedPI in currentlySelected)
            {
                if (selectedPI.IsSimilar(playerInteractable))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckIfAnyDrafted()
        {
            return SelectionManager.Instance.SelectedObjects.Any(pi => pi is Kinling { IsDrafted: true });
        }

        private void OnRightDragStarted()
        {
            Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
            
            List<Kinling> drafted = new List<Kinling>();
            foreach (var pi in SelectionManager.Instance.SelectedObjects)
            {
                if (pi is Kinling { IsDrafted: true } kinling)
                {
                    drafted.Add(kinling);
                }
            }
            
            DraftManager.Instance.BeginOrdersPreview(drafted, currentMousePos);
        }

        private void OnRightDrag()
        {
            Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
            DraftManager.Instance.ContinueOrdersPreview(currentMousePos);
        }

        private void ReleaseRightDrag()
        {
            DraftManager.Instance.CompleteOrdersPreview();
        }
    }
}
