/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEditor;
using System.Linq;

namespace Databrain.UI.Elements
{
    public static class DatabrainUIElements
    {
        public static void Grid(VisualElement _parent, List<VisualElement> _items, Vector2Int _gridSize)
        {

            var _parentWidth = _parent.resolvedStyle.width - _gridSize.x;
            var _scrollView = new ScrollView();

            // Create row and add first row to scrollview
            var _row = CreateGridRow();
            _scrollView.Add(_row);

            for (int i = 0; i < _items.Count; i++)
            {
                if (_parentWidth - _gridSize.x >= 0)
                {
                    _parentWidth -= _gridSize.x;

                    _row.Add(_items[i]);
                }
                else
                {
                    // next row
                    _row = CreateGridRow();
                    _scrollView.Add(_row);

                    _parentWidth = _parent.resolvedStyle.width - _gridSize.x;
                }
            }

            _parent.Add(_scrollView);


        }

        static VisualElement CreateGridRow()
        {
            var _row = new VisualElement();
            _row.name = "row";
            _row.style.flexDirection = FlexDirection.Row;
            _row.style.alignSelf = Align.FlexStart;

            return _row;
        }

        public static VisualElement EnableAddonElement(string _title, string _description, string _defineSymbol, string _iconName = "")
        {
            var _root = new VisualElement();
            var _panel = new VisualElement();
            _panel.style.paddingBottom = 10;
            _panel.style.paddingTop = 10;
            _panel.style.paddingLeft = 10;
            _panel.style.paddingRight = 10;

            _panel.style.flexDirection = FlexDirection.Row;

            DatabrainHelpers.SetBorder(_panel, 1);

            var _header = new VisualElement();
            _header.style.flexDirection = FlexDirection.Row;

            var _icon = new VisualElement();
            if (!string.IsNullOrEmpty(_iconName))
            {
                _icon.style.backgroundImage = DatabrainHelpers.LoadResourceTexture(_iconName + ".png");
                _icon.style.width = 50;
                _icon.style.height = 50;
            }

            var _textContainer = new VisualElement();
            _textContainer.style.flexGrow = 1;

            var _titleElement = new Label();
            _titleElement.text = _title;
            _titleElement.style.fontSize = 14;
            _titleElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleElement.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            DatabrainHelpers.SetMargin(_titleElement, 5, 5, 5, 0);


            var _labelDescription = new Label();
            _labelDescription.text = _description;
            _labelDescription.style.whiteSpace = WhiteSpace.Normal;
            _labelDescription.style.unityTextAlign = TextAnchor.MiddleCenter;
            _labelDescription.style.fontSize = 10;
            //_techtreeLocalizationLabel.style.paddingBottom = 10;
            DatabrainHelpers.SetMargin(_labelDescription, 5, 5, 0, 5);

            _textContainer.Add(_titleElement);
            _textContainer.Add(_labelDescription);

            _header.Add(_icon);
            _header.Add(_textContainer);


            var _enableAddon = new Button();
            _enableAddon.text = "Enable";
            _enableAddon.style.height = 40;
            _enableAddon.style.width = 150;

            _enableAddon.RegisterCallback<ClickEvent>(click =>
            {
                DatabrainHelpers.SetScriptingDefineSymbols(new string[] { _defineSymbol });
            });

            var _space = new VisualElement();
            _space.style.flexDirection = FlexDirection.Row;
            _space.style.flexGrow = 1;


            _panel.Add(_header);
            //_panel.Add(_labelDescription);
            _panel.Add(_space);
            _panel.Add(_enableAddon);

            _root.Add(_panel);

            return _root;
        }

        public static VisualElement SmallButton(Texture2D _icon, Color _color)
    {
        var _newButton = new VisualElement();
        _newButton.style.alignContent = Align.Center;
        _newButton.style.alignItems = Align.Center;
        _newButton.style.alignSelf = Align.FlexStart;
        _newButton.style.flexDirection = FlexDirection.Row;
        _newButton.style.justifyContent = new StyleEnum<Justify>(Justify.Center);
        _newButton.style.borderRightWidth = 1;
        _newButton.style.borderRightColor = DatabrainHelpers.colorDarkGrey;

        DatabrainHelpers.SetMargin(_newButton, 4, 0, 0, 0);

        _newButton.RegisterCallback<MouseOverEvent>(evt => 
        {
            _newButton.style.backgroundColor = Color.grey;
        });
        _newButton.RegisterCallback<MouseLeaveEvent>(evt => 
        {
            _newButton.style.backgroundColor = new Color(0,0,0,0);
        });

        var _iconElement = new VisualElement();
        _iconElement.style.width = 14;
        _iconElement.style.height = 14;
        // _iconElement.style.marginTop = 5;
        _iconElement.style.backgroundImage = _icon;
        _iconElement.style.unityBackgroundImageTintColor = _color;
        _iconElement.style.backgroundPositionX = new StyleBackgroundPosition(StyleKeyword.Auto);
        _iconElement.style.backgroundPositionY = new StyleBackgroundPosition(StyleKeyword.Auto);
        _iconElement.style.alignSelf = Align.Center;
       
        _newButton.style.width = 25;
        _newButton.style.height = 22;
        
        _newButton.Add(_iconElement);

        return _newButton;
    }

        //public static VisualElement ReferenceFromOtherDataLibrary(System.Type _dataObjectType) //out DataObject otherDataObjectReference)
        //{
        //    var _root = new VisualElement();

        //    string[] guids = AssetDatabase.FindAssets("t:" + typeof(DataLibrary).Name);

        //    Debug.Log(guids.Length);

        //    DataLibrary[] a = new DataLibrary[guids.Length];
        //    for (int i = 0; i < guids.Length; i++) //probably could get optimized
        //    {
        //        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
        //        a[i] = (DataLibrary)AssetDatabase.LoadAssetAtPath(path, typeof(DataLibrary));
        //    }

        //    var _ls = a.Select(x => x.name).ToList();
        //    var _dataLibraryDropdown = new DropdownField(_ls, 0);

        //    _dataLibraryDropdown.RegisterValueChangedCallback(x =>
        //    {
        //        if (x.newValue != x.previousValue)
        //        {

        //        }
        //    });


        //    _root.Add(_dataLibraryDropdown);


        //    return _root;
        //}
    }
}
#endif