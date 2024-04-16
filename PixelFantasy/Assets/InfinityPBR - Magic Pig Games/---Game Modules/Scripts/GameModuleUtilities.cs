using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    public static class GameModuleUtilities
    {
        public static string ModulesScriptableObjectTypeName<T>() => typeof(T).ToString();

        public const string GameModulesGeneratedScriptPath = "Assets/Game Module Objects/Game Modules Generated Scripts";

        // March 15, 2024
        // I just added this feature, it will be added in the code in time, but today, I won't be adding these calls
        // throughout the code, so it will be a bit before it is used heavily.
        public static void DebugConsoleMessage(string message)
        {
#if UNITY_EDITOR
            if (!EditorPrefs.GetBool("Show Console Messages in Edit Mode", false))
                return;
            
            // HEY! Did you think you'd find another line? Click the 2nd link in the 
            // stack trace, as that's the line that you're probably looking for!
            Debug.Log($"<color=magenta>GM4:</color> {message} \n<color=magenta>Click 2nd stacktrace for location.</color>");
#endif
        }
    
        //-------------------------------------------------------------------------------------------------------------
        // GAME MODULE OBJECTS - GameModuleObject[]
        //-------------------------------------------------------------------------------------------------------------
        
        
        private static Dictionary<string, ModulesScriptableObject[]> gameModuleObjects = new Dictionary<string, ModulesScriptableObject[]>();
        
        // This is the method to call to get all objects of a T type, i.e. GameModuleObjects<Stat> and optionally choose
        // to recompute (often during a caching phase) or not.
        public static T[] GameModuleObjects<T>(bool recompute = false) where T : ModulesScriptableObject
        {
            #if UNITY_EDITOR
            var modulesObjectType = ModulesScriptableObjectTypeName<T>(); // Cache the string of the GameModules type (Stat, ItemObject etc)

            // If we aren't recomputing and we found the dictionary key, then return the value
            if (gameModuleObjects.TryGetValue(modulesObjectType, out var foundDictionary) && !recompute)
                return (T[])foundDictionary;
            
            // Get the array of objects. All other options require this
            var i = 0;
            var guids1 = AssetDatabase.FindAssets($"t:{typeof(T)}", null);
            var allObjects = new T[guids1.Length];
            
            foreach (var guid1 in guids1)
            {
                allObjects[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }
            
            
            // Order the assets by objectType then objectName, alphabetically
            allObjects = allObjects.OrderBy(x => x.objectType).ThenBy(x => x.objectName).ToArray();

            // foundDictionary is null if we are not recomputing. If it is not, then add the newArray
            if (foundDictionary != null)
            {
                gameModuleObjects[modulesObjectType] = allObjects;
                return allObjects;
            }
            
            // Regardless of whether we are recomputing or not, we didn't find it, so it has to be added, and 
            // therefor, it will be empty if we don't recompute it, and generally I think users would expect the value
            // to be added correctly when added.
            
            gameModuleObjects.Add(modulesObjectType, allObjects);
            return allObjects;
            #else
            return null;
            #endif
        }
        
#if UNITY_EDITOR
        public static UnityEngine.Object[] FindAssetsByInterface<T>() where T : class
        {
            var guids = AssetDatabase.FindAssets("", new[] { "Assets" });
            var objects = new List<UnityEngine.Object>();
    
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset is T)
                {
                    objects.Add(asset);
                }
            }

            return objects.ToArray();
        }
#endif


        public static T GameModuleObject<T>(string uid, bool recompute = false) where T : ModulesScriptableObject
        {
            #if UNITY_EDITOR
            var modulesObjectType = ModulesScriptableObjectTypeName<T>(); // Cache the string of the GameModules type (Stat, ItemObject etc)

            // If we aren't recomputing and we found the dictionary key, then return the value
            if (gameModuleObjects.TryGetValue(modulesObjectType, out var foundDictionary) && !recompute)
                return foundDictionary.FirstOrDefault(x => x.Uid() == uid) as T;

            // Find the object by uid based on the type T
            var guids1 = AssetDatabase.FindAssets($"t:{typeof(T)}", null);

            return guids1.Select(guid1 => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid1)))
                .FirstOrDefault(foundObject => foundObject.Uid() == uid);
#else
            return null;
#endif
        }
        
        public static T GameModuleObjectByName<T>(string name, bool recompute = false) where T : ModulesScriptableObject
        {
            #if UNITY_EDITOR
            var modulesObjectType = ModulesScriptableObjectTypeName<T>(); // Cache the string of the GameModules type (Stat, ItemObject etc)

            // If we aren't recomputing and we found the dictionary key, then return the value
            if (gameModuleObjects.TryGetValue(modulesObjectType, out var foundDictionary) && !recompute)
                return foundDictionary.FirstOrDefault(x => x.objectName == name) as T;

            // Find the object by uid based on the type T
            var guids1 = AssetDatabase.FindAssets($"t:{typeof(T)}", null);

            return guids1.Select(guid1 => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid1)))
                .FirstOrDefault(foundObject => foundObject.objectName == name);
            
#else
            return null;
#endif
        }
        
        //-------------------------------------------------------------------------------------------------------------
        // GAME MODULE OBJECT OF TYPE - GameModuleObject[]
        //-------------------------------------------------------------------------------------------------------------
        
        private static Dictionary<string, Dictionary<string, ModulesScriptableObject[]>> gameModuleObjectsOfType = new Dictionary<string, Dictionary<string, ModulesScriptableObject[]>>();
        
        // This is the method that is called which will get data from the objectsOfType Dictionary, and add new values (or update them)
        // when needed
        public static T[] GameModuleObjectsOfType<T>(string objectType, bool recompute = false) where T : ModulesScriptableObject
        {
            if (string.IsNullOrWhiteSpace(objectType))
            {
                Debug.LogWarning("objectType provided is null or empty");
                return default;
            }
    
            var modulesObjectType = ModulesScriptableObjectTypeName<T>();

            // Check if the key exists in the dictionary and try to retrieve the value
            if (gameModuleObjectsOfType.TryGetValue(modulesObjectType, out var foundDictionary) &&
                foundDictionary.TryGetValue(objectType, out var value) &&
                !recompute)
                return (T[])value;

            // Get all objects of type T
            ModulesScriptableObject[] newArray = GameModuleObjects<T>().Where(x => x.objectType == objectType).OrderBy(x => x.objectName).ToArray();

            // If the key exists in the dictionary, add the newArray to the child dictionary
            if (foundDictionary != null)
            {
                foundDictionary[objectType] = newArray;
                return (T[])newArray;
            }
            
            // Key does not exist in the main dictionary, so we must populate it. Regardless of recompute, we will
            // recompute so that the new value is not empty.
            //var newDictionary = new Dictionary<string, ModulesScriptableObject[]> { { objectType, newArray } };
            //gameModuleObjectsOfType[modulesObjectType] = newDictionary;
            gameModuleObjectsOfType[modulesObjectType] = new Dictionary<string, ModulesScriptableObject[]>();
            gameModuleObjectsOfType[modulesObjectType][objectType] = newArray;

            return (T[])newArray;
        }
        
        //-------------------------------------------------------------------------------------------------------------
        // GAME MODULE OBJECT TYPES - string[]
        //-------------------------------------------------------------------------------------------------------------
        
        private static Dictionary<string, string[]> gameModuleObjectTypes = new Dictionary<string, string[]>();
        
        // This is the method to call to get all objects of a T type, i.e. GameModuleObjects<Stat> and optionally choose
        // to recompute (often during a caching phase) or not.
        public static string[] GameModuleObjectTypes<T>(bool recompute = false) where T : ModulesScriptableObject
        {
            var modulesObjectType = ModulesScriptableObjectTypeName<T>();

            // If we have it cached and we aren't recomputing, then return the value
            if (gameModuleObjectTypes.TryGetValue(modulesObjectType, out var values) && !recompute)
                return values;

            values = GameModuleObjects<T>(recompute).Select(x => x.objectType).OrderBy(x => x).Distinct().ToArray();
            gameModuleObjectTypes[modulesObjectType] = values;

            return values;
        }
        
        //-------------------------------------------------------------------------------------------------------------
        // GAME MODULE OBJECT NAMES - string[]
        // Note: This will still be ordered by objectType then objectName!
        //-------------------------------------------------------------------------------------------------------------
        
        private static Dictionary<string, string[]> _gameModuleObjectNames = new Dictionary<string, string[]>();
        
        // This is the method to call to get all objects of a T type, i.e. GameModuleObjects<Stat> and optionally choose
        // to recompute (often during a caching phase) or not.
        public static string[] GameModuleObjectNames<T>(bool recompute = false) where T : ModulesScriptableObject
        {
            var modulesObjectType = ModulesScriptableObjectTypeName<T>();

            // If we have it cached and we aren't recomputing, then return the value
            if (_gameModuleObjectNames.TryGetValue(modulesObjectType, out var values) && !recompute)
                return values;

            values = GameModuleObjects<T>(recompute).Select(x => x.objectName).Distinct().ToArray();
            _gameModuleObjectNames[modulesObjectType] = values;

            return values;
        }
        
        //-------------------------------------------------------------------------------------------------------------
        // GAME MODULE OBJECT TYPE AND NAMES - string[]
        // This will be something like [ObjectType] ObjectName -- useful for popups
        //-------------------------------------------------------------------------------------------------------------
        
        private static Dictionary<string, string[]> _gameModuleObjectTypeAndNames = new Dictionary<string, string[]>();
        
        // This is the method to call to get all objects of a T type, i.e. GameModuleObjects<Stat> and optionally choose
        // to recompute (often during a caching phase) or not.
        public static string[] GameModuleObjectTypeAndNames<T>(bool recompute = false) where T : ModulesScriptableObject
        {
            var modulesObjectType = ModulesScriptableObjectTypeName<T>();

            // If we have it cached and we aren't recomputing, then return the value
            if (_gameModuleObjectTypeAndNames.TryGetValue(modulesObjectType, out var values) && !recompute)
                return values;

            values = GameModuleObjects<T>(recompute).Select(x => $"[{x.objectType}] {x.objectName}").Distinct().ToArray();
            _gameModuleObjectTypeAndNames[modulesObjectType] = values;

            return values;
        }
        
        //-------------------------------------------------------------------------------------------------------------
        // DICTIONARIES - Dictionaries[]
        //-------------------------------------------------------------------------------------------------------------
        
        public static Dictionaries[] GameModuleObjectDictionaries<T>(string objectType)
            where T : ModulesScriptableObject
        {
            // MasteryLevels is unique, in that it has to get the dictionaries from the levels as well
            if (ModulesScriptableObjectTypeName<T>() != "InfinityPBR.Modules.MasteryLevels")
                return GameModuleObjectsOfType<T>(objectType).Select(x => x.dictionaries).ToArray();
            
            var masteryLevels = GameModuleObjects<MasteryLevels>()
                .FirstOrDefault(x => x.objectName == objectType);
                
            return masteryLevels == null
                ? Array.Empty<Dictionaries>()
                : masteryLevels.levels
                    .Select(x => x.dictionaries)
                    .Concat(new[] { masteryLevels.dictionaries })
                    .ToArray();
        }

        // Returns a single object based on the uid
        public static T GetObject<T>(string uid) where T : ModulesScriptableObject 
            => GameModuleObjects<T>(true).FirstOrDefault(x => x.Uid() == uid);
        
        // With the provided key and objectType, will return an array of type T of all the objectType objects
        // which have the key in their dictionary
        public static T[] ObjectsOfTypeWithDictionaryKeyValue<T>(string objectType, string key) where T : ModulesScriptableObject 
            => GameModuleObjectsOfType<T>(objectType).Where(x => x.dictionaries != null && x.HasKeyValue(key)).ToArray(); 
        
        //-------------------------------------------------------------------------------------------------------------
        // ITEM OBJECT - Methods specific to ItemObject that can't be generics
        //-------------------------------------------------------------------------------------------------------------
        
        public static ItemObject[] ItemObjectsOfType(string itemObjectType) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType).ToArray();
        
        public static ItemObject[] ItemObjectsOfTypeWithStartingItemAttributeType(string itemObjectType, string itemAttributeType) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType)
                .Where(x => x.startingItemAttributes.Any(itemAttribute => itemAttribute.objectType == itemAttributeType))
                .ToArray();
        
        public static ItemObject[] ItemObjectsOfTypeWithItemAttributeTypeVariable(string itemObjectType, string itemAttributeType) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType)
                .Where(x => x.variables.Any(itemObjectVariable => itemObjectVariable.ActiveAttribute.objectType == itemAttributeType))
                .ToArray();
        
        public static ItemObject[] ItemObjectsOfTypeThatCanUseItemAttributeType(string itemObjectType, string itemAttributeType) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType)
                .Where(x => x.CanUseAttributeType(itemAttributeType)).ToArray();
        
        public static ItemObject[] ItemObjectsOfTypeThatCanUseItemAttribute(string itemObjectType, ItemAttribute itemAttribute) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType)
                .Where(x => x.CanUseAttribute(itemAttribute)).ToArray();
        
        public static ItemObject[] ItemObjectsOfTypeThatCanUseItemAttribute(string itemObjectType, string itemAttribute) 
            => GameModuleObjectsOfType<ItemObject>(itemObjectType)
                .Where(x => x.CanUseAttribute(itemAttribute)).ToArray();
        
        //-------------------------------------------------------------------------------------------------------------
        // STAT - Methods specific to Stat that can't be generics
        //-------------------------------------------------------------------------------------------------------------

        // These deal with stats that are marked "canBeTrained"
        private static Stat[] _cachedTrainableStats = Array.Empty<Stat>();
        public static Stat[] TrainableStats(bool recompute = false)
            => recompute
                ? _cachedTrainableStats = GameModuleObjects<Stat>(true).Where(x => x.canBeTrained)
                    .OrderBy(x => x.objectType).ThenBy(x => x.objectName).ToArray()
                : _cachedTrainableStats;
        
        private static string[] TrainableStatNames(bool recompute = false) 
            => TrainableStats(recompute).Select(x => $"{x.objectName}").ToArray();
        
        private static string[] TrainableStatTypeAndNames(bool recompute = false) 
            => TrainableStats(recompute).Select(x => $"[{x.objectType}] {x.objectName}").ToArray();
        
        // These deal with stats that are marked "canBeModified"
        private static Stat[] _cachedModifiableStats = Array.Empty<Stat>();
        public static Stat[] ModifiableStats(bool recompute = false)
            => recompute
                ? _cachedModifiableStats = GameModuleObjects<Stat>(true).Where(x => x.canBeModified)
                    .OrderBy(x => x.objectType).ThenBy(x => x.objectName).ToArray()
                : _cachedModifiableStats;

        private static string[] ModifiableStatNames(bool recompute = false) 
            => ModifiableStats(recompute).Select(x => $"{x.objectName}").ToArray();
        
        private static string[] ModifiableStatTypeAndNames(bool recompute = false) 
            => ModifiableStats(recompute).Select(x => $"[{x.objectType}] {x.objectName}").ToArray();
        
        //-------------------------------------------------------------------------------------------------------------
        // BLACKBOARD NOTE - Methods specific to BlackboardNote that can't be generics
        //-------------------------------------------------------------------------------------------------------------
        
        // Returns all the Starting Notes assigned to Blackboard Values
        public static BlackboardNote[] BlackboardStartingNotes =>
            GameModuleObjects<BlackboardValues>().SelectMany(x => x.startingNotes).OrderBy(x => x.name).ToArray();

        // Returns all the "Topics" on all of the starting notes assigned to Blackboard Values
        public static string[] BlackboardStartingNoteTopics =>
            BlackboardStartingNotes.Select(x => x.topic).Distinct().OrderBy(x => x).ToArray();
        
        // Returns all the "Subjects" on all of the starting notes assigned to Blackboard Values
        public static string[] BlackboardStartingNoteSubjects(string topic) =>
            BlackboardStartingNotes.Where(x => x.topic == topic).Select(x => x.subject).Distinct().OrderBy(x => x).ToArray();
        
        // Returns all the names of all of the starting notes assigned to Blackboard Values
        public static string[] BlackboardStartingNoteNames =>
            BlackboardStartingNotes.Select(x => x.name).Distinct().OrderBy(x => x).ToArray();

        public static BlackboardNote BlackboardStartingNote(string name) 
            => BlackboardStartingNotes.FirstOrDefault(x => x.name == name);
        public static BlackboardNote BlackboardStartingNote(string topic, string subject) 
            => BlackboardStartingNotes.FirstOrDefault(x => x.topic == topic && x.subject == subject);
        
        //-------------------------------------------------------------------------------------------------------------
        // OTHER STUFF
        //-------------------------------------------------------------------------------------------------------------
        
        public static int CountUids(string uid) 
            => GameModuleObjects<ItemObject>(true).Count(obj => obj.Uid() == uid) 
               + GameModuleObjects<ItemAttribute>(true).Count(obj => obj.Uid() == uid) 
               + GameModuleObjects<Stat>(true).Count(obj => obj.Uid() == uid) 
               + GameModuleObjects<Condition>(true).Count(obj => obj.Uid() == uid) 
               + GameModuleObjects<MasteryLevels>(true).Count(obj => obj.Uid() == uid) 
               + GameModuleObjects<LookupTable>(true).Count(obj => obj.Uid() == uid)
               + GameModuleObjects<LootBox>(true).Count(obj => obj.Uid() == uid)
               + GameModuleObjects<LootItems>(true).Count(obj => obj.Uid() == uid)
               + GameModuleObjects<Quest>(true).Count(obj => obj.Uid() == uid)
               + GameModuleObjects<QuestCondition>(true).Count(obj => obj.Uid() == uid)
               + GameModuleObjects<QuestReward>(true).Count(obj => obj.Uid() == uid);

        public static void ResetUid(ModulesScriptableObject modulesScriptableObject)
        {
            modulesScriptableObject.ResetUid();
        }

        public static ObjectReference GetObjectReference()
        {
#if UNITY_EDITOR
            var i = 0;
            string[] guids1 = AssetDatabase.FindAssets("t:ObjectReference", null);
            var allThings = new ObjectReference[guids1.Length];
            
            foreach (string guid1 in guids1)
            {
                allThings[i] = AssetDatabase.LoadAssetAtPath<ObjectReference>(AssetDatabase.GUIDToAssetPath(guid1));
                i++;
            }

            if (allThings.Length > 1)
            {
                Debug.LogWarning($"There were {allThings.Length} ObjectReference objects found! There should be " +
                                 "only one. We will use the first one found, but you should probably figure out which" +
                                 " one should be deleted.");
            }

            return allThings.Length == 0 ? default : allThings[0];
#else
            return default;
#endif
        }
        
        public static T GetObjectByType<T>(Type type)
        {
#if UNITY_EDITOR
            return (T)Activator.CreateInstance(type);
#endif
            return default;
        }
        
        public static int CountSaveIdsInScene(string saveId)
        {
#if UNITY_EDITOR
            return Object.FindObjectsOfType<Saveable>().Count(x => x.SaveId == saveId);
#else
            return -1;
#endif
        }
        
        public static string CleanTypeName(string value)
        {
            value = value.Replace("System.", "");
            value = value.Replace("UnityEngine.", "");
            value = value.Replace("InfinityPBR.Modules.", "");
            if (value == "Int32") value = "Int";
            if (value == "Single") value = "Float";
            if (value == "Boolean") value = "Bool";
            return value;
        }

        public static bool NewObjectTypeIsSelected<T>(int objectTypeIndex) where T : ModulesScriptableObject 
            => objectTypeIndex == GameModuleObjectTypes<T>().Length;

        public static string CreateAssets<T>(string locationToSave, string newTypeName, int objectTypeIndex, string[] newObjectNameArray) where T : ModulesScriptableObject
        {
            #if UNITY_EDITOR
            
            // Create the directories, including the new type if that is being created
            if (GameModuleObjectTypes<T>().Length == 0)
                objectTypeIndex = 0;
            else if (objectTypeIndex > GameModuleObjectTypes<T>().Length)
                objectTypeIndex = GameModuleObjectTypes<T>().Length - 1;

            // Get the directory to save the new object
            // If the objectTypeIndex == GameModouleObjectTypes length it means we have new type selected
            // If the type exists, we don't want 2, so we will save it at the location that already exists,
            // ignoring the new type.
            var finalDestination = NewObjectTypeIsSelected<T>(objectTypeIndex) && !GameModuleObjectTypes<T>().Contains(newTypeName)
                ? $"{locationToSave}/{newTypeName}"
                : $"{locationToSave}";
           
            var trimmedLocation = locationToSave.TrimEnd('/');

            if (NewObjectTypeIsSelected<T>(objectTypeIndex))
            {
                DebugConsoleMessage($"This is a new type ({newTypeName}), so we will save it at {finalDestination}");
            }
            else
            {
                DebugConsoleMessage($"This is an existing type ({GameModuleObjectTypes<T>()[objectTypeIndex]}), so we will save it at {finalDestination}");
            }
            
            // If we're making a new type, use the location selected by the user, unless we already have this type!
            var typeOfObjectBeingCreated = NewObjectTypeIsSelected<T>(objectTypeIndex) && !GameModuleObjectTypes<T>().Contains(newTypeName)
                ? newTypeName
                : trimmedLocation[(trimmedLocation.LastIndexOf('/') + 1)..];

            CreateDirectory(finalDestination);

            // Create each object, but skip any that are already existing
            foreach (var objName in newObjectNameArray)
            {
                if (string.IsNullOrWhiteSpace(objName))
                    continue;
                
                var fixedName = objName.AddUnderscoreIfFirstCharacterIsANumber();
                
                if (GameModuleObjectByName<T>(fixedName) != null)
                {
                    continue;
                }

                var newGameModulesObject = ScriptableObject.CreateInstance<T>();
                newGameModulesObject.objectType = typeOfObjectBeingCreated;
                newGameModulesObject.objectName = fixedName;

                AssetDatabase.CreateAsset(newGameModulesObject, $"Assets/{finalDestination}/{fixedName}.asset");
            }

            // Finish up
            GameModuleObjects<T>(true); // Recompute objects
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            DebugConsoleMessage($"typeOfObjectBeingCreated is {typeOfObjectBeingCreated}");
            var firstObject = GameModuleObjectsOfType<T>(typeOfObjectBeingCreated)[0];
            GameModuleObjectsOfType<T>(typeOfObjectBeingCreated, true); // Recompute objects
            
            if (firstObject != null)
            {
                // Confirm new assets have all the dictionaries
                AlignDictionaries<T>(typeOfObjectBeingCreated, firstObject);
            }
            
            return typeOfObjectBeingCreated;
            #endif
            return default;
        }
        
        public static void AlignDictionaries<T>(string objectType, object modulesScriptableObject) where T : ModulesScriptableObject
        {
#if UNITY_EDITOR
            var moduleObjects = GameModuleObjectsOfType<T>(objectType);
            if (moduleObjects == null)
            {
                return;
            }
            
            // If there is only one, then we skip, nothing to align.
            if (moduleObjects.Length == 0)
                return;

            var firstObject = modulesScriptableObject as T;
            DebugConsoleMessage($"Aligning dictionaries for {objectType}. There are {moduleObjects.Length} " +
                                $"objects of this type. First object {firstObject.ObjectName} has {firstObject.dictionaries.Count()} " +
                                $"keyValues;");

            int added = 0;
            // For each object past the first
            for (var i = 0; i < moduleObjects.Length; i++)
            {
                var currentObject = moduleObjects[i];
                foreach (var keyValue in firstObject.dictionaries.keyValues)
                {
                    if (currentObject.HasKeyValue(keyValue.key))
                        continue;

                    added++;
                    currentObject.dictionaries.AddNewKeyValue(keyValue.key);
                }
            }
            
            DebugConsoleMessage($"Added {added} new keys to {objectType} objects");
#endif
        }
        
        public static void SetAllDirty<T>(bool recompute = false) where T : ModulesScriptableObject
        {
#if UNITY_EDITOR
            
            // This should ensure that if the session is new, we always recache the objects December 16 2023
            var sessionStateString = SessionStateString<T>();
            var sessionStateValue = SessionState.GetBool(sessionStateString, false);
            if (!sessionStateValue)
            {
                recompute = true;
                SessionState.SetBool(sessionStateString, true);
            }
            
            foreach (var itemObject in GameModuleObjects<T>(recompute))
            {
                if (itemObject == null)
                    continue;
                EditorUtility.SetDirty(itemObject);
            }
#endif
        }

        public static string SessionStateString<T>(string defaultValue = "")
            where T : ModulesScriptableObject
        {
#if UNITY_EDITOR
            var typeName = ModulesScriptableObjectTypeName<T>();
            var sessionStateString = $"SetAllDirty<{typeName}>";
            return sessionStateString;
#endif
            return default;
        }

        public static void CreateDirectory(string path)
        {
            #if UNITY_EDITOR
            var absolutePath = Path.Combine(Application.dataPath, path);
            Directory.CreateDirectory(absolutePath);
            AssetDatabase.Refresh();
#else
            return;
#endif
        }
        
        public static string LocationToSave<T>(int newObjectTypeIndex, string saveLocation, string newObjectType) where T : ModulesScriptableObject
        {
            #if UNITY_EDITOR
            var objectTypes = GameModuleObjectTypes<T>();
            var newTypeIsSelected = newObjectTypeIndex == objectTypes.Length;

            if (objectTypes.Length == 0)
            {
                DebugConsoleMessage($"Length is 0 will return saveLocation {saveLocation}");
                return saveLocation;
            }
            
            if (newTypeIsSelected && string.IsNullOrWhiteSpace(newObjectType))
            {
                Debug.LogWarning("newObjectType was null or whitespace!");
                return saveLocation;
            }
            
            // If we're making a new type, use the location selected by the user, unless we already have this type!
            if (newTypeIsSelected && !objectTypes.Contains(newObjectType))
            {
                DebugConsoleMessage($"LocationToSave is {saveLocation} from newObjectType {newObjectType}");
                return saveLocation;
                //return $"{saveLocation}/{newObjectType}";
            }
            
            var selectedType = newObjectTypeIndex == objectTypes.Length
                ? newObjectType 
                : objectTypes[newObjectTypeIndex];
            var firstOfType = GameModuleObjectsOfType<T>(selectedType)[0];

            GameModuleObjectsOfType<T>(selectedType, true);
                
            // Get path to firstOfType
            var path = AssetDatabase.GetAssetPath(firstOfType);
            var relativePath = Path.GetRelativePath(Application.dataPath, path);
            relativePath = relativePath.Replace(firstOfType.name + ".asset", "");
        
            DebugConsoleMessage($"Path is {relativePath} from {firstOfType.name}");
            
            return relativePath;
#else
            return null;
#endif
        }

        private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
        public static string[] NewObjectsNameArray(string newObjectNames)
        {
            if (string.IsNullOrWhiteSpace(newObjectNames))
                return Array.Empty<string>();

            var array = newObjectNames.Split(',');
            for (var i = 0; i < array.Length; i++)
                array[i] = TrimAndRemoveInvalidChars(array[i]);

            return array;
        }
        
        public static int NamesInArray(string newObjectNames) => NewObjectsNameArray(newObjectNames).Length;

        public static bool NamesInArrayAreValid(string newObjectNames) 
            => NewObjectsNameArray(newObjectNames).Where(x => !string.IsNullOrWhiteSpace(x)).All(x => !StartsWithInvalidCharacter(x));

        public static string TrimAndRemoveInvalidChars(string value) 
            => InvalidChars.Aggregate(value, (current, c) 
                => current.Replace(c.ToString(), "")).Trim();

        public static string AddUnderscoresBeforeNumbers(string value)
        {
            //If the first character is a number, add an underscore
            if (char.IsDigit(value[0]))
                value = "_" + value;
            
            // If any character after a "," is a number, add an underscore
            for (var i = 1; i < value.Length; i++)
            {
                if (value[i] != ',') 
                    continue;
                
                if (char.IsDigit(value[i + 1]))
                    value = value.Insert(i + 1, "_");
            }

            return value;
        }

        public static string AddUnderscoreIfFirstCharacterIsANumber(this string value)
        {
            if (!string.IsNullOrEmpty(value) && char.IsDigit(value[0]))
                value = "_" + value;
            return value;
        }
        
        public static string GetRelativePathFromUserSelection()
        {
            #if UNITY_EDITOR
            var absolutePath = EditorUtility.OpenFolderPanel("Choose Save Location", "Assets/", "");
            if (!absolutePath.StartsWith(Application.dataPath))
                return null;
            
            var relativePath = Path.GetRelativePath(Application.dataPath, absolutePath);
            return relativePath;
            #endif
            return default;
        }

        public static void CloseAllInManager<T>() where T : ModulesScriptableObject
        {
            foreach (var stat in GameModuleObjects<T>())
                stat.showInManager = false;
        }

        public static string StatKind(Stat stat)
        {
            if (stat.canBeTrained && stat.canBeModified) return "Skill (Modifiable)";
            if (stat.canBeTrained && !stat.canBeModified) return "Skill";
            if (!stat.canBeTrained && stat.canBeModified) return "Stat";
            if (!stat.canBeTrained && !stat.canBeModified) return "Counter";

            return "Error";
        }
        
        public static void CheckObjectNamesAndTypes(IEnumerable<ModulesScriptableObject> objects)
        {
            foreach(var moduleObject in objects)
            {
                var tempName = moduleObject.ObjectName;
                var tempType = moduleObject.ObjectType;
                moduleObject.CheckObjectType();
                moduleObject.CheckObjectName();
                if (tempName != moduleObject.ObjectName)
                    DebugConsoleMessage($"Renamed {tempName} to {moduleObject.ObjectName}");
                if (tempType != moduleObject.ObjectType)
                    DebugConsoleMessage($"{moduleObject.ObjectName} type changed from {tempType} to {moduleObject.ObjectType}");
            }
        }
        
        public static List<T> SearchResults<T>(int searchTypeIndex, string searchString) where T : ModulesScriptableObject
        {
            var moduleObjects = GameModuleObjects<T>();
            if (searchTypeIndex < GameModuleObjectTypes<T>().Length)
                moduleObjects = moduleObjects.Where(x => x.ObjectType == GameModuleObjectTypes<T>()[searchTypeIndex]).ToArray();

            return moduleObjects.Where(x => x.ObjectName.ToLower().Contains(searchString.ToLower()) 
                                            || x.ObjectType.ToLower().Contains(searchString.ToLower()))
                .ToList();
        }

        public static bool TypeNameIsAllowed(string typeName) =>
            !string.IsNullOrEmpty(typeName) &&
            typeName is not ("[Choose new type]" or "Stat" or "Stats" or "Condition" or "Conditions" or "Quest"
                or "Quests" or "ItemObject"
                or "ItemObjects" or "ItemAttribute" or "ItemAttributes" or "LootItem" or "LootItems" or "LootBox"
                or "LootBoxes" or "QuestCondition" or "QuestConditions" or "QuestReward" or "QuestRewards"
                or "LookupTable" or "LookupTables" or "MasteryLevels" or "Voices" or "Item Object" or "Item Attribute"
                or "Item Objects"
                or "Item Attributes" or "Lookup Table" or "Lookup Tables" or "Mastery Levels" or "MasteryLevel"
                or "Mastery Levels"
                or "Quest Condition" or "Quest Conditions" or "Quest Reward" or "Quest Rewards" or "Loot Item"
                or "Loot Items"
                or "Loot Box" or "Loot Boxes")
            && !StartsWithInvalidCharacter(typeName);
        
        public static bool StartsWithInvalidCharacter(string typeName) =>
            !char.IsLetter(typeName[0]) && typeName[0] != '_';
        
        // March 5, 2024 -- adding this as a one stop location for storing types, in case we eventually support new
        // types. This is used in ObjectReference.cs.
        public static HashSet<string> validObjectReferenceTypes = new HashSet<string>
        {
            "InfinityPBR.Modules.Stat",
            "InfinityPBR.Modules.ItemObject",
            "InfinityPBR.Modules.ItemAttribute",
            "InfinityPBR.Modules.Condition",
            "InfinityPBR.Modules.Quest",
            "InfinityPBR.Modules.QuestCondition",
            "InfinityPBR.Modules.QuestReward",
            "InfinityPBR.Modules.LootBox",
            "InfinityPBR.Modules.LootItems",
            "UnityEngine.AnimationClip",
            "UnityEngine.Texture2D",
            "UnityEngine.AudioClip",
            "UnityEngine.Sprite",
            "UnityEngine.Color",
            "UnityEngine.Vector4",
            "UnityEngine.Vector3",
            "UnityEngine.Vector2",
            "UnityEngine.GameObject"
        };
    }
}