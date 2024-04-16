/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

using Databrain.Attributes;
using Databrain.Helpers;

namespace Databrain.UI
{
#if UNITY_EDITOR
    public static class DatabrainTags
    {
        public static VisualElement ShowAssignTags(VisualElement _tagContainer, DataObject _dataObject)
        {

            var _addTag = new Button();
            _addTag.style.backgroundColor = DatabrainColor.DarkBlue.GetColor();

            var _tagIcon = new VisualElement();
            _tagIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("tagoutline");
            _tagIcon.style.width = 14;
            _tagIcon.style.height = 14;
            _addTag.Add(_tagIcon);
            _addTag.text = "";
            _addTag.tooltip = "Assign tags";
            _addTag.name = "assignTagsButton";
            DatabrainHelpers.SetBorderRadius(_addTag, 10, 10, 10, 10);
            _addTag.RegisterCallback<ClickEvent>(evt =>
            {
                var _popup = new AssignTagsPopup(_dataObject, _dataObject.relatedLibraryObject.tags, _tagContainer);
                var _position = _addTag.LocalToWorld(_addTag.transform.position);
                AssignTagsPopup.ShowPanel(new Vector2(_position.x , _position.y + 15), _popup);
            });

            if (_dataObject.relatedLibraryObject != null)
            {
                if (_dataObject.relatedLibraryObject.tags == null)
                {
                    _addTag.SetEnabled(false);
                    _addTag.tooltip = "No tags defined, add new tags in the settings.";
                }
                else
                {
                    if (_dataObject.relatedLibraryObject.tags.Count == 0)
                    {
                        _addTag.SetEnabled(false);
                        _addTag.tooltip = "No tags defined, add new tags in the settings.";
                    }

                }
            }

            return _addTag;
        }


        public static void ShowTagsDataObject(VisualElement _tagContainer, List<string> _tagsList, List<string> _existingTags, Action<Type> _onRemoveItem = null)
        {
            if (_tagsList == null)
                return;

            if (_tagContainer != null)
            {
                List<VisualElement> list = new List<VisualElement>(_tagContainer.Children().ToList());
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].name != "assignTagsButton")
                    {
                        try
                        {
                            _tagContainer.Remove(list[i]);
                        }
                        catch { }
                    }
                }

                _tagContainer.style.flexDirection = FlexDirection.Row;
                _tagContainer.style.flexWrap = Wrap.Wrap;
            }
            else
            {
                _tagContainer = new VisualElement();
                _tagContainer.style.flexDirection = FlexDirection.Row;
                _tagContainer.style.flexWrap = Wrap.Wrap;
            }

            // Cleanup tags list for non exsting tags
            for (int t = _tagsList.Count - 1; t >= 0; t --)
            {
                var _exists = false;
                for (int j = 0; j < _existingTags.Count; j ++)
                {
                    if (_tagsList[t] == _existingTags[j])
                    {
                        _exists = true;
                        break;
                    }
                }

                if (!_exists)
                {
                    _tagsList.RemoveAt(t);
                }
            }


            for (int i = 0; i < _tagsList.Count; i++)
            {
                var _tagIndex = i;

                var _tagItem = new VisualElement();
                DatabrainHelpers.SetBorderRadius(_tagItem, 10, 10, 10, 10);
                DatabrainHelpers.SetBorder(_tagItem, 0);
                DatabrainHelpers.SetPadding(_tagItem, 5, 5, 0, 0);
                DatabrainHelpers.SetMargin(_tagItem, 2, 2, 2, 2);

                _tagItem.style.flexDirection = FlexDirection.Row;
                _tagItem.style.minHeight = 18;
                _tagItem.style.maxHeight = 18;
                _tagItem.style.backgroundColor = DatabrainColor.DarkBlue.GetColor();

                var _tagIcon = new VisualElement();
                _tagIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("tagoutline");
                _tagIcon.style.width = 14;
                _tagIcon.style.height = 14;
                _tagIcon.style.alignSelf = Align.Center;
                DatabrainHelpers.SetMargin(_tagIcon, 2, 2, 4, 0);

                var _tagItemLabel = new Label();
                _tagItemLabel.text = _tagsList[i];
                _tagItemLabel.style.fontSize = 12;
                _tagItemLabel.style.flexGrow = 1;
                _tagItemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _tagItemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

                var _tagItemDelete = new Label();
                _tagItemDelete.text = "x";
                _tagItemDelete.style.marginLeft = 4;
                _tagItemDelete.style.fontSize = 12;
                _tagItemDelete.style.unityTextAlign = TextAnchor.MiddleLeft;
                _tagItemDelete.RegisterCallback<ClickEvent>(evt =>
                {
                    _tagsList.RemoveAt(_tagIndex);
                    _onRemoveItem?.Invoke(null);
                    ShowTagsDataObject(_tagContainer, _tagsList, _existingTags, null);
                
                });

                _tagItem.Add(_tagIcon);
                _tagItem.Add(_tagItemLabel);
                _tagItem.Add(_tagItemDelete);

                _tagContainer.Add(_tagItem);
            }

        }

        public static void ShowTitlesDataObject(VisualElement _tagContainer, List<string> _tagsList, Action<Type> _onRemoveItem = null)
        {
            if (_tagContainer != null)
            {
                List<VisualElement> list = new List<VisualElement>(_tagContainer.Children().ToList());
                for (int i = 0; i < list.Count; i++)
                {
                    try
                    {
                        _tagContainer.Remove(list[i]);
                    }
                    catch { }
                }


                _tagContainer.style.flexDirection = FlexDirection.Row;
                _tagContainer.style.flexWrap = Wrap.Wrap;
            }
            else
            {
                _tagContainer = new VisualElement();
                _tagContainer.style.flexDirection = FlexDirection.Row;
                _tagContainer.style.flexWrap = Wrap.Wrap;

            }

            if (_tagsList == null)
                return;


            for (int i = 0; i < _tagsList.Count; i++)
            {
                var _tagIndex = i;

                var _tagItem = new VisualElement();
                DatabrainHelpers.SetBorderRadius(_tagItem, 10, 10, 10, 10);
                DatabrainHelpers.SetBorder(_tagItem, 1, Color.white);
                DatabrainHelpers.SetPadding(_tagItem, 5, 5, 0, 0);
                DatabrainHelpers.SetMargin(_tagItem, 2, 2, 2, 2);

                _tagItem.style.flexDirection = FlexDirection.Row;
                _tagItem.style.minHeight = 18;
                _tagItem.style.maxHeight = 18;
                //_tagItem.style.backgroundColor = Color.white; // DatabrainColor.DarkBlue.GetColor();

                var _tagIcon = new VisualElement();
                _tagIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("title");
                _tagIcon.style.unityBackgroundImageTintColor = Color.white;
                _tagIcon.style.width = 16;
                _tagIcon.style.height = 16;
                _tagIcon.style.alignSelf = Align.Center;
                DatabrainHelpers.SetMargin(_tagIcon, 2, 2, 4, 0);

                var _tagItemLabel = new Label();
                _tagItemLabel.text = _tagsList[i];
                _tagItemLabel.style.fontSize = 12;
                _tagItemLabel.style.flexGrow = 1;
                _tagItemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _tagItemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
                _tagItemLabel.style.color = Color.white;

                var _tagItemDelete = new Label();
                _tagItemDelete.text = "x";
                _tagItemDelete.style.marginLeft = 4;
                _tagItemDelete.style.fontSize = 12;
                _tagItemDelete.style.unityTextAlign = TextAnchor.MiddleLeft;
                _tagItemDelete.RegisterCallback<ClickEvent>(evt =>
                {
                    _tagsList.RemoveAt(_tagIndex);
                    ShowTitlesDataObject(_tagContainer, _tagsList);

                    _onRemoveItem?.Invoke(null);
                });

                _tagItem.Add(_tagIcon);
                _tagItem.Add(_tagItemLabel);
                _tagItem.Add(_tagItemDelete);

                _tagContainer.Add(_tagItem);
            }

        }
    }



public class AssignTagsPopup : PopupWindowContent
    {
        DataObject dataObject;
        VisualElement tagContainer;
        List<string> tagList = new List<string>();

        public static void ShowPanel(Vector2 _pos, AssignTagsPopup _panel)
        {
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 300);
        }

        public override void OnGUI(Rect rect) { }

        public override void OnOpen()
        {
            var _root = new VisualElement();
            var _scrollView = new ScrollView();

            for (int i = 0; i < tagList.Count; i++)
            {
                var _tagIndex = i;
                var _tag = new Button();
                _tag.text = tagList[i];
                _tag.style.backgroundColor = DatabrainColor.DarkBlue.GetColor();
                _tag.RegisterCallback<ClickEvent>(evt =>
                {
                    if (!dataObject.tags.Contains(tagList[_tagIndex]))
                    {
                        dataObject.tags.Add(tagList[_tagIndex]);

                        DatabrainTags.ShowTagsDataObject( tagContainer, dataObject.tags, dataObject.relatedLibraryObject.tags);
                    }

                    editorWindow.Close();
                });

                _scrollView.Add(_tag);
            }

            _root.Add(_scrollView);

            editorWindow.rootVisualElement.Add(_root);

        }

        public override void OnClose() { }


        public AssignTagsPopup(DataObject _dataObject, List<string> _tagList, VisualElement _tagContainer)
        {
            dataObject = _dataObject;
            tagList = _tagList;
            tagContainer = _tagContainer;
        }
    }

    //public class FilterByTagsPopup : PopupWindowContent
    //{
    //    Action<List<string>> onSelected;
    //    List<string> selected;
    //    List<string> tags = new List<string>();
    //    Texture2D checkIcon;

    //    public static void ShowPanel(Vector2 _pos, FilterByTagsPopup _panel)
    //    {
    //        UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
    //    }

    //    public override Vector2 GetWindowSize()
    //    {
    //        return new Vector2(200, 300);
    //    }

    //    public override void OnGUI(Rect rect){}

    //    public override void OnOpen()
    //    {
    //        var _root = new VisualElement();
    //        DatabrainHelpers.SetMargin(_root, 5, 5, 5, 5);
    //        var _scrollView = new ScrollView();

    //        for (int i = 0; i < tags.Count; i ++)
    //        {
    //            var _tagItem = new VisualElement();
    //            _tagItem.style.flexDirection = FlexDirection.Row;
    //            _tagItem.style.flexGrow = 1;

    //            var _checked = new VisualElement();
    //            _checked.style.width = 18;
    //            _checked.style.height = 18;

    //            if (selected.Contains(tags[i]))
    //            {
                  
    //                _checked.style.backgroundImage = checkIcon;
    //            }
    //            else
    //            {
    //                _checked.style.backgroundImage = null;
    //            }

    //            var _tagIndex = i;
    //            var _tag = new Button();
    //            _tag.style.flexGrow = 1;
    //            _tag.text = tags[i];
    //            _tag.style.backgroundColor = DatabrainColor.DarkBlue.GetColor();
    //            _tag.RegisterCallback<ClickEvent>(evt =>
    //            {
    //                //if (!dataObject.tags.Contains(tagList[_tagIndex]))
    //                //{
    //                //    dataObject.tags.Add(tagList[_tagIndex]);

    //                //    DatabrainTags.ShowTagsDataObject(tagContainer, dataObject.tags);
    //                //}

    //                //editorWindow.Close();
    //                if (selected == null)
    //                {
    //                    selected = new List<string>();
    //                }

    //                if (!selected.Contains(tags[_tagIndex]))
    //                {
    //                    Debug.Log("ADD");
    //                    selected.Add(tags[_tagIndex]);
    //                    _checked.style.backgroundImage = checkIcon;
    //                }
    //                else
    //                {
    //                    Debug.Log("REMOVE");
    //                    //try
    //                    //{
    //                        selected.Remove(tags[_tagIndex]);
    //                    //}
    //                    //catch { }
    //                    _checked.style.backgroundImage = null;
    //                }

    //                onSelected.Invoke(selected);
    //            });

    //            _tagItem.Add(_checked);          
    //            _tagItem.Add(_tag);

    //            _scrollView.Add(_tagItem);
    //        }

    //        _root.Add(_scrollView);

    //        editorWindow.rootVisualElement.Add(_root);

    //    }

    //    public override void OnClose()
    //    {
    //        onSelected.Invoke(selected);
    //    }

    //    public FilterByTagsPopup(List<string> _tags, List<string> _selected, Action<List<string>> _onSelected) 
    //    {
    //        tags = _tags;
    //        onSelected = _onSelected;
    //        selected = _selected;
    //        checkIcon = DatabrainHelpers.LoadIcon("check");
    //    }

    //}
#endif
}

