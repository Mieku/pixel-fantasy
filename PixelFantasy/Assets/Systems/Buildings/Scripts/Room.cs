using System;
using System.Collections.Generic;
using Controllers;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    [Serializable]
    public class Room
    {
        public List<Vector2Int> Cells { get; private set; }
        public Color RoomColor;

        public Room(List<Vector2Int> cells)
        {
            Cells = cells;
            RoomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            
            ApplyRoomTiles();
            HideTiles();
        }

        public void UpdateRoom(List<Vector3Int> newCells)
        {
            ClearTiles();
            Cells.Clear();

            foreach (var cell in newCells)
            {
                Cells.Add((Vector2Int)cell);
            }
            
            ApplyRoomTiles();
            HideTiles();
        }

        public void ClearTiles()
        {
            foreach (var cell in Cells)
            {
                TilemapController.Instance.SetTileByCell(TilemapLayer.Rooms, (Vector3Int) cell, null);
            }
            
            TilemapController.Instance.UpdateRoomLight();
        }

        private void ApplyRoomTiles()
        {
            var tile = GameSettings.Instance.LoadTileBase("RoomTiles");
            foreach (var cell in Cells)
            {
                TilemapController.Instance.SetTileByCell(TilemapLayer.Rooms, (Vector3Int) cell, tile);
            }
            
            TilemapController.Instance.UpdateRoomLight();
        }

        private void HideTiles()
        {
            foreach (var cell in Cells)
            {
                var curColour = TilemapController.Instance.GetTileColour(TilemapLayer.Rooms, (Vector3Int) cell);
                Color noAlpha = new Color(curColour.r, curColour.g, curColour.b, 0);
                TilemapController.Instance.ColourTileByCell(TilemapLayer.Rooms, (Vector3Int) cell, noAlpha);
            }
        }

        private void ShowTiles()
        {
            foreach (var cell in Cells)
            {
                var curColour = TilemapController.Instance.GetTileColour(TilemapLayer.Rooms, (Vector3Int) cell);
                Color withAlpha = new Color(curColour.r, curColour.g, curColour.b, 1);
                TilemapController.Instance.ColourTileByCell(TilemapLayer.Rooms, (Vector3Int) cell, withAlpha);
            }
        }

        private void ApplyColorToCells()
        {
            var tm = TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
            foreach (var cell in Cells)
            {
                tm.SetColor(new Vector3Int(cell.x, cell.y), RoomColor);
            }
        }
        
        private void ClearCellsColour()
        {
            var tm = TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
            foreach (var cell in Cells)
            {
                tm.SetColor(new Vector3Int(cell.x, cell.y), Color.white);
            }
        }
    }
}