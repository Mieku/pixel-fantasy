using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Interfaces;
using Sirenix.Utilities;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Managers
{
    public class SelectionManager : Singleton<SelectionManager>
    {
        [SerializeField] private Transform _selectionBox;
        
        private bool _selectBoxActive;
        private bool _startedSelecting;
        private Vector2 _selectBoxStartPos;
        private List<IClickableObject> _selectedObjects = new List<IClickableObject>();
        private List<ClickObject> _prevClickObjects = new List<ClickObject>();
        private int _prevClickObjectIndex;
        private Action _onCompleted;
        private Command _pendingCommand;

        protected override void Awake()
        {
            GameEvents.OnRightClickDown += TurnOffSelectionBox;
        }

        private void OnDestroy()
        {
            GameEvents.OnRightClickDown -= TurnOffSelectionBox;
        }

        public void TurnOffSelectionBox( Vector3 mousePos, PlayerInputState inputState, bool isOverUI )
        {
            ClearAllValidInSelection();
            _selectBoxActive = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.localScale = Vector3.zero;
        }

        public void BeginCommandSelectionBox(Command command, Action onCompleted)
        {
            _pendingCommand = command;
            _selectBoxActive = true;
            _onCompleted = onCompleted;
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
        }

        /// <summary>
        /// Displays the selection box
        /// </summary>
        private Vector2 lowerLeft;
        private Vector2 upperRight;
        private void ResizeSelectionBox()
        {
            ClearAllValidInSelection();
            
            Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
            lowerLeft = new Vector2(
                Mathf.Min(_selectBoxStartPos.x, currentMousePos.x),
                Mathf.Min(_selectBoxStartPos.y, currentMousePos.y)
            );
            upperRight = new Vector2(
                Mathf.Max(_selectBoxStartPos.x, currentMousePos.x),
                Mathf.Max(_selectBoxStartPos.y, currentMousePos.y)
            );
            
            _selectionBox.position = lowerLeft;
            _selectionBox.localScale = upperRight - lowerLeft;
            
            RecordAllValidInSelection(_pendingCommand);
        }

        /// <summary>
        /// Gets the Object in the box, and assigns the order to them (if applicable)
        /// </summary>
        public void ReleaseOrdersSelectionBox()
        {
            foreach (var clickableObject in _selectedObjects)
            {
                clickableObject.AssignCommand(_pendingCommand);
            }
            
            DeactivateSelectionBox();
            
            _onCompleted.Invoke();
        }

        public void DeactivateSelectionBox()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            
            _startedSelecting = false;
            _selectionBox.gameObject.SetActive(false);
            _selectionBox.localScale = Vector3.zero;
            _selectBoxActive = false;
            
            ClearAllValidInSelection();
        }

        private void RecordAllValidInSelection(Command pendingCommand)
        {
            var allItems = Physics2D.OverlapAreaAll(_selectBoxStartPos, UtilsClass.GetMouseWorldPosition());
            foreach (var itemOverlapped in allItems)
            {
                var clickObject = itemOverlapped.gameObject.GetComponent<IClickableObject>();
                if (clickObject != null && clickObject.GetClickObject().ObjectValidForSelection(pendingCommand))
                {
                    _selectedObjects.Add(clickObject);
                    clickObject.GetClickObject().AreaSelectObject(pendingCommand);
                }
            }
        }

        private void ClearAllValidInSelection()
        {
            foreach (var clickObject in _selectedObjects)
            {
                clickObject.GetClickObject().UnselectAreaSelection();
            }
            
            _selectedObjects.Clear();
        }
        
        private void Update()
        {
            HandleSelectBoxUpdate();
            HandleUserClick();
        }

        private void HandleUserClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var isOverUI = EventSystem.current.IsPointerOverGameObject();
                if(isOverUI) return;
                
                var clickObjs = Helper.GetClickObjectsAtPos(UtilsClass.GetMouseWorldPosition());
                if (clickObjs.IsNullOrEmpty())
                {
                    PlayerInputController.Instance.ClearSelection();
                }
                
                if (clickObjs.Count > 0)
                {
                    if (AreClickObjectsSame(_prevClickObjects, clickObjs))
                    {
                        _prevClickObjectIndex++;
                        if (_prevClickObjectIndex > _prevClickObjects.Count - 1)
                        {
                            _prevClickObjectIndex = 0;
                        }
                        clickObjs[_prevClickObjectIndex].TriggerSelected();
                    }
                    else
                    {
                        _prevClickObjectIndex = 0;
                        clickObjs[_prevClickObjectIndex].TriggerSelected();
                    }
                }
                else
                {
                    _prevClickObjectIndex = 0;
                }
                _prevClickObjects = clickObjs;
            }

            if (Input.GetMouseButtonDown(1))
            {
                var isOverUI = EventSystem.current.IsPointerOverGameObject();
                if(isOverUI) return;
                
                if (PlayerInputController.Instance.SelectedKinling != null)
                {
                    var clickObjs = Helper.GetClickObjectsAtPos(UtilsClass.GetMouseWorldPosition());
                    if (clickObjs.IsNullOrEmpty())
                    {
                        // Give Move Command
                        CommandController.Instance.ShowMoveCommand(UtilsClass.GetMouseWorldPosition(), PlayerInputController.Instance.SelectedKinling);
                    }
                    
                    if (clickObjs.Count > 0)
                    {
                        if (AreClickObjectsSame(_prevClickObjects, clickObjs))
                        {
                            _prevClickObjectIndex++;
                            if (_prevClickObjectIndex > _prevClickObjects.Count - 1)
                            {
                                _prevClickObjectIndex = 0;
                            }
                            clickObjs[_prevClickObjectIndex].TriggerShowCommands(PlayerInputController.Instance.SelectedKinling);
                        }
                        else
                        {
                            _prevClickObjectIndex = 0;
                            clickObjs[_prevClickObjectIndex].TriggerShowCommands(PlayerInputController.Instance.SelectedKinling);
                        }
                    }
                    else
                    {
                        _prevClickObjectIndex = 0;
                    }

                    _prevClickObjects = clickObjs;
                }
                else
                {
                    PlayerInputController.Instance.ClearSelection();
                }
            }
        }

        private bool AreClickObjectsSame(List<ClickObject> prev, List<ClickObject> current)
        {
            if (prev.Count != current.Count) return false;

            foreach (var curObj in current)
            {
                if (!prev.Contains(curObj)) return false;
            }

            return true;
        }
        
        private void HandleSelectBoxUpdate()
        {
            if (!_selectBoxActive) return;

            if (Input.GetMouseButtonDown(0))
            {
                _startedSelecting = true;
                _selectionBox.gameObject.SetActive(true);
                _selectBoxStartPos = UtilsClass.GetMouseWorldPosition();
            } 
            else if (Input.GetMouseButton(0))
            {
                ResizeSelectionBox();
            }
            else if (Input.GetMouseButtonUp(0) && _startedSelecting)
            {
                ReleaseOrdersSelectionBox();
            }
        }
    }
}
