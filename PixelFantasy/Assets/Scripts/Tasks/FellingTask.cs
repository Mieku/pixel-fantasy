using System;
using Actions;
using Characters;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class FellingTask : TaskBase
    {
        public class CutTree : FellingTask
        {
            public override TaskType TaskType => TaskType.CutTree;
            public Action<UnitTaskAI> claimTree;
            public Vector3 treePosition;
            public float workAmount;
        }
    }
}
