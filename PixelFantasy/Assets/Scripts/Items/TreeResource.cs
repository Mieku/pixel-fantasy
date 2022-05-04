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
        }

        public void CancelCutTreeTask()
        {
            _queuedToCut = false;
            SetIcon(null);
            CancelTasks();
        }
    }
}
