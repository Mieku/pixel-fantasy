using System;
using System.Collections.Generic;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using Systems.Buildings.Scripts;
using Systems.Input_Management;
using Systems.Notifications.Scripts;
using UnityEngine;
using Zones;

namespace Managers
{
    public class Spawner : Singleton<Spawner>
    {
        [SerializeField] private Transform _flooringParent;
        [SerializeField] private GameObject _soilPrefab;
        
        [SerializeField] private SpriteRenderer _placementIcon;
        [SerializeField] private Sprite _genericPlacementSprite;

        [SerializeField] private Transform _kinlingsParent;
        [SerializeField] private Kinling _kinlingPrefab;

        private bool _showPlacement;
        private List<string> _invalidPlacementTags = new List<string>();
        private List<string> _requiredPlacementTags = new List<string>();
        private Color? _colourOverride;
        
        // Structure
        private bool _planningStructure;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private List<Vector2> _plannedGrid = new List<Vector2>();
        
        private PlacementDirection _prevPlacementDirection;
        public CropSettings CropSettings { get; set; }
        
        
        private void OnEnable()
        {
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown += GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld += GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDisable()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown -= GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld -= GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        protected virtual void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            if (isOverUI) return;
            if (inputState == PlayerInputState.BuildFlooring
                || inputState == PlayerInputState.BuildFarm)
            {
                _planningStructure = true;
                _startPos = Helper.SnapToGridPos(mousePos);
            }
        }
        
        protected virtual void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            if (inputState == PlayerInputState.BuildFarm)
            {
                PlanFarm(mousePos);
            }
        }
        
        protected virtual void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            
            if (inputState == PlayerInputState.BuildFarm)
            {
                if (_planningStructure)
                {
                    SpawnPlannedFarm();
                }
                else
                {
                    SpawnSoilTile(Helper.SnapToGridPos(mousePos), CropSettings);
                }
            }
        }
        
        protected virtual void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }
        
        protected virtual void GameEvents_OnRightClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }
        
        protected virtual void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            CancelInput();
        }

        public void CancelInput()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ShowPlacementIcon(false);
            _invalidPlacementTags.Clear();
            _requiredPlacementTags = null;
            _prevPlacementDirection = default;
        }

        public void ShowPlacementIconForReqTags(bool show, Sprite icon = null,
            List<String> requiredPlacementTags = null, float sizeOverride = 1f, Color? colourOverride = null)
        {
            _placementIcon.enabled = true;
            _colourOverride = colourOverride;

            if (icon == null)
            {
                icon = _genericPlacementSprite;
            }
            
            List<string> tags = null;
            if (requiredPlacementTags != null)
            {
                tags = new List<string>(requiredPlacementTags);
            }
            
            if (_placementIcon.transform.childCount > 0)
            {
                var child = _placementIcon.transform.GetChild(0).gameObject;
                Destroy(child);
            }
            
            if (show)
            {
                _placementIcon.gameObject.transform.localScale = new Vector2(sizeOverride, sizeOverride);
                _placementIcon.sprite = icon;
                _placementIcon.gameObject.SetActive(true);
                _showPlacement = true;
                _requiredPlacementTags = tags;
                _invalidPlacementTags = new List<string>();
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
            }
        }

        public void ShowPlacementIcon(bool show, Sprite icon = null, List<String> invalidPlacementTags = null, float sizeOverride = 1f, Color? colourOverride = null)
        {
            _placementIcon.enabled = true;
            _colourOverride = colourOverride;

            if (icon == null)
            {
                icon = _genericPlacementSprite;
            }

            List<string> tags = null;
            if (invalidPlacementTags != null)
            {
                tags = new List<string>(invalidPlacementTags);
            }
            
            if (_placementIcon.transform.childCount > 0)
            {
                var child = _placementIcon.transform.GetChild(0).gameObject;
                Destroy(child);
            }
            
            if (show)
            {
                _placementIcon.gameObject.transform.localScale = new Vector2(sizeOverride, sizeOverride);
                _placementIcon.sprite = icon;
                _placementIcon.gameObject.SetActive(true);
                _showPlacement = true;
                _invalidPlacementTags = tags;
                _requiredPlacementTags = new List<string>();
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
                _requiredPlacementTags = null;
            }
        }

        private void Update()
        {
            if (_showPlacement)
            {
                var gridPos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
                _placementIcon.transform.position = gridPos;
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags, _requiredPlacementTags))
                {
                    Color placementColour = (Color)(_colourOverride != null ? _colourOverride : Librarian.Instance.GetColour("Placement Green"));
                    _placementIcon.color = placementColour;
                    if (_placementIcon.transform.childCount > 0)
                    {
                        var renderers = _placementIcon.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var renderer in renderers)
                        {
                            renderer.color = placementColour;
                        }
                    }
                }
                else
                {
                    _placementIcon.color = Librarian.Instance.GetColour("Placement Red");
                    if (_placementIcon.transform.childCount > 0)
                    {
                        var renderers = _placementIcon.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var renderer in renderers)
                        {
                            renderer.color = Librarian.Instance.GetColour("Placement Red");
                        }
                    }
                }
            }
        }

        private int _agentPriority = 0;
        public Kinling SpawnKinling(string fullname, Vector2 spawnPosition)
        {
            Kinling kinling = Instantiate(_kinlingPrefab, _kinlingsParent);
            kinling.transform.position = spawnPosition;
            kinling.gameObject.name = $"Kinling_{fullname}";
            
            kinling.KinlingAgent.SetPriority(_agentPriority);
            _agentPriority++;
            if (_agentPriority > 99)
            {
                _agentPriority = 0;
            }
            
            return kinling;
        }
        
        #region Structure
        
        private void ClearPlannedBlueprint()
        {
            foreach (var blueprint in _blueprints)
            {
                Destroy(blueprint);
            }
            _blueprints.Clear();
        }
        
        #endregion
        
        private void PlanFarm(Vector2 mousePos)
        {
            if (!_planningStructure) return;

            Vector3 curGridPos = Helper.SnapToGridPos(mousePos);
            List<Vector2> gridPositions = new List<Vector2>();
            gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);

            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous display, then display new blueprints
                ClearPlannedBlueprint();

                foreach (var gridPos in gridPositions)
                {
                    var blueprint = new GameObject("blueprint", typeof(SpriteRenderer));
                    blueprint.transform.position = gridPos;
                    var spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
                    var soil = _soilPrefab.GetComponent<Crop>();
                    spriteRenderer.sprite = _genericPlacementSprite;
                    if (Helper.IsGridPosValidToBuild(gridPos, soil.InvalidPlacementTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "DirtTile";
                    _blueprints.Add(blueprint);
                }
            }
        }

        private void SpawnPlannedFarm()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                var soil = _soilPrefab.GetComponent<Crop>();
                if (Helper.IsGridPosValidToBuild(gridPos, soil.InvalidPlacementTags))
                {
                    SpawnSoilTile(gridPos, CropSettings);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
            CancelInput();
        }

        public void SpawnSoilTile(Vector2 spawnPos, CropSettings cropSettings)
        {
            var soil = Instantiate(_soilPrefab, spawnPos, Quaternion.identity);
            soil.transform.SetParent(_flooringParent);
            soil.GetComponent<Crop>().Init(cropSettings);
        }
    }
}
