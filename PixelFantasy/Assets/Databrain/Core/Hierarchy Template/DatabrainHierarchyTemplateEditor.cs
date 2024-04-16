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
    [CustomEditor(typeof(DatabrainHierarchyTemplate))]
    public class DatabrainHierarchyTemplateEditor : Editor
    {
        DatabrainHierarchyTemplate template;
        VisualElement root;
        Texture2D icon;
        Texture2D upIcon;
        Texture2D downIcon;
        Texture2D removeIcon;
        Texture2D namespaceIcon;

        public void OnEnable()
        {
            template = (DatabrainHierarchyTemplate)target;
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
                icon = DatabrainHelpers.LoadHierarchyLogoIcon();
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
            _description.text = "Here you can create a Databrain hierarchy template by creating groups and adding databrain types to it. Types can also have nested sub-types.";
            _description.style.marginRight = 55;

            _header.Add(_icon);
            _header.Add(_description);

            root.Add(_header);




            if (template.rootDatabrainTypes != null)
            {
                var _addGroup = new Button();
                _addGroup.text = "Add Group";
                _addGroup.RegisterCallback<ClickEvent>(evt =>
                {
                    template.rootDatabrainTypes.subTypes.Add(new DatabrainHierarchyTemplate.DatabrainTypes());
             
                    Draw();
                });

                root.Add(_addGroup);

                var _childElement = new VisualElement();
                DatabrainHelpers.SetBorder(_childElement, 1);
                root.Add(_childElement);
                DrawElement(template.rootDatabrainTypes, template.rootDatabrainTypes, 0, 0, null);

            }


            EditorUtility.SetDirty(template);
        }

        void DrawElement(DatabrainHierarchyTemplate.DatabrainTypes nodeType, DatabrainHierarchyTemplate.DatabrainTypes parentType, int _depth, int _arrayIndex, Foldout _groupFoldout)
        {
            if (_depth == 0)
            {
                var _c = 0;
                _depth++;
                foreach (var child in nodeType.subTypes)
                {

                    DrawElement(child, nodeType, _depth, _c, null);
                    _c++;
                }
                return;
            }

            var _childElement = new VisualElement();
            _childElement.style.marginTop = 4;
            _childElement.style.backgroundColor = DatabrainColor.Grey.GetColor();
            DatabrainHelpers.SetBorder(_childElement, 1, DatabrainColor.DarkGrey.GetColor());
            _childElement.style.marginLeft = (10 * _depth);

            var _toolbar = new Toolbar();
            _toolbar.style.maxHeight = 20;
            _toolbar.style.minHeight = 20;
            _toolbar.style.flexDirection = FlexDirection.Row;
            _toolbar.style.flexGrow = 1;

            var _toolbarLabel = new Label();
            _toolbarLabel.text = nodeType.name;
            _toolbarLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _toolbarLabel.style.flexGrow = 1;
            _toolbarLabel.style.marginLeft = 4;
            _toolbarLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            var _upButton = new ToolbarButton();
            _upButton.style.width = 20;
            _upButton.style.backgroundImage = upIcon;
            _upButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (_arrayIndex > 0)
                {
                    var _item = parentType.subTypes[_arrayIndex];
                    parentType.subTypes.RemoveAt(_arrayIndex);
                    parentType.subTypes.Insert(_arrayIndex - 1, _item);

                    Draw();
                }
            });

            if (_arrayIndex <= 0)
            {
                _upButton.SetEnabled(false);
            }

            var _downButton = new ToolbarButton();
            _downButton.style.width = 20;
            _downButton.style.backgroundImage = downIcon;
            _downButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (_arrayIndex < parentType.subTypes.Count - 1)
                {
                    var _item = parentType.subTypes[_arrayIndex];
                    parentType.subTypes.RemoveAt(_arrayIndex);
                    parentType.subTypes.Insert(_arrayIndex + 1, _item);

                    Draw();
                }

            });

            if (_arrayIndex >= parentType.subTypes.Count - 1)
            {
                _downButton.SetEnabled(false);
            }

            var _removeButton = new ToolbarButton();
            _removeButton.style.width = 20;
            _removeButton.style.backgroundImage = removeIcon;
            _removeButton.RegisterCallback<ClickEvent>(evt =>
            {
                parentType.subTypes.RemoveAt(_arrayIndex);
                Draw();
            });

            if (_depth == 1)
            {
                var _namespaceIcon = new VisualElement();
                _namespaceIcon.style.backgroundImage = namespaceIcon;
                _namespaceIcon.style.width = 18;
                _namespaceIcon.style.height = 18;

           
                _toolbar.Add(_namespaceIcon);
            }

            _toolbar.Add(_toolbarLabel);
            _toolbar.Add(_upButton);
            _toolbar.Add(_downButton);
            _toolbar.Add(_removeButton);


            _childElement.Add(_toolbar);


            var _nameField = new TextField();
            _nameField.label = _depth == 1 ? "Group name" : "Type name";
            _nameField.value = nodeType.name;
            _nameField.RegisterValueChangedCallback(evt =>
            {
                nodeType.name = evt.newValue;
                EditorUtility.SetDirty(template);
            });
            _nameField.RegisterCallback<KeyUpEvent>(key =>
            {
                if (key.keyCode == KeyCode.Return)
                { 
                    Draw();                 
                }
            });

            _childElement.Add(_nameField);

            if (_depth > 1)
            {
                var _typeContainer = new VisualElement();
                _typeContainer.style.flexDirection = FlexDirection.Row;
                _typeContainer.style.flexGrow = 1;

                Texture2D _typeIcon = null;
                Color _typeIconColor = Color.white;
                var _t = System.Type.GetType(nodeType.assemblyQualifiedTypeName);
                if (_t != null)
                {
                    var _iconAttribute = _t.GetCustomAttribute<DataObjectIconAttribute>();
                    if (_iconAttribute != null)
                    {
                        _typeIconColor = _iconAttribute.iconColor;
                        _typeIcon = DatabrainHelpers.LoadIcon(_iconAttribute.iconPath);
                    }
                    else
                    {
                        _typeIcon = DatabrainHelpers.LoadIcon("typeIcon");
                    }

                }

                var _typeLabel = new Label();
                _typeLabel.style.marginLeft = 4;
                _typeLabel.text = "Type";

                var _typeFieldButton = new Button();
                _typeFieldButton.style.flexGrow = 1;
                _typeFieldButton.text = string.IsNullOrEmpty(nodeType.type) ? "Select Type" : nodeType.type; // nodeType.type;
                _typeFieldButton.RegisterCallback<ClickEvent>(evt =>
                {
                    var _panel = new DataObjectTypeSelectPopup(nodeType, Draw);
                    DataObjectTypeSelectPopup.ShowPanel(Event.current.mousePosition, _panel);
                });

                if (_typeIcon != null)
                {
                    var _typeIconElement = new VisualElement();
                    _typeIconElement.style.backgroundImage = _typeIcon;
                    _typeIconElement.style.unityBackgroundImageTintColor = new StyleColor(_typeIconColor);
                    _typeIconElement.style.width = 16;
                    _typeIconElement.style.height = 16;
                    _typeIconElement.style.marginLeft = 5;

                    _typeContainer.Add(_typeIconElement);
                }
                _typeContainer.Add(_typeLabel);
                _typeContainer.Add(_typeFieldButton);


                _childElement.Add(_typeContainer);
            }

            if (_depth == 1)
            {
                var _firstClass = new Toggle();
                _firstClass.label = "Is FirstClass type";
                _firstClass.value = nodeType.isFirstClassType;
                _firstClass.RegisterCallback<ChangeEvent<bool>>(evt => 
                {
                    if (evt.newValue != evt.previousValue)
                    {
                        nodeType.isFirstClassType = evt.newValue;
                        EditorUtility.SetDirty(template);
                    }
                });


                _childElement.Add(_firstClass);
            }

            var _addButton = new Button();
            _addButton.text = "Add Subtype";
            _addButton.style.marginBottom = 2;
            _addButton.RegisterCallback<ClickEvent>(evt =>
            {
                nodeType.subTypes.Add(new DatabrainHierarchyTemplate.DatabrainTypes());
                Draw();
            });

            
            _childElement.Add(_addButton);

            if (_groupFoldout != null)
            {
                _groupFoldout.Add(_childElement);
            }
            else
            {
                root.Add(_childElement);
            }

            var _childArray = 0;
            _depth++;


            Foldout _foldout = null;
            if (nodeType.subTypes.Count > 0)
            {
                _foldout = new Foldout();
                _foldout.style.backgroundColor = DatabrainColor.Indigo.GetColor();
                _foldout.RegisterValueChangedCallback(e =>
                {
                    nodeType.foldout = _foldout.value;
                    EditorUtility.SetDirty(template);
                });
                _foldout.value = nodeType.foldout;
                _foldout.text = "Sub-types";
                _foldout.style.backgroundColor = DatabrainColor.Grey.GetColor();

                _childElement.Add(_foldout);
            }
            foreach (var child in nodeType.subTypes)
            {
                DrawElement(child, nodeType, _depth, _childArray, _foldout);
                _childArray++;
            }
        }
    }
}
#endif