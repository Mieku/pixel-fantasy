using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ObjectReferenceType
    {
        public string typeName;
        public List<ObjectReferenceObject> objects = new List<ObjectReferenceObject>();

        //public Dictionary<string, object> cachedObjects = new Dictionary<string, object>();
        //public Dictionary<object, string> cachedGuids = new Dictionary<object, string>();
        
        private Dictionary<string, object> _cachedObjects;
        public Dictionary<string, object> cachedObjects
        {
            get
            {
                if (_cachedObjects == null)
                {
                    _cachedObjects = new Dictionary<string, object>();
                }
                return _cachedObjects;
            }
        }

        private Dictionary<object, string> _cachedGuids;
        public Dictionary<object, string> cachedGuids
        {
            get
            {
                if (_cachedGuids == null)
                {
                    _cachedGuids = new Dictionary<object, string>();
                }
                return _cachedGuids;
            }
        }


        public ObjectReferenceType(string newType)
        {
            typeName = newType;
        }

        public void Add(object obj)
        {
#if UNITY_EDITOR
            var guid = FindGuid(obj);

            // If it's already in the list, then return
            if (objects.Any(x => x.guid == guid))
                return;

            objects.Add(new ObjectReferenceObject(guid, obj, typeName));
#endif
        }

        public string FindGuid(object obj)
        {
#if UNITY_EDITOR
            var objAsObject = obj as Object;
            if (objAsObject == null)
            {
                return default;
            }

            var path = AssetDatabase.GetAssetPath(objAsObject);
            if (path == null)
            {
                Debug.LogWarning("Path returned null, indicating this does not have a guid.");
                return default;
            }

            return AssetDatabase.AssetPathToGUID(path);
#endif
            return default;
        }

        public T Get<T>(string guid)
        {
            // If it's already in the cached dictionary, return the value object
            if (cachedObjects.TryGetValue(guid, out var value))
                return (T)value;

            // If we don't have the object, then return default
            var foundObject = objects.FirstOrDefault(x => x.guid == guid);
            if (foundObject == null)
                return default;
            
            // Get the object of correct type
            var objectOfType = foundObject.GetObject();
            if (objectOfType is T castedObjectOfType)
            {
                // Cache the object and return it
                cachedObjects[guid] = castedObjectOfType;
                return castedObjectOfType;
            }
            else
            {
                return default;
            }


            // Cache the object and return it
            //cachedObjects[guid] = foundObject.obj;
            //return (T)foundObject.obj;
        }

        public string GetGuid(object obj)
        {
            // If it's already in the cached dictionary, return the value object
            if (cachedGuids.TryGetValue(obj, out var value))
                return value;

            // If we don't have the object, then return default
            var foundObject = objects.FirstOrDefault(x => x.GetObject() == obj);
            if (foundObject == null)
                return default;

            // Cache the object and return it
            cachedGuids[obj] = foundObject.guid;
            return foundObject.guid;
        }
    }
}