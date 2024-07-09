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

        private List<RampData> _rampsData = new List<RampData>();

        public List<RampData> GetRampsData()
        {
            return _rampsData;
        }

        public void LoadRampsData(List<RampData> data)
        {
            DeleteRamps();

            foreach (var rampData in data)
            {
                SpawnRamp(rampData.RampDirection, rampData.Position.x, rampData.Position.y);
            }
        }
    
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
            
            _rampsData.Clear();
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

            Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            var data = new RampData(new Vector2(x, y), direction);
            _rampsData.Add(data);
        }
    }

    public enum ERampDirection
    {
        North,
        South,
        East,
        West
    }

    [Serializable]
    public class RampData
    {
        public Vector2 Position;
        public ERampDirection RampDirection;

        public RampData(Vector2 pos, ERampDirection direction)
        {
            Position = pos;
            RampDirection = direction;
        }
    }
}