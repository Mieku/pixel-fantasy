/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public class HorizontalLineDecoratorDrawer : DecoratorDrawer
    {

        public override VisualElement CreatePropertyGUI()
        {
            HorizontalLineAttribute _attribute = (attribute as HorizontalLineAttribute);
            
            var _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;

            var _line = new VisualElement();
            _line.style.flexGrow = 1;
            _line.style.height = _attribute.height;
            _line.style.backgroundColor = _attribute.color.GetColor();
            _line.style.marginBottom = 2;
            _line.style.marginTop = 2;

            _root.Add(_line);

            return _root;
        }
    }
}
#endif