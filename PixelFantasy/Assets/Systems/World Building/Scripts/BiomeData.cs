using System.Collections.Generic;
using ScriptableObjects;
using TWC;
using UnityEngine;

namespace Systems.World_Building.Scripts
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Game/Biome", order = 1)]
    public class BiomeData : ScriptableObject
    {
        public string BiomeName;
        public TileWorldCreatorAsset WorldCreatorAsset;

        public List<MountainDataPercentage> Mountains = new List<MountainDataPercentage>();
        public MountainTileType DefaultMountainType;

        public float GetMountainTypePercentage(MountainTileType tileType)
        {
            if (tileType == MountainTileType.Empty) return 0f;

            foreach (var mountain in Mountains)
            {
                if (mountain.GetMountainTileType() == tileType)
                {
                    return mountain.spawnPercentage;
                }
            }

            return 0f;
        }

        public MountainData GetMountainData(MountainTileType tileType)
        {
            if (tileType == MountainTileType.Empty) return null;

            foreach (var mountain in Mountains)
            {
                if (mountain.GetMountainTileType() == tileType)
                {
                    return mountain.mountainData;
                }
            }

            return null;
        }
    }
    
    
    [System.Serializable]
    public class MountainDataPercentage
    {
        public MountainData mountainData;
        public float spawnPercentage = 0f; // Default to 0 for new entries, except for the first one which we'll manage in the editor script.

        // Constructor to easily create new instances with default values
        public MountainDataPercentage(MountainData data, float percentage)
        {
            mountainData = data;
            spawnPercentage = percentage;
        }

        public MountainTileType GetMountainTileType()
        {
            if (mountainData == null)
            {
                return MountainTileType.Empty;
            }

            return mountainData.MountainTileType;
        }
    }
}
