using Items;
using UnityEngine;

namespace TaskSystem
{
    public class RelocateItemAction : TaskAction
    {
        private Item _item;
        private Vector2 _relocationPosition;
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _item = _task.Requestor as Item;
            _relocationPosition = (Vector2)task.Payload;

            _ai.Kinling.KinlingAgent.SetMovePosition(_item.transform.position, () =>
            {
                _ai.HoldItem(_item);
                _ai.Kinling.KinlingAgent.SetMovePosition(_relocationPosition, () =>
                {
                    _ai.DropCarriedItem(true);
                    ConcludeAction();
                });
            });
        }

        public override void DoAction()
        {
            
        }
    }
}
