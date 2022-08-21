using System;
using System.Collections.Generic;
using Actions;
using Gods;
using ScriptableObjects;
using Tasks;
using Characters;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace Items
{
    public class TreeResource : GrowingResource
    {
        protected override void AttemptReproduction()
        {
            var pos = _growingResourceData.GetReproductionPos(transform.position);
            var valid = _growingResourceData.IsReproductionPosValid(pos);
            if (valid)
            {
                spawner.SpawnTree(pos, GetResourceData());
            }
        }
        
        // public void CancelCutTreeTask()
        // {
        //     _queuedToCut = false;
        //     //SetIcon(null);
        //     //CancelTasks();
        // }

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
            Debug.Log($"Work Done, remaining: {_remainingWork}");
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
