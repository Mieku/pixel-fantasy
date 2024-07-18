using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Floor Settings", menuName = "Settings/Floor Settings")]
public class FloorSettings : ConstructionSettings
{
    [SerializeField] private string _floorName;
    [SerializeField] private List<FloorStyle> _styleOptions;
    [SerializeField] private int _maxDurability;
    [SerializeField] private Sprite _materialIcon;

    public string FloorName => _floorName;
    public List<FloorStyle> StyleOptions => _styleOptions;
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
