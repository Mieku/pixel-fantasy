using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class KeyValueList
    {
        public string typeName;
        public List<KeyValueObject> objects = new List<KeyValueObject>();
        
        // Inspector variables
        [HideInInspector] public bool showValues = true;
        [HideInInspector] public string newValueString = "";

        public KeyValueList(string newTypeName)
        {
            typeName = CleanTypeName(newTypeName);
        }

        public T RandomObject<T>() => objects.TakeRandom().Object<T>();
        
        public void SetValue<T>(int index, T value)
        {
            var modulesValue = value as ModulesScriptableObject;
            if (modulesValue != null)
            {
                objects[index].SetObject<T>(value, modulesValue.Uid(), typeof(T).ToString());
                return;
            }
            
#if UNITY_EDITOR
            EnsureObjectIsInObjectReference(value, index);
#endif

            objects[index].SetObject<T>(value, "", typeof(T).ToString());
        }

        public void AddValue<T>(T value)
        {
            if (value == null) return;
            
            var modulesValue = value as ModulesScriptableObject;
            if (modulesValue != null)
            {
                Debug.Log($"adding a modules object");
                objects.Add(new KeyValueObject(value, modulesValue.Uid(), typeof(T).ToString()));
                return;
            }
            
#if UNITY_EDITOR
            // Object reference holds non-serializable reference via the objects GUID. we need to make sure they exist
            // in the object reference or things break.
            EnsureObjectIsInObjectReference(value);
            return;
#endif
            
            // This will run at runtime, the UNITY_EDITOR define will not be present.
            objects.Add(new KeyValueObject(value, "", typeof(T).ToString()));
        }

        private void EnsureObjectIsInObjectReference<T>(T value, int index = -1)
        {
#if UNITY_EDITOR
            // Check if it's an object with a GUID, in which case, ensure it's in the ObjectReference
            var valueAsObject = value as Object;
            var path = AssetDatabase.GetAssetPath(valueAsObject);
            if (string.IsNullOrWhiteSpace(path)) return;

            if (path.Contains("unity_builtin_extra") || path.Contains("unity default resources"))
            {
                Debug.LogWarning("You probably selected a built-in Unity resource, which is not allowed. " +
                                 "Please select a resource from your project.");
                return;
            }
            
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (index < 0)
            {
                DebugConsoleMessage($"Will add a new object of type {typeof(T)} with GUID {guid}: {value}");
                objects.Add(new KeyValueObject(value, guid, typeof(T).ToString()));
            }
            else
            {
                DebugConsoleMessage($"Index {index} is being updated to a new object of type {typeof(T)} with GUID {guid}: {value}");
                objects[index].SetObject<T>(value, guid, typeof(T).ToString());
            }
            
            AddToObjectReference<T>(value);
#endif
            
            return;
        }

        public void RemoveValue<T>(T value) where T : class
        {
            foreach (var i in objects)
            {
                if (typeof(T) != i.GetType())
                    continue;
                if (i as T == value)
                {
                    objects.Remove(i);
                    return;
                }
            }
        }

        public void AddToObjectReference<T>(object value)
        {
#if UNITY_EDITOR
            //var objectReference = GetObjectReference();
            ObjectReference.Instance.Add<T>(value);
#endif
        }

        public void RemoveIndex(int i)
        {
            objects.RemoveAt(i);
        }

        public void CheckForMissingObjectReferences()
        {
            foreach (var keyValueObject in objects)
            {
                // Match the type with each of the explicit types and call AddToObjectReference<T> for each type.
                if (typeName == "Texture2D") AddToObjectReference<Texture2D>(keyValueObject.Object<Texture2D>());
                else if (typeName == "Sprite") AddToObjectReference<Sprite>(keyValueObject.Object<Sprite>());
                else if (typeName == "AudioClip") AddToObjectReference<AudioClip>(keyValueObject.Object<AudioClip>());
                else if (typeName == "AnimationClip") AddToObjectReference<AnimationClip>(keyValueObject.Object<AnimationClip>());
                else if (typeName == "GameObject") AddToObjectReference<GameObject>(keyValueObject.Object<GameObject>());
                else if (typeName == "Stat") AddToObjectReference<Stat>(keyValueObject.Object<Stat>());
                else if (typeName == "Condition") AddToObjectReference<Condition>(keyValueObject.Object<Condition>());
                else if (typeName == "Quest") AddToObjectReference<Quest>(keyValueObject.Object<Quest>());
                else if (typeName == "QuestCondition") AddToObjectReference<QuestCondition>(keyValueObject.Object<QuestCondition>());
                else if (typeName == "QuestReward") AddToObjectReference<QuestReward>(keyValueObject.Object<QuestReward>());
                else if (typeName == "ItemAttribute") AddToObjectReference<ItemAttribute>(keyValueObject.Object<ItemAttribute>());
                else if (typeName == "ItemObject") AddToObjectReference<ItemObject>(keyValueObject.Object<ItemObject>());
                else if (typeName == "LootBox") AddToObjectReference<LootBox>(keyValueObject.Object<LootBox>());
                else if (typeName == "LootItems") AddToObjectReference<LootItems>(keyValueObject.Object<LootItems>());
            }
        }
    }
}