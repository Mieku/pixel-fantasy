using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

namespace Controllers
{
    public class TilemapController : MonoBehaviour
    {
        public TileBase testerTile;
        
        [SerializeField] private Grid grid;
        [SerializeField] private List<TilemapReference> tilemaps;

        private GenericGrid<LandGridCell> tilemapGrid;

        private void Awake()
        {
            DetermineTilemapGrid();
        }
        
        private void Update()
        {
            // TODO: For testing only
            // if (Input.GetMouseButtonDown(0))
            // {
            //     var pos = UtilsClass.GetMouseWorldPosition();
            //     SetTile((int)pos.x, (int)pos.y, TilemapLayer.Water, testerTile);
            // }
        }

        private void DetermineTilemapGrid()
        {
            tilemapGrid = new GenericGrid<LandGridCell>(75, 75, 1f, Vector3.zero);
            for (int x = 0; x < tilemapGrid.Width; x++)
            {
                for (int y = 0; y < tilemapGrid.Height; y++)
                {
                    LandGridCell cell = new LandGridCell(x, y);
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    foreach (var tilemapRef in tilemaps)
                    {
                        var cellTile = tilemapRef.Tilemap.GetTile(pos);
                        cell.SetTileBase(tilemapRef.TilemapLayer, cellTile);
                    }

                    tilemapGrid.SetValue(x, y, cell);
                }
            }
        }

        public TileBase GetTile(int x, int y, TilemapLayer tilemapLayer)
        {
            var cell = tilemapGrid.GetValue(x, y);
            return cell.GetTileBase(tilemapLayer);
        }

        public void SetTile(int x, int y, TilemapLayer tilemapLayer, TileBase tileBase)
        {
            tilemapGrid.GetValue(x, y).SetTileBase(tilemapLayer, tileBase);
            var map = tilemaps.Find(reference => reference.TilemapLayer == tilemapLayer).Tilemap;
            var pos = new Vector3Int(x, y, 0);
            map.SetTile(pos, tileBase);
        }
    }

    [Serializable]
    public class TilemapReference
    {
        public TilemapLayer TilemapLayer;
        public Tilemap Tilemap;
    }

    [Serializable]
    public enum TilemapLayer
    {
        Ground,
        Water
    }
}
