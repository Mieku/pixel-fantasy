using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    public static class EditorUtilities
    {
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
        
        // --------------------------------------------------------------------------------------------------------
        // Cache lists of item objects that are otherwise very time-consuming to obtain
        // --------------------------------------------------------------------------------------------------------
        
        public static bool OnOffButton(bool isOn, bool colors = true)
        {
            if (colors)
                BackgroundColor(isOn ? Color.green : Color.grey);

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ViewToolOrbit On"), GUILayout.Width(25)))
            {
                ResetColor();
                return !isOn;
            }
            
            ResetColor();
            return isOn;
        }

        public static void DrawBox(Action content)
        {
            StartRow("helpbox");
            content?.Invoke();
            EndRow();
        }
        
        public static void DrawField(SerializedObject obj, string propName)
        {
            try
            {
                var prop = obj.FindProperty(propName);
                EditorGUILayout.PropertyField(prop);
            }
            catch
            {
                MessageBox(($"No Property Exists {propName}"), MessageType.Warning);
            }
        }
        
        public static void DrawField(SerializedProperty prop) => EditorGUILayout.PropertyField(prop);
        
        /*
        * ----------------------------------------------------------------------------------------------
        * BUTTONS, LABELS, ETC
        * ----------------------------------------------------------------------------------------------
        */
        
        // Horizontal Gap
        public static void Gap(int width = 20) => Label("", width);

        // Buttons Group Code
        // Open / Close button
        public static bool ButtonOpenClose(string key, bool value = false, bool grabValueFromKey = true)
        {
            if (grabValueFromKey)
                value = GetBool(key);
            ColorsIf(value, Color.green, Color.black, Color.white, Color.grey);
            SetBool(key, ButtonToggleEye(value, 25));
            ResetColor();
            return GetBool(key);
        }

        public static bool ButtonOpenClose(bool value)
        {
            ColorsIf(value, Color.green, Color.black, Color.white, Color.grey);
            var newValue = ButtonToggleEye(value, 25);
            ResetColor();
            return newValue;
        }
        
        // Buttons
        public static bool ButtonToggle(bool value, string label, int width)
        {
            if (GUILayout.Button(label, GUILayout.Width(width)))
                return !value;
            return value;
        }
        
        public static bool ButtonToggleEye(bool value, int width)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_ViewToolOrbit On"), GUILayout.Width(width)))
                return !value;
            return value;
        }
        
        public static bool ButtonToggle(bool value, string label, int width, int height)
        {
            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(height)))
                return !value;
            return value;
        }
        
        public static bool ButtonToggle(bool value, string label, string tooltip, int width)
        {
            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(width)))
                return !value;
            return value;
        }
        
        public static bool ButtonToggle(bool value, string label, string tooltip, int width, int height)
        {
            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(width), GUILayout.Height(height)))
                return !value;
            return value;
        }
        
        public static bool Button(string label, string tooltip, int width, int height)
        {
            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(width), GUILayout.Height(height)))
                return true;
            return false;
        }
        
        public static bool Button(string label, string tooltip, int width)
        {
            
            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(width)))
                return true;
            return false;
        }
        
        public static bool Button(string label, string tooltip)
        {
            if (GUILayout.Button(new GUIContent(label, tooltip)))
                return true;
            return false;
        }
        
        public static bool Button(string label, int width, int height)
        {
            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(height)))
                return true;
            return false;
        }

        public static bool Button(string label, int width)
        {
            if (GUILayout.Button(label, GUILayout.Width(width)))
                return true;
            return false;
        }

        public static bool Button(string label)
        {
            //Debug.Log("Button Name: " + label);
            if (GUILayout.Button(label))
                return true;
            return false;
        }

        // Label Fields
        public static void LabelSized(string label, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(label, style);
        }
        
        public static void LabelSized(string label, string tooltip, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style);
        }
        
        public static void LabelSized(string label, int width, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(width));
        }
        
        public static void LabelSized(string label, string tooltip, int width, int fontSize = 18, bool bold = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            style.fontSize = fontSize;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style, GUILayout.Width(width));
        }
        
        public static void Label(string label, bool bold = false, bool wordwrap = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(label, style);
        }
        
        public static void LabelGrey(string label, bool bold = false, bool wordwrap = false)
        {
            ContentColor(Color.grey);
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(label, style);
            ContentColor(Color.white);
        }
        
        public static void LabelGrey(string label, int width, bool bold = false, bool wordwrap = false)
        {
            ContentColor(Color.grey);
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(width));
            ContentColor(Color.white);
        }
        
        public static void Label(string label, int width, bool bold = false, bool wordwrap = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(width));
        }
        
        public static void Label(string label, int width, int height, bool bold = false, bool wordwrap = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(width), GUILayout.Height(height));
        }
        
        public static void Label(string label, string tooltip, bool bold = false, bool wordwrap = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style);
        }
        
        public static void Label(string label, string tooltip, int width, bool bold = false, bool wordwrap = false)
        {
            GUIStyle style = new GUIStyle(bold ? EditorStyles.boldLabel : EditorStyles.label);
            if (wordwrap) style.wordWrap = true;
            EditorGUILayout.LabelField(new GUIContent(label, tooltip), style, GUILayout.Width(width));
        }
        
        // Sliders

        public static int SliderInt(int value, int min, int max) 
            => Mathf.RoundToInt(EditorGUILayout.Slider(value, min, max));
        
        public static int SliderInt(int value, int min, int max, int width) 
            => Mathf.RoundToInt(EditorGUILayout.Slider(value, min, max, GUILayout.Width(width)));
        
        public static int SliderInt(string label, int value, int min, int max) 
            => Mathf.RoundToInt(EditorGUILayout.Slider(label, value, min, max));
        
        public static int SliderInt(string label, int value, int min, int max, int width) 
            => Mathf.RoundToInt(EditorGUILayout.Slider(label, value, min, max, GUILayout.Width(width)));
        
        public static float SliderFloat(float value, float min, float max) 
            => EditorGUILayout.Slider(value, min, max);
        
        public static float SliderFloat(float value, float min, float max, int width) 
            => EditorGUILayout.Slider(value, min, max, GUILayout.Width(width));
        
        public static float SliderFloat(string label, float value, float min, float max) 
            => EditorGUILayout.Slider(label, value, min, max);
        
        public static float SliderFloat(string label, float value, float min, float max, int width) 
            => EditorGUILayout.Slider(label, value, min, max, GUILayout.Width(width));
        
        // Text Areas
        public static string TextArea(string text, int width, int height = 50)
        {
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            return EditorGUILayout.TextArea(text, style, GUILayout.Width(width), GUILayout.Height(height));
        }
        
        public static string TextArea(string text, int height = 50)
        {
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            return EditorGUILayout.TextArea(text, style, GUILayout.Height(height));
        }
        
        // Enum
        public static Enum EnumPopup(Enum selected) => EditorGUILayout.EnumPopup(selected);

        public static Enum EnumPopup(Enum selected, int width) => EditorGUILayout.EnumPopup(selected, GUILayout.Width(width));
        
        
        // Toolbar
        
        //public static int Toolbar(int value, string[] options) => GUILayout.Toolbar(value, options);
        //public static int Toolbar(int value, string[] options, int width) => GUILayout.Toolbar(value, options, GUILayout.Width(width));
        
        // Text Fields

        public static string TextFieldSetString(string key, int width, bool bold = false)
        {
            var text = HasKey(key) ? GetString(key) : "";
            SetString(key, TextField(text, width, bold));
            return GetString(key);
        }
        
        public static string TextField(string text, bool bold = false) => bold 
            ? EditorGUILayout.TextField(text, EditorStyles.boldLabel) 
            : EditorGUILayout.TextField(text);
        
        public static string TextField(string label, string text, bool bold = false) => bold 
            ? EditorGUILayout.TextField(label, text, EditorStyles.boldLabel) 
            : EditorGUILayout.TextField(label, text);
        
        public static string TextField(string label, string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.TextField(label, text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.TextField(label, text, GUILayout.Width(width));
        
        public static string TextField(string label, string tooltip, string text, bool bold = false) => bold 
            ? EditorGUILayout.TextField(new GUIContent(label, tooltip), text, EditorStyles.boldLabel) 
            : EditorGUILayout.TextField(new GUIContent(label, tooltip), text);
        
        public static string TextField(string label, string tooltip, string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.TextField(new GUIContent(label, tooltip), text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.TextField(new GUIContent(label, tooltip), text, GUILayout.Width(width));
        
        public static string TextField(string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.TextField(text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.TextField(text, GUILayout.Width(width));
        
        public static string TextField(string text, int width, int height, bool bold = false) => bold 
            ? EditorGUILayout.TextField(text, EditorStyles.boldLabel, GUILayout.Width(width), GUILayout.Height(height)) 
            : EditorGUILayout.TextField(text, GUILayout.Width(width), GUILayout.Height(height));

        // Delayed Text Fields
        public static string DelayedText(string text, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(text, EditorStyles.boldLabel) 
            : EditorGUILayout.DelayedTextField(text);

        public static string DelayedText(string label, string text, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(label, text, EditorStyles.boldLabel) 
            : EditorGUILayout.DelayedTextField(label, text);
        
        public static string DelayedText(string label, string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(label, text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.DelayedTextField(label, text, GUILayout.Width(width));

        public static string DelayedText(string label, string tooltip, string text, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(new GUIContent(label, tooltip), text, EditorStyles.boldLabel) 
            : EditorGUILayout.DelayedTextField(new GUIContent(label, tooltip), text);

        public static string DelayedText(string label, string tooltip, string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(new GUIContent(label, tooltip), text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.DelayedTextField(new GUIContent(label, tooltip), text, GUILayout.Width(width));

        public static string DelayedText(string text, int width, bool bold = false) => bold 
            ? EditorGUILayout.DelayedTextField(text, EditorStyles.boldLabel, GUILayout.Width(width)) 
            : EditorGUILayout.DelayedTextField(text, GUILayout.Width(width));

        // Bool Checkboxes
        public static bool LeftCheck(string label, bool value) => EditorGUILayout.ToggleLeft(label, value);
        public static bool LeftCheck(string label, bool value, int width) => EditorGUILayout.ToggleLeft(label, value, GUILayout.Width(width));
        public static bool LeftCheck(string label, string tooltip, bool value) => EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), value);
        public static bool LeftCheck(string label, string tooltip, bool value, int width) => EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), value, GUILayout.Width(width));
        
        public static bool Check(string label, string tooltip, bool value) => EditorGUILayout.Toggle(new GUIContent(label, tooltip), value);
        public static bool Check(string label, string tooltip, bool value, int width) => EditorGUILayout.Toggle(new GUIContent(label, tooltip), value, GUILayout.Width(width));
        public static bool Check(string label, bool value) => EditorGUILayout.Toggle(label, value);
        public static bool Check(string label, bool value, int width) => EditorGUILayout.Toggle(label, value, GUILayout.Width(width));
        public static bool Check(bool value) => EditorGUILayout.Toggle(value);

        // Bool checkboxes that set prefs
        public static void LeftCheckSetBool(string boolName, string label) => SetBool(boolName, LeftCheck(label, GetBool(boolName)));

        public static bool Check(bool value, int width) => EditorGUILayout.Toggle(value, GUILayout.Width(width));

        // Float field
        public static float Float(string label, float value) => EditorGUILayout.FloatField(label, value);
        public static float Float(string label, float value, int width) => EditorGUILayout.FloatField(label, value, GUILayout.Width(width));
        public static float Float(float value) => EditorGUILayout.FloatField(value);
        public static float Float(float value, int width) => EditorGUILayout.FloatField(value, GUILayout.Width(width));
        
        public static float Float(string label, string tooltip, float value) => EditorGUILayout.FloatField(new GUIContent(label, tooltip), value);
        public static float Float(string label, string tooltip, float value, int width) => EditorGUILayout.FloatField(new GUIContent(label, tooltip), value, GUILayout.Width(width));

        public static float DelayedFloat(string label, float value) => EditorGUILayout.DelayedFloatField(label, value);
        public static float DelayedFloat(string label, float value, int width) => EditorGUILayout.DelayedFloatField(label, value, GUILayout.Width(width));
        public static float DelayedFloat(float value) => EditorGUILayout.DelayedFloatField(value);
        public static float DelayedFloat(float value, int width) => EditorGUILayout.DelayedFloatField(value, GUILayout.Width(width));
        public static float DelayedFloat(string label, string tooltip, float value) => EditorGUILayout.DelayedFloatField(new GUIContent(label, tooltip), value);
        public static float DelayedFloat(string label, string tooltip, float value, int width) => EditorGUILayout.DelayedFloatField(new GUIContent(label, tooltip), value, GUILayout.Width(width));
        
        // Int Field
        public static int Int(string label, int value) => EditorGUILayout.IntField(label, value);
        public static int Int(string label, int value, int width) => EditorGUILayout.IntField(label, value, GUILayout.Width(width));
        public static int Int(int value) => EditorGUILayout.IntField(value);
        public static int Int(int value, int width) => EditorGUILayout.IntField(value, GUILayout.Width(width));
        
        public static int DelayedInt(string label, int value) => EditorGUILayout.DelayedIntField(label, value);
        public static int DelayedInt(string label, int value, int width) => EditorGUILayout.DelayedIntField(label, value, GUILayout.Width(width));
        public static int DelayedInt(int value) => EditorGUILayout.DelayedIntField(value);
        public static int DelayedInt(int value, int width) => EditorGUILayout.DelayedIntField(value, GUILayout.Width(width));

        // Object Field
        public static Object Object(Object obj, Type objType, bool allowSceneObjects = false) => EditorGUILayout.ObjectField(obj, objType, allowSceneObjects);
        public static Object Object(Object obj, Type objType, string label, bool allowSceneObjects = false) => EditorGUILayout.ObjectField(label, obj, objType, allowSceneObjects);
        public static Object Object(Object obj, Type objType, string label, int width, bool allowSceneObjects = false) => EditorGUILayout.ObjectField(label, obj, objType, allowSceneObjects, GUILayout.Width(width));
        public static Object Object(Object obj, Type objType, int width, bool allowSceneObjects = false) => EditorGUILayout.ObjectField(obj, objType, allowSceneObjects, GUILayout.Width(width));
        
        // Object Selector Code
        public static T ObjectSelectField<T>(int width = 150, bool allowSceneObjects = false) where T : ModulesScriptableObject
        {
            BackgroundColor(Color.yellow);
            T newObject = null;
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
        
        // Color Select
        public static Color ColorField(Color color, int width) => EditorGUILayout.ColorField(color, GUILayout.Width(width));
        public static Color ColorField(Color color) => EditorGUILayout.ColorField(color);

        // Vector 4 Field
        public static Vector4 Vector4Field(Vector4 value, string label = "") => EditorGUILayout.Vector4Field(label, value);
        public static Vector4 Vector4Field(Vector4 value, int width, string label = "") => EditorGUILayout.Vector4Field(label, value, GUILayout.Width(width));
        
        // Vector 3 Field
        public static Vector3 Vector3Field(Vector3 value, string label = "") => EditorGUILayout.Vector3Field(label, value);
        public static Vector3 Vector3Field(Vector3 value, int width, string label = "") => EditorGUILayout.Vector3Field(label, value, GUILayout.Width(width));

        // Vector 2 Field
        public static Vector2 Vector2Field(Vector2 value, string label = "") => EditorGUILayout.Vector2Field(label, value);
        public static Vector2 Vector2Field(Vector2 value, int width, string label = "") => EditorGUILayout.Vector2Field(label, value, GUILayout.Width(width));

        // Popup
        public static int Popup(int index, string[] options) => EditorGUILayout.Popup(index, options);

        public static int PopupSetInt(string key, string[] options, int width)
        {
            var index = HasKey(key) ? GetInt(key) : 0;
            SetInt(key, Popup(index, options, width));
            return GetInt(key);
        }
        
        public static int Popup(int index, string[] options, int width) => EditorGUILayout.Popup(index, options, GUILayout.Width(width));
        public static int Popup(string label, int index, string[] options) => EditorGUILayout.Popup(label, index, options);
        public static int Popup(string label, int index, string[] options, int width) => EditorGUILayout.Popup(label, index, options, GUILayout.Width(width));
        public static int Popup(string label, string tooltip, int index, string[] options, int width) => EditorGUILayout.Popup(new GUIContent(label, tooltip), index, options, GUILayout.Width(width));

        /*
        * ----------------------------------------------------------------------------------------------
        * GUI LAYOUTS
        * ----------------------------------------------------------------------------------------------
        */
        
        // Boxes
        public static void StartVerticalBox() => EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        public static void StartVerticalBox(int width) => EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(width));
        public static void EndVerticalBox() => EditorGUILayout.EndVertical();
        public static void MessageBox(string content, MessageType messageType = MessageType.None) => EditorGUILayout.HelpBox(content, messageType);

        // Vertical
        public static void StartVertical() => EditorGUILayout.BeginVertical();
        public static void EndVertical() => EditorGUILayout.EndVertical();
        
        // Horizontal
        public static void StartRow() => EditorGUILayout.BeginHorizontal();
        public static void StartRow(string style) => EditorGUILayout.BeginHorizontal(style);
        public static void EndRow() => EditorGUILayout.EndHorizontal();
        
        // Space
        public static void Space() => EditorGUILayout.Space();
        public static void Space(int height) => EditorGUILayout.Space(height);

        // Foldout Header Group
        public static bool StartFoldoutHeaderGroup(bool value, string text) => EditorGUILayout.BeginFoldoutHeaderGroup(value, text);
        public static void StartFoldoutHeaderGroupSetBool(string boolName, string text) => SetBool(boolName, StartFoldoutHeaderGroup(GetBool(boolName), text));
        public static void EndFoldoutHeaderGroup() => EditorGUILayout.EndFoldoutHeaderGroup();

        // Foldout
        public static bool Foldout(bool value, string label) => EditorGUILayout.Foldout(value, label);

        public static bool FoldoutSetBool(string key, string label)
        {
            SetBool(key, Foldout(GetBool(key), label));
            return GetBool(key);
        }
        
        /*
        * ----------------------------------------------------------------------------------------------
        * PREFS ETC
        * ----------------------------------------------------------------------------------------------
        */
        
        // Set
        public static void SetBool(string name, bool value) => EditorPrefs.SetBool(name, value);
        public static void SetString(string name, string value) => EditorPrefs.SetString(name, value);
        public static void SetFloat(string name, float value) => EditorPrefs.SetFloat(name, value);
        public static void SetDouble(string name, double value) => EditorPrefs.SetFloat(name, (float)value);
        public static void SetInt(string name, int value) => EditorPrefs.SetInt(name, value);
        
        // Get
        public static bool GetBool(string name) => EditorPrefs.GetBool(name);
        public static string GetString(string name) => EditorPrefs.GetString(name);
        public static float GetFloat(string name) => EditorPrefs.GetFloat(name);
        public static int GetInt(string name) => EditorPrefs.GetInt(name);
        
        // Other
        public static bool HasKey(string name) => EditorPrefs.HasKey(name);
        public static void DeleteKey(string name) => EditorPrefs.DeleteKey(name);

        public static bool DeleteButton()
        {
            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                ResetColor();
                return true;
            }
            ResetColor();
            return false;
        }
        
        // Toggle
        public static bool ToggleBool(string name)
        {
            var value = GetBool(name);
            SetBool(name, !value);
            return !value;
        }

        /*
        * ----------------------------------------------------------------------------------------------
        * UTILITIES
        * ----------------------------------------------------------------------------------------------
        */
        
        // Open Folder Panel
        public static string OpenFolderPanel(string title, string folder = null, string defaultName = null) 
            => EditorUtility.OpenFolderPanel(title, folder, defaultName);

        public static string OpenFolderPanelSetString(string key, string title,
            string defaultName = null)
        {
            var newPath = OpenFolderPanel(title, GetString(key), defaultName);
            SetString(key, newPath);
            return newPath;
        }
        
        // Save Folder Panel
        public static string SaveFolderPanel(string title, string folder = null, string defaultName = null) 
            => EditorUtility.SaveFolderPanel(title, folder, defaultName);
        
        public static string SaveFolderPanelSetString(string key, string title,
            string defaultName = null)
        {
            var newPath = SaveFolderPanel(title, GetString(key), defaultName);
            newPath = newPath.Replace(Application.dataPath, "Assets");
            SetString(key, newPath);
            return newPath;
        }
        
        // Dialog
        public static bool Dialog(string title, string message, string ok = "Yes", string cancel = "Cancel")
        {
            if (EditorUtility.DisplayDialog(title, message, ok, cancel))
                return true;
            return false;
        }

        // Color
        public static void Colors(Color backgroundColor, Color contentColor)
        {
            BackgroundColor(backgroundColor);
            ContentColor(contentColor);
        }

        public static void Colors(Color colors) => Colors(colors, colors);
        public static void ColorsIf(bool boolean, Color backgroundColorTrue, Color backgroundColorFalse, Color contentColorTrue, Color contentColorFalse)
        {
            BackgroundColorIf(boolean, backgroundColorTrue, backgroundColorFalse);
            ContentColorIf(boolean, contentColorTrue, contentColorFalse);
        }
        public static void BackgroundColorIf(bool boolean, Color trueColor, Color falseColor) =>
            BackgroundColor(boolean ? trueColor : falseColor);
        public static void ContentColorIf(bool boolean, Color trueColor, Color falseColor) => 
            ContentColor(boolean ? trueColor : falseColor);

        public static void BackgroundColor(Color color) => GUI.backgroundColor = color;
        public static void ContentColor(Color color) => GUI.contentColor = color;
        public static void ResetColor()
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
        }
        
        // Animation Curve
        public static AnimationCurve Curve(AnimationCurve curve, float handlePos = -1f, int width = 0, int height = 100)
        {
            var curveRect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(width == 0));
            curve = EditorGUI.CurveField(curveRect, curve, Color.red, new Rect(0, 0, 1, 1));
            
            if (handlePos < 0)
                return curve;

            var lastRect = GUILayoutUtility.GetLastRect();
            //lastRect.x += EditorGUIUtility.labelWidth;
            //lastRect.width -= EditorGUIUtility.labelWidth;

            Handles.color = Color.white;
            var x = lastRect.x + lastRect.width * handlePos;
            Handles.DrawLine(new Vector3(x, lastRect.y), new Vector3(x, lastRect.y + lastRect.height));
            
            return curve;
        }
        
        // Other
        public static void OpenURL(string url) => Application.OpenURL(url);

        public static void ExitGUI() => EditorGUIUtility.ExitGUI();

        public static void IndentPlus() => EditorGUI.indentLevel++;
        public static void IndentMinus() => EditorGUI.indentLevel--;

        // Debug Logs
        public static void Log(string text)
        {
#if UNITY_EDITOR
            Debug.Log(text);
#endif
        }
        
        public static void LogError(string text)
        {
#if UNITY_EDITOR
            Debug.LogError(text);
#endif
        }
        
        public static void LogWarning(string text)
        {
#if UNITY_EDITOR
            Debug.LogWarning(text);
#endif
        }

        /*
        * ----------------------------------------------------------------------------------------------
        * KEYS PRESSED
        * ----------------------------------------------------------------------------------------------
        */

        public static bool KeyAlt => Event.current.alt;
        public static bool KeyShift => Event.current.shift;

        /*
        * ----------------------------------------------------------------------------------------------
        * LIST ORDERING ETC
        * ----------------------------------------------------------------------------------------------
        */

        /// <summary>
        /// Moves the item to a new location in the list, by moveValue
        /// </summary>
        /// <param name="thisList"></param>
        /// <param name="index"></param>
        /// <param name="moveValue"></param>
        public static void MoveItem<T>(List<T> thisList, int index, int moveValue)
        {
            if (index + moveValue < 0) return;
            if (index + moveValue >= thisList.Count) return;

            MoveItemTo(thisList, index, index + moveValue);
        }

        /// <summary>
        /// Moves the item to a new location in the List() as specified
        /// </summary>
        /// <param name="thisList"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        public static void MoveItemTo<T>(List<T> thisList, int fromIndex, int toIndex)
        {
            if (toIndex < 0) return;
            if (toIndex >= thisList.Count) return;
            
            var item = thisList[fromIndex];
            thisList.RemoveAt(fromIndex);
            thisList.Insert(toIndex, item);
        }
        
        /*
        * ----------------------------------------------------------------------------------------------
        * SEARCHING GAME MODULE TYPES
        * ----------------------------------------------------------------------------------------------
        */

        public static string SearchStringStat => GetString("Stat Search");
        public static string SearchStringStatModification => GetString("Stat Modification Search");
        public static string SearchStringQuestReward => GetString("Quest Reward Search");
        public static string SearchStringQuestCondition => GetString("Quest Condition Search");

        public static string SearchField(string key, string label, string value)
        {
            StartRow();
            Label(label, 150);
            SetString(key, TextField(value, 200));
            EndRow();
            return GetString(key);
        }

        public static string StatSearchField() => SearchField("Stat Search", "Search Stats: ", SearchStringStat);
        public static string StatModificationSearchField() => SearchField("Stat Modification Search", "Search Modifiable Stats: ", SearchStringStatModification);
        public static string QuestConditionSearchField() => SearchField("Quest Condition Search", "Search Quest Conditions: ", SearchStringQuestCondition);
        public static string QuestRewardSearchField() => SearchField("Quest Reward Search", "Search Quest Rewards: ", SearchStringQuestReward);
        
        public static bool CanMoveUp(int index, int total) => index != 0;
        
        public static bool CanMoveDown(int index, int total) => index != total - 1;

        public static string symbolInfo = "ⓘ";
        public static string symbolX = "✘";
        public static string symbolCheck = "✔";
        public static string symbolCheckSquare = "☑";
        public static string symbolDollar = "$";
        public static string symbolCent = "¢";
        public static string symbolCarrotRight = "‣";
        public static string symbolCarrotLeft = "◄";
        public static string symbolCarrotUp = "▲";
        public static string symbolCarrotDown = "▼";
        public static string symbolDash = "⁃";
        public static string symbolBulletClosed = "⦿";
        public static string symbolBulletOpen = "⦾";
        public static string symbolHeartClosed = "♥";
        public static string symbolHeartOpen = "♡";
        public static string symbolStarClosed = "★";
        public static string symbolStarOpen = "☆";
        public static string symbolArrowUp = "↑";
        public static string symbolArrowDown = "↓";
        public static string symbolRandom = "↬";
        public static string symbolMusic = "♫";
        public static string symbolImportant = "‼";
        public static string symbolCircleArrow = "➲";
        public static string symbolOneCircleOpen = "➀";
        public static string symbolPneCircleClosed = "➊";
        public static string symbolTwoCircleOpen = "➁";
        public static string symbolTwoCircleClosed = "➋";
        public static string symbolThreeCircleOpen = "➂";
        public static string symbolThreeCircleClosed = "➌";
        public static string symbolFourCircleOpen = "➃";
        public static string symbolFourCircleClosed = "➍";
        public static string symbolFiveCircleOpen = "➄";
        public static string symbolFiveCircleClosed = "➎";
        public static string symbolSixCircleOpen = "➅";
        public static string symbolSixCircleClosed = "➏";
        public static string symbolSevenCircleOpen = "➆";
        public static string symbolSevenCircleClosed = "➐";
        public static string symbolEightCircleOpen = "➇";
        public static string symbolEightCircleClosed = "➑";
        public static string symbolNineCircleOpen = "➈";
        public static string symbolNineCircleClosed = "➒";
        public static string symbolArrowCircleRight = "↺";
        public static string symbolArrowCircleLeft = "↻";
        public static string symbolPlusCircle = "⊕";
        public static string symbolMinusCircle = "⊖";
        public static string symbolMultiplyCircle = "⊗";
        public static string symbolDivideCircle = "⊘";
        public static string symbolEqualCircle = "⊜";
        public static string symbolRecycle = "♻";
        public static string symbolWww = "🌎";
        public static string symbolCircleOpen = "○";
    }
}