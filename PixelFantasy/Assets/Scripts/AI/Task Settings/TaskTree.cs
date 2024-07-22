using System;
using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

public class TaskTree : MonoBehaviour
{
    public string TaskID;
    public BehaviourTreeOwner BTOwner;

    private Action<bool> _onFinishedCallback;

    private void Awake()
    {
        if (BTOwner == null)
        {
            BTOwner = GetComponent<BehaviourTreeOwner>();
        }
    }

    public void ExecuteTask(Dictionary<string, object> TaskData, Action<bool> OnTaskComplete)
    {
        _onFinishedCallback = OnTaskComplete;
        
        foreach (var data in TaskData)
        {
            BTOwner.blackboard.SetVariableValue(data.Key, data.Value);
        }
        
        BTOwner.StartBehaviour(OnFinished);
    }
    
    private void OnFinished(bool success)
    {
        ResetBlackboard();
        
        Debug.Log($"Task is finished, was it successful? {success}");
        _onFinishedCallback.Invoke(success);
    }

    private void ResetBlackboard()
    {
        foreach (var kvp in BTOwner.blackboard.variables)
        {
            BTOwner.blackboard.SetVariableValue(kvp.Key, default);
        }
    }
}
