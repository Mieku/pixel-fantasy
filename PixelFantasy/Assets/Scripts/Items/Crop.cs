using System;
using System.Collections.Generic;
using Gods;
using Interfaces;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Crop : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _soilHoleRenderer;
        [SerializeField] private SpriteRenderer _cropRenderer;
        
        // Soil Types
        [SerializeField] private Sprite _soilHole, _soilCovered, _soilWatered;

        private CropData _cropData;
        private bool _isWatered;
        private bool _wateringTaskSet;
        private bool _cropReadyToHarvest;
        private bool _harvestTaskSet;
        private bool _hasCrop;
        private DirtTile _dirtTile;
        private TaskMaster taskMaster => TaskMaster.Instance;
        private List<int> _assignedTaskRefs = new List<int>();
        private UnitTaskAI _incomingUnit;
        private void Awake()
        {
            _dirtTile = GetComponent<DirtTile>();
        }

        public void Init(CropData cropData)
        {
            _cropData = cropData;
            _dirtTile.Init(OnDirtDug);
            _soilHoleRenderer.gameObject.SetActive(false);
            _cropRenderer.gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateWatering();
            UpdateCropGrowth();
        }

        private float _timeWithWater;
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
                    _timeWithWater += Time.deltaTime;
                    if (_timeWithWater > _cropData.WaterFrequencySec)
                    {
                        _timeWithWater = 0f;
                        DrySoil();
                    }
                }
            }
        }

        private float _timeGrowing;
        private void UpdateCropGrowth()
        {
            if (_hasCrop)
            {
                if (_isWatered && !_cropReadyToHarvest)
                {
                    _timeGrowing += Time.deltaTime;
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

        private void OnDirtDug()
        {
            CreateDigHoleTask();
        }

        private void CreateDigHoleTask()
        {
            var task = new FarmingTask.DigHole()
            {
                claimHole = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                holePosition = Helper.ConvertMousePosToGridPos(transform.position),
                workAmount = 5,
                completeWork = HoleDug
            };
        
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        private void HoleDug()
        {
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);
            CreatePlantCropTask();
        }

        private void CreatePlantCropTask()
        {
            var task = new FarmingTask.PlantCrop()
            {
                claimHole = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                holePosition = Helper.ConvertMousePosToGridPos(transform.position),
                workAmount = 5,
                completeWork = CropPlanted
            };
        
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        private void CropPlanted()
        {
            _soilHoleRenderer.sprite = _soilCovered;
            _hasCrop = true;
        }

        private void CreateWaterCropTask()
        {
            _wateringTaskSet = true;
            
            var task = new FarmingTask.WaterCrop()
            {
                claimCrop = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                cropPosition = Helper.ConvertMousePosToGridPos(transform.position),
                workAmount = 2,
                completeWork = CropWatered
            };
        
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        private void CropWatered()
        {
            _isWatered = true;
            _wateringTaskSet = false;
            _soilHoleRenderer.sprite = _soilWatered;
        }

        private void CreateHarvestCropTask()
        {
            var task = new FarmingTask.HarvestCrop()
            {
                claimCrop = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                cropPosition = Helper.ConvertMousePosToGridPos(transform.position),
                workAmount = 8,
                completeWork = CropHarvested
            };
        
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        private void CropHarvested()
        {
            _cropReadyToHarvest = false;
            
            _soilHoleRenderer.sprite = _soilHole;
            _soilHoleRenderer.gameObject.SetActive(true);
            CreatePlantCropTask();
            
            // Spawn the crop
            Spawner.Instance.SpawnItem(_cropData.HarvestedItem, transform.position, true, _cropData.AmountToHarvest);
        }
    }
}
