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

        public virtual void OnTaskComplete(Interactable requestor)
        {
            // Needs to be filled
        }

        public virtual bool IsTaskAvailable(Interactable requestor)
        {
            return true;
        }
        
        public TaskBase RestoreTask(Interactable requestor, bool autoAssign = true)
        {
            return CreateTask(requestor, autoAssign);
        }
    }
}
