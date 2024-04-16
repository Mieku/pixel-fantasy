using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ModulesScriptableObject : ScriptableObject, IHaveUid, IHaveDictionaries
    {
        [HideInInspector] public string objectName;
        [HideInInspector] public string objectType;
        
        public string ObjectName => objectName;
        public string ObjectType => objectType;
        
        public Dictionaries dictionaries;
        [HideInInspector] public bool showInManager;
        
        // ******************************************************************
        // Uid & related methods. Uid is created via this scriptable object!
        // ******************************************************************
        
        [SerializeField] private string _uid;
        
        // Will create a new _uid if one does not exist
        public virtual string Uid() =>
            String.IsNullOrWhiteSpace(_uid) 
                ? _uid = Guid.NewGuid().ToString() 
                : _uid;
        
        // Used to reset the Uid, via the Inspector script when duplicate Uids
        // are found
        public void ResetUid()
        {
            Debug.LogWarning("Resetting uid!");
            _uid = "";
            Uid();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        // TODO: Refactor this so that all references to uid just call Uid() instead
        //public string uid => Uid();
        
        private void OnEnable()
        {
            dictionaries ??= new Dictionaries(objectName);
            dictionaries.keyValues ??= new List<KeyValue>();
        }
        
        
        // -------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // -------------------------------------------------------------------------------------
        
        public virtual void AddDictionaryKey(string key)
        {
            throw new NotImplementedException();
            /*
             * This is blank on purpose. In this context, we use AddDictionaryKeyToSO() which
             * will add the key to all of the objects of this type, so that they always match.
             *
             * If you got the NotImplementedException error, please let me know, as it may indicate
             * there is a bug to squash!
             */
        }

        public virtual KeyValue GetKeyValue(string key) 
            => dictionaries.keyValues.FirstOrDefault(x => x.key == key);

        public virtual bool HasKeyValue(string key) 
            => dictionaries.keyValues.Any(x => x.key == key);


        // -------------------------------------------------------------------------------------
        // EDITOR / INSPECTOR
        // -------------------------------------------------------------------------------------

        public void AddDictionaryKeyToSo(string keyName, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (var itemObject in ItemObjectArray().Where(x => x.objectType == objectType))
                    itemObject.AddDictionaryKey(keyName);

                return;
            }

            if (GetKeyValue(keyName) != null)
            {
                Debug.LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            #if UNITY_EDITOR
            Undo.RecordObject(this, "Undo add key to all dictionaries");
            #endif
            var newKeyValue = new KeyValue
            {
                key = keyName
            };
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public static ItemObject[] ItemObjectArray()
        {
            int i = 0;
            #if UNITY_EDITOR
            string[] guids1 = AssetDatabase.FindAssets("t:ItemObject", null);
            var allItems = new ItemObject[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allItems[i] = AssetDatabase.LoadAssetAtPath<ItemObject>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            return allItems;
            #else
            return null;
            #endif
        }
        
        // Editor / Inspector
        //[HideInInspector] public bool showFullInspector;

        public string Name => name;

        public void ChangeName(string newName)
        {
            #if UNITY_EDITOR
            objectName = newName;
            name = newName;
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), newName);
#else
            return;
#endif
        }

        public void CheckObjectType()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrWhiteSpace(path)) return;
            var pathParts = path.Split("/"[0]);
            objectType = pathParts[^2];
#endif
        }
        
        public void CheckObjectName()
        {
#if UNITY_EDITOR
            if (objectName != null && objectName != name)
                objectName = name;
#endif
        }

        public void CheckForMissingObjectReferences()
        {
#if !UNITY_EDITOR
            return;
#endif
            foreach (var keyValue in dictionaries.keyValues)
                keyValue.CheckForMissingObjectReferences();
        }
    }
}