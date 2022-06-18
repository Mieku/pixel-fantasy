using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gods
{
    public class UIDManager : God<UIDManager>
    {
        private Dictionary<string, GameObject> _uidDictionary;

        private void Awake()
        {
            _uidDictionary = new Dictionary<string, GameObject>();
        }

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
    }
}
