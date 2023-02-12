using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Actions;
using CodeMonkey.Utils;
using Characters;
using Gods;
using UnityEngine;

[Serializable]
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

    public int Hash()
    {
        return tryGetTaskFunc.GetHashCode();
    }
}
    
// Base Task Class
[Serializable]
public abstract class TaskBase
{
    public virtual TaskType TaskType { get; }
    public virtual string TargetUID { get; set; }
    public ActionBase TaskAction;
    public string RequestorUID;
    public Action<ActionBase> OnTaskAccepted;
    public Action OnCompleteTask;

    public Interactable Requestor => UIDManager.Instance.GetGameObject(RequestorUID).GetComponent<Interactable>();
}

[Serializable]
public class TaskSystem<TTask> where TTask : TaskBase
{
    [SerializeField] private bool _verbose;
    public List<TTask> taskList; // List of tasks ready to be executed
    public List<QueuedTask<TTask>> queuedTaskList; // Any queued tasks that need to be validated before being dequeued

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
        if (_verbose)
        {
            Debug.Log("Adding Task: " + taskBase.TargetUID);
        }
        
        taskList.Add(taskBase);
    }

    public void EnqueueTask(QueuedTask<TTask> queuedTask)
    {
        queuedTaskList.Add(queuedTask);
    }

    public QueuedTask<TTask> EnqueueTask(Func<TTask> tryGetTaskFunc)
    {
        if (_verbose)
        {
            Debug.Log("Enquing Task: " + tryGetTaskFunc.Target.ToString());
        }
        
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

    public void CancelTask(string requestorUID)
    {
        var removeTasks = new List<TTask>();
        foreach (var task in taskList)
        {
            var taskBase = task as TaskBase;
            if (taskBase.RequestorUID.Equals(requestorUID))
            {
                removeTasks.Add(task);
            }
        }
        foreach (var removeTask in removeTasks)
        {
            taskList.Remove(removeTask);
        }
    }
    
    public bool CancelTask(int taskRef)
    {
        foreach (var queuedTask in queuedTaskList)
        {
            if (queuedTask.Hash() == taskRef)
            {
                queuedTaskList.Remove(queuedTask);
                return true;
            }
        }
        
        foreach (var task in taskList)
        {
            if (task.GetHashCode() == taskRef)
            {
                taskList.Remove(task);
                return true;
            }
        }

        return false;
    }

    public void ClearTasks()
    {
        taskList.Clear();
        queuedTaskList.Clear();
    }
}