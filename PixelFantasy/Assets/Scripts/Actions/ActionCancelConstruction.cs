using Items;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionCancelConstruction", menuName ="Actions/CancelConstruction", order = 50)]
    public class ActionCancelConstruction : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var construction = requestor.GetComponent<Construction>();
            construction.CancelConstruction();
            return null;
        }
        
        public override bool IsTaskAvailable(Interactable requestor)
        {
            var construction = requestor.GetComponent<Construction>();
            return !construction.IsBuilt;
        }
    }
}
