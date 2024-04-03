/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Attributes;
using Databrain.Helpers;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain
{
    [CustomEditor(typeof(DatabrainThemeTemplate))]
    public class DatabrainThemeTemplateEditor : Editor
    {
        DatabrainThemeTemplate template;
        VisualElement root;
        Texture2D icon;
        Texture2D upIcon;
        Texture2D downIcon;
        Texture2D removeIcon;
        Texture2D namespaceIcon;

        public void OnEnable()
        {
            template = (DatabrainThemeTemplate)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            Draw();

            return root;
        }

        void Draw()
        {
            root.Clear();

            if (icon == null)
            {
                icon = DatabrainHelpers.LoadLogoIcon();
            }

            if (upIcon == null)
            {
                upIcon = DatabrainHelpers.LoadIcon("up");
            }

            if (downIcon == null)
            {
                downIcon = DatabrainHelpers.LoadIcon("down");
            }

            if (removeIcon == null)
            {
                removeIcon = DatabrainHelpers.LoadIcon("remove");
            }

            if (namespaceIcon == null)
            {
                namespaceIcon = DatabrainHelpers.LoadIcon("namespaceIcon");
            }

            var _header = new VisualElement();
            _header.style.flexDirection = FlexDirection.Row;
            _header.style.flexGrow = 1;
            DatabrainHelpers.SetMargin(_header, 5, 5, 5, 5);
            DatabrainHelpers.SetPadding(_header, 10, 10, 10, 10);
            DatabrainHelpers.SetBorder(_header, 1, DatabrainColor.DarkGrey.GetColor());

            var _icon = new VisualElement();
            _icon.style.backgroundImage = icon;
            _icon.style.width = 50;
            _icon.style.height = 50;
            _icon.style.minWidth = 50;
            _icon.style.minHeight = 50;
            _icon.style.marginRight = 10;

            var _description = new Label();
            _description.style.whiteSpace = WhiteSpace.Normal;
            _description.text = "Here you can create a Databrain Theme template.";
            _description.style.alignSelf = Align.Center;
            _description.style.marginRight = 55;

            _header.Add(_icon);
            _header.Add(_description);

            root.Add(_header);

            DrawElement();

            EditorUtility.SetDirty(template);
        }

        void DrawElement()
        {

            var _childElement = new VisualElement();
            _childElement.style.marginTop = 4;
            _childElement.style.backgroundColor = DatabrainColor.Grey.GetColor();
            DatabrainHelpers.SetBorder(_childElement, 1, DatabrainColor.DarkGrey.GetColor());

            var _toolbar = new Toolbar();
            _toolbar.style.maxHeight = 20;
            _toolbar.style.minHeight = 20;
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.flexGrow = 1;

            var _toolbarLabel = new Label();
            _toolbarLabel.text = template.name;
            _toolbarLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _toolbarLabel.style.flexGrow = 1;
            _toolbarLabel.style.marginLeft = 4;
            _toolbarLabel.style.unityTextAlign = TextAnchor.MiddleLeft;


            _toolbar.Add(_toolbarLabel);
            _childElement.Add(_toolbar);

            var _hierarchyTemplate = new PropertyField();
            _hierarchyTemplate.style.flexGrow = 1;
            _hierarchyTemplate.style.marginTop = 8;
            _hierarchyTemplate.style.marginLeft = 14;
            _hierarchyTemplate.style.marginRight = 14;
            _hierarchyTemplate.style.marginBottom = 8;
            _hierarchyTemplate.BindProperty(this.serializedObject.FindProperty("serializedGroup"));
            _hierarchyTemplate.label = "Theme";
            _childElement.Add(_hierarchyTemplate);

            root.Add(_childElement);
        }
    }
}
#endif