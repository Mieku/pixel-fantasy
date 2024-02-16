using System;
using System.Collections.Generic;
using Controllers;
using Sirenix.OdinInspector;
using TWC;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Systems.World_Building.Scripts
{
    public class WorldBuilder : MonoBehaviour
    {
        [SerializeField] private TileWorldCreator _tileWorldCreator;
        [SerializeField] private TileWorldCreatorAsset _currentBiome;

        [SerializeField] private Tilemap _grassTilemap;
        [SerializeField] private RuleTile _grassRuleTile;
        [SerializeField] private Tilemap _waterTilemap;
        [SerializeField] private RuleTile _waterRuleTile;
        [SerializeField] private Tilemap _elevationTilemap;
        [SerializeField] private RuleTile _elevationRuleTile;
        
        [SerializeField] private Tilemap _groundCoverTilemap;
        [SerializeField] private RuleTile _dirtRuleTile;
        [SerializeField] private RuleTile _forestFloorRuleTile;

        [SerializeField] private GameObject _starterStockPile;
        
        [Button("Generate Plane")]
        private void GeneratePlane()
        {
            _tileWorldCreator.twcAsset = _currentBiome;
            _tileWorldCreator.ExecuteAllBlueprintLayers();

            RefreshPlane();
        }

        [Button("Refresh Plane")]
        private void RefreshPlane()
        {
            ClearAllTilemaps();
            
            var blueprintLayers = _tileWorldCreator.twcAsset.mapBlueprintLayers;
            
            var grassBlueprint = blueprintLayers.Find(layer => layer.layerName == "Grass");
            if (grassBlueprint != null)
            {
                BuildTilemap(grassBlueprint.map, _grassTilemap, _grassRuleTile);
            }
            
            var waterBlueprint = blueprintLayers.Find(layer => layer.layerName == "Water");
            if (waterBlueprint != null)
            {
                BuildTilemap(waterBlueprint.map, _waterTilemap, _waterRuleTile);
            }
            
            var elevationBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevation");
            if (elevationBlueprint != null)
            {
                BuildTilemap(elevationBlueprint.map, _elevationTilemap, _elevationRuleTile);
            }
            
            var dirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Dirt");
            if (dirtBlueprint != null)
            {
                BuildTilemap(dirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile);
            }
            
            var elevatedDirtBlueprint = blueprintLayers.Find(layer => layer.layerName == "Elevated Dirt");
            if (elevatedDirtBlueprint != null)
            {
                BuildTilemap(elevatedDirtBlueprint.map, _groundCoverTilemap, _dirtRuleTile);
            }
            
            
            DetermineStartPosition(blueprintLayers.Find(layer => layer.layerName == "Start Points"));
        }

        [Button("Clear All Tilemaps")]
        private void ClearAllTilemaps()
        {
            _grassTilemap.ClearAllTiles();
            _waterTilemap.ClearAllTiles();
            _elevationTilemap.ClearAllTiles();
            _groundCoverTilemap.ClearAllTiles();
        }

        private void DetermineStartPosition(TileWorldCreatorAsset.BlueprintLayerData layerData)
        {
            List<Vector3Int> startpoints = new List<Vector3Int>();
            var map = layerData.map;
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    var cellStart = new Vector3Int(x * 2, y * 2, 0);
                    if (map[x, y])
                    {
                        startpoints.Add(cellStart);
                    }
                }
            }

            if (startpoints.Count == 0)
            {
                Debug.LogError("No start point found");
            }

            var random = Random.Range(0, startpoints.Count - 1);
            _starterStockPile.transform.position = startpoints[random];
        }

        private void BuildTilemap(bool [,] blueprint, Tilemap tileMap, RuleTile ruleTile)
        {
            for (int x = 0; x < blueprint.GetLength(0); x++)
            {
                for (int y = 0; y < blueprint.GetLength(1); y++)
                {
                    var cellStart = new Vector3Int(x * 2, y * 2, 0);
                    if (blueprint[x, y])
                    {
                        tileMap.SetTile(cellStart, ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(0 , 1, 0), ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(1 , 0, 0), ruleTile);
                        tileMap.SetTile(cellStart + new Vector3Int(1 , 1, 0), ruleTile);
                    }
                }
            }
        }
    }
}
