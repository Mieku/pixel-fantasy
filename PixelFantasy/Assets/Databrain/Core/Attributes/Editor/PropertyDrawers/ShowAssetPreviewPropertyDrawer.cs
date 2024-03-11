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
    [CustomPropertyDrawer(typeof(ShowAssetPreviewAttribute))]
	public class ShowAssetPreviewPropertyDrawer : PropertyDrawer
    {
        private VisualElement preview;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ShowAssetPreviewAttribute _attribute = (attribute as ShowAssetPreviewAttribute);

            var _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;

            DatabrainHelpers.SetPadding(_root, 4, 4, 4, 4);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var _label = new Label();
                _label.text = property.displayName;

                preview = new VisualElement();

                preview.style.alignSelf = Align.FlexEnd;

                Texture2D previewTexture = GetAssetPreview(property);
                if (previewTexture != null)
                {
                    preview.style.backgroundImage = previewTexture;
                    preview.style.width = _attribute.width;
                    preview.style.height = _attribute.height;
                }

            
                var _field = new PropertyField();
                _field.BindProperty(property);
                _field.label = "";
                _field.style.flexGrow = 1;
                _field.RegisterValueChangeCallback(x =>
                {
                    Texture2D previewTexture = GetAssetPreview(property);
                    if (previewTexture != null)
                    {
                        preview.style.backgroundImage = previewTexture;
                        preview.style.width = _attribute.width;
                        preview.style.height = _attribute.height;
                    }
                });

                var _left = new VisualElement();

                var _space = new VisualElement();
                _space.style.width = 65;

                _left.Add(preview);
                _left.Add(_field);

                _root.Add(_label);
                _root.Add(_space);
                _root.Add(_left);
            }


  
            return _root;
        }



        private Texture2D GetAssetPreview(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue != null)
                {
                    Texture2D previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
                    return previewTexture;
                }

                return null;
            }

            return null;
        }

       
    }
}
#endif