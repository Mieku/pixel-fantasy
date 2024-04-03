/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(SliderAttribute))]
	public class SliderPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();
            _root.style.flexGrow = 1;
            _root.style.flexDirection = FlexDirection.Row;

            if (property.propertyType == SerializedPropertyType.Integer ||
                property.propertyType == SerializedPropertyType.Float)
            {
                SliderAttribute _minMaxSliderAttribute = (SliderAttribute)attribute;

                var _label = new Label();
                _label.style.unityTextAlign = TextAnchor.MiddleCenter;
                _label.style.marginLeft = 5;
                _label.style.marginRight = 5;

                if (property.propertyType == SerializedPropertyType.Float)
                {
                    _label.text = property.floatValue.ToString();
                }
                else
                {
                    _label.text = property.intValue.ToString();
                }

                
                var _slider = new Slider(_minMaxSliderAttribute.MinValue, _minMaxSliderAttribute.MaxValue);
                _slider.label = property.displayName;
                _slider.style.flexGrow = 1;
                _slider.RegisterValueChangedCallback(x =>
                {
                    _label.text = x.newValue.ToString();
                });

                _slider.BindProperty(property);

               
                _root.Add(_slider);
                _root.Add(_label);

            }

           return _root;
        }
    }
}
#endif