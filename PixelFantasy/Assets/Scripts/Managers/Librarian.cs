using System;
using System.Collections.Generic;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

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
        [SerializeField] private List<ZoneTypeData> _zoneTypeLibrary;
        [SerializeField] private List<ItemData> _itemDataLibrary;
        [SerializeField] private List<BuildingData> _buildingDataLibrary;
        [SerializeField] private List<Command> _commandLibrary;
        [SerializeField] private List<ProfessionData> _professionLibrary;
        [SerializeField] private List<WallData> _wallLibrary;
        [SerializeField] private List<RoomData> _roomLibrary;

        public RoomData GetRoom(string roomName)
        {
            var result = _roomLibrary.Find(room => room.RoomName == roomName);
            if (result == null)
            {
                Debug.LogError($"Unknown Room: {roomName}");
            }

            return result;
        }
        
        public ProfessionData GetProfession(string professionName)
        {
            var result = _professionLibrary.Find(prof => prof.ProfessionName == professionName);
            if (result == null)
            {
                Debug.LogError("Unknown Profession: " + professionName);
            }
            return result;
        }

        public List<ProfessionData> GetAllProfessions()
        {
            return new List<ProfessionData>(_professionLibrary);
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

        public BuildingData GetBuildingData(string key)
        {
            var result = _buildingDataLibrary.Find(b => b.ConstructionName == key);
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

        public ZoneTypeData GetZoneTypeData(ZoneType zoneType)
        {
            var result = _zoneTypeLibrary.Find(s => s.ZoneType == zoneType);
            if (result == null)
            {
                Debug.LogError("Unknown Zone Type Data: " + zoneType);
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
