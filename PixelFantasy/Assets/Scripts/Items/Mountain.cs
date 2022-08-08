using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using Interfaces;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Mountain : Interactable, IClickableTile
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private MountainData _mountainData;
        [SerializeField] private Color _selectionTintColour;

        private Tilemap _mountainTM;
        
        private void Awake()
        {
            _tempPlacementDisp.SetActive(false);
            _mountainTM =
                TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
        }

        private void Start()
        {
            SetTile();
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, _mountainData.GetRuleTile());
        }

        public SelectionData GetSelectionData()
        {
            var actions = GetActions();
            var cancellableActions = GetCancellableActions();
        
            SelectionData result = new SelectionData
            {
                ItemName = _mountainData.ResourceName,
                Actions = actions,
                CancellableActions = cancellableActions,
                ClickObject = GetClickObject(),
                Requestor = GetComponent<Interactable>(),
            };

            return result;
        }

        public ClickObject GetClickObject()
        {
            var clickObj = gameObject.GetComponent<ClickObject>();
            return clickObj;
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            
        }

        public MountainData GetMountainData()
        {
            return _mountainData;
        }

        public List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public List<ActionBase> GetCancellableActions()
        {
            return CancellableActions();
        }

        public void AssignOrder(ActionBase orderToAssign)
        {
            CreateTask(orderToAssign);
        }

        public void TintTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetColor(cell, _selectionTintColour);
        }

        public void UnTintTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetColor(cell, Color.white);
        }
    }
}
