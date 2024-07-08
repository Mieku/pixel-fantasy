using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controllers
{
    public class TilemapController : Singleton<TilemapController>
    {
        [SerializeField] private Tilemap _waterTM;
        [SerializeField] private Tilemap _grassTM;
        [SerializeField] private Tilemap _dirtTM;
        [SerializeField] private Tilemap _elevationTM;
        [SerializeField] private Tilemap _flooringTM;
        [SerializeField] private Tilemap _structureTM;
        [SerializeField] private Tilemap _mountainTM;
        [SerializeField] private Tilemap _pendingZonesTM;
        [SerializeField] private LayeredTilemapManager _zonesLayeredTilemap;

        public TileMapData GetTileMapData()
        {
            return new TileMapData
            {
                //GrassLayer = CollectTileMapData(_grassTM),
                ElevationLayer = CollectTileMapData(_elevationTM),
                DirtLayer = CollectTileMapData(_dirtTM),
                FlooringLayer = CollectTileMapData(_flooringTM),
                MountainLayer = CollectTileMapData(_mountainTM),
                StructureLayer = CollectTileMapData(_structureTM),
                WaterLayer = CollectTileMapData(_waterTM),
                ZoneLayers = _zonesLayeredTilemap.CollectTileMapData(),
            };
        }

        public void LoadTileMapData(TileMapData data)
        {
            //SetTileMapData(_grassTM, data.GrassLayer);
            SetTileMapData(_elevationTM, data.ElevationLayer);
            SetTileMapData(_dirtTM, data.DirtLayer);
            SetTileMapData(_flooringTM, data.FlooringLayer);
            SetTileMapData(_mountainTM, data.MountainLayer);
            SetTileMapData(_structureTM, data.StructureLayer);
            SetTileMapData(_waterTM, data.WaterLayer);

            _zonesLayeredTilemap.LoadTileMapData(data.ZoneLayers);
        }

        public void SetTileMapData(Tilemap tileMap, TileMapLayerData data)
        {
            tileMap.ClearAllTiles();

            foreach (var tile in data.Tiles)
            {
                TileBase tileBase = Resources.Load<TileBase>($"Tiles/{tile.TileName}");
                tileMap.SetTile(tile.Position, tileBase);
                tileMap.SetColor(tile.Position, tile.Color);
            }
        }

        public TileMapLayerData CollectTileMapData(Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;
            TileMapLayerData data = new TileMapLayerData();

            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var tileBase = tilemap.GetTile(pos);
                    var color = tilemap.GetColor(pos);
                    if (tileBase != null)
                    {
                        data.Tiles.Add(new SerializableTile(tileBase.name, pos, color));
                    }
                }
            }

            return data;
        }
        
        public Tilemap GetTilemap(TilemapLayer layer)
        {
            switch (layer)
            {
                case TilemapLayer.Grass:
                    return _grassTM;
                case TilemapLayer.Water:
                    return _waterTM;
                case TilemapLayer.Structure:
                    return _structureTM;
                case TilemapLayer.Flooring:
                    return _flooringTM;
                case TilemapLayer.Dirt:
                    return _dirtTM;
                case TilemapLayer.Mountain:
                    return _mountainTM;
                case TilemapLayer.PendingZones:
                    return _pendingZonesTM;
                case TilemapLayer.Elevation:
                    return _elevationTM;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }
    }
    
    [Serializable]
    public class SerializableTile
    {
        public string TileName;
        public Vector3Int Position;
        public Color Color;

        public SerializableTile(string tileName, Vector3Int position, Color color)
        {
            TileName = tileName;
            Position = position;
            Color = color;
        }
    }

    [Serializable]
    public class TileMapLayerData
    {
        public List<SerializableTile> Tiles = new List<SerializableTile>();
    }

    [Serializable]
    public class TileMapData
    {
        public TileMapLayerData GrassLayer;
        public TileMapLayerData ElevationLayer;
        public TileMapLayerData DirtLayer;
        public TileMapLayerData WaterLayer;
        public TileMapLayerData FlooringLayer;
        public TileMapLayerData StructureLayer;
        public TileMapLayerData MountainLayer;
        public List<LayeredTileMapLayerData> ZoneLayers;
    }

    [Serializable]
    public class LayeredTileMapLayerData : TileMapLayerData
    {
        public int Layer;
    }
    
    [Serializable]
    public enum TilemapLayer
    {
        Grass,
        Water,
        Structure,
        Flooring,
        Dirt,
        Mountain,
        //Zones,
        PendingZones,
        Elevation,
    }
}
