using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class KeyValueObject
    {
        public string uid;
        public object obj; // Cached object
        
        [SerializeReference]
        private DictionariesObject _dictionariesObject;
        public DictionariesObject DictionariesObject
        {
            get => _dictionariesObject;
            set => _dictionariesObject = value;
        }


        public KeyValueObject(object value, string uidValue = "", string objectTypeValue = "")
        {
            uid = uidValue;

            if (objectTypeValue == typeof(float).ToString())
                DictionariesObject = new DictionariesFloat(value as float? ?? 0);
            
            if (objectTypeValue == typeof(int).ToString())
                DictionariesObject = new DictionariesInt(value as int? ?? 0);
            
            if (objectTypeValue == typeof(bool).ToString())
                DictionariesObject = new DictionariesBool(value is true);
            
            if (objectTypeValue == typeof(string).ToString())
                DictionariesObject = new DictionariesString(value as string);
            
            if (objectTypeValue == typeof(Stat).ToString())
                DictionariesObject = new DictionariesStat(value as Stat);
            
            if (objectTypeValue == typeof(ItemObject).ToString())
                DictionariesObject = new DictionariesItemObject(value as ItemObject);
            
            if (objectTypeValue == typeof(ItemAttribute).ToString())
                DictionariesObject = new DictionariesItemAttribute(value as ItemAttribute);
            
            if (objectTypeValue == typeof(Condition).ToString())
                DictionariesObject = new DictionariesCondition(value as Condition);
            
            if (objectTypeValue == typeof(Quest).ToString())
                DictionariesObject = new DictionariesQuest(value as Quest);
            
            if (objectTypeValue == typeof(QuestCondition).ToString())
                DictionariesObject = new DictionariesQuestCondition(value as QuestCondition);
            
            if (objectTypeValue == typeof(QuestReward).ToString())
                DictionariesObject = new DictionariesQuestReward(value as QuestReward);
            
            if (objectTypeValue == typeof(LootBox).ToString())
                DictionariesObject = new DictionariesLootBox(value as LootBox);
            
            if (objectTypeValue == typeof(LookupTable).ToString())
                DictionariesObject = new DictionariesLookupTable(value as LookupTable);
            
            if (objectTypeValue == typeof(LootItems).ToString())
                DictionariesObject = new DictionariesLootItems(value as LootItems);
            
            if (objectTypeValue == typeof(AnimationClip).ToString())
                DictionariesObject = new DictionariesAnimationClip(value as AnimationClip);
            
            if (objectTypeValue == typeof(Texture2D).ToString())
                DictionariesObject = new DictionariesTexture2D(value as Texture2D);
            
            if (objectTypeValue == typeof(AudioClip).ToString())
                DictionariesObject = new DictionariesAudioClip(value as AudioClip);
            
            if (objectTypeValue == typeof(Sprite).ToString())
                DictionariesObject = new DictionariesSprite(value as Sprite);

            if (objectTypeValue == typeof(Color).ToString())
                DictionariesObject = new DictionariesColor(value is Color ? (Color)value : default);
            
            if (objectTypeValue == typeof(Vector4).ToString())
                DictionariesObject = new DictionariesVector4(value is Vector4 ? (Vector4)value : default);
            
            if (objectTypeValue == typeof(Vector3).ToString())
                DictionariesObject = new DictionariesVector3(value is Vector3 ? (Vector3)value : default);
            
            if (objectTypeValue == typeof(Vector2).ToString())
                DictionariesObject = new DictionariesVector2(value is Vector2 ? (Vector2)value : default);
            
            if (objectTypeValue == typeof(GameObject).ToString())
                DictionariesObject = new DictionariesGameObject(value as GameObject);
        }

        public T Object<T>()
        {
            //Debug.Log($"KeyValueObject.cs Object type of {typeof(T)}");
            //Debug.Log($"KeyValueObject.cs obj is null: {obj == null}, DictionariesObject is null: {DictionariesObject == null}");
            if (obj != null)
                return (T)obj;

            // Default if the object is null. This should not happen.
            if (DictionariesObject == null) return default;

            var typeName = DictionariesObject.TypeName();

            //Debug.Log($"KeyValueObject.cs typeName: {typeName}");
            if (typeName == "Int")
                return (T)(object)Convert.ToInt32(obj ??= DictionariesObject.Value<T>());

            if (typeName == "Float")
                return (T)(object)Convert.ToSingle(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "String")
                return (T)(object)Convert.ToString(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Bool")
                return (T)(object)Convert.ToBoolean(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Texture2D")
                return (T)(object)(Texture2D)(obj ??= DictionariesObject.Value<T>());

            if (typeName == "Sprite")
                return (T)(object)(Sprite)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "AudioClip")
                return (T)(object)(AudioClip)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "AnimationClip")
                return (T)(object)(AnimationClip)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "GameObject")
                return (T)(object)(GameObject)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Color")
                return (T)(object)(Color)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Vector4")
                return (T)(object)(Vector4)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Vector3")
                return (T)(object)(Vector3)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Vector2")
                return (T)(object)(Vector2)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Stat")
                return (T)(object)(Stat)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Condition")
                return (T)(object)(Condition)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "Quest")
                return (T)(object)(Quest)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "QuestCondition")
                return (T)(object)(QuestCondition)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "QuestReward")
                return (T)(object)(QuestReward)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "ItemAttribute")
                return (T)(object)(ItemAttribute)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "ItemObject")
                return (T)(object)(ItemObject)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "LookupTable")
                return (T)(object)(LookupTable)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "LootBox")
                return (T)(object)(LootBox)(obj ??= DictionariesObject.Value<T>());
            
            if (typeName == "LootItems")
                return (T)(object)(LootItems)(obj ??= DictionariesObject.Value<T>());

            Debug.Log("Object was not handled by the conversion code. Did we forget a type?");
            return default;
        }

        public void SetObject<T>(object value, string uidValue = "", string objectTypeValue = "")
        {
            Debug.Log($"DictionariesObject is null: {DictionariesObject == null}");
            Debug.Log($"Set Object type of {typeof(T)}");
            Debug.Log($"Value is null? {value == null}");
            Debug.Log($"DictionariesObject {DictionariesObject.TypeName()}");
            DictionariesObject.Set<T>(value);
            uid = uidValue;
            obj = value;
            
#if UNITY_EDITOR
            //var objectReference = GameModuleUtilities.GetObjectReference();
            ObjectReference.Instance.Add<T>(value);
#else
            // Does not happen in runtime
#endif
        }
    }
    

    [Serializable]
    public abstract class DictionariesObject
    {
        public abstract T Value<T>();
        public abstract void Set<T>(object newValue);
        public abstract string TypeName();
    }

    [Serializable]
    public class DictionariesFloat : DictionariesObject
    {
        public float value;
        public override string TypeName() => "Float";
        public override T Value<T>() => (T)(object)value;

        public override void Set<T>(object newValue) => value = newValue is float saveValue ? saveValue : default;

        public DictionariesFloat(float newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesInt : DictionariesObject
    {
        public int value;
        public override string TypeName() => "Int";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is int saveValue ? saveValue : default;

        public DictionariesInt(int newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesBool : DictionariesObject
    {
        public bool value;
        public override string TypeName() => "Bool";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is bool saveValue ? saveValue : default;

        public DictionariesBool(bool newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesString : DictionariesObject
    {
        public string value;
        public override string TypeName() => "String";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is string saveValue ? saveValue : default;

        public DictionariesString(string newValue)
        {
            value = newValue;
        }
    }

    [Serializable]
    public class DictionariesStat : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "Stat";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<Stat>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<Stat>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is Stat saveValue ? saveValue.Uid() : default;

        public DictionariesStat(Stat newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesItemObject : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "ItemObject";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<ItemObject>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<ItemObject>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is ItemObject saveValue ? saveValue.Uid() : default;

        public DictionariesItemObject(ItemObject newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesItemAttribute : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "ItemAttribute";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<ItemAttribute>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<ItemAttribute>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is ItemAttribute saveValue ? saveValue.Uid() : default;

        public DictionariesItemAttribute(ItemAttribute newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesQuest : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "Quest";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<Quest>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<Quest>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is Quest saveValue ? saveValue.Uid() : default;

        public DictionariesQuest(Quest newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesQuestCondition : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "QuestCondition";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<QuestCondition>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<QuestCondition>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is QuestCondition saveValue ? saveValue.Uid() : default;

        public DictionariesQuestCondition(QuestCondition newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesQuestReward : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "QuestReward";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            
            return (T)(object)GameModuleUtilities.GameModuleObjects<QuestReward>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<QuestReward>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is QuestReward saveValue ? saveValue.Uid() : default;

        public DictionariesQuestReward(QuestReward newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesCondition : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "Condition";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<Condition>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<Condition>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is Condition saveValue ? saveValue.Uid() : default;

        public DictionariesCondition(Condition newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesLookupTable : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "LookupTable";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<LookupTable>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<LookupTable>(uid);
#endif
        }
        public override void Set<T>(object newValue)
        {
            Debug.Log($"Lookup tablre set value is null? {newValue == null}");
            uid = newValue is LookupTable saveValue ? saveValue.Uid() : default;
        }

        public DictionariesLookupTable(LookupTable newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesLootBox : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "LootBox";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<LootBox>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<LootBox>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is LootBox saveValue ? saveValue.Uid() : default;

        public DictionariesLootBox(LootBox newValue)
        {
            uid = newValue.Uid();
        }
    }
    
    [Serializable]
    public class DictionariesLootItems : DictionariesObject
    {
        public string uid;
        public override string TypeName() => "LootItems";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            return (T)(object)GameModuleUtilities.GameModuleObjects<LootItems>().FirstOrDefault(x => x.Uid() == uid);
#else
            return (T)(object)GameModuleRepository.Instance.Get<LootItems>(uid);
#endif
        }
        public override void Set<T>(object newValue) => uid = newValue is LootItems saveValue ? saveValue.Uid() : default;

        public DictionariesLootItems(LootItems newValue)
        {
            uid = newValue.Uid();
        }
    }

    [Serializable]
    public class DictionariesAnimationClip : DictionariesObject
    {
        public string guid;
        public override string TypeName() => "AnimationClip";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return (T)Convert.ChangeType(obj, typeof(T));
#else
            return (T)ObjectReferenceRepository.objectReferenceRepository.objectReference
                .cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects[guid];
#endif
        }

        public sealed override void Set<T>(object newValue)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath((Object)newValue);
            if (path == null)
                return;
            
            guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            
#else
            guid = ObjectReferenceRepository.objectReferenceRepository.objectReference.GetGuid(newValue);
#endif
        }

        public DictionariesAnimationClip(AnimationClip newValue)
        {
            Set<AnimationClip>(newValue);
        }
    }
    
    [Serializable]
    public class DictionariesSprite : DictionariesObject
    {
        public string guid;
        public override string TypeName() => "Sprite";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            return (T)Convert.ChangeType(obj, typeof(T));
#else
            return (T)ObjectReferenceRepository.objectReferenceRepository.objectReference
                .cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects[guid];
#endif
        }

        public sealed override void Set<T>(object newValue)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath((Sprite)newValue);
            if (path == null)
                return;
            
            guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            //Debug.Log($"Adding {typeof(T).ToString()} guid {guid}");
#else
            guid = ObjectReferenceRepository.objectReferenceRepository.objectReference.GetGuid(newValue);
#endif
        }

        public DictionariesSprite(Sprite newValue)
        {
            Set<Sprite>(newValue);
        }
    }
    
    [Serializable]
    public class DictionariesTexture2D : DictionariesObject
    {
        public string guid;
        public override string TypeName() => "Texture2D";
        public override T Value<T>()
        {
            
#if UNITY_EDITOR
                        
            //Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: there are {ObjectReferenceRepository.objectReferenceRepository.objectReference.cachedObjectReferenceTypes[TypeName()].cachedObjects.Count} objects in the cache");
            //Debug.Log($"key is {TypeName()}");
            //Debug.Log($"Cachedobjectreferencetypes count is {ObjectReferenceRepository.objectReferenceRepository.objectReference.cachedObjectReferenceTypes.Count}");
            //Debug.Log($"there are {ObjectReferenceRepository.objectReferenceRepository.objectReference.cachedObjectReferenceTypes[TypeName()].cachedObjects.Count} objects in the cache");
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return (T)Convert.ChangeType(obj, typeof(T));
#else
//Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: Getting Texture2D at runtime {guid}");
//Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: key is {typeof(T).ToString()}");
//            Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: Cachedobjectreferencetypes count is {ObjectReferenceRepository.objectReferenceRepository.objectReference.cachedObjectReferenceTypes.Count}");
//Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: objectTypes count is {ObjectReferenceRepository.objectReferenceRepository.objectReference.objectTypes.Count}");            
//Debug.Log($"KeyValueObject.cs | DictionariesTexture2D: there are {ObjectReferenceRepository.objectReferenceRepository.objectReference.cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects.Count} objects in the cache");
            return (T)ObjectReferenceRepository.objectReferenceRepository.objectReference
                .cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects[guid];
#endif
        }

        public sealed override void Set<T>(object newValue)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath((Object)newValue);
            if (path == null)
                return;
            
            guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            
#else
            guid = ObjectReferenceRepository.objectReferenceRepository.objectReference.GetGuid(newValue);
#endif
        }

        public DictionariesTexture2D(Texture2D newValue)
        {
            Set<Texture2D>(newValue);
        }
    }
    
    [Serializable]
    public class DictionariesAudioClip : DictionariesObject
    {
        public string guid;
        public override string TypeName() => "AudioClip";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return (T)Convert.ChangeType(obj, typeof(T));
#else
            return (T)ObjectReferenceRepository.objectReferenceRepository.objectReference
                .cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects[guid];
#endif
        }

        public sealed override void Set<T>(object newValue)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath((Object)newValue);
            if (path == null)
                return;
            
            guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            
#else
            guid = ObjectReferenceRepository.objectReferenceRepository.objectReference.GetGuid(newValue);
#endif
        }

        public DictionariesAudioClip(AudioClip newValue)
        {
            Set<AudioClip>(newValue);
        }
    }
    
    [Serializable]
    public class DictionariesGameObject : DictionariesObject
    {
        public string guid;
        public override string TypeName() => "GameObject";
        public override T Value<T>()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            return (T)Convert.ChangeType(obj, typeof(T));
#else
            return (T)ObjectReferenceRepository.objectReferenceRepository.objectReference
                .cachedObjectReferenceTypes[typeof(T).ToString()].cachedObjects[guid];
#endif
        }

        public sealed override void Set<T>(object newValue)
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath((Object)newValue);
            if (path == null)
                return;
            
            guid = AssetDatabase.GUIDFromAssetPath(path).ToString();
            
#else
            guid = ObjectReferenceRepository.objectReferenceRepository.objectReference.GetGuid(newValue);
#endif
        }

        public DictionariesGameObject(GameObject newValue)
        {
            Set<GameObject>(newValue);
        }
    }
    
    [Serializable]
    public class DictionariesColor : DictionariesObject
    {
        public Color value;
        public override string TypeName() => "Color";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is Color saveValue ? saveValue : default;

        public DictionariesColor(Color newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesVector3 : DictionariesObject
    {
        public Vector3 value;
        public override string TypeName() => "Vector3";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is Vector3 saveValue ? saveValue : default;

        public DictionariesVector3(Vector3 newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesVector2 : DictionariesObject
    {
        public Vector2 value;
        public override string TypeName() => "Vector2";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is Vector2 saveValue ? saveValue : default;

        public DictionariesVector2(Vector2 newValue)
        {
            value = newValue;
        }
    }
    
    [Serializable]
    public class DictionariesVector4 : DictionariesObject
    {
        public Vector4 value;
        public override string TypeName() => "Vector4";
        public override T Value<T>() => (T)(object)value;
        public override void Set<T>(object newValue) => value = newValue is Vector4 saveValue ? saveValue : default;

        public DictionariesVector4(Vector4 newValue)
        {
            value = newValue;
        }
    }
}