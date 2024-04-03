/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;

using Databrain.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.UI
{
    public class DataObjectSelectionPopup : PopupWindowContent
    {
        private DataLibrary dataLibrary;
        private SerializedProperty property;
        private Type dataType;
        //private Action onCloseCallback;
        private Action<int> updateSelectedIndex;
        private string searchInput = "";
        private VisualElement root;
        private ScrollView scrollView;
        private int searchIndex;
        private bool includeSubtypes;
        static Vector2 position;



        public DataObjectSelectionPopup(Type dataType, DataLibrary dataLibrary, SerializedProperty property, Action<int> updateSelectedIndex, bool includeSubtypes)
        {
            this.dataType = dataType;
            this.dataLibrary = dataLibrary;
            this.property = property;
            //this.onCloseCallback = onCloseCallback;
            this.includeSubtypes = includeSubtypes;
            this.updateSelectedIndex = updateSelectedIndex;
        }
        public static void ShowPanel(Vector2 _pos, DataObjectSelectionPopup _panel)
        {
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 350);
        }

        public override void OnGUI(Rect rect) { }

        public override void OnOpen()
        {
            searchIndex = -1;

            root = editorWindow.rootVisualElement;
            scrollView = new ScrollView();
            DatabrainHelpers.SetPadding(scrollView, 5, 5, 5, 5);

            var _searchContainer = new VisualElement();
            _searchContainer.style.flexDirection = FlexDirection.Row;
            //_searchContainer.style.flexGrow = 1;
            _searchContainer.style.height = 20;
            _searchContainer.style.minHeight = 20;
            DatabrainHelpers.SetMargin(_searchContainer, 4, 4, 4, 4);

            var _searchIcon = new VisualElement();
            _searchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("search");
            _searchIcon.style.width = 20;
            _searchIcon.style.minWidth = 20;
            _searchIcon.style.height = 20;

            var _searchField = new TextField();
            _searchField.style.maxHeight = 20;
            _searchField.style.flexGrow = 1;
            _searchField.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode == KeyCode.Return)
                {
                    if (!string.IsNullOrEmpty(searchInput) && searchIndex > -1)
                    {
                        var _availableObjects = dataLibrary.GetAllInitialDataObjectsByType(dataType, includeSubtypes);
                        property.objectReferenceValue = _availableObjects[searchIndex];

                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();

                        editorWindow.Close();
                    }
                }
            });

            _searchField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue != x.previousValue)
                {
                    searchIndex = -1;
                    searchInput = x.newValue;
                    BuildList();
                }
            });

            var _cancelSearch = DatabrainHelpers.DatabrainButton("");
            _cancelSearch.style.backgroundColor = new Color(0, 0, 0, 0);
            _cancelSearch.RegisterCallback<ClickEvent>(e =>
            {
                searchInput = "";
                _searchField.value = "";
            });
            _cancelSearch.style.minWidth = 20;
            _cancelSearch.style.width = 20;

            var _cancelSearchIcon = new VisualElement();
            _cancelSearchIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("delete2");
            _cancelSearchIcon.style.width = 18;
            _cancelSearchIcon.style.height = 18;

            _cancelSearch.Add(_cancelSearchIcon);

            _searchContainer.Add(_searchIcon);
            _searchContainer.Add(_searchField);
            _searchContainer.Add(_cancelSearch);

            root.Add(_searchContainer);
            root.Add(scrollView);

            BuildList();
        }


        void BuildList()
        {
            scrollView.Clear();

            var _availableObjects = dataLibrary.GetAllInitialDataObjectsByType(dataType, includeSubtypes);
            for (int i = -1; i < _availableObjects.Count; i++)
            {
                var _index = i;
                if (_index > -1)
                {
                    if (!string.IsNullOrEmpty(searchInput))
                    {
                        if (!_availableObjects[i].title.ToLower().Contains(searchInput.ToLower()))
                        {
                            continue;
                        }
                    }
                }

                if (searchIndex == -1)
                {
                    searchIndex = i;
                }

                var _b = DatabrainHelpers.DatabrainButton("");
                _b.style.marginBottom = 2;
                _b.style.height = 24;

                if (i == -1)
                {
                    if (string.IsNullOrEmpty(searchInput))
                    {

                        _b.text = "- none -";
                        _b.RegisterCallback<ClickEvent>(x =>
                        {
                            
                            property.objectReferenceValue = null;

                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();

                            updateSelectedIndex?.Invoke(-1);

                            editorWindow.Close();
                        });

                        scrollView.Add(_b);
                    }
                }
                else
                {


                    if (_availableObjects[_index].icon != null)
                    {
                        var _icon = new VisualElement();
                        _icon.style.backgroundImage = _availableObjects[_index].icon.texture;
                        _icon.style.width = 18;
                        _icon.style.height = 18;
                        _icon.style.marginLeft = 5;
                        _b.Add(_icon);
                    }

                    _b.text = _availableObjects[i].title;
                    _b.RegisterCallback<ClickEvent>(x =>
                    {
                        property.objectReferenceValue = _availableObjects[_index];
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();

                        updateSelectedIndex?.Invoke(_index);

                        editorWindow.Close();
                    });

                    scrollView.Add(_b);
                }



            }



        }

        public override void OnClose()
        {
            //onCloseCallback?.Invoke();
        }
    }
}
#endif