using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using Random = UnityEngine.Random;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class KeyValue : IHaveUid
    {
        // Key for the lists of value objects. One dictionary can hold multiple keyValues by key. Each keyValue object
        // also has a Uid() value;
        public string key; 
        public string uid => Uid();
        private string _uid;
        public string SetUid(string value) => _uid = value;
        public ModulesScriptableObject parent;

        // Values are stored here. Each KeyValueList has a string typeName (string, int, Stat, ItemObject, etc), and 
        // a list of KeyValueObject objects. Each of those has an object obj and a string uid. The uid is used for
        // Game Module object types (ItemAttribute, Condition, Quest etc), and will automatically return the correct
        // value based on the type of object it is, as the Objects themselves do not serialize.
        public List<KeyValueList> values = new List<KeyValueList>();
        public Dictionary<string, KeyValueList> cachedLists = new Dictionary<string, KeyValueList>();
        
        // Inspector
        [HideInInspector] public Dictionaries.Types selectedType = Dictionaries.Types.Float;
        [HideInInspector] public string SelectedType => GetSelectedType();
        [HideInInspector] public int SelectedIndex => GetSelectedIndex();
        [HideInInspector] [SerializeField] public string selectedTypeString;
        [HideInInspector] public Dictionaries.Types[] availableTypes;

        public int TypesInUse => values.Count;
        public bool UsingAllTypes => values.Count == Enum.GetValues(typeof(Dictionaries.Types)).Length;
        public string[] AvailableTypes => GetAvailableTypes();
        public string[] Types => Enum.GetNames(typeof(Dictionaries.Types));

        private string[] GetAvailableTypes() => Types
            .Where(type => values.FirstOrDefault(x => x.typeName == type) == null)
            .ToArray();

        private int GetSelectedIndex()
        {
            for (var i = 0; i < AvailableTypes.Length; i++)
            {
                if (SelectedType == AvailableTypes[i])
                    return i;
            }

            return -1;
        }

        private string GetSelectedType()
        {
            if (UsingAllTypes) return selectedTypeString = default;

            return string.IsNullOrWhiteSpace(selectedTypeString) || !AvailableTypes.Contains(selectedTypeString)
                ? selectedTypeString = AvailableTypes[0]
                : selectedTypeString;
        }

        public KeyValueList KeyValueList(string typeFull, bool useCacheIfPossible = true)
        {
            var typeName = CleanTypeName(typeFull);
            // Return the list if it's already populated in the Dictionary
            if (cachedLists.TryGetValue(typeName, out var valueList) && useCacheIfPossible)
                return valueList;

            // Add to the Dictionary and return if it's found. If we haven't created this list yet, then
            // create it and return it
            var foundList = values.FirstOrDefault(x => x.typeName == typeName);
            if (foundList != null)
                return cachedLists[typeName] = foundList;

            var newList = new KeyValueList(typeFull);
            values.Add(newList);
            
            DebugConsoleMessage($"New {typeFull} type added. Total types: {values.Count}");
            return cachedLists[typeName] = newList;
        }

        // Variables used in the Inspector scripts
        [HideInInspector] public bool stringAsTextBox;
        [HideInInspector] public bool showAllValueTypes;
        [HideInInspector] public bool migratedToVersion4;
        [HideInInspector] public bool showValues = true;
        
        public int TotalObjects => values.Sum(x => x.objects.Count);

        /*
         * GENERIC METHODS ETC
         */
        private string TypeName<T>() => typeof(T).ToString();

        public T Value<T>(bool random = false, int index = 0)
        {
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null) return default; // If we did not find the list, return default
            if (keyValueList.objects.Count == 0) return default; // If the list is empty, return default

            return random ? keyValueList.RandomObject<T>() : keyValueList.objects[index].Object<T>();
        }
        
        public T[] Values<T>()
        {
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null) return default; // If we did not find the list, return default
            if (keyValueList.objects.Count == 0) return default; // If the list is empty, return default

            return keyValueList.objects.Select(x => x.Object<T>()).ToArray();
        }

        public List<T> ValuesList<T>() => Values<T>()?.ToList();
        
        public KeyValueObject[] ValueObjects<T>()
        {
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null)
                return default; // If we did not find the list, return default
            
            return keyValueList.objects.ToArray();
        }
        
        public List<KeyValueObject> ValueObjectsList<T>() => ValueObjects<T>()?.ToList();
        
        public void SetValue<T>(int index, T value)
        {
            Debug.Log("Set Value");
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null)// If we did not find the list, return
            {
                Debug.LogWarning($"Index of value {typeName} was not found.");
                return; 
            }
            if (keyValueList.objects.Count <= index) // If the index is out of range, return
            {
                Debug.LogWarning($"Index {index} is out of range. List has {keyValueList.objects.Count} items.");
                return;
            }

            keyValueList.SetValue(index, value);
        }
        
        /// <summary>
        /// Adds a value of type T to the KeyValue lists. Will create the List<T> if it does not exist already.
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void AddValue<T>(T value)
        {
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null) return; // If we did not find the list, return

            keyValueList.AddValue(value);
        }
        
        public void RemoveValue<T>(T value) where T : class
        {
            var typeName = TypeName<T>(); // Cache this value
            var keyValueList = KeyValueList(typeName); // Grab the list from the dictionary, or populate the dictionary
            if (keyValueList == null) return; // If we did not find the list, return
            
            keyValueList.RemoveValue(value);
        }

        // This will migrate data from v3 to v4. Eventually this will be removed.
        public void MigrateToVersion4(bool force = false)
        {
            if (migratedToVersion4 && !force) return;

            // For each list, add all values using the new method, which will populate the KeyValueList
            // objects as required
            // Serializable types
            foreach (var value in valuesString)
                AddValue<string>(value);
            
            foreach (var value in valuesInt)
                AddValue<int>(value);
            
            foreach (var value in valuesFloat)
                AddValue<float>(value);
            
            foreach (var value in valuesBool) AddValue<bool>(value);
            foreach (var value in valuesVector2)
            {
                if (value == Vector2.zero) continue;
                AddValue<Vector2>(value);
            }
            foreach (var value in valuesVector3)
            {
                if (value == Vector3.zero) continue;
                AddValue<Vector3>(value);
            }

            // Non-Serializable Types
            foreach (var value in valuesAnimationClip)
                AddValue<AnimationClip>(value);
            
            foreach (var value in valuesColor)
            {
                if (value == default) continue;
                AddValue<Color>(value);
                Debug.Log($"Color is {value}");
            }
            foreach (var value in valuesPrefab)
            {
                if (value == null) continue;
                AddValue<GameObject>(value);
            }
            foreach (var value in valuesSprite)
            {
                if (value == null) continue;
                AddValue<Sprite>(value);
            }
            foreach (var value in valuesAudioClip)
            {
                if (value == null) continue;
                AddValue<AudioClip>(value);
            }
            foreach (var value in valuesTexture2D)
            {
                if (value == null) continue;
                AddValue<Texture2D>(value);
            }
            
            // Game Module Types
            foreach (var value in valuesCondition)
            {
                if (value == null) continue;
                AddValue<Condition>(value);
            }
            foreach (var value in valuesStat)
            {
                if (value == null) continue;
                AddValue<Stat>(value);
            }
            foreach (var value in valuesItemAttribute)
            {
                if (value == null) continue;
                AddValue<ItemAttribute>(value);
            }
            foreach (var value in valuesItemObject)
            {
                if (value == null) continue;
                AddValue<ItemObject>(value);
            }

            Debug.Log($"Migrated KeyValues {key} to Version 4!");
            migratedToVersion4 = true;
        }
        
        // V3 GENERIC METHODS BELOW! Will be removed.
        public T Value<T>(List<T> list, bool random = false, int index = 0)
        {
            if (list.Count == 0) list.Add(default);
            return list[random ? Random.Range(0, list.Count) : index];
        }

        public void SetValue<T>(List<T> list, int index, T value)
        {
            if (list.Count <= index) return;
            list[index] = value;
        }
        
        public void AddValue<T>(List<T> list, T value) => list.Add(value);
        public void RemoveValue<T>(List<T> list, T value) => list.Remove(value);
        // END V3 GENERIC METHODS
        
        /*
         * END GENERIC METHODS ETC
         */
        
        // STRING
        [HideInInspector] public bool showString;
        public List<string> valuesString = new List<string>();

        public string ValueString(bool random = false, int index = 0) => Value(valuesString, random, index);
        public void AddValue(string value) => valuesString.Add(value);
        public void RemoveValue(string value) => valuesString.Remove(value);
        public void SetValue(int index, string value) => SetValue(valuesString, index, value);
        
        // INT
        [HideInInspector] public bool showInt;
        public List<int> valuesInt = new List<int>();
        
        public int ValueInt(bool random = false, int index = 0) => Value(valuesInt, random, index);
        public void AddValue(int value) => AddValue(valuesInt, value);
        public void RemoveValue(int value) => RemoveValue(valuesInt, value);
        public void SetValue(int index, int value) => SetValue(valuesInt, index, value);

        // FLOAT
        [HideInInspector] public bool showFloat;
        public List<float> valuesFloat = new List<float>();
        
        public float ValueFloat(bool random = false, int index = 0) => Value(valuesFloat, random, index);
        public void AddValue(float value) => AddValue(valuesFloat, value);
        public void RemoveValue(float value) => RemoveValue(valuesFloat, value);
        public void SetValue(int index, float value) => SetValue(valuesFloat, index, value);
        
        [HideInInspector] public bool showBool;
        public List<bool> valuesBool = new List<bool>();
        
        public bool ValueBool(bool random = false, int index = 0) => Value(valuesBool, random, index);
        public void AddValue(bool value) => AddValue(valuesBool, value);
        public void RemoveValue(bool value) => RemoveValue(valuesBool, value);
        public void SetValue(int index, bool value) => SetValue(valuesBool, index, value);
        
        [HideInInspector] public bool showAnimationClip;
        public List<AnimationClip> valuesAnimationClip = new List<AnimationClip>();
        
        public AnimationClip ValueAnimationClip(bool random = false, int index = 0) => Value(valuesAnimationClip, random, index);
        public void AddValue(AnimationClip value) => AddValue(valuesAnimationClip, value);
        public void RemoveValue(AnimationClip value) => RemoveValue(valuesAnimationClip, value);
        public void SetValue(int index, AnimationClip value) => SetValue(valuesAnimationClip, index, value);
        
        [HideInInspector] public bool showTexture2D;
        public List<Texture2D> valuesTexture2D = new List<Texture2D>();
        
        public Texture2D ValueTexture2D(bool random = false, int index = 0) => Value(valuesTexture2D, random, index);
        public void AddValue(Texture2D value) => AddValue(valuesTexture2D, value);
        public void RemoveValue(Texture2D value) => RemoveValue(valuesTexture2D, value);
        public void SetValue(int index, Texture2D value) => SetValue(valuesTexture2D, index, value);
        
        [HideInInspector] public bool showSprite;
        public List<Sprite> valuesSprite = new List<Sprite>();
        
        public Sprite ValueSprite(bool random = false, int index = 0) => Value(valuesSprite, random, index);
        public void AddValue(Sprite value) => AddValue(valuesSprite, value);
        public void RemoveValue(Sprite value) => RemoveValue(valuesSprite, value);
        public void SetValue(int index, Sprite value) => SetValue(valuesSprite, index, value);
        
        // AUDIO CLIP
        [HideInInspector] public bool showAudioClip;
        public List<AudioClip> valuesAudioClip = new List<AudioClip>();
        
        public AudioClip ValueAudioClip(bool random = false, int index = 0) => Value(valuesAudioClip, random, index);
        public void AddValue(AudioClip value) => AddValue(valuesAudioClip, value);
        public void RemoveValue(AudioClip value) => RemoveValue(valuesAudioClip, value);
        public void SetValue(int index, AudioClip value) => SetValue(valuesAudioClip, index, value);
        
        // PREFAB (GAME OBJECT)
        [HideInInspector] public bool showPrefab;
        public List<GameObject> valuesPrefab = new List<GameObject>();
        
        public GameObject ValuePrefab(bool random = false, int index = 0) => Value(valuesPrefab, random, index);
        public void AddValue(GameObject value) => AddValue(valuesPrefab, value);
        public void RemoveValue(GameObject value) => RemoveValue(valuesPrefab, value);
        public void SetValue(int index, GameObject value) => SetValue(valuesPrefab, index, value);
        
        // COLOR
        [HideInInspector] public bool showColor;
        public List<Color> valuesColor = new List<Color>();
        
        public Color ValueColor(bool random = false, int index = 0) => Value(valuesColor, random, index);
        public void AddValue(Color value) => AddValue(valuesColor, value);
        public void RemoveValue(Color value) => RemoveValue(valuesColor, value);
        public void SetValue(int index, Color value) => SetValue(valuesColor, index, value);
        
        // VECTOR3
        [HideInInspector] public bool showVector3;
        public List<Vector3> valuesVector3 = new List<Vector3>();
        
        public Vector3 ValueVector3(bool random = false, int index = 0) => Value(valuesVector3, random, index);
        public void AddValue(Vector3 value) => AddValue(valuesVector3, value);
        public void RemoveValue(Vector3 value) => RemoveValue(valuesVector3, value);
        public void SetValue(int index, Vector3 value) => SetValue(valuesVector3, index, value);
        
        // VECTOR2
        [HideInInspector] public bool showVector2;
        public List<Vector2> valuesVector2 = new List<Vector2>();
        
        public Vector2 ValueVector2(bool random = false, int index = 0) => Value(valuesVector2, random, index);
        public void AddValue(Vector2 value) => AddValue(valuesVector2, value);
        public void RemoveValue(Vector2 value) => RemoveValue(valuesVector2, value);
        public void SetValue(int index, Vector2 value) => SetValue(valuesVector2, index, value);
        
        /*
         * GAME MODULES TYPES
         */
        
        // STAT
        [HideInInspector] public bool showStat;
        public List<Stat> valuesStat = new List<Stat>();
        public List<string> valuesStatUid = new List<string>();
        
        public string ValueStatUid(bool random = false, int index = 0) => Value(valuesStatUid, random, index);
        public Stat ValueStat(bool random = false, int index = 0)
        {
            if (random) index = RandomIndex(0, valuesStat.Count); // Randomize index if random = true
            if (index >= valuesStat.Count) return default; // This means index was out of the range of the list

            if (valuesStat[index] != null) return valuesStat[index]; // The object existed, return it

            var foundObject = GameModuleRepository.Instance.Get<Stat>(valuesStatUid[index]); // Find and cache the value
            SetValue(index, foundObject); // Set the value

            return foundObject; // Return the object we found
        }

        public void AddValue(Stat value)
        {
            AddValue(valuesStatUid, value == null ? "" : value.Uid());
            AddValue(valuesStat, value);
        }
        public void RemoveValue(Stat value)
        {
            RemoveValue(valuesStatUid, value == null ? "" : value.Uid());
            RemoveValue(valuesStat, value);
        }
        public void SetValue(int index, Stat value)
        {
            Debug.Log($"setting stat value at index {index} to {value.ObjectName}");
            SetValue(valuesStatUid, index, value ? value.Uid() : default);
            SetValue(valuesStat, index, value);
        }

        // ITEM OBJECT
        [HideInInspector] public bool showItemObject;
        public List<ItemObject> valuesItemObject = new List<ItemObject>();
        public List<string> valuesItemObjectUid = new List<string>();
        
        public string ValueItemObjectUid(bool random = false, int index = 0) => Value(valuesItemObjectUid, random, index);
        public ItemObject ValueItemObject(bool random = false, int index = 0)
        {
            if (random) index = RandomIndex(0, valuesItemObject.Count); // Randomize index if random = true
            if (index >= valuesItemObject.Count) return default; // This means index was out of the range of the list

            if (valuesItemObject[index] != null) return valuesItemObject[index]; // The object existed, return it

            var foundObject = GameModuleRepository.Instance.Get<ItemObject>(valuesItemObjectUid[index]); // Find and cache the value
            SetValue(index, foundObject); // Set the value

            return foundObject; // Return the object we found
        }
        public void AddValue(ItemObject value)
        {
            AddValue(valuesItemObjectUid, value == null ? "" : value.Uid());
            AddValue(valuesItemObject, value);
        }
        public void RemoveValue(ItemObject value)
        {
            RemoveValue(valuesItemObjectUid, value == null ? "" : value.Uid());
            RemoveValue(valuesItemObject, value);
        }
        public void SetValue(int index, ItemObject value){
            SetValue(valuesItemObjectUid, index, value ? value.Uid() : default);
            SetValue(valuesItemObject, index, value);
        }
        
        // ITEM ATTRIBUTE
        [HideInInspector] public bool showItemAttribute;
        public List<ItemAttribute> valuesItemAttribute = new List<ItemAttribute>();
        public List<string> valuesItemAttributeUid = new List<string>();
        
        public string ValueItemAttributeUid(bool random = false, int index = 0) => Value(valuesItemAttributeUid, random, index);
        public ItemAttribute ValueItemAttribute(bool random = false, int index = 0)
        {
            if (random) index = RandomIndex(0, valuesItemAttribute.Count); // Randomize index if random = true
            if (index >= valuesItemAttribute.Count) return default; // This means index was out of the range of the list

            if (valuesItemAttribute[index] != null) return valuesItemAttribute[index]; // The object existed, return it

            var foundObject = GameModuleRepository.Instance.Get<ItemAttribute>(valuesItemAttributeUid[index]); // Find and cache the value
            SetValue(index, foundObject); // Set the value

            return foundObject; // Return the object we found
        }
        public void AddValue(ItemAttribute value)
        {
            AddValue(valuesItemAttributeUid, value == null ? "" : value.Uid());
            AddValue(valuesItemAttribute, value);
        }
        public void RemoveValue(ItemAttribute value)
        {
            RemoveValue(valuesItemAttributeUid, value == null ? "" : value.Uid());
            RemoveValue(valuesItemAttribute, value);
        }
        public void SetValue(int index, ItemAttribute value){
            SetValue(valuesItemAttributeUid, index, value ? value.Uid() : default);
            SetValue(valuesItemAttribute, index, value);
        }
        
        // CONDITION
        [HideInInspector] public bool showCondition;
        public List<Condition> valuesCondition = new List<Condition>();
        public List<string> valuesConditionUid = new List<string>();
        
        public string ValueConditionUid(bool random = false, int index = 0) => Value(valuesConditionUid, random, index);
        public Condition ValueCondition(bool random = false, int index = 0)
        {
            if (random) index = RandomIndex(0, valuesCondition.Count); // Randomize index if random = true
            if (index >= valuesCondition.Count) return default; // This means index was out of the range of the list

            if (valuesCondition[index] != null) return valuesCondition[index]; // The object existed, return it
            
            var foundObject = GameModuleRepository.Instance.Get<Condition>(valuesConditionUid[index]); // Find and cache the value
            SetValue(index, foundObject); // Set the value

            return foundObject; // Return the object we found
        }
        public void AddValue(Condition value)
        {
            AddValue(valuesConditionUid, value == null ? "" : value.Uid());
            AddValue(valuesCondition, value);
        }
        public void RemoveValue(Condition value)
        {
            RemoveValue(valuesConditionUid, value == null ? "" : value.Uid());
            RemoveValue(valuesCondition, value);
        }
        public void SetValue(int index, Condition value){
            SetValue(valuesConditionUid, index, value ? value.Uid() : default);
            SetValue(valuesCondition, index, value);
        }
        
        // ITEM OBJECT TYPE
        [HideInInspector] public bool showItemObjectType;
        public List<string> valuesItemObjectType = new List<string>();
        
        public string ValueItemObjectType(bool random = false, int index = 0) => Value(valuesItemObjectType, random, index);
        public void AddValueItemObjectType(string value) => AddValue(valuesItemObjectType, value);
        public void RemoveValueItemObjectType(string value) => RemoveValue(valuesItemObjectType, value);
        public void SetValueItemObjectType(int index, string value) => SetValue(valuesItemObjectType, index, value);

        // ITEM ATTRIBUTE TYPE
        [HideInInspector] public bool showItemAttributeType;
        public List<string> valuesItemAttributeType = new List<string>();
        
        public string ValueItemAttributeType(bool random = false, int index = 0) => Value(valuesItemAttributeType, random, index);
        public void AddValueItemAttributeType(string value) => AddValue(valuesItemAttributeType, value);
        public void RemoveValueItemAttributeType(string value) => RemoveValue(valuesItemAttributeType, value);
        public void SetValueItemAttributeType(int index, string value) => SetValue(valuesItemAttributeType, index, value);
        
        // CONDITION TYPE
        [HideInInspector] public bool showConditionType;
        public List<string> valuesConditionType = new List<string>();
        
        public string ValueConditionType(bool random = false, int index = 0) => Value(valuesConditionType, random, index);
        public void AddValueConditionType(string value) => AddValue(valuesConditionType, value);
        public void RemoveValueConditionType(string value) => RemoveValue(valuesConditionType, value);
        public void SetValueConditionType(int index, string value) => SetValue(valuesConditionType, index, value);

        public KeyValue Clone()
        {
            return JsonUtility.FromJson<KeyValue>(JsonUtility.ToJson(this));
        }

        public void Rename(string oldKey, string newKey)
        {
            if (key != oldKey) return;
            key = newKey;
        }
        
        

        /// <summary>
        /// This will copy all values from this KeyValue to the provided KeyValue
        /// </summary>
        /// <param name="toKeyValue"></param>
        public void CopyValueData(KeyValue toKeyValue)
        {
            toKeyValue._uid = _uid;
        }
        
        public string Uid()
        {
            if (!string.IsNullOrWhiteSpace(_uid)) return _uid;
            CreateUid();
            return _uid;
        }

        public void CreateUid()
        {
            if (!string.IsNullOrWhiteSpace(_uid)) return;
            
            _uid = Guid.NewGuid().ToString();
        }

        public void Draw(KeyValueDrawer keyValueDrawer, bool structureOnly = false)
        {
            MigrateToVersion4();
            keyValueDrawer.Draw(this, structureOnly);
            keyValueDrawer.Draw_v3(this, structureOnly);
        }

        public void ReplaceContentWith(KeyValue copyKeyValue)
        {
            values = copyKeyValue.Clone().values;
        }
        
        private int RandomIndex(int min, int max) => Random.Range(min, max);
        

        // This will reconnect the non-serializable objects (Sprites, AudioClips etc) from the 
        // ....
        public void LoadSavedObjects()
        {
            throw new NotImplementedException();
        }

        public void AddList(string typeString)
        {
            var selection = (Dictionaries.Types)Enum.Parse(typeof(Dictionaries.Types), typeString);
            
        }

        public void RemoveIndex(int i)
        {
            
        }

        public void CheckForMissingObjectReferences()
        {
            foreach (var keyValueList in values)
                keyValueList.CheckForMissingObjectReferences();
        }
    }
}