using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Unit;
using UnityEngine;

public class QueuedTask<TTask> where TTask : TaskBase
{
    private Func<TTask> tryGetTaskFunc;
    public QueuedTask(Func<TTask> tryGetTaskFunc)
    {
        this.tryGetTaskFunc = tryGetTaskFunc;
    }

    public TTask TryDequeueTask()
    {
        return tryGetTaskFunc();
    }
}
    
// Base Task Class
public abstract class TaskBase
{
    
}

public class TaskSystem<TTask> where TTask : TaskBase
{
    private List<TTask> taskList; // List of tasks ready to be executed
    private List<QueuedTask<TTask>> queuedTaskList; // Any queued tasks that need to be validated before being dequeued

    public TaskSystem()
    {
        taskList = new List<TTask>();
        queuedTaskList = new List<QueuedTask<TTask>>();
    }
    
    /// <summary>
    /// Used by a worker to request the next task
    /// </summary>
    public TTask RequestNextTask()
    {
        if (taskList.Count > 0)
        {
            TTask taskBase = taskList[0];
            taskList.RemoveAt(0);
            return taskBase;
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
    public void AddTask(TTask taskBase)
    {
        taskList.Add(taskBase);
    }

    public void EnqueueTask(QueuedTask<TTask> queuedTask)
    {
        queuedTaskList.Add(queuedTask);
    }

    public QueuedTask<TTask> EnqueueTask(Func<TTask> tryGetTaskFunc)
    {
        QueuedTask<TTask> queuedTask = new QueuedTask<TTask>(tryGetTaskFunc);
        queuedTaskList.Add(queuedTask);
        return queuedTask;
    }

    public void DequeueTasks()
    {
        for (int i = 0; i < queuedTaskList.Count; i++)
        {
            QueuedTask<TTask> queuedTask = queuedTaskList[i];
            TTask taskBase = queuedTask.TryDequeueTask();
            if (taskBase != null)
            {
                // Task dequeued! Add to the normal list
                AddTask(taskBase);
                queuedTaskList.RemoveAt(i);
                i--;
            }
            else
            {
                // Returned task is null, keep it queued
            }
        }
    }

    public void CancelTask(int taskRef)
    {
        foreach (var queuedTask in queuedTaskList)
        {
            if (queuedTask.GetHashCode() == taskRef)
            {
                queuedTaskList.Remove(queuedTask);
                return;
            }
        }
        
        foreach (var task in taskList)
        {
            if (task.GetHashCode() == taskRef)
            {
                taskList.Remove(task);
                return;
            }
        }
    }
}