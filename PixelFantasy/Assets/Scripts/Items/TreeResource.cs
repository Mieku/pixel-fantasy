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
            
            // Choose a random side of the tree
            var sideMod = 1;
            var rand = Random.Range(0, 2);
            if (rand == 1)
            {
                sideMod *= -1;
            }

            var cutPos = transform.position;
            cutPos.x += sideMod;

            var task = new FellingTask.CutTree()
            {
                claimTree = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                treePosition = cutPos,
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
