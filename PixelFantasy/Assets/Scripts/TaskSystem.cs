using System.Collections.Generic;
using UnityEngine;

public class TaskSystem
{
    public abstract class Task
    {
            
            
        public class MoveToPosition : Task
        {
            public Vector3 targetPosition;
        }
    }

    private List<Task> taskList;

    public TaskSystem()
    {
        taskList = new List<Task>();
    }

    /// <summary>
    /// Used by a worker to request the next task
    /// </summary>
    public Task RequestNextTask()
    {
        if (taskList.Count > 0)
        {
            Task task = taskList[0];
            taskList.RemoveAt(0);
            return task;
        }
        else
        {
            // No tasks are available
            return null;
        }
    }

    /// <summary>
    /// Adds a new task to the end of the task queue
    /// </summary>
    public void AddTask(Task task)
    {
        taskList.Add(task);
    }
}