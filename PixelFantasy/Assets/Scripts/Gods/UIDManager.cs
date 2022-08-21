using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gods
{
    public class UIDManager : God<UIDManager>
    {
        private Dictionary<string, GameObject> _uidDictionary = new Dictionary<string, GameObject>();
        
        public void AddUID(UID uid)
        {
            if (_uidDictionary.ContainsKey(uid.uniqueID))
            {
                _uidDictionary[uid.uniqueID] = uid.gameObject;
            }
            else
            {
                _uidDictionary.Add(uid.uniqueID, uid.gameObject);
            }
        }

        public void RemoveUID(UID uid)
        {
            if (_uidDictionary.ContainsKey(uid.uniqueID))
            {
                _uidDictionary.Remove(uid.uniqueID);
            }
        }

        public GameObject GetGameObject(string uniqueId)
        {
            if (_uidDictionary.ContainsKey(uniqueId))
            {
                return _uidDictionary[uniqueId];
            }
            else
            {
                Debug.LogError("Unable to find gameobject with uniqueId: " + uniqueId);
                return null;
            }
        }

        [Button("UIDs To Console")]
        private void PrintOutUIDs()
        {
            if (_uidDictionary == null) return;
            foreach (var kvPair in _uidDictionary)
            {
                Debug.Log($"{kvPair.Key} : {kvPair.Value}");
            }
        }
    }
}
