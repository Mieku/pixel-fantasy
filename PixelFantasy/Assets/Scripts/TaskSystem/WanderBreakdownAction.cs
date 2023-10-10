using System;
using UnityEngine;

namespace TaskSystem
{
    public class WanderBreakdownAction : TaskAction
    {
        public override bool CanDoTask(Task task)
        {
            return true;
        }

        public override void PrepareAction(Task task)
        {
            ChooseNewLocation();
        }

        public override void DoAction()
        {
            
        }

        private void ChooseNewLocation()
        {
            var wanderTarget = _ai.Unit.UnitAgent.PickLocationInRange(1.0f);
            _ai.Unit.UnitAgent.SetMovePosition(wanderTarget, OnReachedLocation);
        }

        private void OnReachedLocation()
        {
            ChooseNewLocation();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
        }
    }
}
