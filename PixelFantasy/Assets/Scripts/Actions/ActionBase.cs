using System;
using Gods;
using UnityEngine;

namespace Actions
{
    public class ActionBase : ScriptableObject
    {
        public string id;
        public Sprite Icon;

        protected TaskMaster taskMaster => TaskMaster.Instance;
        
        public virtual TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            return null;
        }
        
        public virtual void CancelTask(Interactable requestor)
        {
            Debug.LogError($"{requestor.name} is cancelling task but there is no override");
        }

        public virtual void OnTaskComplete(Interactable requestor)
        {
            // Needs to be filled
        }

        public virtual bool IsTaskAvailable(Interactable requestor)
        {
            return true;
        }
        
        public virtual TaskBase RestoreTask(Interactable requestor, bool autoAssign = true)
        {
            return CreateTask(requestor, autoAssign);
        }
    }
}
