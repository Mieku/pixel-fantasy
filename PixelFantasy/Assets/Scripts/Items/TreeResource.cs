using System;
using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using Tasks;
using Unit;
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

        public void CreateCutTreeTask()
        {
            if (_queuedToCut)
            {
                CancelCutTreeTask();
                return;
            }
            
            CancelTasks();
            _queuedToCut = true;
            SetIcon("Axe");

            var task = new FellingTask.CutTree()
            {
                claimTree = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                treePosition = transform.position,
                workAmount = _growingResourceData.GetWorkToCut(_growthIndex),
                completeWork = CutDownPlant
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FellingTaskSystem.AddTask(task);
            
            RefreshSelection();
        }

        public void CancelCutTreeTask()
        {
            _queuedToCut = false;
            SetIcon(null);
            CancelTasks();
        }

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

        public override void AssignOrder(Order orderToAssign)
        {
            switch (orderToAssign)
            {
                case Order.CutPlant:
                    CreateCutTreeTask();
                    break;
                case Order.Harvest:
                    CreateHarvestFruitTask();
                    break;
            }
        }
    }
}
