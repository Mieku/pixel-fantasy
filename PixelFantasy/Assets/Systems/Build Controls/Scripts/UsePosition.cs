using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class UsePosition : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Clearance"};
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            ShowMarker(false);
        }

        public Vector2 GetPosition()
        {
            return transform.position;
        }

        public void ShowMarker(bool show)
        {
            _spriteRenderer.enabled = show;
        }
    
        public bool CheckPlacement(Transform parent)
        {
            bool canPlace = Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags, null, parent);

            if (canPlace)
            {
                _spriteRenderer.color = GameSettings.Instance.PassedUsePosColour;
            }
            else
            {
                _spriteRenderer.color = GameSettings.Instance.FailedUsePosColour;
            }
            
            return canPlace;
        }
        
        public bool IsValid(Transform parent)
        {
            bool check = Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags, null, parent);
            return check;
        }

        public void EnableClearance(bool enable)
        {
            _boxCollider.enabled = enable;
        }
    }
}
