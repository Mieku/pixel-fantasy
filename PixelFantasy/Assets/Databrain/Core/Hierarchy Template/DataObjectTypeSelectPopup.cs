/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using Databrain.Helpers;
using Databrain;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace Databrain
{
    public class DataObjectTypeSelectPopup : PopupWindowContent
    {
        DatabrainHierarchyTemplate.DatabrainTypes type;
        Action callback;
        string searchString;
        ScrollView scrollView;
        static Vector2 position;


        public static void ShowPanel(Vector2 _pos, DataObjectTypeSelectPopup _panel)
        {
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 500);
        }

        public override void OnOpen()
        {
            var _root = editorWindow.rootVisualElement;

            var _header = new VisualElement();
            _header.style.flexDirection = FlexDirection.Row;
            _header.style.flexGrow = 1;
            _header.style.minHeight = 20;
            _header.style.maxHeight = 20;
            DatabrainHelpers.SetMargin(_header, 5, 5, 5, 5);

            var _searchIcon = new VisualElement();
            _searchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("search");
            _searchIcon.style.minWidth = 20;
            _searchIcon.style.minHeight = 20;


            var _searchField = new TextField();
            _searchField.style.flexGrow = 1;

            _searchField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (evt.newValue != evt.previousValue)
                {
                    searchString = evt.newValue;
                    BuildList();
                }
            });

            _header.Add(_searchIcon);
            _header.Add(_searchField);

            scrollView = new ScrollView();

            BuildList();

            _root.Add(_header);
            _root.Add(scrollView);


        }

        void BuildList()
        {
            scrollView.Clear();

            var _allTypes = TypeCache.GetTypesDerivedFrom(typeof(DataObject));


            for (int i = 0; i < _allTypes.Count; i++)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    if (!_allTypes[i].Name.ToLower().Contains(searchString.ToLower()))
                    {
                        continue;
                    }
                }
                var _index = i;

                var _selectButton = new Button();
                _selectButton.text = _allTypes[i].Name;
                _selectButton.tooltip = "Namespace: " + _allTypes[i].Namespace + "\n" + "Base type: " + _allTypes[i].BaseType;
                _selectButton.RegisterCallback<ClickEvent>(evt =>
                {
                    type.type = _allTypes[_index].Name;
                    type.assemblyQualifiedTypeName = _allTypes[_index].AssemblyQualifiedName;

                    callback?.Invoke();

                    editorWindow.Close();
                });

                scrollView.Add(_selectButton);
            }

        }

        public override void OnClose()
        {

        }

        public override void OnGUI(Rect rect) { }

        public DataObjectTypeSelectPopup(DatabrainHierarchyTemplate.DatabrainTypes _type, Action _callback) //, Action<Type, SerializedProperty> _action)
        {
            type = _type;
            callback = _callback;
        }
    } 
}
#endif