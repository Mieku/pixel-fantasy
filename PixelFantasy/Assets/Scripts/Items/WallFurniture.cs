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
            
            bool result = Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags);

            // Check the useage markers
            if (_useageMarkers != null)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, _invalidPlacementTags))
                    {
                        //ColourArt(ColourStates.CanPlace);
                        marker.color = Color.white;
                        markersPass = true;
                    }
                    else
                    {
                        //ColourArt(ColourStates.CantPlace);
                        marker.color = Color.red;
                    }
                }

                if (!markersPass)
                {
                    result = false;
                }
            }

            if (result)
            {
                ColourArt(ColourStates.CanPlace);
            }
            else
            {
                ColourArt(ColourStates.CantPlace);
            }
            
            return result;
        }
    }
}
