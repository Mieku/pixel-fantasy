using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controllers
{
    public class LayeredTilemapManager : MonoBehaviour
    {
        public Grid grid; // Assign your main Grid object in the inspector
        public string LayerNamePrefix;

        [SerializeField] private Tilemap _tilemapPrefab;
        private readonly Dictionary<int, Tilemap> _tilemaps = new Dictionary<int, Tilemap>();

        public List<LayeredTileMapLayerData> CollectTileMapData()
        {
            List<LayeredTileMapLayerData> results = new List<LayeredTileMapLayerData>();
            
            foreach (var kvp in _tilemaps)
            {
                var data = TilemapController.Instance.CollectTileMapData(kvp.Value);

                var layerData = new LayeredTileMapLayerData
                {
                    Layer = kvp.Key,
                    Tiles = data.Tiles
                };
                
                results.Add(layerData);
            }

            return results;
        }

        public void LoadTileMapData(List<LayeredTileMapLayerData> layers)
        {
            ClearLayers();
            
            foreach (var layerData in layers)
            {
                var tm = FindOrCreateLayer(layerData.Layer);
                TilemapController.Instance.SetTileMapData(tm, layerData);
            }
        }

        private void ClearLayers()
        {
            foreach (var kvp in _tilemaps)
            {
                kvp.Value.ClearAllTiles();
                Destroy(kvp.Value.gameObject);
            }
            
            _tilemaps.Clear();
        }

        public int GetLowestNotUsedLayer()
        {
            int layerNum = 0;
            while (layerNum < 1000) // just in case...
            {
                var possibleLayer = GetLayer(layerNum);
                if (possibleLayer == null)
                {
                    return layerNum;
                }
                else
                {
                    layerNum++;
                }
            }

            Debug.LogError("Unable to find a new layer, this should never be reached");
            return -1;
        }
        
        public Tilemap FindOrCreateLayer(int layer)
        {
            var tilemapLayer = GetLayer(layer);
            if (tilemapLayer != null)
            {
                return tilemapLayer;
            }
            
            var newLayer = AddLayer(layer);
            return newLayer;
        }
        
        // Method to add a new layer
        public Tilemap AddLayer(int layer)
        {
            var tilemap = Instantiate(_tilemapPrefab, grid.transform);
            tilemap.name = $"{LayerNamePrefix} {layer}";
            tilemap.gameObject.SetActive(true);
            _tilemaps.Add(layer, tilemap);
            return tilemap;
        }

        // Method to remove a layer
        public void RemoveLayer(int layer)
        {
            if (_tilemaps.ContainsKey(layer))
            {
                Destroy(GetLayer(layer).gameObject);
                _tilemaps.Remove(layer);
            }
        }

        // Get a specific layer (Tilemap)
        public Tilemap GetLayer(int layer)
        {
            return _tilemaps.GetValueOrDefault(layer);
        }
    }
}
