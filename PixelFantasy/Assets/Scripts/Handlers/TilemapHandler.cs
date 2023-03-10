using System;
using System.Collections.Generic;
using Controllers;
using DataPersistence;
using UnityEngine;

namespace Handlers
{
    public class TilemapHandler : Saveable
    {
        protected override string StateName => "TilemapHandler";
        public override int LoadOrder => 1;

        [SerializeField] private GameObject _groundTM, _waterTM, _structureTM, _flooringTM, _dirtTM, _mountainTM, _zonesTM, _pendingZonesTM;
        
        protected override void ClearChildStates(List<object> childrenStates) { } // Not used
        
        protected override void SetChildStates(List<object> childrenStates)
        {
            foreach (var childState in childrenStates)
            {
                var data = (PersistentTilemap.MapData)childState;
                var tilemap = GetTilemapByLayer(data.TilemapLayer).GetComponent<PersistentTilemap>();
                tilemap.RestoreState(childState);
            }
        }

        private GameObject GetTilemapByLayer(TilemapLayer layer)
        {
            return layer switch
            {
                TilemapLayer.Grass => _groundTM,
                TilemapLayer.Water => _waterTM,
                TilemapLayer.Structure => _structureTM,
                TilemapLayer.Flooring => _flooringTM,
                TilemapLayer.Dirt => _dirtTM,
                TilemapLayer.Mountain => _mountainTM,
                TilemapLayer.Zones => _zonesTM,
                TilemapLayer.PendingZones => _pendingZonesTM,
                _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }
    }
}
