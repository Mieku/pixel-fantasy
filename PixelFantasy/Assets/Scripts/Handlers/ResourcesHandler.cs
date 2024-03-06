using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Handlers
{
    public class ResourcesHandler : MonoBehaviour
    {

        public void DeleteResources()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                // Destroy immediate to ensure the object is removed instantly
                children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                DestroyImmediate(child);
            }
        }
        
        public void SpawnResource(ResourceSettings settings, Vector2 spawnPos)
        {
            var resource = Instantiate(settings.ResourcePrefab, new Vector3(spawnPos.x, spawnPos.y, -1), Quaternion.identity, transform);
            resource.gameObject.name = settings.ResourceName;
            resource.Init(settings);
        }
    }
}
