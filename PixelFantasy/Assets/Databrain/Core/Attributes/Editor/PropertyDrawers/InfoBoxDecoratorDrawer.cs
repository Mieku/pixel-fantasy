/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI()
        {
            InfoBoxAttribute _attribute = (attribute as InfoBoxAttribute);

            var _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;

            DatabrainHelpers.SetBorder(_root, 1, Color.black);

            var _icon = new VisualElement();
            DatabrainHelpers.SetMargin(_icon, 5, 5, 5, 5); 
            _icon.style.width = 40;
            _icon.style.height = 40;
          
           
            switch (_attribute.type)
            {
                case InfoBoxType.Normal:
                    _icon.style.backgroundImage = DatabrainHelpers.LoadIcon("info");
                    break;
                case InfoBoxType.Warning:
                    _icon.style.backgroundImage = DatabrainHelpers.LoadIcon("warning");
                    break;
                case InfoBoxType.Error:
                    _icon.style.backgroundImage = DatabrainHelpers.LoadIcon("error");
                    break;
            }


            var _text = new Label();
            _text.text = _attribute.text;
            _text.style.whiteSpace = WhiteSpace.Normal;
            _text.style.unityTextAlign = TextAnchor.MiddleLeft;

            _root.Add(_icon);
            _root.Add(_text);


            return _root;
        }
    }
}
#endif