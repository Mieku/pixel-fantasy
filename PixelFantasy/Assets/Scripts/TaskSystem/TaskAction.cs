using System;
using Gods;
using UnityEngine;

namespace TaskSystem
{
    public abstract class TaskAction : MonoBehaviour
    {
        public string TaskId;

        protected TaskAI _ai;
        protected Task _task;
        // protected Family Owner
        // {
        //     get
        //     {
        //         if (_task.Owner == null)
        //         {
        //             return FamilyManager.Instance.GetPlayerFamily();
        //         }
        //
        //         return _task.Owner;
        //     }
        // }

        protected UnitAnimController UnitAnimController => _ai.Unit.UnitAnimController;

        private void Awake()
        {
            GameEvents.OnTaskCancelled += Event_OnTaskCancelled;
        }

        private void OnDestroy()
        {
            GameEvents.OnTaskCancelled -= Event_OnTaskCancelled;
        }
        
        public void AssignOwner(TaskAI ai)
        {
            _ai = ai;
        }
        
        public virtual bool CanDoTask(Task task)
        {
            return true;
        }

        /// <summary>
        /// Triggers once on Action Begin
        /// </summary>
        public abstract void PrepareAction(Task task);
        
        /// <summary>
        /// Runs on an update loop to do the action
        /// </summary>
        public abstract void DoAction();

        public abstract void OnTaskCancel();
        
        /// <summary>
        /// Trigger on Action End
        /// </summary>
        public virtual void ConcludeAction()
        {
            _ai.CurrentTaskDone();
        }
        
        private void Event_OnTaskCancelled(Task task)
        {
            if (_task != null && _task.IsEqual(task))
            {
                OnTaskCancel();
            }
        }
    }
}
