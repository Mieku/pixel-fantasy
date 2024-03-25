using System;
using System.Collections.Generic;
using Data.Item;
using Databrain;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Structure
{
    public class FloorSettings : DataObject
    {
        [SerializeField] private List<FloorStyle> _styleOptions;
        [SerializeField] private int _maxDurability;
        [SerializeField] private CraftRequirements _craftRequirements;
        [SerializeField] private Sprite _materialIcon;
        
        public List<FloorStyle> StyleOptions => _styleOptions;
        public CraftRequirements CraftRequirements => _craftRequirements.Clone();
        public Sprite MaterialIcon => _materialIcon;
        public int MaxDurability => _maxDurability;
        
        public List<string> GetStatsList()
        {
            // TODO: build me
            return new List<string>();
        }
    }

    [Serializable]
    public class FloorStyle : StyleOption
    {
        public TileBase Tiles;
    }

    [Serializable]
    public class StyleOption
    {
        public string StyleName;
        public Sprite StyleIcon;
    }
}
