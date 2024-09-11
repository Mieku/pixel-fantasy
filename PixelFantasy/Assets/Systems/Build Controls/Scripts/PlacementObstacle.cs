using System.Collections.Generic;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class PlacementObstacle : MonoBehaviour
    {
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};
    
        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = false;
        }

        public bool CheckPlacement(Transform parent)
        {
            return Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags, null, parent);
        }
    }
}
