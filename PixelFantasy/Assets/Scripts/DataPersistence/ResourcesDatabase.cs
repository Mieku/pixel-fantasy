using System.Collections;
using System.Collections.Generic;
using Controllers;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DataPersistence
{
    public class ResourcesDatabase : MonoBehaviour
    {
        public static ResourcesDatabase Instance { get; private set; }

        private List<BasicResource> resources = new List<BasicResource>();
        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterResource(BasicResource resource)
        {
            resources.Add(resource);
        }

        public void DeregisterResource(BasicResource resource)
        {
            resources.Remove(resource);
        }

        public List<BasicResourceData> GetResourcesData()
        {
            var resourcesData = new List<BasicResourceData>();
            foreach (var resource in resources)
            {
                resourcesData.Add(resource.RuntimeData);
            }
            return resourcesData;
        }

        public void LoadResourcesData(List<BasicResourceData> resourcesData)
        {
            DeleteResources();

            foreach (var data in resourcesData)
            {
                ResourceSettings settings;
                if (data is GrowingResourceData)
                {
                    settings = Resources.Load<GrowingResourceSettings>($"Settings/Resource/Growing Resources/{data.SettingsName}");
                }
                else if (data is MountainResourceData)
                {
                    settings = Resources.Load<MountainSettings>($"Settings/Resource/Mountains/{data.SettingsName}");
                }
                else
                {
                    settings = Resources.Load<ResourceSettings>($"Settings/Resource/Basic Resources/{data.SettingsName}");
                }

                var resource = SpawnResource(settings, data.Position);
                if (resource is GrowingResource growingResource && data is GrowingResourceData growingData)
                {
                    growingResource.LoadResource(growingData);
                }
                else
                {
                    resource.LoadResource(data);
                }
            }
        }

        public BasicResource SpawnResource(ResourceSettings settings, Vector2 spawnPos)
        {
            var resource = Instantiate(settings.Prefab, new Vector3(spawnPos.x, spawnPos.y, -1), Quaternion.identity, transform);
            resource.gameObject.name = settings.ResourceName;
            resource.InitializeResource(settings);
            return resource;
        }
        
        public IEnumerator BatchSpawnMountainsAsync(List<Vector3Int> positions, List<MountainSettings> settings, int batchSize = 100)
        {
            for (int i = 0; i < positions.Count; i += batchSize)
            {
                for (int j = 0; j < batchSize && (i + j) < positions.Count; j++)
                {
                    var pos = new Vector3(positions[i + j].x + 0.5f, positions[i + j].y + 0.5f, -1);
                    var mountainPrefab = Resources.Load<Mountain>($"Prefabs/MountainPrefab");
                    var mountain = Instantiate(mountainPrefab, pos, Quaternion.identity, transform);
                    mountain.InitializeResource(settings[i + j]);
                    mountain.gameObject.name = settings[i + j].ResourceName;
                }

                yield return null; // Wait for the next frame
            }
        }

        public void DeleteResources()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                DestroyImmediate(child);
            }

            resources.Clear();
            
            _mountainTM.ClearAllTiles();
        }
    }
}
