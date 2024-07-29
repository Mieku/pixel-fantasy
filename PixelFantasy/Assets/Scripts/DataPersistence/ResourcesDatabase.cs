using System.Collections;
using System.Collections.Generic;
using Controllers;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DataPersistence
{
    public class ResourcesDatabase : MonoBehaviour
    {
        public static ResourcesDatabase Instance { get; private set; }

        [ShowInInspector] private Dictionary<string, BasicResource> _registeredResources = new Dictionary<string, BasicResource>();
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
            _registeredResources[resource.UniqueID] = resource;
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(resource);
        }

        public void DeregisterResource(BasicResource resource)
        {
            _registeredResources.Remove(resource.UniqueID);
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(resource);
        }

        public BasicResource QueryResource(string uniqueID)
        {
            var result = _registeredResources[uniqueID];
            return result;
        }

        public Dictionary<string, BasicResourceData> GetResourcesData()
        {
            Dictionary<string, BasicResourceData> results = new Dictionary<string, BasicResourceData>();
            foreach (var kvp in _registeredResources)
            {
                results.Add(kvp.Key, kvp.Value.RuntimeData);
            }
            return results;
        }

        public void LoadResourcesData(Dictionary<string, BasicResourceData> resourcesData)
        {
            foreach (var kvp in resourcesData)
            {
                SpawnLoadedResource(kvp.Value);
            }
        }

        public BasicResource SpawnLoadedResource(BasicResourceData data)
        {
            var settings = data.Settings;
            var resource = Instantiate(settings.Prefab, data.Position, Quaternion.identity, transform);
            resource.gameObject.name = settings.ResourceName;
            resource.LoadResource(data);
            return resource;
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

            _registeredResources.Clear();
            
            _mountainTM.ClearAllTiles();
        }
    }
}
