using System;
using System.Collections.Generic;
using Buildings;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class Librarian : Singleton<Librarian>
    {
        [SerializeField] private List<ColourData> _colourLibrary;
        [SerializeField] private List<StructureData> _structureLibrary;
        [SerializeField] private List<FloorData> _floorLibrary;
        [SerializeField] private List<DoorData> _doorLibrary;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private List<SpriteRef> _orderIcons;
        [SerializeField] private List<CropData> _cropLibrary;
        [SerializeField] private List<GrowingResourceData> _growingResourceLibrary;
        [SerializeField] private List<HairData> _hairLibrary;
        [SerializeField] private List<ItemData> _itemDataLibrary;
        [SerializeField] private List<Building> _buildingLibrary;
        [SerializeField] private List<Command> _commandLibrary;
        [SerializeField] private List<WallData> _wallLibrary;
        [SerializeField] private List<RoofData> _roofLibrary;
        [SerializeField] private List<NeedData> _needLibrary;
        [SerializeField] private List<JobData> _jobLibrary;
        [SerializeField] private List<RaceData> _races;

        public RaceData GetRace(string raceName)
        {
            var result = _races.Find(race => race.RaceName == raceName);
            if (result == null)
            {
                Debug.LogError($"Unkown Race: {raceName}");
            }

            return result;
        }

        public JobData GetJob(string jobName)
        {
            var result = _jobLibrary.Find(job => job.JobName == jobName);
            if (result == null)
            {
                Debug.LogError($"Unkown Job: {jobName}");
            }

            return result;
        }
        
        public NeedData GetNeedData(NeedType needType)
        {
            var result = _needLibrary.Find(need => need.NeedType == needType);
            if (result == null)
            {
                Debug.LogError($"Unkown Need: {needType}");
            }

            return result;
        }
        
        public RoofData GetRoofData(string roofName)
        {
            var result = _roofLibrary.Find(roof => roof.ConstructionName == roofName);
            if (result == null)
            {
                Debug.LogError($"Unknown Roof: {roofName}");
            }

            return result;
        }
        
        public Command GetCommand(string taskId)
        {
            var result = _commandLibrary.Find(cmd => cmd.Task.TaskId == taskId);
            if (result == null)
            {
                Debug.LogError("Unknown Command for Task Id: " + taskId);
            }
            return result;
        }
        
        public Color GetColour(string colourName)
        {
            var result = _colourLibrary.Find(c => c.Name == colourName);
            if (result != null)
            {
                return result.Colour;
            }
            else
            {
                Debug.LogError("Unknown Colour: " + colourName);
                return Color.magenta;
            }
        }
        
        public DoorData GetDoorData(string key)
        {
            var result = _doorLibrary.Find(s => s.ConstructionName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Door: " + key);
            }
            return result;
        }

        public StructureData GetStructureData(string key)
        {
            var result = _structureLibrary.Find(s => s.ConstructionName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Structure: " + key);
            }
            return result;
        }
        
        public Building GetBuilding(string key)
        {
            var result = _buildingLibrary.Find(b => b.BuildingID == key);
            if (result == null)
            {
                Debug.LogError("Unknown Building: " + key);
            }
            return result;
        }

        public WallData GetWallData(string key)
        {
            var result = _wallLibrary.Find(w => w.Name == key);
            if (result == null)
            {
                Debug.LogError("Unknown WallData: " + key);
            }

            return result;
        }

        public ItemData GetItemData(string key)
        {
            var result = _itemDataLibrary.Find(i => i.ItemName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Item: " + key);
            }
            return result;
        }

        public List<CraftedItemData> GetAllCraftedItemDatas()
        {
            List<CraftedItemData> results = new List<CraftedItemData>();
            foreach (var itemData in _itemDataLibrary)
            {
                var craftable = itemData as CraftedItemData;
                if (craftable != null)
                {
                    results.Add(craftable);
                }
            }

            return results;
        }
        
        public FloorData GetFloorData(string key)
        {
            var result = _floorLibrary.Find(s => s.ConstructionName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Floor: " + key);
            }
            return result;
        }

        public CropData GetCropData(string key)
        {
            var result = _cropLibrary.Find(s => s.CropName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Crop: " + key);
            }
            return result;
        }

        public List<CropData> GetAllCropData()
        {
            return new List<CropData>(_cropLibrary);
        }

        public Sprite GetSprite(string spriteName)
        {
            var result = _sprites.Find(s => s.name == spriteName);
            return result;
        }

        public Sprite GetOrderIcon(string spriteName)
        {
            return _orderIcons.Find(i => i.Name == spriteName).Sprite;
        }
        
        public GrowingResourceData GetGrowingResourceData(string resourceName)
        {
            return _growingResourceLibrary.Find(i => i.ResourceName == resourceName);
        }

        public HairData GetHairData(string hairName)
        {
            var result = _hairLibrary.Find(s => s.Name == hairName);
            if (result == null)
            {
                Debug.LogError("Unknown Hair Data: " + hairName);
            }
            return result;
        }
    }

    [Serializable]
    public class ColourData
    {
        public string Name;
        public Color Colour;
    }

    [Serializable]
    public class SpriteRef
    {
        [HorizontalGroup("Split", Width = 50), HideLabel, PreviewField(50)]
        public Sprite Sprite;
        [VerticalGroup("Split/Properties")]
        public string Name;
    }
}
