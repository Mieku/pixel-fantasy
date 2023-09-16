using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using CodeMonkey.Utils;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Items
{
    public class Furniture : PlayerInteractable
    {
        [SerializeField] private Transform _spritesRoot;
        [SerializeField] private Transform _useageMarkersRoot;
        [SerializeField] protected FurnitureItemData _furnitureItemData;

        public bool IsBuilt;
        
        private SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        private List<Material> _materials = new List<Material>();
        private int _fadePropertyID;
        
        private bool _isPlanning;
        private float _remainingWork;
        private bool _isOutlineLocked;
        protected Building _parentBuilding;
        //protected RoomZone _parentRoom;

        public FurnitureItemData FurnitureItemData => _furnitureItemData;
        public Building ParentBuilding => _parentBuilding;
        //public RoomZone ParentRoom => _parentRoom;
        
        protected virtual void Awake()
        {
            _allSprites = _spritesRoot.GetComponentsInChildren<SpriteRenderer>();
            _useageMarkers = _useageMarkersRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            foreach (var spriteRenderer in _allSprites)
            {
                _materials.Add(spriteRenderer.material);
            }
        }

        private void OnDestroy()
        {
            if (_parentBuilding != null)
            {
                _parentBuilding.DeregisterFurniture(this);
            }
        }

        protected virtual void Start()
        {
            if (_furnitureItemData != null && !_isPlanning)
            {
                if (IsBuilt)
                {
                    CompletePlacement();
                }
                else
                {
                    _remainingWork = _furnitureItemData.WorkCost;
                    PrepareToBuild();
                }
            }
        }

        public virtual void Init(FurnitureItemData furnitureItemData)
        {
            _furnitureItemData = furnitureItemData;
        }

        public List<Transform> UseagePositions()
        {
            List<Transform> results = new List<Transform>();
            foreach (var marker in _useageMarkers)
            {
                results.Add(marker.transform);
            }

            return results;
        }
        
        public void Plan(FurnitureItemData furnitureItemData)
        {
            Init(furnitureItemData);
            _remainingWork = _furnitureItemData.WorkCost;
            // Follows cursor
            _isPlanning = true;
            DisplayUseageMarkers(true);
        }

        public void PrepareToBuild()
        {
            DisplayUseageMarkers(false);
            // Stop Following cursor, set build task
            _isPlanning = false;
            ColourArt(ColourStates.Blueprint);
            CreateFurnitureHaulingTask();
        }

        public void DisplayUseageMarkers(bool showMarkers)
        {
            foreach (var marker in _useageMarkers)
            {
                marker.gameObject.SetActive(showMarkers);
            }
        }
        
        private void CreateFurnitureHaulingTask()
        {
            // If item exists, claim it
            var claimedItem = InventoryManager.Instance.ClaimItem(_furnitureItemData);
            if (claimedItem != null)
            {
                Task task = new Task
                {
                    TaskId = "Place Furniture",
                    Requestor = this,
                    Materials = new List<CraftingBill.RequestedItemInfo>(){ new CraftingBill.RequestedItemInfo(claimedItem, 1)},
                    TaskType = TaskType.Haul,
                };
                TaskManager.Instance.AddTask(task);
            }
            else
            {
                CraftingBill bill = new CraftingBill
                {
                    ItemToCraft = _furnitureItemData,
                    Requestor = this,
                    OnCancelled = OnCraftingBillCancelled,
                };
                TaskManager.Instance.AddBill(bill);
            }
        }

        public void OnCraftingBillCancelled()
        {
            
        }

        public override void ReceiveItem(Item item)
        {
            Destroy(item.gameObject);
        }
        
        public bool DoPlacement(float workAmount)
        {
            _remainingWork -= workAmount;
            if (_remainingWork <= 0)
            {
                CompletePlacement();
                return true;
            }
            
            return false;
        }

        protected virtual void CompletePlacement()
        {
            DisplayUseageMarkers(false);
            _remainingWork = _furnitureItemData.WorkCost;
            ColourArt(ColourStates.Built);
            IsBuilt = true;
            
            // Check if this was placed in a room, if so add it to the room
            var building = Helper.IsPositionInBuilding(transform.position);
            if (building != null)
            {
                AssignBuilding(building);
            }
        }

        private void AssignBuilding(Building building)
        {
            if (building == null)
            {
                Debug.LogError($"Attmepted to assign {_furnitureItemData.ItemName} to null building");
                return;
            }

            _parentBuilding = building;
            building.RegisterFurniture(this);
        }
        
        private void Update()
        {
            if (_isPlanning)
            {
                FollowCursor();
                CheckPlacement();
            }
        }
        
        private void OnMouseEnter()
        {
            if(!IsBuilt) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if(!IsBuilt) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(false);
        }

        private void OnMouseDown()
        {
            if(_isPlanning) return;
            
            OnClicked();
        }

        protected virtual void OnClicked()
        {
            // TODO: Build me!
        }

        private void FollowCursor()
        {
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }
        
        public void LockOutline(bool isLocked, bool showOutline)
        {
            _isOutlineLocked = isLocked;
            TriggerOutline(showOutline);
        }
        
        private void TriggerOutline(bool showOuline)
        {
            foreach (var material in _materials)
            {
                if (showOuline)
                {
                    material.SetFloat(_fadePropertyID, 1);
                    ColourArt(ColourStates.Built);
                }
                else
                {
                    material.SetFloat(_fadePropertyID, 0);
                    if (!IsBuilt)
                    {
                        ColourArt(ColourStates.Blueprint);
                    }
                }
            }
        }
        
        public virtual bool CheckPlacement()
        {
            bool result = Helper.IsGridPosValidToBuild(transform.position, _furnitureItemData.InvalidPlacementTags);

            // Check the useage markers
            if (_useageMarkers != null)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, _furnitureItemData.InvalidPlacementTags))
                    {
                        //ColourArt(ColourStates.CanPlace);
                        marker.color = Color.white;
                        markersPass = true;
                    }
                    else
                    {
                        //ColourArt(ColourStates.CantPlace);
                        marker.color = Color.red;
                    }
                }

                if (!markersPass)
                {
                    result = false;
                }
            }

            if (result)
            {
                ColourArt(ColourStates.CanPlace);
            }
            else
            {
                ColourArt(ColourStates.CantPlace);
            }
            
            return result;
        }
        
        public void ColourArt(ColourStates colourState)
        {
            Color colour;
            switch (colourState)
            {
                case ColourStates.Built:
                    colour = Color.white;
                    break;
                case ColourStates.Blueprint:
                    colour = Librarian.Instance.GetColour("Blueprint");
                    break;
                case ColourStates.CanPlace:
                    colour = Librarian.Instance.GetColour("Placement Green");
                    break;
                case ColourStates.CantPlace:
                    colour = Librarian.Instance.GetColour("Placement Red");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colourState), colourState, null);
            }
        
            foreach (var spriteRenderer in _allSprites)
            {
                spriteRenderer.color = colour;
            }
        }
        
        public enum ColourStates
        {
            Built,
            Blueprint,
            CanPlace,
            CantPlace,
        }
    }
}
