using UnityEngine;

namespace Data.Resource
{
    public class MountainSettings : ResourceSettings
    {
        // Settings
        [SerializeField] protected RuleTile _ruleTile;
        [SerializeField] protected MountainTileType _mountainTileType;
        
        // Accessors
        public MountainTileType MountainTileType => _mountainTileType;
        public RuleTile RuleTile => _ruleTile;
    }
    
    public enum MountainTileType
    {
        Empty,
        Stone,
        Copper,
        Coal,
        Tin,
        Iron,
        Gold,
    }
}
