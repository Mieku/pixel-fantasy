using System;
using System.Collections.Generic;
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

        public FellingTask.CutTree CreateCutTreeTask(bool autoAssign = true)
        {
            if (_queuedToCut)
            {
                CancelCutTreeTask();
                return null;
            }
            
            CancelTasks();
            _queuedToCut = true;
            SetIcon("Axe");

            var task = new FellingTask.CutTree()
            {
                TargetUID = UniqueId,
                claimTree = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                    PendingTask = TaskType.None;
                },
                treePosition = transform.position,
                workAmount = _growingResourceData.GetWorkToCut(_growthIndex),
                completeWork = CutDownPlant
            };
            PendingTask = TaskType.CutTree;
            
            _assignedTaskRefs.Add(task.GetHashCode());

            if (autoAssign)
            {
                taskMaster.FellingTaskSystem.AddTask(task);
            }

            RefreshSelection();

            return task;
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
        
        protected override void RestorePendingTask(TaskType pendingTask)
        {
            base.RestorePendingTask(pendingTask);

            if (pendingTask == TaskType.CutTree)
            {
                CreateCutTreeTask();
            }
        }
    }
}
