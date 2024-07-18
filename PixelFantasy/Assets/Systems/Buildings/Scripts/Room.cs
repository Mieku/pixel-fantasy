using System;
using System.Collections.Generic;
using Controllers;
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

            // Optionally, apply the color to cells if you have a method to do so
            ApplyColorToCells();
        }

        public void UpdateRoom(List<Vector2Int> newCells)
        {
            ClearCellsColour();
            
            Cells.Clear();

            foreach (var cell in newCells)
            {
                Cells.Add(cell);
            }
            
            ApplyColorToCells();
        }

        public void ClearTiles()
        {
            ClearCellsColour();
        }

        private void ApplyColorToCells()
        {
            // Assuming you have access to a method to set tile colors
            var tm = TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
            foreach (var cell in Cells)
            {
                tm.SetColor(new Vector3Int(cell.x, cell.y), RoomColor);
            }
        }
        
        private void ClearCellsColour()
        {
            // Assuming you have access to a method to set tile colors
            var tm = TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
            foreach (var cell in Cells)
            {
                tm.SetColor(new Vector3Int(cell.x, cell.y), Color.white);
            }
        }
    }
}