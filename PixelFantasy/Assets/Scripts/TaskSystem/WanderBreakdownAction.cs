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
            var wanderTarget = _ai.Kinling.KinlingAgent.PickLocationInRange(1.0f);
            _ai.Kinling.KinlingAgent.SetMovePosition(wanderTarget, OnReachedLocation);
        }

        private void OnReachedLocation()
        {
            ChooseNewLocation();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            _ai.Kinling.KinlingAgent.SetMovePosition(transform.position);
        }
    }
}
