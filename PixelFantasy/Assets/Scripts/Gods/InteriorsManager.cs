using System.Collections.Generic;
using UnityEngine;

namespace Gods
{
    public class InteriorsManager : God<InteriorsManager>
    {
        [SerializeField] private Vector3 _interiorSpacing;

        private Dictionary<Building, Interior> _interiorsDictionary = new Dictionary<Building, Interior>();

        public void GenerateInterior(Building building)
        {
            var interior = Instantiate(building.Interior, transform);
            _interiorsDictionary.Add(building, interior);
            interior.transform.position = DeterminePosition();
            building.LinkEntrance(interior.EntrancePos);
            NavMeshManager.Instance.UpdateNavMesh();
        }

        public Vector3 DeterminePosition()
        {
            int index = _interiorsDictionary.Count;
            return _interiorSpacing * index;
        }
    }
}
