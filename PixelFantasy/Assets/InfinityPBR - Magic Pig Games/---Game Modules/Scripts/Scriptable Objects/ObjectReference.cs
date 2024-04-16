using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    [CreateAssetMenu(fileName = "Object Reference", menuName = "Game Modules/Create/Object Reference", order = 1)]
    public class ObjectReference : ScriptableObjectSingleton<ObjectReference>
    {
        public List<ObjectReferenceType> objectTypes = new List<ObjectReferenceType>();

        public Dictionary<string, ObjectReferenceType> cachedObjectReferenceTypes =
            new Dictionary<string, ObjectReferenceType>();

        private void Awake()
        {
            PopulateDictionaries();
        }
        
        protected override void Setup()
        {
#if UNITY_EDITOR
           
#endif
        }
        

        public void PopulateDictionaries()
        {
            foreach (var objectType in objectTypes)
            {
                //Debug.Log($"Populating dictionaries for {objectType.typeName}");
                cachedObjectReferenceTypes[objectType.typeName] = objectType;
                //Debug.Log($"Now Count is: {cachedObjectReferenceTypes.Count}");
                // Populating cachedObjects and cachedGuids inside each objectType
                foreach (var objectReferenceObject in objectType.objects)
                {
                    //Debug.Log($"doing objectRefObj type {objectReferenceObject.typeName} -- obj is null? : {objectReferenceObject.GetObject() == null}");
                    
                    //Debug.Log($"objectReferenceObject.guid: {objectReferenceObject.guid}");
                    //Debug.Log($"objectType.cachedObjects is null? {objectType.cachedObjects == null}");
                    //Debug.Log($"objectType.cachedGuids is null? {objectType.cachedGuids == null}");
                    objectType.cachedObjects[objectReferenceObject.guid] = objectReferenceObject.GetObject();
                    //Debug.Log("Next");
                    objectType.cachedGuids[objectReferenceObject.GetObject()] = objectReferenceObject.guid;
                    //Debug.Log("End of loop");
                }
            }

            //Debug.Log("----- DONE POPULATEING DICTIONARIES");
            //Debug.Log($"cachedObjectReferenceTypes.Count: {cachedObjectReferenceTypes.Count}");
        }

        public T Get<T>(string guid)
        {
            var typeName = typeof(T).ToString();
            //Debug.Log($"-------- ObjectReference.cs: Object Reference type of {typeName}");
            // If it's already in the cached dictionary, get the value
            if (cachedObjectReferenceTypes.TryGetValue(typeName, out var value))
            {
               // Debug.Log("Was able t oget an objcet");
                return value.Get<T>(guid);
            }

            // Debug.Log("Did not find an object");

            // If we don't have the List of this type, then return default
            var foundType = objectTypes.FirstOrDefault(x => x.typeName == typeName);
           // Debug.Log($"foundType is null? {foundType == null}");
            if (foundType == null)
                return default;

            // Cache the list and get the value
            cachedObjectReferenceTypes[typeName] = foundType;
            //Debug.Log("$should return found type");
            return foundType.Get<T>(guid);
        }

        public string GetGuid<T>(T obj)
        {
            var typeName = typeof(T).ToString();

            //Debug.Log($"typeName is {typeName}");
            if (cachedObjectReferenceTypes.TryGetValue(typeName, out var cachedObjectsOfType))
                return cachedObjectsOfType.GetGuid(obj);

            // Add it to the Dictionary
            var typesList = objectTypes.FirstOrDefault(x => x.typeName == typeName);
            if (typesList == null)
            {
                Debug.Log($"Typename {typeName} was not found in the objectTypes list!");
                return default;
            }

            var foundObject = typesList.objects.FirstOrDefault(x => x.GetObject() == (object)obj);
            if (foundObject == null)
            {
                Debug.Log($"Typename {typeName} was found, but no object matched!");
                return default;
            }

            return foundObject.guid;
        }

        public void Add<T>(object obj)
        {
            var typeName = typeof(T).ToString();

            if (!GameModuleUtilities.validObjectReferenceTypes.Contains(typeName))
            {
                Debug.LogError($"ObjectReference.cs: Type {typeName} is not a valid ObjectReference type!");
                return;
            }
            
            if (obj == null) return;
            var foundList = objectTypes.FirstOrDefault(x => x.typeName == typeName);
            if (foundList == null)
            {
                objectTypes.Add(new ObjectReferenceType(typeName));
                objectTypes[^1].Add(obj);
                return;
            }

            foundList.Add(obj);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Will search all GameModule objects and populate any missing references. Edit time only
        /// </summary>
        public void PopulateMissingReferences()
        {
#if UNITY_EDITOR
            var objs = GameModuleUtilities.FindAssetsByInterface<IHaveDictionaries>();
            foreach (var iHaveDictionariesObject in GameModuleUtilities.FindAssetsByInterface<IHaveDictionaries>())
            {
                var obj = (IHaveDictionaries)iHaveDictionariesObject;
                obj.CheckForMissingObjectReferences();
            }
#endif


            return;
        }
    }
}

