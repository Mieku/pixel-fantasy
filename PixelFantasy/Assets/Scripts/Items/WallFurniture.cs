using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class WallFurniture : Furniture
    {
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};
        
        public override bool CheckPlacement()
        {
            // TODO: alter this to only go on walls
            return base.CheckPlacement();
        }
    }
}
