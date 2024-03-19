/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
	public class LayerPropertyDrawer : PropertyDrawer
    {
        private const string TypeWarningMessage = "{0} must be an int or a string";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

           
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    _root.Add(DrawPropertyForString(property, GetLayers()));
                    break;
                case SerializedPropertyType.Integer:
                    _root.Add(DrawPropertyForInt(property, GetLayers()));
                    break;
                default:
                    string _message = string.Format(TypeWarningMessage, property.name);
                    var _label = new Label();
                    _label.text = _message;
                    _root.Add(_label);
                    break;
            }

            _root.Bind(property.serializedObject);

            return _root;
        }


        private string[] GetLayers()
        {
            return UnityEditorInternal.InternalEditorUtility.layers;
        }

        private DropdownField DrawPropertyForString(SerializedProperty property, string[] layers)
        {
            int index = IndexOf(layers, property.stringValue);

            var _dropdown = new DropdownField(layers.ToList(), index);
            _dropdown.BindProperty(property);
            _dropdown.label = property.displayName;
            _dropdown.RegisterValueChangedCallback(x =>
            {
                if (x.newValue != x.previousValue && _dropdown.index > -1)
                {
                    Debug.Log(_dropdown.index);
                    string newLayer = layers[_dropdown.index];

                    if (!property.stringValue.Equals(newLayer, StringComparison.Ordinal))
                    {
                        property.stringValue = layers[_dropdown.index];
                    }
                }
            });
           

            return _dropdown;
        }

        private DropdownField DrawPropertyForInt(SerializedProperty property, string[] layers)
        {
            int index = 0;
            string layerName = LayerMask.LayerToName(property.intValue);
            for (int i = 0; i < layers.Length; i++)
            {
                if (layerName.Equals(layers[i], StringComparison.Ordinal))
                {
                    index = i;
                    break;
                }
            }

            var _dropdown = new DropdownField(layers.ToList(), index);
            _dropdown.label = property.displayName;
            _dropdown.RegisterValueChangedCallback(x =>
            {
                if (_dropdown.index > -1)
                {
                    string newLayerName = layers[_dropdown.index];
                    int newLayerNumber = LayerMask.NameToLayer(newLayerName);

                    if (property.intValue != newLayerNumber)
                    {
                        property.intValue = newLayerNumber;

                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            });

            return _dropdown;
        }

        private static int IndexOf(string[] layers, string layer)
        {
            var index = Array.IndexOf(layers, layer);
            return Mathf.Clamp(index, 0, layers.Length - 1);
        }
    }
}
#endif