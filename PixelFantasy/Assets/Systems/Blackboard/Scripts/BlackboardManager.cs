using System;
using System.Collections.Generic;
using Managers;
using Systems.Needs.Scripts;
using UnityEngine;

namespace Systems.Blackboard.Scripts
{
    public enum EBlackboardKey
    {
        Character_FocusObject,
        
        Household_ObjectsInUse,
    }
    
    public class Blackboard
    {
        private Dictionary<EBlackboardKey, int>         _intValues           = new Dictionary<EBlackboardKey, int>();
        private Dictionary<EBlackboardKey, float>       _floatValues         = new Dictionary<EBlackboardKey, float>();
        private Dictionary<EBlackboardKey, bool>        _boolValues          = new Dictionary<EBlackboardKey, bool>();
        private Dictionary<EBlackboardKey, string>      _stringValues        = new Dictionary<EBlackboardKey, string>();
        private Dictionary<EBlackboardKey, Vector3>     _vector3Values       = new Dictionary<EBlackboardKey, Vector3>();
        private Dictionary<EBlackboardKey, GameObject>  _gameObjectValues    = new Dictionary<EBlackboardKey, GameObject>();
        private Dictionary<EBlackboardKey, object>      _genericValues       = new Dictionary<EBlackboardKey, object>();

        private Dictionary<NeedSettings, float>               _aIStatValues        = new Dictionary<NeedSettings, float>();

        public void SetGeneric<T>(EBlackboardKey key, T value)
        {
            _genericValues[key] = value;
        }

        public T GetGeneric<T>(EBlackboardKey key)
        {
            if (!_genericValues.ContainsKey(key))
                throw new System.ArgumentException($"Could not find value for {key} in _genericValues");
            
            return (T)_genericValues[key];
        }
        
        public bool TryGetGeneric<T>(EBlackboardKey key, out T value, T defaultValue)
        {
            if (_genericValues.ContainsKey(key))
            {
                value = (T)_genericValues[key];
                return true;
            }

            value = defaultValue;
            return false;
        }

        private T Get<T>(Dictionary<EBlackboardKey, T> keySet, EBlackboardKey key)
        {
            if (!keySet.ContainsKey(key))
                throw new ArgumentException($"Could not find value for {key} in {typeof(T).Name}Values");

            return keySet[key];
        }
        
        private bool TryGet<T>(Dictionary<EBlackboardKey, T> keySet, EBlackboardKey key, out T value, T defaultValue = default)
        {
            if (keySet.ContainsKey(key))
            {
                value = keySet[key];
                return true;
            }

            value = default;
            return false;
        }
        
        public void SetStat(NeedSettings linkedStat, float value)
        {
            _aIStatValues[linkedStat] = value;
        }

        public float GetStat(NeedSettings linkedStat)
        {
            if (!_aIStatValues.ContainsKey(linkedStat))
                throw new System.ArgumentException($"Could not find value for {linkedStat.DisplayName} in _aIStatValues");
            
            return _aIStatValues[linkedStat];
        }
        
        public bool TryGetStat(NeedSettings linkedStat, out float value, float defaultValue = 0f)
        {
            if (_aIStatValues.ContainsKey(linkedStat))
            {
                value = _aIStatValues[linkedStat];
                return true;
            }

            value = defaultValue;
            return false;
        }

        public void Set(EBlackboardKey key, int value)
        {
            _intValues[key] = value;
        }

        public int GetInt(EBlackboardKey key)
        {
            return Get(_intValues, key);
        }
        
        public bool TryGet(EBlackboardKey key, out int value, int defaultValue = 0)
        {
            return TryGet(_intValues, key, out value, defaultValue);
        }
        
        public void Set(EBlackboardKey key, float value)
        {
            _floatValues[key] = value;
        }
        
        public float GetFloat(EBlackboardKey key)
        {
            return Get(_floatValues, key);
        }
        
        public bool TryGet(EBlackboardKey key, out float value, float defaultValue = 0f)
        {
            return TryGet(_floatValues, key, out value, defaultValue);
        }
        
        public void Set(EBlackboardKey key, bool value)
        {
            _boolValues[key] = value;
        }

        public bool GetBool(EBlackboardKey key)
        {
            return Get(_boolValues, key);
        }
        
        public bool TryGet(EBlackboardKey key, out bool value, bool defaultValue = false)
        {
            return TryGet(_boolValues, key, out value, defaultValue);
        }
        
        public void Set(EBlackboardKey key, string value)
        {
            _stringValues[key] = value;
        }
        
        public string GetString(EBlackboardKey key)
        {
            return Get(_stringValues, key);
        }
        
        public bool TryGet(EBlackboardKey key, out string value, string defaultValue = "")
        {
            return TryGet(_stringValues, key, out value, defaultValue);
        }
        
        public void Set(EBlackboardKey key, Vector3 value)
        {
            _vector3Values[key] = value;
        }
        
        public Vector3 GetVector3(EBlackboardKey key)
        {
            return Get(_vector3Values, key);
        }
        
        public bool TryGet(EBlackboardKey key, out Vector3 value, Vector3 defaultValue)
        {
            return TryGet(_vector3Values, key, out value, defaultValue);
        }
        
        public void Set(EBlackboardKey key, GameObject value)
        {
            _gameObjectValues[key] = value;
        }
        
        public GameObject GetGameObject(EBlackboardKey key)
        {
            return Get(_gameObjectValues, key);
        }
        
        public bool TryGet(EBlackboardKey key, out GameObject value, GameObject defaultValue = null)
        {
            return TryGet(_gameObjectValues, key, out value, defaultValue);
        }
    }
    
    public class BlackboardManager : Singleton<BlackboardManager>
    {
        private Dictionary<MonoBehaviour, Blackboard> _individualBlackboards = new Dictionary<MonoBehaviour, Blackboard>();
        private Dictionary<int, Blackboard> _sharedBlackboards = new Dictionary<int, Blackboard>();

        public Blackboard GetIndividualBlackboard(MonoBehaviour requestor)
        {
            if (!_individualBlackboards.ContainsKey(requestor))
            {
                _individualBlackboards[requestor] = new Blackboard();
            }

            return _individualBlackboards[requestor];
        }

        public Blackboard GetSharedBlackboard(int uniqueID)
        {
            if (!_sharedBlackboards.ContainsKey(uniqueID))
            {
                _sharedBlackboards[uniqueID] = new Blackboard();
            }

            return _sharedBlackboards[uniqueID];
        }
    }
}
