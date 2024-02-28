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
        [field: SerializeField] public JobData RequiredJob { get; protected set; }
        [field: SerializeField] public Sprite HorizontalDoorframe { get; protected set; }
        [field: SerializeField] public Sprite VerticalDoorframe { get; protected set; }

        [SerializeField] private List<ItemAmount> _resourceCosts;
        
        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>();
            foreach (var resourceCost in _resourceCosts)
            {
                ItemAmount cost = new ItemAmount
                {
                    Item = resourceCost.Item,
                    Quantity = resourceCost.Quantity
                };
                clone.Add(cost);
            }

            return clone;
        }
    }
}
