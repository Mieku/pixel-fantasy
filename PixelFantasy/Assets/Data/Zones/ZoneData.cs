using System;
using System.Collections.Generic;
using Databrain;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    [DataObjectAddToRuntimeLibrary]
    public abstract class ZoneData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize]
        public string ZoneName;
        
        [ExposeToInspector, DatabrainSerialize]
        public List<Vector3Int> Cells = new List<Vector3Int>();
        
        [ExposeToInspector, DatabrainSerialize]
        public List<ZoneCell> ZoneCells = new List<ZoneCell>();

        [ExposeToInspector, DatabrainSerialize]
        public int AssignedLayer;

        [ExposeToInspector, DatabrainSerialize]
        public bool IsEnabled;

        public int NumCells => Cells.Count;
        public Action OnZoneChanged;
        
        public abstract Color ZoneColour { get; }
        public abstract TileBase DefaultTiles { get; }
        public abstract TileBase SelectedTiles { get; }
        public abstract EZoneType ZoneType { get; }
        public abstract ZoneSettings Settings { get; }
        
        public virtual void SelectZone()
        {
            ZoneManager.Instance.SelectZone(this);
        }

        public virtual void RemoveCell(Vector3Int cell)
        {
            // Remove the cell at position
            var cellObj = ZoneCells.Find(cellObj => cellObj.CellPos == cell);
            ZoneCells.Remove(cellObj);
            
            cellObj.DeleteCell();

            Cells.Remove(cell);
            
            ZoneManager.Instance.ClearTileCell(cell, AssignedLayer);

            // If the zone has no cells, delete it
            if (Cells.Count == 0)
            {
                ZoneManager.Instance.DeleteZone(this);
            }
            else
            {
                // Make sure all cells connect, if not create copies of the zone
                CheckAndSplitZone();
            }
            
            OnZoneChanged.Invoke();
        }
        
        private void CheckAndSplitZone()
        {
            // Uses a flood fill to check connectivity
            // and find distinct groups of connected cells
            var connectedGroups = GetConnectedCellGroups();

            if (connectedGroups.Count > 1)
            {
                // More than one group means the zone is split.
                // Keep the first group in the current zone and create new zones for the rest
                Cells = connectedGroups[0];
                for (int i = 1; i < connectedGroups.Count; i++)
                {
                    CreateNewZoneWithCells(connectedGroups[i]);
                }
            }
            
            OnZoneChanged.Invoke();
        }
        
        private void CreateNewZoneWithCells(List<Vector3Int> cells)
        {
            // Determine a new layer id
            int layerId = ZoneManager.Instance.GetUniqueLayerId();
            List<ZoneCell> cellsToTransfer = new List<ZoneCell>();
            
            foreach (var cell in cells)
            {
                // Delete the old cell objs
                var cellObj = ZoneCells.Find(cellObj => cellObj.CellPos == cell);
                cellsToTransfer.Add(cellObj);
                ZoneCells.Remove(cellObj);
                
                // Clear the currently displayed tiles
                ZoneManager.Instance.ClearTileCell(cell, AssignedLayer);

                ZoneManager.Instance.DrawTileCell(cell, layerId, Settings.DefaultTiles, Settings.ZoneColour);
            }
            
            // Create new zone copy
            ZoneManager.Instance.CopyZone(cells, layerId, this, cellsToTransfer);
        }

        private List<List<Vector3Int>> GetConnectedCellGroups()
        {
            var visited = new HashSet<Vector3Int>();
            var groups = new List<List<Vector3Int>>();

            foreach (var cell in Cells)
            {
                if (!visited.Contains(cell))
                {
                    var group = FloodFill(cell, visited);
                    groups.Add(group);
                }
            }

            return groups;
        }
        
        private List<Vector3Int> FloodFill(Vector3Int start, HashSet<Vector3Int> visited)
        {
            var group = new List<Vector3Int>();
            var stack = new Stack<Vector3Int>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current))
                {
                    continue;
                }

                visited.Add(current);
                group.Add(current);

                var neighbors = GetNeighbors(current);
                foreach (var neighbor in neighbors)
                {
                    if (Cells.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        stack.Push(neighbor);
                    }
                }
            }

            return group;
        }
        
        private List<Vector3Int> GetNeighbors(Vector3Int cell)
        {
            var neighbors = new List<Vector3Int>
            {
                new Vector3Int(cell.x + 1, cell.y, cell.z),
                new Vector3Int(cell.x - 1, cell.y, cell.z),
                new Vector3Int(cell.x, cell.y + 1, cell.z),
                new Vector3Int(cell.x, cell.y - 1, cell.z)
            };

            return neighbors;
        }

        public void ChangeZoneName(string newName)
        {
            ZoneName = newName;
        }
    }
}
