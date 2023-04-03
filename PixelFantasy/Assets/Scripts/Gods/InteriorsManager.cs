using System.Collections.Generic;
using Buildings;
using UnityEngine;

namespace Gods
{
    public class InteriorsManager : God<InteriorsManager>
    {
        [SerializeField] private Vector3 _interiorSpacing;

        private Dictionary<BuildingNode, Interior> _interiorsDictionary = new Dictionary<BuildingNode, Interior>();

        public Interior GenerateInterior(BuildingNode buildingNode)
        {
            var interior = Instantiate(buildingNode.Interior, transform);
            _interiorsDictionary.Add(buildingNode, interior);
            interior.transform.position = DeterminePosition();
            buildingNode.LinkEntrance(interior.EntrancePos);
            NavMeshManager.Instance.UpdateNavMesh();
            return interior;
        }

        public Vector3 DeterminePosition()
        {
            int index = _interiorsDictionary.Count;
            return _interiorSpacing * index;
        }
    }
}
