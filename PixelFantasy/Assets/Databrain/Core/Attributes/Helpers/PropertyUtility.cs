/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Databrain.Attributes
{
    public static class PropertyUtility
    {
        public static SerializedProperty GetParent(this SerializedProperty aProperty)
        {
            var path = aProperty.propertyPath;
            int i = path.LastIndexOf('.');
            if (i < 0)
                return null;
            return aProperty.serializedObject.FindProperty(path.Substring(0, i));
        }
        public static SerializedProperty FindSiblingProperty(this SerializedProperty aProperty, string aPath)
        {
            var parent = aProperty.GetParent();
            if (parent == null)
                return aProperty.serializedObject.FindProperty(aPath);
            return parent.FindPropertyRelative(aPath);
        }


        public static bool HasExposeToInspector(Editor inspector)
        {
            if (inspector == null)
                return false;

            List<SerializedProperty> _properties = new List<SerializedProperty>();


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


            foreach (var property in _properties)
            {
                Type _iteratorType = property.serializedObject.targetObject.GetType();
                //Debug.Log(_iteratorType.Name + " " + _iteratorType.BaseType.Name);

                //Debug.Log("BASE: " + _iteratorType.BaseType.Name + " " + property.propertyPath);

                var _baseClassFieldInfo = _iteratorType.BaseType.GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (_baseClassFieldInfo != null)
                {
                    
                    var _exp = _baseClassFieldInfo.GetCustomAttribute(typeof(ExposeToInspector), true);
                    if (_exp != null)
                    {
                        return true;
                    }
                }


                FieldInfo _fieldInfo = _iteratorType.GetField(property.propertyPath);
                if (_fieldInfo != null)
                {
                    var _exposeToInspectorAttribute = _fieldInfo.GetCustomAttribute(typeof(ExposeToInspector), true);
                    if (_exposeToInspectorAttribute != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            Type _iteratorType = property.serializedObject.targetObject.GetType();
            FieldInfo fieldInfo = _iteratorType.GetField(property.propertyPath);
            if (fieldInfo == null)
            {
                return null;
            }

            T[] attributes = (T[])fieldInfo.GetCustomAttributes(typeof(T), true);

            return (attributes.Length > 0) ? attributes[0] : null; ;
        }

        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            string path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf("["));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            Type type = source.GetType();

            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }
    }
}
#endif