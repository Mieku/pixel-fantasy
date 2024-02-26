using Controllers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    public class StructureCell : MonoBehaviour
    {
        private Tilemap _groundTM => TilemapController.Instance.GetTilemap(TilemapLayer.Grass);
        
        public EStructureCell CellType;

        public Vector2Int CellPos
        {
            get
            {
                Vector3Int cell = _groundTM.WorldToCell(transform.position);
                return (Vector2Int)cell;
            }
        }
    }
}
