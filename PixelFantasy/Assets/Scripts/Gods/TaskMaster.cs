using System;
using CodeMonkey.Utils;
using Gods;
using UnityEngine;

namespace Gods
{
    public class TaskMaster : God<TaskMaster>
    {
        private TaskSystem taskSystem = new TaskSystem();

        public TaskSystem GetTaskSystem()
        {
            return taskSystem;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                var newTask = new TaskSystem.Task.MoveToPosition
                {
                    targetPosition = UtilsClass.GetMouseWorldPosition()
                };
                taskSystem.AddTask(newTask);
            }
        }
    }
}
