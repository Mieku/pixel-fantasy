using Items;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionLockDoor", menuName ="Actions/LockDoor", order = 50)]
    public class ActionLockDoor : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var door = requestor.GetComponent<Door>();
            door.SetLocked(true);
            return null;
        }
        
        public override bool IsTaskAvailable(Interactable requestor)
        {
            var construction = requestor.GetComponent<Construction>();
            return construction.IsBuilt;
        }
        
        public override void CancelTask(Interactable requestor)
        {
            var door = requestor.GetComponent<Door>();
            door.SetLocked(false);
        }
    }
}
