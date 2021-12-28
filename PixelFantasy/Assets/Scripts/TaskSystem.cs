using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Unit;
using UnityEngine;

public class TaskSystem : MonoBehaviour
{
    public class QueuedTask
    {
        private Func<Task> tryGetTaskFunc;
        public QueuedTask(Func<Task> tryGetTaskFunc)
        {
            this.tryGetTaskFunc = tryGetTaskFunc;
        }

        public Task TryDequeueTask()
        {
            return tryGetTaskFunc();
        }
    }
    
    // Base Task Class
    public abstract class Task
    {
        /// <summary>
        /// Unit moves to the target position
        /// </summary>
        public class MoveToPosition : Task
        {
            public Vector3 targetPosition;
        }

        /// <summary>
        /// Unit moves to the target position, and executes the cleanup action
        /// </summary>
        public class GarbageCleanup : Task
        {
            public Vector3 targetPosition;
            public Action cleanUpAction;
        }

        /// <summary>
        /// Unit moves to the item position, grabs the item, takes it to the item slot, drops item
        /// </summary>
        public class TakeItemToItemSlot : Task
        {
            public Vector3 itemPosition;
            public Action<UnitTaskAI> grabItem;
            public Vector3 itemSlotPosition;
            public Action dropItem;
        }
    }

    private List<Task> taskList; // List of tasks ready to be executed
    private List<QueuedTask> queuedTaskList; // Any queued tasks that need to be validated before being dequeued

    public TaskSystem()
    {
        taskList = new List<Task>();
        queuedTaskList = new List<QueuedTask>();
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

    public void EnqueueTask(QueuedTask queuedTask)
    {
        queuedTaskList.Add(queuedTask);
    }

    public void EnqueueTask(Func<Task> tryGetTaskFunc)
    {
        QueuedTask queuedTask = new QueuedTask(tryGetTaskFunc);
        queuedTaskList.Add(queuedTask);
    }

    public void DequeueTasks()
    {
        Debug.Log("Dequeue Tasks");
        for (int i = 0; i < queuedTaskList.Count; i++)
        {
            QueuedTask queuedTask = queuedTaskList[i];
            Task task = queuedTask.TryDequeueTask();
            if (task != null)
            {
                // Task dequeued! Add to the normal list
                AddTask(task);
                queuedTaskList.RemoveAt(i);
                i--;
            }
            else
            {
                // Returned task is null, keep it queued
            }
        }
    }
}