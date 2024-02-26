using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    [CreateAssetMenu(fileName = "DoorSO", menuName = "CraftedData/DoorSO", order = 1)]
    public class DoorSO : ScriptableObject
    {
        [field: SerializeField] public string DoorName { get; protected set; }
        [field: SerializeField] public Sprite Icon { get; protected set; }
        [field: SerializeField] public Door DoorPrefab { get; protected set; }
        public List<string> InvalidPlacementTags => new List<string>() { "Water", "Floor", "Obstacle" };
        [field: SerializeField] public JobData RequiredJob { get; protected set; }
        [field: SerializeField] public List<ItemAmount> ResourceCosts { get; protected set; }
    }
}
