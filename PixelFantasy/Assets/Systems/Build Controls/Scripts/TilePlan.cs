using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Build_Controls.Scripts
{
    public class TilePlan : MonoBehaviour
    {
        private TileBase _ruleTile;
        private Tilemap _tilemap;
        private Color _colour;
        private Vector3Int _cell;
        
        private TileBase _prevTile;
        private Color _prevColour;
        

        public void Init(TileBase ruleTile, Tilemap tilemap, Color colour)
        {
            _ruleTile = ruleTile;
            _tilemap = tilemap;
            _colour = colour;
            
            _cell = _tilemap.WorldToCell(transform.position);
            _prevTile = _tilemap.GetTile(_cell);
            _prevColour = _tilemap.GetColor(_cell);
            
            _tilemap.SetTile(_cell, _ruleTile);
            _tilemap.SetColor(_cell, _colour);
        }
        
        public void Clear()
        {
            _tilemap.SetTile(_cell, _prevTile);
            _tilemap.SetColor(_cell, _prevColour);
        }
    }
}
