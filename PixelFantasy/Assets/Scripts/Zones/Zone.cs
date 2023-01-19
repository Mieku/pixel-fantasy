using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Gods;
using HUD;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    [Serializable]
    public class Zone 
    {
        /*
         * Each zone needs:
         * - A unique id
         * - Name
         * - Type (Icon is provided in type)
         * - The tiles it encompasses
         * - Savable
         * - Center point (For showing the menu)
         * - A way to expand/shrink/delete
         * - A way to detect and inform of issues (not connected, not enclosed, missing furniture... etc have this be expandable based on type)
         *
         * Optional:
         * - References to the furniture inside them
         * - Kinlings assigned to them
         */

        public string UID;
        public string Name;
        public ZoneType ZoneType;
        public ZoneTypeData ZoneTypeData => Librarian.Instance.GetZoneTypeData(ZoneType);
        public List<Vector3Int> GridPositions;
        public LayeredRuleTile LayeredRuleTile;
        public ZonePanel Panel;

        public Zone(string uid, string name, ZoneType zoneType, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile)
        {
            UID = uid;
            Name = name;
            ZoneType = zoneType;
            GridPositions = gridPositions;
            LayeredRuleTile = layeredRuleTile;

            DisplayZonePanel();
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
