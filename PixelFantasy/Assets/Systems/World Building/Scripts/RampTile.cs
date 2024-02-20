using System;
using Controllers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace Systems.World_Building.Scripts
{
    public class RampTile : MonoBehaviour
    {
        private Tilemap _elevationTM => TilemapController.Instance.GetTilemap(TilemapLayer.Elevation);
        
        private void Start()
        {
            var cell = _elevationTM.WorldToCell(transform.position);
            var tileGO = _elevationTM.GetInstantiatedObject(cell);
            if (tileGO != null)
            {
                var obstacle = tileGO.GetComponent<NavMeshObstacle>();
                if (obstacle != null)
                {
                    obstacle.enabled = false;
                }
            }
        }
    }
}
