using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Sirenix.OdinInspector;
using Systems.Details.Build_Details.Scripts;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    public class StructureDatabase : Singleton<StructureDatabase>
    {
        public WallBuilder WallBuilder;
        
        private EStructureCell[,] _grid;
        [ShowInInspector] private Dictionary<string, StructurePiece> _registeredPieces = new Dictionary<string, StructurePiece>();
        private List<Room> _rooms = new List<Room>();
        private Dictionary<Vector2Int, Room> roomCache = new Dictionary<Vector2Int, Room>();
        private const int MAX_CELLS_PER_ROOM = 300;

        public void Init(Vector2Int size)
        {
            _grid = new EStructureCell[size.x * 2, size.y * 2];
        }

        public void RegisterStructure(StructurePiece structurePiece)
        {
            UpdateGrid(structurePiece.Cell.CellPos, structurePiece.Cell.CellType);
            _registeredPieces.Add(structurePiece.UniqueID, structurePiece);
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(structurePiece);
        }

        public void DeregisterStructure(StructurePiece structurePiece)
        {
            UpdateGrid(structurePiece.Cell.CellPos, EStructureCell.None);
            _registeredPieces.Remove(structurePiece.UniqueID);
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(structurePiece);
        }

        public Dictionary<string, ConstructionData> SaveStructureData()
        {
            Dictionary<string, ConstructionData> results = new Dictionary<string, ConstructionData>();
            foreach (var kvp in _registeredPieces)
            {
                results[kvp.Key] = kvp.Value.RuntimeData;
            }

            return results;
        }

        public void LoadStructureData(Dictionary<string, ConstructionData> loadedData)
        {
            foreach (var data in loadedData)
            {
                if (data.Value is WallData wallData)
                {
                    WallBuilder.SpawnLoadedWall(wallData);
                } 
                else if (data.Value is DoorData doorData)
                {
                    var doorSettings = doorData.DoorSettings;
                    var door = Instantiate(doorSettings.DoorPrefab, doorData.Position, Quaternion.identity, transform);
                    door.LoadData(doorData);
                }
            }
        }

        public void ClearAllStructures()
        {
            var pieces = _registeredPieces.ToList();
            foreach (var kvp in pieces)
            {
                kvp.Value.DeletePiece();
            }
            _registeredPieces.Clear();

            var rooms = _rooms.ToList();
            foreach (var room in rooms)
            {
                room.ClearTiles();
            }
            _rooms.Clear();
            roomCache.Clear();
        }

        public bool IsInteriorBelow(Vector2Int startCell)
        {
            var cell = startCell + Vector2Int.down;
            if (IsWithinGrid(cell))
            {
                return _grid[cell.x, cell.y] == EStructureCell.Interior;
            }

            return false;
        }

        private void UpdateGrid(Vector2Int position, EStructureCell type)
        {
            _grid[position.x, position.y] = type;
            TryFloodFillAdjacent(position, maxRoomSize: MAX_CELLS_PER_ROOM);
        }
        
        private void TryFloodFillAdjacent(Vector2Int cell, int maxRoomSize)
        {
            var adjacentDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var direction in adjacentDirections)
            {
                var startCell = cell + direction;
                if (IsWithinGrid(startCell) && _grid[startCell.x, startCell.y] == EStructureCell.None)
                {
                    var roomCells = FloodFillRoom(startCell, maxRoomSize);
                    if (roomCells != null && roomCells.Count <= maxRoomSize && RoomHasAtLeastOneDoor(roomCells))
                    {
                        AddNewRoom(roomCells);
                    }
                }
            }
        }

        private List<Vector2Int> FloodFillRoom(Vector2Int startCell, int maxRoomSize)
        {
            List<Vector2Int> roomCells = new List<Vector2Int>();
            Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();
            HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();

            cellsToCheck.Enqueue(startCell);
            while (cellsToCheck.Count > 0)
            {
                if (roomCells.Count >= maxRoomSize)
                {
                    // If the room reaches or exceeds the max size before the flood fill is complete,
                    // it's not considered a valid room. Return null to indicate this.
                    return null;
                }

                Vector2Int cell = cellsToCheck.Dequeue();
                if (!visitedCells.Contains(cell) && IsWithinGrid(cell) && _grid[cell.x, cell.y] == EStructureCell.None)
                {
                    roomCells.Add(cell);
                    visitedCells.Add(cell);
                    foreach (var direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                    {
                        var neighbor = cell + direction;
                        if (IsWithinGrid(neighbor) && _grid[neighbor.x, neighbor.y] == EStructureCell.None && !visitedCells.Contains(neighbor))
                        {
                            cellsToCheck.Enqueue(neighbor);
                        }
                    }
                }
            }

            // If the method completes without hitting the max size limit, proceed with the check for a door.
            // This ensures that only enclosed spaces with an accessible door are considered valid rooms.
            return RoomHasAtLeastOneDoor(roomCells) ? roomCells : null;
        }

        private void AddNewRoom(List<Vector2Int> roomCells)
        {
            var newRoom = new Room(roomCells);
            _rooms.Add(newRoom);
            foreach (var cell in roomCells)
            {
                roomCache[cell] = newRoom;
                _grid[cell.x, cell.y] = EStructureCell.Interior;
            }
            
            List<Vector2Int> possibleCells = new List<Vector2Int>();
            foreach (var roomCell in roomCells)
            {
                var aboveCell = roomCell + Vector2Int.up;
                if (IsWithinGrid(aboveCell))
                {
                    if (_grid[aboveCell.x, aboveCell.y] == EStructureCell.Wall ||
                        _grid[aboveCell.x, aboveCell.y] == EStructureCell.Door)
                    {
                        possibleCells.Add(aboveCell);
                    }
                }
            }

            foreach (var possibleCell in possibleCells)
            {
                StructurePiece sPiece = _registeredPieces.Values.FirstOrDefault(s => s.Cell.CellPos == possibleCell);
                sPiece.RefreshTile();
            }
        }

        public StructurePiece GetStructureAtCell(Vector2Int cell)
        {
            if (IsWithinGrid(cell))
            {
                StructurePiece sPiece = _registeredPieces.Values.FirstOrDefault(s => s.Cell.CellPos == cell);
                return sPiece;
            }

            return null;
        }

        public EStructureCell GetStructureCell(Vector2Int cell)
        {
            if (IsWithinGrid(cell))
            {
                return _grid[cell.x, cell.y];
            }

            return EStructureCell.None;
        }

        private bool RoomHasAtLeastOneDoor(List<Vector2Int> roomCells)
        {
            var boundaryCells = GetBoundaryCells(roomCells);
            return boundaryCells.Any(cell => _grid[cell.x, cell.y] == EStructureCell.Door);
        }

        private List<Vector2Int> GetBoundaryCells(List<Vector2Int> roomCells)
        {
            var boundaryCells = new HashSet<Vector2Int>();
            foreach (var cell in roomCells)
            {
                foreach (var direction in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    var adjacentCell = cell + direction;
                    if (IsWithinGrid(adjacentCell) && !roomCells.Contains(adjacentCell))
                    {
                        boundaryCells.Add(adjacentCell);
                    }
                }
            }
            return boundaryCells.ToList();
        }

        private bool IsWithinGrid(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < _grid.GetLength(0) && cell.y >= 0 && cell.y < _grid.GetLength(1);
        }
    }

    public enum EStructureCell { None, Interior, Wall, Door }

    public class NeighbourCells
    {
        public EStructureCell TopLeft;
        public EStructureCell Top;
        public EStructureCell TopRight;
        public EStructureCell Right;
        public EStructureCell BottomRight;
        public EStructureCell Bottom;
        public EStructureCell BottomLeft;
        public EStructureCell Left;

        public NeighbourCells()
        {
            TopLeft = EStructureCell.None;
            Top = EStructureCell.None;
            TopRight = EStructureCell.None;
            Right = EStructureCell.None;
            BottomRight = EStructureCell.None;
            Bottom = EStructureCell.None;
            BottomLeft = EStructureCell.None;
            Left = EStructureCell.None;
        }

        public bool HasTopLeft => TopLeft != EStructureCell.None;
        public bool HasTop => Top != EStructureCell.None;
        public bool HasTopRight => TopRight != EStructureCell.None;
        public bool HasRight => Right != EStructureCell.None;
        public bool HasBottomRight => BottomRight != EStructureCell.None;
        public bool HasBottom => Bottom != EStructureCell.None;
        public bool HasBottomLeft => BottomLeft != EStructureCell.None;
        public bool HasLeft => Left != EStructureCell.None;
    }
}
