using System;
using System.Collections.Generic;
using UnityEngine;

namespace Handlers
{
    public class RampsHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _northRampPrefab;
        [SerializeField] private GameObject _southRampPrefab;
        [SerializeField] private GameObject _eastRampPrefab;
        [SerializeField] private GameObject _westRampPrefab;
    
        public void DeleteRamps()
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
    
        public void SpawnRamp(ERampDirection direction, float x, float y)
        {
            GameObject prefab = null;
            switch (direction)
            {
                case ERampDirection.North:
                    prefab = _northRampPrefab;
                    break;
                case ERampDirection.South:
                    prefab = _southRampPrefab;
                    break;
                case ERampDirection.East:
                    prefab = _eastRampPrefab;
                    break;
                case ERampDirection.West:
                    prefab = _westRampPrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var ramp = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, transform);
        }
    }

    public enum ERampDirection
    {
        North,
        South,
        East,
        West
    }
}