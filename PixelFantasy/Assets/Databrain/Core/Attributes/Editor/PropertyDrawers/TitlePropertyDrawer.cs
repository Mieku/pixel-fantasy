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
using Databrain.Helpers;

namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(TitleAttribute))]
	public class TitlePropertyDrawer : PropertyDrawer 
	{

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			TitleAttribute _attribute = (attribute as TitleAttribute);

			var _root = new VisualElement();

			var _labelContainer = new VisualElement();
			if (_attribute.borderColor != DatabrainColor.Clear.GetColor())
			{
				DatabrainHelpers.SetBorder(_labelContainer, 2, _attribute.borderColor);
			}
			DatabrainHelpers.SetPadding(_labelContainer, 3, 3, 0, 0 );

            var _label = new Label();
			_label.text = (attribute as TitleAttribute).title;
			_label.style.fontSize = 14;
			_label.style.unityFontStyleAndWeight = FontStyle.Bold;
			_label.style.marginBottom = 5;
            _label.style.marginTop = 5;
			_label.style.color = _attribute.textColor;
          

            var _prop = new PropertyField();
			_prop.BindProperty(property);

			_labelContainer.Add(_label);
            _root.Add(_labelContainer);
			_root.Add(_prop);

			return _root;
		}
	}
}
#endif