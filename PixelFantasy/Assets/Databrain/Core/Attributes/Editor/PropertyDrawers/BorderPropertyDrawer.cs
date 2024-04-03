/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Helpers;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(BorderAttribute))]
    public class BorderPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

            DatabrainHelpers.SetPadding(_root, 2, 2, 2, 2);
            DatabrainHelpers.SetBorder(_root, (attribute as BorderAttribute).borderWidth, (attribute as BorderAttribute).color);

            var _prop = new PropertyField();
            _prop.BindProperty(property);
            _prop.label = property.displayName;

            _root.Add(_prop);

            return _root;
        }
    }
}
#endif