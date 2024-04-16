using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;

namespace InfinityPBR.Modules
{
    public abstract class InfinityEditorGameModules : Editor
    {
        
        // Object Selector Code
        public static T ObjectSelectField<T>(int width = 150, bool allowSceneObjects = false) where T : ModulesScriptableObject
        {
            BackgroundColor(Color.yellow);
            T newObject = default(T);
            newObject = Object(newObject, typeof(T), width, allowSceneObjects) as T;
            ResetColor();

            return newObject;
        }
        
        public static T ObjectSelectBox<T>(string label, int labelWidth = 100, int width = 150, int boxWidth = -1, bool allowSceneObjects = false) where T : ModulesScriptableObject
        {
            BackgroundColor(Color.yellow);
            if (boxWidth > 0)
                StartVerticalBox(boxWidth);
            else
                StartVerticalBox();
            StartRow();
            Label(label, labelWidth);
            var newObject = ObjectSelectField<T>(width, allowSceneObjects);
            EndRow();
            EndVerticalBox();
            ResetColor();
            
            return newObject;
        }
        
        /*
        * ----------------------------------------------------------------------------------------------
        * SEARCHING GAME MODULE TYPES
        * ----------------------------------------------------------------------------------------------
        */
        
        public static string SearchString<T>() where T : ModulesScriptableObject => GetString($"{typeof(T).Name} Search");

        public static string SearchField<T>(int labelWidth = 150, int fieldWidth = 200, string keyMod = "") where T : ModulesScriptableObject
        {
            var key = $"{typeof(T).Name}{keyMod} Search";
            var specialEnd = "s";
            if (typeof(T).Name == "LootBox")
                specialEnd = "es";
            if (typeof(T).Name == "LootItems")
                specialEnd = "";
            
            var label = $"Search {typeof(T).Name}{specialEnd}: ";
            var value = GetString(key);
            
            StartRow();
            Label(label, labelWidth);
            SetString(key, TextField(value, fieldWidth));
            EndRow();
            return GetString(key);
        }

        public static string SearchField<T>(string keyMod) where T : ModulesScriptableObject 
            => SearchField<T>(150, 200, keyMod);

        // --------------------------------------------------------------------------------------------------------
        // Selection methods for Game Modules types & folders
        // --------------------------------------------------------------------------------------------------------

        public static string ModulesWindowSelectionKey => "Modules Window Selection Action";
        public static float ModulesWindowLastTime => GetFloat("Modules Window Selection Action");
        public static bool CanDoModulesWindowSelection => ModulesWindowLastTime < EditorApplication.timeSinceStartup;
        public static void ResetModulesWindowTimer() => SetFloat(ModulesWindowSelectionKey, (float)EditorApplication.timeSinceStartup + 0.5f);
        
        public static void TryToSelectInspector()
        {
            if (!CanDoModulesWindowSelection) return;
            ResetModulesWindowTimer();
            
            var guid = Selection.assetGUIDs[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.LoadAssetAtPath<ModulesScriptableObject>(path) == null)
                return;
            
            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        }
        
        
        public static (int, string) SearchBoxWithType<T>(int index, string[] startTypes, string keyMod = "") where T : ModulesScriptableObject
        {
            StartVerticalBox();
            StartRow();
            Label("Object Types", 150);
            var types = startTypes
                .Concat(new[] { "All Types" })
                .ToArray();
            if (index < 0 || index >= types.Length)
                index = types.Length - 1;
            index = Popup(index, types, 200);
            EndRow();
            var searchValue = SearchField<T>(keyMod);
            EndVerticalBox();
            return (index, searchValue);
        }
    }
}

