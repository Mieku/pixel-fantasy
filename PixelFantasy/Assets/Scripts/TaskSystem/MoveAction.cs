using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class MoveAction : TaskAction
    {
        [SerializeField] private float _afterMoveDelay;
        
        private Vector2 _movePosition;
        private bool _atPosition;
        private float _delayTimer;
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _movePosition = (Vector2)task.Payload;
            _atPosition = false;

            _ai.Kinling.KinlingAgent.SetMovePosition(_movePosition, OnReachedPosition, OnTaskCancel);
        }

        private void OnReachedPosition()
        {
            _delayTimer = 0;
            _atPosition = true;
        }

        public override void DoAction()
        {
            if (_atPosition)
            {
                _delayTimer += TimeManager.Instance.DeltaTime;
                if (_delayTimer >= _afterMoveDelay)
                {
                    ConcludeAction();
                }
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            _delayTimer = 0;
            _atPosition = false;
        }
    }
}
