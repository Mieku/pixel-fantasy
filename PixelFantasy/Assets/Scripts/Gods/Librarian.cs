using System;
using System.Collections.Generic;
using Actions;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gods
{
    public class Librarian : God<Librarian>
    {
        [SerializeField] private List<ColourData> _colourLibrary;
        [SerializeField] private List<StructureData> _structureLibrary;
        [SerializeField] private List<FloorData> _floorLibrary;
        [SerializeField] private List<FurnitureData> _furnitureLibrary;
        [SerializeField] private List<DoorData> _doorLibrary;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private List<SpriteRef> _orderIcons;
        [SerializeField] private List<CropData> _cropLibrary;
        [SerializeField] private List<ActionBase> _actions;
        [SerializeField] private List<GrowingResourceData> _growingResourceLibrary;
        [SerializeField] private List<HairData> _hairLibrary;
        [SerializeField] private List<ZoneTypeData> _zoneTypeLibrary;

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
        
        public FloorData GetFloorData(string key)
        {
            var result = _floorLibrary.Find(s => s.ConstructionName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Floor: " + key);
            }
            return result;
        }

        public FurnitureData GetFurnitureData(string key)
        {
            var result = _furnitureLibrary.Find(s => s.FurnitureName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Furniture: " + key);
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

        public Sprite GetSprite(string spriteName)
        {
            var result = _sprites.Find(s => s.name == spriteName);
            return result;
        }

        public Sprite GetOrderIcon(string spriteName)
        {
            return _orderIcons.Find(i => i.Name == spriteName).Sprite;
        }

        public ActionBase GetAction(string actionId)
        {
            return _actions.Find(i => i.id == actionId);
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
