/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using System.Linq;
using System.Text.RegularExpressions;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(SceneAttribute))]
	public class ScenePropertyDrawer : PropertyDrawer
    {
        private const string SceneListItem = "{0} ({1})";
        private const string ScenePattern = @".+\/(.+)\.unity";
        private const string TypeWarningMessage = "{0} must be an int or a string";
        private const string BuildSettingsWarningMessage = "No scenes in the build settings";


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

            string[] scenes = GetScenes();
            bool anySceneInBuildSettings = scenes.Length > 0;
            if (!anySceneInBuildSettings)
            {
                string _message = string.Format(BuildSettingsWarningMessage, property.name);

                var _label = new Label();
                _label.text = _message;

                _root.Add(_label);
            }
            else
            {
                string[] sceneOptions = GetSceneOptions(scenes);
                
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                        _root.Add(DrawPropertyForString(property, scenes, sceneOptions));
                        break;
                    case SerializedPropertyType.Integer:
                        _root.Add(DrawPropertyForInt(property, sceneOptions));
                        break;
                    default:
                        string _message = string.Format(TypeWarningMessage, property.name);
                        var _label = new Label();
                        _label.text = _message;
                        _root.Add(_label);
                        break;
                }
            }


            return _root;
        }

     //   public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
	    //{
	   
     //       EditorGUI.BeginProperty(rect, label, property);

     //       string[] scenes = GetScenes();
     //       bool anySceneInBuildSettings = scenes.Length > 0;
     //       if (!anySceneInBuildSettings)
     //       {
     //       	string message = string.Format(BuildSettingsWarningMessage, property.name);
	    //        EditorGUI.LabelField(rect, message);
     //           return;
     //       }

     //       string[] sceneOptions = GetSceneOptions(scenes);
     //       switch (property.propertyType)
     //       {
     //           case SerializedPropertyType.String:
     //               DrawPropertyForString(rect, property, label, scenes, sceneOptions);
     //               break;
     //           case SerializedPropertyType.Integer:
     //               DrawPropertyForInt(rect, property, label, sceneOptions);
     //               break;
     //           default:
     //               string message = string.Format(TypeWarningMessage, property.name);
	    //            EditorGUI.LabelField(rect, message);
     //               break;
     //       }

     //       EditorGUI.EndProperty();
     //   }

        private string[] GetScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => Regex.Match(scene.path, ScenePattern).Groups[1].Value)
                .ToArray();
        }

        private string[] GetSceneOptions(string[] scenes)
        {
            return scenes.Select((s, i) => string.Format(SceneListItem, s, i)).ToArray();
        }

        private DropdownField DrawPropertyForString(SerializedProperty property, string[] scenes, string[] sceneOptions)
        {
            int index = IndexOf(scenes, property.stringValue);

            var _dropdown = new DropdownField(sceneOptions.ToList(), index);
            _dropdown.label = property.displayName;
            _dropdown.BindProperty(property);
            _dropdown.RegisterValueChangedCallback(x =>
            {
                if (!property.stringValue.Equals(x.newValue, StringComparison.Ordinal))
                {
                    property.stringValue = scenes[_dropdown.index];
                }
            });

            return _dropdown;
        }

        private DropdownField DrawPropertyForInt(SerializedProperty property, string[] sceneOptions)
        {
            int index = property.intValue;

            var _dropdown = new DropdownField(sceneOptions.ToList(), index);
            _dropdown.label = property.displayName;
            _dropdown.BindProperty(property);
            _dropdown.RegisterValueChangedCallback(x =>
            {
                if (property.intValue != _dropdown.index)
                {
                    property.intValue = _dropdown.index;
                }
            });

            return _dropdown;
        }

        private static int IndexOf(string[] scenes, string scene)
        {
            var index = Array.IndexOf(scenes, scene);
            return Mathf.Clamp(index, 0, scenes.Length - 1);
        }
    }
}
#endif