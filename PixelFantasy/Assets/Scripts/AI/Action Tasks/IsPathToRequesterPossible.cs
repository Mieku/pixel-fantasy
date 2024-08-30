using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace AI.Action_Tasks
{
    [Category("Custom/Conditions")]
    [Name("Is Path Possible To Requester")]
    [Description("Returns true if there is a path")]
    public class IsPathToRequesterPossible : ConditionTask
    {
        public BBParameter<string> KinlingUID;
        public BBParameter<string> RequesterUID;
        
        protected override bool OnCheck()
        {
            var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            if (requester == null) return false;

            var usePos = requester.UseagePosition(kinling.transform.position);
            
            return usePos != null;
        }
    }
}
