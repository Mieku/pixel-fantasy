using System;
using UnityEngine;

namespace Actions
{
    public class ActionBase : ScriptableObject
    {
        public string id;
        public Sprite Icon;

        public virtual TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            return null;
        }

        public virtual TaskBase RestoreTask(Interactable requestor, bool autoAssign = true)
        {
            return null;
        }
    }
}
