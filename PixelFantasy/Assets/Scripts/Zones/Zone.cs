using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Gods;
using HUD;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Zones
{
    [Serializable]
    public abstract class Zone : IZone
    {
        public string UID { get; set; }
        public string Name { get; set; }
        public abstract ZoneType ZoneType { get; }
        public List<Vector3Int> GridPositions { get; set; }
        public LayeredRuleTile LayeredRuleTile { get; set; }
        public ZoneTypeData ZoneTypeData => Librarian.Instance.GetZoneTypeData(ZoneType);
        public ZonePanel Panel;

        public List<Vector2> WorldPositions => ConvertGridPositionsToWorldPositions(GridPositions);

        private Tilemap _zonesTM;

        private bool _isSelected;

        protected void Init(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile)
        {
            _zonesTM = TilemapController.Instance.GetTilemap(TilemapLayer.Zones);
        }

        protected Zone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile)
        {
            Init(uid, gridPositions, layeredRuleTile);
            
            UID = uid;
            GridPositions = new List<Vector3Int>(gridPositions);
            LayeredRuleTile = layeredRuleTile;
            
            AssignName();
            DisplayZonePanel();
        }

        protected abstract void AssignName();

        public virtual void ClickZone()
        {
            _isSelected = true;
            ColourZone(Color.white);
            Panel.SetColour(Color.white);
        }

        public virtual void UnclickZone()
        {
            _isSelected = false;
            ColourZone(ZoneTypeData.Colour);
            Panel.Refresh();
        }

        public void ColourZone(Color colour)
        {
            foreach (var cell in GridPositions)
            {
                _zonesTM.SetColor(cell, colour);
            }
        }

        public Vector3 CenterPos()
        {
            var zonesTM = TilemapController.Instance.GetTilemap(TilemapLayer.Zones);
            
            List<float> horCells = new List<float>();
            List<float> vertCells = new List<float>();

            foreach (var gridPosition in GridPositions)
            {
                var pos = zonesTM.CellToWorld(gridPosition);
                
                horCells.Add(pos.x + 0.5f);
                vertCells.Add(pos.y + 0.5f);
            }

            var horMin = horCells.Min();
            var horMax = horCells.Max();
            var vertMin = vertCells.Min();
            var vertMax = vertCells.Max();

            Vector3 result = new Vector3();
            result.x = (horMin + horMax) / 2f;
            result.y = (vertMin + vertMax) / 2f;

            return result;
        }

        public virtual void ExpandZone(List<Vector3Int> newCells)
        {
            GridPositions.AddRange(newCells);
            GridPositions = GridPositions.Distinct().ToList();

            Panel.transform.position = CenterPos();
        }

        public virtual void ShrinkZone(List<Vector3Int> cellsToRemove)
        {
            GridPositions = GridPositions.Except(cellsToRemove).ToList();

            if (GridPositions.Count > 0)
            {
                Panel.transform.position = CenterPos();
            }
            else
            {
                RemoveZone();
            }
        }

        public virtual void RemoveZone()
        {
            GameObject.Destroy(Panel.gameObject);
            ZoneManager.Instance.Zones.Remove(this);
        }

        private void DisplayZonePanel()
        {
            Panel = ZoneManager.Instance.CreatePanel(this, CenterPos());
        }

        public void EditZoneName(string inputName)
        {
            Name = inputName;
            Panel.Refresh();

            // Refreshes the panel highlighting
            if (_isSelected)
            {
                Panel.SetColour(Color.white);
            }
        }

        public List<Vector2> ConvertGridPositionsToWorldPositions(List<Vector3Int> gridPoses)
        {
            List<Vector2> result = new List<Vector2>();
            
            foreach (var gridPos in gridPoses)
            {
                result.Add(_zonesTM.GetCellCenterWorld(gridPos));
            }

            return result;
        }
    }
}
