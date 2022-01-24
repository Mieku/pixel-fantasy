using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    public class FellingTask : TaskBase
    {
        public class CutTree : FellingTask
        {
            public Action<UnitTaskAI> claimTree;
            public Vector3 treePosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
