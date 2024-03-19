/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(CopyGuidAttribute))]
	public class CopyGuidPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var _root = new VisualElement();
			_root.style.flexDirection = FlexDirection.Row;
			_root.style.flexGrow = 1;

			var _buttonCopy = new Button();
			_buttonCopy.text = "copy";
			_buttonCopy.RegisterCallback<ClickEvent>(x =>
			{
				GUIUtility.systemCopyBuffer = property.stringValue;
			});


			var _property = new PropertyField();
            _property.style.flexGrow = 1;
            _property.SetEnabled(false);
			_property.BindProperty(property);


			_root.Add(_property);
			_root.Add(_buttonCopy);


			return _root;

		}
	}
}
#endif
