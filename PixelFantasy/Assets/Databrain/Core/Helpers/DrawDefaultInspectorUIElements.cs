/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Attributes;


namespace Databrain.Helpers
{
    public class DrawDefaultInspectorUIElements
    {
        public static Type selectedDataType;

        public static VisualElement DrawInspector(Editor inspector, Type _selectedDataType, bool _isDropdownDataViewer)
        {
            var _root = new VisualElement();

            selectedDataType = _selectedDataType;
            List<SerializedProperty> _properties = new List<SerializedProperty>();

            // Get All properties and group them by foldout attributes or not
            using (var iterator = inspector.serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        _properties.Add(inspector.serializedObject.FindProperty(iterator.name));
                    }
                    while (iterator.NextVisible(false));
                }
            }

            var _foldoutGroups = _properties.Where(p => PropertyUtility.GetAttribute<FoldoutAttribute>(p) != null)
                .GroupBy(p => PropertyUtility.GetAttribute<FoldoutAttribute>(p).title);

            var _nonGrouped = _properties.Where(p => PropertyUtility.GetAttribute<FoldoutAttribute>(p) == null);

            // Non grouped
            foreach (var property in _nonGrouped)
            {
                if (!SkipProperty(property))
                {
                    if (selectedDataType != null)
                    {
                        Type _iteratorType = property.serializedObject.targetObject.GetType();
                        Attribute _showAssetAttribute = null;
                        FieldInfo _fieldInfo = _iteratorType.GetField(property.propertyPath);
                        if (_fieldInfo != null)
                        {
                            _showAssetAttribute = _fieldInfo.GetCustomAttribute(typeof(ShowAssetPreviewAttribute), true);
                        }


                        if (_isDropdownDataViewer)
                        {
                            if (ShowInDropdownViewer(property))
                            {
                                // Make sure we do not allow scene object references
                               

                                if (IsPropertyGameObject(property) && _showAssetAttribute == null)
                                {
                                    var _objField = new ObjectField();
                                    _objField.objectType = typeof(GameObject);
                                    _objField.allowSceneObjects = false;
                                    _objField.label = property.displayName;
                                    _objField.BindProperty(property);
                                    _root.Add(_objField);
                                }
                                else
                                {
                                    var _propertyField = new PropertyField(property);
                                    _propertyField.BindProperty(property);
                                    _root.Add(_propertyField);
                                }
                            }
                        }
                        else
                        {
                            if (!IsHidden(property))
                            {
                                // Make sure we do not allow scene object references
                                if (IsPropertyGameObject(property) && _showAssetAttribute == null)
                                {
                                    var _objField = new ObjectField();
                                    _objField.objectType = typeof(GameObject);
                                    _objField.allowSceneObjects = false;
                                    _objField.label = property.displayName;
                                    _objField.BindProperty(property);
                                    _root.Add(_objField);
                                }
                                else
                                {
                                    var _propertyField = new PropertyField(property);
                                    _propertyField.BindProperty(property);
                                    _root.Add(_propertyField);

                                }
                            }
                        }
                    }
                }
            }


            // Grouped
            foreach (var group in _foldoutGroups)
            {
                var _foldout = new Foldout();
                _foldout.text = group.Key;

                foreach (var property in group)
                {
                    if (!SkipProperty(property))
                    {
                        if (selectedDataType != null)
                        {
                            if (_isDropdownDataViewer)
                            {
                                if (ShowInDropdownViewer(property))
                                {
                                    var _propertyField = new PropertyField(property);
                                    _propertyField.BindProperty(property);
                                    _foldout.Add(_propertyField);
                                }
                            }
                            else
                            {
                                if (!IsHidden(property))
                                {
                                    var _propertyField = new PropertyField(property);
                                    _propertyField.BindProperty(property);
                                    _foldout.Add(_propertyField);
                                }
                            }
                        }
                    }
                }

                _root.Add(_foldout);
            }

            inspector.serializedObject.ApplyModifiedProperties();        

            return _root;
        }


        public static void DrawIMGUIInspectorWithoutScriptField (Editor inspector, DatabrainEditorWindow databrainEditor)
		{
			
			if (inspector == null)
				return;
			if (inspector.serializedObject == null)
				return;

			EditorGUI.BeginChangeCheck();

			inspector.serializedObject.Update();


			SerializedProperty iterator = inspector.serializedObject.GetIterator();

			iterator.NextVisible(true);

			Type t = iterator.serializedObject.targetObject.GetType();


			while (iterator.NextVisible(false))
			{
				if (iterator.propertyPath != "guid" &&
                    iterator.propertyPath != "initialGuid" &&
					iterator.propertyPath != "runtimeIndexID" &&
                    iterator.propertyPath != "icon" &&
					iterator.propertyPath != "color" &&
					iterator.propertyPath != "title" &&
					iterator.propertyPath != "description" &&
					iterator.propertyPath != "skipRuntimeSerialization" &&
					iterator.propertyPath != "boxFoldout")
				{

					/////// CUSTOM ATTRIBUTES		
					FieldInfo f = null;
					Attribute _hideAttribute = null;
                 
                    f = t.GetField(iterator.propertyPath);
					if (f != null)
					{		
						_hideAttribute = f.GetCustomAttribute(typeof(HideAttribute), true);				
					}
					//////////////////////////////////////


					if (selectedDataType != null)
					{
						var _hideFieldsAttribute = selectedDataType.GetCustomAttribute(typeof(DataObjectHideAllFieldsAttribute)) as DataObjectHideAllFieldsAttribute;
                        

                        if (_hideFieldsAttribute == null)
						{
							if (_hideAttribute == null)
							{
								EditorGUILayout.PropertyField(iterator, true);
							}	
						}
					}
				}
			}

			inspector.serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck() && databrainEditor != null)
			{
				databrainEditor.UpdateData();
			}
		}
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1

        public static void DrawInspectorWithOdin(Sirenix.OdinInspector.Editor.OdinEditor _odinEditor, DatabrainEditorWindow _databrainEditorWindow)
		{
			if (_odinEditor == null)
				return;
			if (_odinEditor.serializedObject == null)
				return;
			
			try
			{
				EditorGUI.BeginChangeCheck();
				_odinEditor.serializedObject.Update();

				_odinEditor.DrawDefaultInspector();

				_odinEditor.serializedObject.ApplyModifiedProperties();

				if (EditorGUI.EndChangeCheck() && _databrainEditorWindow != null)
				{
					_databrainEditorWindow.UpdateData();
				}
			}
			catch{}
		}
#endif

        static bool SkipProperty(SerializedProperty _property)
        {
            if (_property.propertyPath != "guid" &&
                    _property.propertyPath != "initialGuid" &&
                    _property.propertyPath != "runtimeIndexID" &&
                    _property.propertyPath != "icon" &&
                    _property.propertyPath != "color" &&
                    _property.propertyPath != "title" &&
                    _property.propertyPath != "description" &&
                    _property.propertyPath != "skipRuntimeSerialization" &&
                    _property.propertyPath != "m_Script" &&
                    _property.propertyPath != "boxFoldout")
            {
                return false;
            }

            return true;
        }

        static bool IsPropertyGameObject(SerializedProperty _property)
        {
            if (_property.type.Contains("PPtr<$GameObject>"))
            {
                return true;
            }
            return false;
        }

        static bool IsHidden(SerializedProperty _property)
        {
            Type _iteratorType = _property.serializedObject.targetObject.GetType();
            Attribute _hideAttribute = null;
            FieldInfo _fieldInfo = _iteratorType.GetField(_property.propertyPath);
            if (_fieldInfo != null)
            {
                _hideAttribute = _fieldInfo.GetCustomAttribute(typeof(HideAttribute), true);
            }

            DataObjectHideAllFieldsAttribute _hideAllFieldsAttribute = selectedDataType.GetCustomAttribute(typeof(DataObjectHideAllFieldsAttribute)) as DataObjectHideAllFieldsAttribute;

            if (_hideAllFieldsAttribute == null && _hideAttribute == null)
            {
                return false;
            }

            return true;
        }

        static bool ShowInDropdownViewer(SerializedProperty _property)
        {
            Type _iteratorType = _property.serializedObject.targetObject.GetType();

            Attribute _attribute = null;
            Attribute _hideAttribute = null;

            FieldInfo _baseFieldInfo = _iteratorType.BaseType.GetField(_property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            FieldInfo _fieldInfo = _iteratorType.GetField(_property.propertyPath);

            if (_baseFieldInfo != null)
            {
                _attribute = _baseFieldInfo.GetCustomAttribute(typeof(ExposeToInspector), true);
            }

            if (_fieldInfo != null)
            {
                _attribute = _fieldInfo.GetCustomAttribute(typeof(ExposeToInspector), true);
                _hideAttribute = _fieldInfo.GetCustomAttribute(typeof(HideAttribute), true);
            }

            if (_attribute == null)
            {
                return false;
            }
            else
            {
                if (_hideAttribute == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        
    }
}
#endif