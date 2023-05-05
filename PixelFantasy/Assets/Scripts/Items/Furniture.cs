using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Items
{
    public class Furniture : Interactable
    {
        [SerializeField] private Transform _spritesRoot;
        
        private SpriteRenderer[] _allSprites;
        private List<Material> _materials = new List<Material>();
        private int _fadePropertyID;
        
        private FurnitureItemData _furnitureItemData;
        private bool _isBuilt;
        private bool _isPlanning;
        private float _remainingWork;
        private bool _isOutlineLocked;

        public FurnitureItemData FurnitureItemData => _furnitureItemData;
        
        protected virtual void Awake()
        {
            _allSprites = _spritesRoot.GetComponentsInChildren<SpriteRenderer>();
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            foreach (var spriteRenderer in _allSprites)
            {
                _materials.Add(spriteRenderer.material);
            }
        }

        public virtual void Init(FurnitureItemData furnitureItemData)
        {
            _furnitureItemData = furnitureItemData;
        }
        
        public void Plan(FurnitureItemData furnitureItemData)
        {
            Init(furnitureItemData);
            _remainingWork = _furnitureItemData.WorkCost;
            // Follows cursor
            _isPlanning = true;
        }

        public void PrepareToBuild()
        {
            // Stop Following cursor, set build task
            _isPlanning = false;
            CreateFurnitureHaulingTask();
        }
        
        private void CreateFurnitureHaulingTask()
        {
            // If item exists, claim it
            var storage = InventoryManager.Instance.ClaimItem(_furnitureItemData);
            if (storage != null)
            {
                Task task = new Task
                {
                    TaskId = "Place Furniture",
                    Category = TaskCategory.Hauling,
                    Requestor = this,
                    Payload = storage.UniqueId,
                    Profession = _furnitureItemData.CraftersProfession,
                };
                TaskManager.Instance.AddTask(task);
            }
            else
            {
                // TODO: Set up build order
            }
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

        protected void CompletePlacement()
        {
            _remainingWork = _furnitureItemData.WorkCost;
            ColourArt(ColourStates.Built);
            _isBuilt = true;
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
            if(!_isBuilt) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if(!_isBuilt) return;
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
                    if (!_isBuilt)
                    {
                        ColourArt(ColourStates.Blueprint);
                    }
                }
            }
        }
        
        public bool CheckPlacement()
        {
            bool result = true;

            if (Helper.IsGridPosValidToBuild(transform.position, _furnitureItemData.InvalidPlacementTags))
            {
                ColourArt(ColourStates.CanPlace);
            }
            else
            {
                ColourArt(ColourStates.CantPlace);
                result = false;
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
