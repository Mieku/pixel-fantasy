using System;
using System.Collections.Generic;
using Actions;
using Gods;
using Interfaces;
using ScriptableObjects;
using Tasks;
using Characters;
using Controllers;
using DataPersistence;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Crop : Interactable, IPersistent
    {
        [SerializeField] private SpriteRenderer _soilHoleRenderer;
        [SerializeField] private SpriteRenderer _cropRenderer;

        // Soil Types
        [SerializeField] private Sprite _soilHole, _soilCovered, _soilWatered;

        private CropData _cropData;
        private bool _isTilled;
        private bool _isWatered;
        private bool _wateringTaskSet;
        private bool _cropReadyToHarvest;
        private bool _harvestTaskSet;
        private bool _hasCrop;
        private float _timeWithWater;
        private float _timeGrowing;
        [SerializeField] private List<string> _invalidPlacementTags;
        private Tilemap _flooringTilemap;
        
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
            _flooringTilemap =
                TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
        }

        public void Init(CropData cropData)
        {
            _cropData = cropData;
            _soilHoleRenderer.gameObject.SetActive(false);
            _cropRenderer.gameObject.SetActive(false);
            
            ShowBlueprint(true);
            ClearPlantsForClearingGrass();
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
                    growResource.TaskRequestors.Add(gameObject);

                    // if (!growResource.QueuedToCut)
                    // {
                    //     growResource.CreateTaskById("Cut Plant");
                    // }
                    growResource.CreateTaskById("Cut Plant");
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

            if (Helper.DoesGridContainTag(transform.position, "Grass"))
            {
                // if clear, clear the grass
                CreateTaskById("Till Soil");
            }
            else
            {
                OnDirtDug();
            }
        
            
        }
        
        private void ColourRenderers(Color colour)
        {
            var cell = _flooringTilemap.WorldToCell(transform.position);
            _flooringTilemap.SetColor(cell, colour);
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
            var cell = _flooringTilemap.WorldToCell(transform.position);
            _flooringTilemap.SetTile(cell, null);
        }
        
        private void UpdateWatering()
        {
            if(_hasCrop)
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
            if (_hasCrop)
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

        public void OnDirtDug()
        {
            UpdateSprite(true);
            ShowBlueprint(false);
            _isTilled = true;

            CreateDigHoleTask();
        }

        private void CreateDigHoleTask()
        {
            var digAction = Librarian.Instance.GetAction("Dig Hole");
            CreateTask(digAction);
        }

        public void HoleDug()
        {
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);

            CreatePlantCropTask();
        }

        private void CreatePlantCropTask()
        {
            var plantCropAction = Librarian.Instance.GetAction("Plant Crop");
            CreateTask(plantCropAction);
        }

        public void CropPlanted()
        {
            _soilHoleRenderer.sprite = _soilCovered;
            _hasCrop = true;
        }

        private void CreateWaterCropTask()
        {
            _wateringTaskSet = true;
            
            var waterCropAction = Librarian.Instance.GetAction("Water Crop");
            CreateTask(waterCropAction);
        }

        public void CropWatered()
        {
            _isWatered = true;
            _wateringTaskSet = false;
            _soilHoleRenderer.sprite = _soilWatered;
        }

        private void CreateHarvestCropTask()
        {
            var harvestCropAction = Librarian.Instance.GetAction("Harvest Crop");
            CreateTask(harvestCropAction);
        }

        public void CropHarvested()
        {
            _cropReadyToHarvest = false;
            
            _cropRenderer.gameObject.SetActive(false);
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);
            CreatePlantCropTask();
            
            // Spawn the crop
            Spawner.Instance.SpawnItem(_cropData.HarvestedItem, transform.position, true, _cropData.AmountToHarvest);
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
                PendingTasks = PendingTasks,
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
            
            ShowBlueprint(!_isTilled);
            
            RestoreTasks(state.PendingTasks);
        }

        public struct CropState
        {
            public string UID;
            public Vector2 Position;
            public List<ActionBase> PendingTasks;
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
        }
    }
}
