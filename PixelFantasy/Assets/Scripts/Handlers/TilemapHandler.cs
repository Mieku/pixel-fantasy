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

        [SerializeField] private GameObject _groundTM, _waterTM, _structureTM, _flooringTM;
        
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
            switch (layer)
            {
                case TilemapLayer.Ground:
                    return _groundTM;
                case TilemapLayer.Water:
                    return _waterTM;
                case TilemapLayer.Structure:
                    return _structureTM;
                case TilemapLayer.Flooring:
                    return _flooringTM;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
            }
        }
    }
}
