using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Interfaces;
using UnityEngine;

namespace Managers
{
    public class SelectionManager : Singleton<SelectionManager>
    {
        [SerializeField] private Transform _selectionBox;
        
        // private ActionBase _selectBoxOrder;
        private bool _selectBoxActive;
        private Vector2 _selectBoxStartPos;
        private List<IClickableObject> _selectedObjects = new List<IClickableObject>();
        private List<ClickObject> _prevClickObjects = new List<ClickObject>();
        private int _prevClickObjectIndex;

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

        // public void BeginOrdersSelectionBox(ActionBase order)
        // {
        //     _selectBoxOrder = order;
        //     _selectBoxActive = true;
        // }

        /// <summary>
        /// Displays the selection box
        /// </summary>
        private Vector2 lowerLeft;
        private Vector2 upperRight;
        private void ResizeSelectionBox()
        {
            // ClearAllValidInSelection();
            //
            // Vector3 currentMousePos = UtilsClass.GetMouseWorldPosition();
            // lowerLeft = new Vector2(
            //     Mathf.Min(_selectBoxStartPos.x, currentMousePos.x),
            //     Mathf.Min(_selectBoxStartPos.y, currentMousePos.y)
            // );
            // upperRight = new Vector2(
            //     Mathf.Max(_selectBoxStartPos.x, currentMousePos.x),
            //     Mathf.Max(_selectBoxStartPos.y, currentMousePos.y)
            // );
            //
            // _selectionBox.position = lowerLeft;
            // _selectionBox.localScale = upperRight - lowerLeft;
            //
            // RecordAllValidInSelection(_selectBoxOrder);
        }

        /// <summary>
        /// Gets the Object in the box, and assigns the order to them (if applicable)
        /// </summary>
        // public void ReleaseOrdersSelectionBox(ActionBase filteredOrder)
        // {
        //     _selectionBox.gameObject.SetActive(false);
        //     _selectionBox.localScale = Vector3.zero;
        //
        //     foreach (var clickableObject in _selectedObjects)
        //     {
        //         clickableObject.AssignOrder(filteredOrder);
        //     }
        //     
        //     ClearAllValidInSelection();
        // }

        // private void RecordAllValidInSelection(ActionBase filteredOrder)
        // {
        //     var allItems = Physics2D.OverlapAreaAll(_selectBoxStartPos, UtilsClass.GetMouseWorldPosition());
        //     foreach (var itemOverlapped in allItems)
        //     {
        //         var clickObject = itemOverlapped.gameObject.GetComponent<IClickableObject>();
        //         if (clickObject != null && clickObject.GetClickObject().ObjectValidForSelection(filteredOrder))
        //         {
        //             _selectedObjects.Add(clickObject);
        //             clickObject.GetClickObject().AreaSelectObject(filteredOrder);
        //         }
        //     }
        // }

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
                var clickObjs = Helper.GetClickObjectsAtPos(UtilsClass.GetMouseWorldPosition());
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
                _selectionBox.gameObject.SetActive(true);
                _selectBoxStartPos = UtilsClass.GetMouseWorldPosition();
            } 
            else if (Input.GetMouseButton(0))
            {
                ResizeSelectionBox();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // ReleaseOrdersSelectionBox(_selectBoxOrder);
            }
        }
    }
}
