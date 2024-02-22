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

        public List<ResourceDataPercentage> ForestTreeResources = new List<ResourceDataPercentage>();
        public List<ResourceDataPercentage> ForestAdditionalResources = new List<ResourceDataPercentage>();
        public int ForestTreeDensity; // The min number of trees per tile
        public int ForestAdditionalDensity; // The max number of trees per tile
        
        public List<ResourceDataPercentage> VegitationResources = new List<ResourceDataPercentage>();
        public int VegitationClusterRadius;
        public int MaxVegetationPerCluster;
        
        public List<ResourceDataPercentage> AdditionalResources = new List<ResourceDataPercentage>();
        public float AdditionalsChanceToSpawn;

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

        public void AddMountain(MountainData mountainData)
        {
            var mountainPercent = new MountainDataPercentage(mountainData, 0f);
            Mountains.Add(mountainPercent);
        }

        public void AddForestTree(GrowingResourceData treeData)
        {
            var treePercent = new ResourceDataPercentage(treeData, 0f);
            ForestTreeResources.Add(treePercent);
        }

        public GrowingResourceData GetRandomForestTree()
        {
            float totalWeight = 0f;
            foreach (var resource in ForestTreeResources)
            {
                totalWeight += resource.SpawnPercentage;
            }

            float randomPoint = Random.Range(0f, totalWeight);
            foreach (var resource in ForestTreeResources)
            {
                if (randomPoint < resource.SpawnPercentage)
                {
                    return resource.ResourceData as GrowingResourceData;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceData GetRandomForestAdditional()
        {
            float totalWeight = 0f;
            foreach (var resource in ForestAdditionalResources)
            {
                totalWeight += resource.SpawnPercentage;
            }

            float randomPoint = Random.Range(0f, totalWeight);
            foreach (var resource in ForestAdditionalResources)
            {
                if (randomPoint < resource.SpawnPercentage)
                {
                    return resource.ResourceData;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceData GetRandomVegitation()
        {
            float totalWeight = 0f;
            foreach (var resource in VegitationResources)
            {
                totalWeight += resource.SpawnPercentage;
            }

            float randomPoint = Random.Range(0f, totalWeight);
            foreach (var resource in VegitationResources)
            {
                if (randomPoint < resource.SpawnPercentage)
                {
                    return resource.ResourceData;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceData GetRandomAdditional()
        {
            float totalWeight = 0f;
            foreach (var resource in AdditionalResources)
            {
                totalWeight += resource.SpawnPercentage;
            }

            float randomPoint = Random.Range(0f, totalWeight);
            foreach (var resource in AdditionalResources)
            {
                if (randomPoint < resource.SpawnPercentage)
                {
                    return resource.ResourceData;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }
    }

    [System.Serializable]
    public class ResourceDataPercentage
    {
        public ResourceData ResourceData;
        public float SpawnPercentage = 0f;

        public ResourceDataPercentage(ResourceData data, float percentage)
        {
            ResourceData = data;
            SpawnPercentage = percentage;
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
