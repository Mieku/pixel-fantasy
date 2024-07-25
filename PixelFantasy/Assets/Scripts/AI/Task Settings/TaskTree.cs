using System;
using System.Collections.Generic;
using Managers;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Task_Settings
{
    public class TaskTree : MonoBehaviour
    {
        public string TaskID;
        public BehaviourTreeOwner BTOwner;
        public Blackboard Blackboard;

        private Action<bool> _onFinishedCallback;

        private void Awake()
        {
            if (BTOwner == null)
            {
                BTOwner = GetComponent<BehaviourTreeOwner>();
            }
        }

        private void Update()
        {
            if (TimeManager.Instance.GameSpeed != GameSpeed.Paused)
            {
                if (BTOwner.isRunning)
                {
                    BTOwner.Tick();
                }
            }
        }

        public void ExecuteTask(Dictionary<string, string> TaskData, Action<bool> onTaskComplete)
        {
            _onFinishedCallback = onTaskComplete;
        
            foreach (var data in TaskData)
            {
                Blackboard.SetVariableValue(data.Key, data.Value);
            }
            
            BTOwner.StartBehaviour(Graph.UpdateMode.Manual, OnFinished);
        }
    
        private void OnFinished(bool success)
        {
            ResetBlackboard();
        
            _onFinishedCallback.Invoke(success);
        }

        private void ResetBlackboard()
        {
            foreach (var kvp in BTOwner.blackboard.variables)
            {
                var variable = kvp.Value;
                var type = variable.GetType().GetGenericArguments()[0];

                object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;

                BTOwner.blackboard.SetVariableValue(kvp.Key, defaultValue);
            }
            
            // Graphs BB
            foreach (var kvp in BTOwner.graph.blackboard.variables)
            {
                var variable = kvp.Value;
                var type = variable.GetType().GetGenericArguments()[0];

                object defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;

                BTOwner.graph.blackboard.SetVariableValue(kvp.Key, defaultValue);
            }
        }
    }
}
