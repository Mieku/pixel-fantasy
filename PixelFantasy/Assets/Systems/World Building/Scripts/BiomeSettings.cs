using System.Collections.Generic;
using ScriptableObjects;
using TWC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.World_Building.Scripts
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Settings/World/Biome")]
    public class BiomeSettings : ScriptableObject
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

        public MountainSettings GetMountainData(MountainTileType tileType)
        {
            if (tileType == MountainTileType.Empty) return null;

            foreach (var mountain in Mountains)
            {
                if (mountain.GetMountainTileType() == tileType)
                {
                    return mountain.mountainSettings;
                }
            }

            return null;
        }

        public void AddMountain(MountainSettings mountainSettings)
        {
            var mountainPercent = new MountainDataPercentage(mountainSettings, 0f);
            Mountains.Add(mountainPercent);
        }

        public void AddForestTree(GrowingResourceSettings treeSettings)
        {
            var treePercent = new ResourceDataPercentage(treeSettings, 0f);
            ForestTreeResources.Add(treePercent);
        }

        public GrowingResourceSettings GetRandomForestTree()
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
                    return resource.ResourceSettings as GrowingResourceSettings;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceSettings GetRandomForestAdditional()
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
                    return resource.ResourceSettings;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceSettings GetRandomVegitation()
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
                    return resource.ResourceSettings;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }

        public ResourceSettings GetRandomAdditional()
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
                    return resource.ResourceSettings;
                }
                randomPoint -= resource.SpawnPercentage;
            }

            return null; // In case something goes wrong
        }
    }

    [System.Serializable]
    public class ResourceDataPercentage
    {
        [FormerlySerializedAs("ResourceData")] public ResourceSettings ResourceSettings;
        public float SpawnPercentage = 0f;

        public ResourceDataPercentage(ResourceSettings settings, float percentage)
        {
            ResourceSettings = settings;
            SpawnPercentage = percentage;
        }
    }
    
    [System.Serializable]
    public class MountainDataPercentage
    {
        [FormerlySerializedAs("mountainData")] public MountainSettings mountainSettings;
        public float spawnPercentage = 0f; // Default to 0 for new entries, except for the first one which we'll manage in the editor script.

        // Constructor to easily create new instances with default values
        public MountainDataPercentage(MountainSettings settings, float percentage)
        {
            mountainSettings = settings;
            spawnPercentage = percentage;
        }

        public MountainTileType GetMountainTileType()
        {
            if (mountainSettings == null)
            {
                return MountainTileType.Empty;
            }

            return mountainSettings.MountainTileType;
        }
    }
}
