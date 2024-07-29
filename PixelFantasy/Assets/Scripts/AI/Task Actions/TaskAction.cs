using System;
using UnityEngine;

namespace AI
{
    [Serializable]
    public abstract class TaskAction : ScriptableObject
    {
        public abstract bool IsActionPossible(string kinlingID, Task task);
        public abstract void ExecuteAction(string kinlingID, Task task);
        public abstract void CancelAction(string kinlingID, Task task);
    }
}