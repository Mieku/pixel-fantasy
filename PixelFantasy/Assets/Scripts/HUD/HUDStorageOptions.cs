using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using UnityEngine;

namespace HUD
{
    public class HUDStorageOptions : InputAwareComponent
    {
        private bool _planningStorage;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private readonly List<string> _storagePlacementInvalidTags = new List<string>
        {
            "Water",
            "Zone"
        };
        
        [SerializeField] private Sprite _storageZoneBlueprint;

        public void BuildStorageZonePressed()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStorage);
        }

        protected override void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnRightClickDown(mousePos, inputState);
            
            if (inputState == PlayerInputState.BuildStorage)
            {
                CancelPlanning();
            }
        }

        protected override void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnLeftClickDown(mousePos, inputState);
            
            if (inputState == PlayerInputState.BuildStorage)
            {
                _planningStorage = true;
                _startPos = Helper.ConvertMousePosToGridPos(mousePos);
            }
        }

        protected override void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnLeftClickHeld(mousePos, inputState);
            
            if (inputState == PlayerInputState.BuildStorage)
            {
                PlanStorageSlots(mousePos);
            }
        }

        protected override void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState)
        {
            base.GameEvents_OnLeftClickUp(mousePos, inputState);
            
            if (inputState == PlayerInputState.BuildStorage)
            {
                SpawnPlannedStorageSlots();
            }
        }

        private void CancelPlanning()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ClearPlannedSlots();
            _plannedGrid.Clear();
        }

        /// <summary>
        /// Begin recording all the slots being planned for construction,
        /// Also display their blue print on the tiles,
        /// If player right clicks, this is cancelled
        /// </summary>
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private void PlanStorageSlots(Vector2 mousePos)
        {
            if (!_planningStorage) return;

            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetGridPositionsBetweenPoints(_startPos, curGridPos);
            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous display, then display new blueprints
                ClearPlannedSlots();

                foreach (var gridPos in gridPositions)
                {
                    var blueprint = new GameObject("blueprint", typeof(SpriteRenderer));
                    blueprint.transform.position = gridPos;
                    var spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = _storageZoneBlueprint;
                    if (Helper.IsGridPosValidToBuild(gridPos, _storagePlacementInvalidTags))
                    {
                        spriteRenderer.color = Color.green;
                    }
                    else
                    {
                        spriteRenderer.color = Color.red;
                    }
                    
                    spriteRenderer.sortingLayerName = "Item";
                    _blueprints.Add(blueprint);
                }
            }
            
        }

        private void ClearPlannedSlots()
        {
            foreach (var blueprint in _blueprints)
            {
                Destroy(blueprint);
            }
            _blueprints.Clear();
        }

        /// <summary>
        /// Using the planned tiles recorded when held,
        /// Spawn the Item slots on the tiles
        /// </summary>
        private void SpawnPlannedStorageSlots()
        {
            if (!_planningStorage) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _storagePlacementInvalidTags))
                {
                    CreateStorageSlot(gridPos);
                }
            }

            ClearPlannedSlots();
            _plannedGrid.Clear();
            _planningStorage = false;
            CancelPlanning();
        }

        private void CreateStorageSlot(Vector2 pos)
        {
            InventoryController.Instance.SpawnStorageSlot(pos);
        }
    }
}
