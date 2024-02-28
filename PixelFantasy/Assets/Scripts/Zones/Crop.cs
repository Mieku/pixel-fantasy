using System.Collections.Generic;
using Buildings;
using Controllers;
using DataPersistence;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zones
{
    public class Crop : PlayerInteractable, IPersistent
    {
        [SerializeField] private SpriteRenderer _soilHoleRenderer;
        [SerializeField] private SpriteRenderer _cropRenderer;
        [SerializeField] private RuleTile _dirtRuleTile;
        
        // Soil Types
        [SerializeField] private Sprite _soilHole, _soilCovered, _soilWatered;

        private CropData _cropData;
        private bool _isTilled;
        private bool _isWatered;
        private bool _wateringTaskSet;
        private bool _cropReadyToHarvest;
        private bool _harvestTaskSet;
        private bool _hasCrop;
        private bool _pauseGrowing;
        private float _timeWithWater;
        private float _timeGrowing;
        [SerializeField] private List<string> _invalidPlacementTags;
        private Tilemap _dirtTilemap;
        private CropData _pendingCropSwap;

        private float _remainingTillWork;
        private float _remainingDigWork;
        private float _remainingPlantingWork;
        private float _remainingWaterWork;
        private float _remainingHarvestWork;

        private Building _building;
        
        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var invalidPlacementTag in _invalidPlacementTags)
                {
                    clone.Add(invalidPlacementTag);
                }

                return clone;
            }
        }
        
        private void Awake()
        {
            _dirtTilemap =
                TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
        }

        public void Init(CropData cropData, Building linkedBuilding)
        {
            _cropData = cropData;
            _soilHoleRenderer.gameObject.SetActive(false);
            _cropRenderer.gameObject.SetActive(false);
            
            UpdateSprite(true);
            ShowBlueprint(true);
            ClearPlantsForClearingGrass();

            _remainingTillWork = GetTillWorkAmount();
            _remainingDigWork = GetDigWorkAmount();
            _remainingPlantingWork = GetPlantingWorkAmount();
            _remainingWaterWork = GetWaterWorkAmount();
            _remainingHarvestWork = GetHarvestWorkAmount();

            _building = linkedBuilding;
        }

        public float GetTillWorkAmount()
        {
            return 3;
        }
        
        public float GetDigWorkAmount()
        {
            return 5;
        }
        
        public float GetPlantingWorkAmount()
        {
            return 5;
        }
        
        public float GetWaterWorkAmount()
        {
            return 2;
        }
        
        public float GetHarvestWorkAmount()
        {
            return 8;
        }
        
        public bool DoTillingWork(float workAmount)
        {
            _remainingTillWork -= workAmount;
            if (_remainingTillWork <= 0)
            {
                OnDirtDug();
                return true;
            }
            
            return false;
        }
        
        public bool DoDiggingWork(float workAmount)
        {
            _remainingDigWork -= workAmount;
            if (_remainingDigWork <= 0)
            {
                HoleDug();
                return true;
            }
            
            return false;
        }
        
        public bool DoPlantingWork(float workAmount)
        {
            _remainingPlantingWork -= workAmount;
            if (_remainingPlantingWork <= 0)
            {
                CropPlanted();
                return true;
            }
            
            return false;
        }
        
        public bool DoWateringWork(float workAmount)
        {
            _remainingWaterWork -= workAmount;
            if (_remainingWaterWork <= 0)
            {
                CropWatered();
                return true;
            }
            
            return false;
        }
        
        public bool DoHarvestingWork(float workAmount)
        {
            _remainingHarvestWork -= workAmount;
            if (_remainingHarvestWork <= 0)
            {
                CropHarvested();
                return true;
            }
            
            return false;
        }

        public bool DoClearingWork(float workAmount)
        {
            _remainingPlantingWork -= workAmount;
            if (_remainingPlantingWork <= 0)
            {
                CropCleared();
                return true;
            }
            
            return false;
        }
        
        public bool DoCropSwappingWork(float workAmount)
        {
            _remainingPlantingWork -= workAmount;
            if (_remainingPlantingWork <= 0)
            {
                CropSwapped();
                return true;
            }
            
            return false;
        }
        
        public float TillWorkDone(float workAmount)
        {
            _remainingTillWork -= workAmount;
            return _remainingTillWork;
        }
        
        public float DigWorkDone(float workAmount)
        {
            _remainingDigWork -= workAmount;
            return _remainingDigWork;
        }
        
        public float PlantingWorkDone(float workAmount)
        {
            _remainingPlantingWork -= workAmount;
            return _remainingPlantingWork;
        }
        
        public float WaterWorkDone(float workAmount)
        {
            _remainingWaterWork -= workAmount;
            return _remainingWaterWork;
        }
        
        public float HarvestWorkDone(float workAmount)
        {
            _remainingHarvestWork -= workAmount;
            return _remainingHarvestWork;
        }
        
        private void Update()
        {
            UpdateWatering();
            UpdateCropGrowth();
        }
        
        private void ClearNatureFromTile()
        {
            var objectsOnTile = Helper.GetGameObjectsOnTile(transform.position);
            foreach (var tileObj in objectsOnTile)
            {
                var growResource = tileObj.GetComponent<GrowingResource>();
                if (growResource != null)
                {
                    Debug.LogError("Not built yet");
                    // growResource.TaskRequestors.Add(gameObject);
                    //
                    // // if (!growResource.QueuedToCut)
                    // // {
                    // //     growResource.CreateTaskById("Cut Plant");
                    // // }
                    // growResource.CreateTaskById("Cut Plant");
                }
            }
        }
        
        public void ClearPlantsForClearingGrass()
        {
            // Check if there are any plants on the tile, if so, cut them down first
            if (Helper.DoesGridContainTag(transform.position, "Nature"))
            {
                ClearNatureFromTile();
                return;
            }
            
            if (!Helper.DoesGridContainTag(transform.position, "Dirt"))
            {
                CreateTillSoilTask();
            }
            else
            {
                OnDirtDug();
            }
        }
        
        private void ColourRenderers(Color colour)
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetColor(cell, colour);
        }
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                ColourRenderers(Librarian.Instance.GetColour("Blueprint"));
            }
            else
            {
                ColourRenderers(Color.white);
            }
        }
        
        public void UpdateSprite(bool informNeighbours)
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetTile(cell, _dirtRuleTile);
        }
        
        private void UpdateWatering()
        {
            if(_hasCrop && !_pauseGrowing)
            {
                if (!_isWatered && !_wateringTaskSet && !_cropReadyToHarvest)
                {
                    CreateWaterCropTask();
                }

                if (_isWatered)
                {
                    _timeWithWater += TimeManager.Instance.DeltaTime;
                    if (_timeWithWater > _cropData.WaterFrequencySec)
                    {
                        _timeWithWater = 0f;
                        DrySoil();
                    }
                }
            }
        }
        
        private void UpdateCropGrowth()
        {
            if (_hasCrop && !_pauseGrowing)
            {
                if (_isWatered && !_cropReadyToHarvest)
                {
                    _timeGrowing += TimeManager.Instance.DeltaTime;
                    RefreshCropStage(_timeGrowing);
                    if (_timeGrowing > _cropData.TimeToHarvestSec)
                    {
                        _timeGrowing = 0f;
                        CropReadyToHarvest();
                    }
                }
            }
        }

        private void RefreshCropStage(float timeGrowing)
        {
            var cropImage = _cropData.GetCropImage(timeGrowing);
            if (cropImage == null)
            {
                _cropRenderer.gameObject.SetActive(false);
            }
            else
            {
                _cropRenderer.sprite = cropImage;
                _cropRenderer.gameObject.SetActive(true);
            }
        }

        private void CropReadyToHarvest()
        {
            _cropReadyToHarvest = true;
            CreateHarvestCropTask();
        }

        private void DrySoil()
        {
            _isWatered = false;
            _soilHoleRenderer.sprite = _soilCovered;
        }

        private void CreateTillSoilTask()
        {
            Task task = new Task("Till Soil", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }

        public void OnDirtDug()
        {
            ShowBlueprint(false);
            _isTilled = true;
            _remainingTillWork = GetTillWorkAmount();

            CreateDigHoleTask();
        }
        
        private void CreateDigHoleTask()
        {
            Task task = new Task("Dig Hole", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }
        
        public void HoleDug()
        {
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);
            _remainingDigWork = GetDigWorkAmount();

            CreatePlantCropTask();
        }
        
        private void CreatePlantCropTask()
        {
            Task task = new Task("Plant Crop", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }

        public void CropPlanted()
        {
            _soilHoleRenderer.sprite = _soilCovered;
            _hasCrop = true;
            _remainingPlantingWork = GetPlantingWorkAmount();
        }
        
        private void CreateWaterCropTask()
        {
            _wateringTaskSet = true;

            Task task = new Task("Water Crop", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }

        public void CropWatered()
        {
            _isWatered = true;
            _wateringTaskSet = false;
            _soilHoleRenderer.sprite = _soilWatered;
            _remainingWaterWork = GetWaterWorkAmount();
        }
        
        private void CreateHarvestCropTask()
        {
            Task task = new Task("Harvest Crop", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }

        public void CropHarvested()
        {
            _cropReadyToHarvest = false;
            
            _cropRenderer.gameObject.SetActive(false);
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);
            _remainingHarvestWork = GetHarvestWorkAmount();
            
            CreatePlantCropTask();
            
            // Spawn the crop
            Spawner.Instance.SpawnItem(_cropData.HarvestedItem, transform.position, true, _cropData.AmountToHarvest);
        }

        public void ChangeCrop(CropData newCrop)
        {
            _pauseGrowing = true;
            
            if (newCrop == null)
            {
                // Remove Crop
                RemoveCrop();
            }
            else
            {
                // Change the crop
                if (_hasCrop == false) // If no crop is planted yet, continue with actions but with new crop
                {
                    _cropData = newCrop;
                    _pauseGrowing = false;
                }
                else
                {
                    SwapCrop(newCrop);
                }
            }
        }

        public void RemoveCrop()
        {
            ClearCrop();
            // Kinling goes to crop, does Digging anim and the soil and crop are removed
        }

        public void SwapCrop(CropData newCrop)
        {
            _pendingCropSwap = newCrop;
            CreateSwapCropTask();
            // Kinling goes to the crop, does digging anim and clears the crop and brings it back to the tilled state
        }
        
        private void CreateSwapCropTask()
        {
            Task task = new Task("Crop Swap", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }
        
        private void CreateClearCropTask()
        {
            Task task = new Task("Clear Crop", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }
        
        public void CropSwapped()
        {
            _cropReadyToHarvest = false;
            _isWatered = false;
            _wateringTaskSet = false;
            _harvestTaskSet = false;
            _hasCrop = false;

            _timeGrowing = 0f;
            _remainingTillWork = GetTillWorkAmount();
            _remainingDigWork = GetDigWorkAmount();
            _remainingPlantingWork = GetPlantingWorkAmount();
            _remainingWaterWork = GetWaterWorkAmount();
            _remainingHarvestWork = GetHarvestWorkAmount();
            
            _cropData = _pendingCropSwap;
            _cropRenderer.gameObject.SetActive(false);

            _pauseGrowing = false;
            HoleDug();
        }

        private void ClearCrop()
        {
            if (_isTilled)
            {
                CreateClearCropTask();
            }
            else
            {
                CropCleared();
            }
        }

        public void CropCleared()
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetTile(cell, null);
            Destroy(this.gameObject);
        }

        private Sprite GetSpriteByName(string spriteName)
        {
            if (spriteName == _soilHole.name)
            {
                return _soilHole;
            }
            if (spriteName == _soilCovered.name)
            {
                return _soilCovered;
            }
            if (spriteName == _soilWatered.name)
            {
                return _soilWatered;
            }
            
            Debug.LogError("Unknown Sprite: " + spriteName);
            return null;
        }

        public object CaptureState()
        {
            return new CropState
            {
                UID = UniqueId,
                Position = transform.position,
                CropData = _cropData,
                IsTilled = _isTilled,
                IsWatered = _isWatered,
                WateringTaskSet = _wateringTaskSet,
                CropReadyToHarvest = _cropReadyToHarvest,
                HarvestTaskSet = _harvestTaskSet,
                HasCrop = _hasCrop,
                TimeWithWater = _timeWithWater,
                TimeGrowing = _timeGrowing,
                HoleSprite = _soilHoleRenderer.sprite.name,
                CropSprite = _cropRenderer.sprite,
                HoleRendererActive = _soilHoleRenderer.gameObject.activeSelf,
                CropRendererActive = _cropRenderer.gameObject.activeSelf,
                RemainingTillWork = _remainingTillWork,
                RemainingDigWork = _remainingDigWork,
                RemainingPlantingWork = _remainingPlantingWork,
                RemainingWaterWork = _remainingWaterWork,
                RemainingHarvestWork = _remainingHarvestWork,
            };
        }

        public void RestoreState(object data)
        {
            var state = (CropState)data;
            UniqueId = state.UID;
            transform.position = state.Position;
            _cropData = state.CropData;
            _isTilled = state.IsTilled;
            _isWatered = state.IsWatered;
            _wateringTaskSet = state.WateringTaskSet;
            _cropReadyToHarvest = state.CropReadyToHarvest;
            _harvestTaskSet = state.HarvestTaskSet;
            _hasCrop = state.HasCrop;
            _timeWithWater = state.TimeWithWater;
            _timeGrowing = state.TimeGrowing;
            _soilHoleRenderer.sprite = GetSpriteByName(state.HoleSprite);
            _cropRenderer.sprite = state.CropSprite;
            _soilHoleRenderer.gameObject.SetActive(state.HoleRendererActive);
            _cropRenderer.gameObject.SetActive(state.CropRendererActive);
            _remainingTillWork = state.RemainingTillWork;
            _remainingDigWork = state.RemainingDigWork;
            _remainingPlantingWork = state.RemainingPlantingWork;
            _remainingWaterWork = state.RemainingWaterWork;
            _remainingHarvestWork = state.RemainingHarvestWork;

            ShowBlueprint(!_isTilled);
        }

        public struct CropState
        {
            public string UID;
            public Vector2 Position;
            public CropData CropData;
            public bool IsTilled;
            public bool IsWatered;
            public bool WateringTaskSet;
            public bool CropReadyToHarvest;
            public bool HarvestTaskSet;
            public bool HasCrop;
            public float TimeWithWater;
            public float TimeGrowing;
            public bool HoleRendererActive;
            public bool CropRendererActive;
            public string HoleSprite;
            public Sprite CropSprite;
            public float RemainingTillWork;
            public float RemainingDigWork;
            public float RemainingPlantingWork;
            public float RemainingWaterWork;
            public float RemainingHarvestWork;
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
