using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AI
{
    [Serializable]
    public class TaskContext
    {
        [ShowInInspector] public Dictionary<string, string> Data;

        public TaskContext()
        {
            Data = new Dictionary<string, string>();
        }

        public T GetData<T>(string key)
        {
            if (Data.ContainsKey(key))
            {
                return JsonUtility.FromJson<T>(Data[key]);
            }
            return default(T);
        }

        public void SetData<T>(string key, T value)
        {
            Data[key] = JsonUtility.ToJson(value);
        }
    }
}