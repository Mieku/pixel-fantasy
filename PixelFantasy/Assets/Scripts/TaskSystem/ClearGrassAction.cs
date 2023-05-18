using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class ClearGrassAction : TaskAction
    {
        
        private DirtTile _dirtTile;
        private float _timer;
        private Vector2? _movePos;
        private bool _isMoving;

        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        private float DistanceFromRequestor => Vector2.Distance((Vector2)_movePos, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _dirtTile = task.Requestor as DirtTile;
            _movePos = _ai.GetAdjacentPosition(_dirtTile.transform.position);
        }

        public override void DoAction()
        {
            if (DistanceFromRequestor <= 0.25f)
            {
                DoClearGrass();
            }
            else
            {
                MoveToRequestor();
            }
        }
        
        private void DoClearGrass()
        {
            UnitAnimController.SetUnitAction(UnitAction.Digging, _ai.GetActionDirection(_dirtTile.transform.position));
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_dirtTile.DoWork(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }
        
        private void MoveToRequestor()
        {
            if (!_isMoving)
            {
                _ai.Unit.UnitAgent.SetMovePosition((Vector2)_movePos);
                _isMoving = true;
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _dirtTile = null;
            _task = null;
            _movePos = null;
            _isMoving = false;
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            ConcludeAction();
        }
    }
}
