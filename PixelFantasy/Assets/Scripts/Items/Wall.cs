using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Wall : MonoBehaviour
    {
        public DynamicWallData _wallData; // TODO: Make private when done testing

        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private List<Wall> _neighbours = new List<Wall>();

        public void Init(DynamicWallData wallData)
        {
            _wallData = wallData;
            UpdateSprite(true);
        }

        private WallNeighbourConnectionInfo RefreshNeighbourData()
        {
            _neighbours.Clear();
            var result = new WallNeighbourConnectionInfo();

            var pos = transform.position;
            Vector2 topPos = new Vector2(pos.x, pos.y + 1);
            Vector2 botPos = new Vector2(pos.x, pos.y - 1);
            Vector2 leftPos = new Vector2(pos.x - 1, pos.y);
            Vector2 rightPos = new Vector2(pos.x + 1, pos.y);
            
            var allHitTop = Physics2D.RaycastAll(topPos, Vector2.down, 0.4f);
            var allHitBot = Physics2D.RaycastAll(botPos, Vector2.up, 0.4f);
            var allHitLeft = Physics2D.RaycastAll(leftPos, Vector2.right, 0.4f);
            var allHitRight = Physics2D.RaycastAll(rightPos, Vector2.left, 0.4f);

            // Top
            foreach (var hit in allHitTop)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Top = true;
                    break;
                }
            }
            // Bottom
            foreach (var hit in allHitBot)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Bottom = true;
                    break;
                }
            }
            // Left
            foreach (var hit in allHitLeft)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Left = true;
                    break;
                }
            }
            // Right
            foreach (var hit in allHitRight)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    _neighbours.Add(hit.transform.gameObject.GetComponent<Wall>());
                    result.Right = true;
                    break;
                }
            }

            return result;
        }
        
        public void UpdateSprite(bool informNeighbours)
        {
            // collect data on connections
            var connectData = RefreshNeighbourData();

            // use connection data to update sprite
            _spriteRenderer.sprite = _wallData.GetWallSprite(connectData);

            // If inform neighbours, tell neighbours to UpdateSprite (but they shouldn't inform their neighbours
            if (informNeighbours)
            {
                RefreshNeighbours();
            }
        }

        private void RefreshNeighbours()
        {
            foreach (var neighbour in _neighbours)
            {
                neighbour.UpdateSprite(false);
            }
        }
    }
}
