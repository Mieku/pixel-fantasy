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
        
        private Tilemap _zonesTM;

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

        public void ClickZone()
        {
            ColourZone(Color.white);
            Panel.SetColour(Color.white);
        }

        public void UnclickZone()
        {
            ColourZone(ZoneTypeData.Colour);
            Panel.SetColour(ZoneTypeData.Colour);
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

        public void ExpandZone(List<Vector3Int> newCells)
        {
            GridPositions.AddRange(newCells);
            GridPositions = GridPositions.Distinct().ToList();

            Panel.transform.position = CenterPos();
        }

        private void DisplayZonePanel()
        {
            Panel = ZoneManager.Instance.CreatePanel(this, CenterPos());
        }
    }
}
