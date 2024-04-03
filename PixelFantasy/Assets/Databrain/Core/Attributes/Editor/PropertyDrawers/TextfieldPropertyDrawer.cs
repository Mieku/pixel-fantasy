/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(TextfieldAttribute))]
	public class TextfieldPropertyDrawer : PropertyDrawer
	{

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var _root = new VisualElement();

			var _textField = new UnityEngine.UIElements.TextField();
			_textField.multiline = true;
			_textField.style.minHeight = EditorGUIUtility.singleLineHeight * 3;
			_textField.BindProperty(property);
			_textField.label = property.displayName;

			_root.Add(_textField);

			return _root;
		}
	}
}
#endif