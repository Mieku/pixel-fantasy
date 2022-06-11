using System;
using Characters;
using UnityEngine;

namespace Tasks
{
    [Serializable]
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
