/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

using Databrain.Helpers;
using Databrain.UI.Manipulators;
using Databrain.Attributes;

namespace Databrain.UI.Elements
{
    public class DataObjectListItemElement : VisualElement
    {
        private DatabrainEditorWindow databrainEditor;

        public DataLibrary dataLibrary;
        public DataObject dataObject;

        public Label dataListLabel;
        public Button dataListDuplicateButton;
        public Button dataListRemoveButton;
        public VisualElement dataListElement;
        public VisualElement dataListIcon;
        public VisualElement dataListWarningIcon;
        public VisualElement dataListFavoriteButton;
        public VisualElement dataListDragObjectButton;
        public ListView dataListView;

        private Manipulator dragManipulator;

        public DataObjectListItemElement(VisualTreeAsset _asset)
        {
            this.style.height = 40;
            this.style.flexDirection = FlexDirection.Row;
            this.style.borderBottomWidth = 1;
            this.style.borderBottomColor = DatabrainColor.DarkGrey.GetColor();
            this.style.borderTopWidth = 1;
            this.style.borderTopColor = DatabrainColor.Grey.GetColor();

            var _dragIcon = new VisualElement();
           
            _dragIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("dragIndicator");
            _dragIcon.pickingMode = PickingMode.Ignore;
            var _bgSize = new BackgroundSize();
            _bgSize.sizeType = BackgroundSizeType.Contain;
            _dragIcon.style.backgroundSize = _bgSize;
            _dragIcon.style.width = 20;
            _dragIcon.style.unityBackgroundImageTintColor = new Color(128f / 255f, 128f / 255f, 128f / 255f);
            this.Add(_dragIcon);


            _asset.CloneTree(this);

            dataListElement = this.Q<VisualElement>("dataListElement");
            dataListElement.style.flexGrow = 1;
            dataListLabel = dataListElement.Q<Label>("dataListLabel");
            dataListDuplicateButton = dataListElement.Q<Button>("dataListDuplicateButton");
            dataListRemoveButton = dataListElement.Q<Button>("dataListDeleteButton");
            dataListIcon = dataListElement.Q<VisualElement>("dataListIcon");
            dataListWarningIcon = dataListElement.Q<VisualElement>("warningIcon");
            dataListFavoriteButton = dataListElement.Q<VisualElement>("favoriteButton");
            dataListDragObjectButton = dataListElement.Q<VisualElement>("dragDataObjectButton");

            dataListDuplicateButton.tooltip = "Duplicate selected DataObjects";
            dataListRemoveButton.tooltip = "Delete selected DataObjects";

            dataListIcon.style.width = 32;
            dataListIcon.style.minWidth = 32;
            dataListIcon.style.height = 32;

            dataListIcon.style.display = DisplayStyle.Flex;
            dataListIcon.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            DatabrainHelpers.SetMargin(dataListIcon, 0, 10, 0, 0);

            dataListElement.style.paddingLeft = 2;
            dataListElement.style.paddingRight = 2;
            dataListElement.style.marginLeft = 2;
            dataListElement.style.marginTop = -1;
        }


        public void Bind(DataObject _dataObject, int _index, DataLibrary _dataLibrary, DatabrainEditorWindow _editor, ListView _dataListView)
        {
            if (_dataObject == null)
                return;

            name = _dataObject.title;

            dataListFavoriteButton.UnregisterCallback<ClickEvent>(FavoriteButtonCallback);
            dataListRemoveButton.UnregisterCallback<ClickEvent>(DataListRemoveButtonCallback);
            dataListDuplicateButton.UnregisterCallback<ClickEvent>(DuplicateObjectCallback);

            dataListDragObjectButton.RemoveManipulator(dragManipulator);

            databrainEditor = _editor;
            dataLibrary = _dataLibrary;
            dataObject = _dataObject;
            dataObject.index = _index;


            this.name = dataObject.guid;
            dataListElement.name = dataObject.guid;
            dataListLabel.text = dataObject.title;
            dataListLabel.tooltip = dataObject.description;


            var _customIcon = _dataObject.ReturnIcon();
            if (_customIcon != null)
            {
                dataListIcon.style.backgroundImage = _customIcon;
            }
            else
            {
                dataListIcon.style.backgroundImage = new StyleBackground(dataObject.icon);
            }

            dataListIcon.style.backgroundColor = new StyleColor(dataObject.color);

            
            dragManipulator = new DataObjectDragManipulator(dataObject.index, dataListDragObjectButton, dataLibrary.GetInitialDataObjectByGuid(_dataObject.guid));
            

            dataListDragObjectButton.AddManipulator(dragManipulator);


            var _isValid = _dataObject.IsValid();
            dataListWarningIcon.visible = !_isValid;

            //// Set Favorite

            var _favoriteOutline = dataListFavoriteButton.Q<VisualElement>("favoriteIconOutline");
            var _favoriteFilled = dataListFavoriteButton.Q<VisualElement>("favoriteIconFilled");

            if (!dataLibrary.data.IsFavorite(_dataObject))
            {
                _favoriteOutline.style.display = DisplayStyle.Flex;
                _favoriteFilled.style.display = DisplayStyle.None;
            }
            else
            {
                _favoriteOutline.style.display = DisplayStyle.None;
                _favoriteFilled.style.display = DisplayStyle.Flex;
            }


            dataListFavoriteButton.RegisterCallback<ClickEvent>(FavoriteButtonCallback);
            ////dataListButton.RegisterCallback<ClickEvent>(DataListButtonCallback);
            dataListDuplicateButton.RegisterCallback<ClickEvent>(DuplicateObjectCallback);
            dataListRemoveButton.RegisterCallback<ClickEvent>(DataListRemoveButtonCallback);


            //// Hide specific features in runtime objects
            if (dataObject.isRuntimeInstance)
            {
                dataListFavoriteButton.style.display = DisplayStyle.None;
                dataListDragObjectButton.style.display = DisplayStyle.None;
                dataListRemoveButton.style.display = DisplayStyle.None;
                dataListIcon.style.display = DisplayStyle.None;
                dataListDuplicateButton.style.display = DisplayStyle.None;
            }

        }

        void FavoriteButtonCallback(ClickEvent _event)
        {
            var _favoriteOutline2 = dataListFavoriteButton.Q<VisualElement>("favoriteIconOutline");
            var _favoriteFilled2 = dataListFavoriteButton.Q<VisualElement>("favoriteIconFilled");

            if (!dataLibrary.data.IsFavorite(dataObject))
            {
                dataLibrary.data.SetFavorite(dataObject);

                _favoriteOutline2.style.display = DisplayStyle.None;
                _favoriteFilled2.style.display = DisplayStyle.Flex;
            }
            else
            {
                dataLibrary.data.RemoveFromFavorite(dataObject);

                _favoriteOutline2.style.display = DisplayStyle.Flex;
                _favoriteFilled2.style.display = DisplayStyle.None;
            }

            databrainEditor.BuildFavoritesList();
        }

      
        //public void DataListButtonCallback()
        //{
        //    Debug.Log("Click clack");
        //    var _guid = dataObject.guid;
          
        //    databrainEditor.PopulateData(_guid);

            
        //    dataObject.Selected();

        //    dataListView.Query("dataListButton").ForEach(x => x.EnableInClassList("typeListElementSelected--checked", false));
        //    dataListButton.EnableInClassList("typeListElementSelected--checked", true);

        //    dataListView.selectedIndex = dataObject.index;
        //}

        void DataListRemoveButtonCallback(ClickEvent _event)
        {
            if (!dataLibrary.isRuntimeContainer)
            {
                if (databrainEditor.selectedDataObjects.Count >= 2)
                {

                    if (EditorUtility.DisplayDialog("Delete object", databrainEditor.selectedDataObjects.Count > 1 ? "Do you really want to delete selected DataObjects?" : "Do you really want to delete selected DataObject?", "Yes", "No"))
                    {
                        databrainEditor.ClearDataInspectors();

                        for (int k = databrainEditor.selectedDataObjects.Count-1; k >= 0; k --)
                        {
                            databrainEditor.RemoveDataObject(databrainEditor.selectedDataObjects[k]);
                        }

                        databrainEditor.dataTypelistView.selectedIndex = -1;
                        databrainEditor.UpdateSelectedDataTypeList();
                    }
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Delete object", "Do you really want to delete selected DataObject?", "Yes", "No"))
                    {
                        databrainEditor.RemoveDataObject(this.dataObject);
                        databrainEditor.UpdateSelectedDataTypeList();
                    }
                }
            }
        }

        void DuplicateObjectCallback(ClickEvent _event)
        {
            databrainEditor.DuplicateDataObject(this.dataObject);
        }
    }
}
#endif