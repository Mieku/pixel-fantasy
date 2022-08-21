using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class TreeResource : GrowingResource
    {
        public override List<Order> GetOrders()
        {
            List<Order> results = new List<Order>();
            results.Add(Order.CutPlant);
            
            if (_hasFruitAvailable)
            {
                results.Add(Order.Harvest);
            }

            return results;
        }

        public float WorkDone(float workAmount)
        {
            _remainingWork -= workAmount;
            return _remainingWork;
        }

        // Triggered by animation
        public void TreeFallen()
        {
            Debug.Log("Tree Fallen!");
            // TODO: Build me!
        }
    }
}
