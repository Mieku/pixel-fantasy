/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Helpers;
using Databrain.UI;
using Databrain.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(DataObjectDropdownAttribute))]
	public class DataObjectDropdownPropertyDrawer : PropertyDrawer
	{
		private int selectedIndex = 0;
		
		private SerializedProperty container;
        private DataLibrary dataLibrary;
        private DataObject runtimeDataObject;

		private Color colorRed = new Color(240f / 255f, 110f / 255f, 110f / 255f, 255f / 255f);
		private Color colorGreen = new Color(50f / 255f, 255f / 255f, 140f / 255f, 255f / 255f);

        private VisualElement root;
        private FoldoutButtonElement dataFoldout;
        private ToggleSwitchElement runtimeToggle;
        private Button searchRuntimeObjectButton;
		private VisualElement firstRow;
        private VisualElement secondRow;
        private VisualElement dataObjectInspector;
		private VisualElement rootDropdown;
		private VisualElement rootButtons;

		private Texture2D searchIcon;
		private Texture2D searchRuntimeIcon;
		private Texture2D addIcon;
		private Texture2D quickAccessIcon;
        private Texture2D whiteTexture;

        // IMGUI
        private GUIStyle imguiStyle;

		private bool libraryNotAvailable;

        Button SmallButton(Texture2D _icon)
		{
			var _newButton = new Button();
			_newButton.style.alignContent = Align.Center;
			_newButton.style.alignItems = Align.Center;

			var _iconElement = new VisualElement();
			_iconElement.style.width = 14;
			_iconElement.style.height = 14;
			_iconElement.style.marginTop = 3;
			_iconElement.style.backgroundImage = _icon;
            _iconElement.style.unityBackgroundImageTintColor = Color.white;


			DatabrainHelpers.SetMargin(_newButton, 0, 0, -3, -3);
			DatabrainHelpers.SetBorderRadius(_newButton, 0, 0, 0, 0);
			_newButton.style.width = 25;

			_newButton.Add(_iconElement);

			return _newButton;
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
            
            // if (fieldInfo.FieldType.BaseType == typeof(DataPropertyBase))
            // {
            //     var _labelNotSupported = new Label();
            //     DatabrainHelpers.SetBorder(_labelNotSupported, 1, Color.black);
            //     DatabrainHelpers.SetMargin(_labelNotSupported, 4, 4, 4, 4);
            //     DatabrainHelpers.SetPadding(_labelNotSupported, 4, 4, 4, 4);
            //     _labelNotSupported.text = "DataObjectDropdown attribute not supported on a DataProperty";

            //     return _labelNotSupported;
            // }

			searchIcon = DatabrainHelpers.LoadIcon("search");
			searchRuntimeIcon = DatabrainHelpers.LoadIcon("searchRuntime");
			addIcon = DatabrainHelpers.LoadIcon("add");
			quickAccessIcon = DatabrainHelpers.LoadIcon("eye");
            

            root = new VisualElement();
            root.style.marginBottom = 1;
            root.style.marginTop = 1;
            DatabrainHelpers.SetBorder(root, 1, Color.black);

			firstRow = new VisualElement();
			firstRow.style.flexDirection = FlexDirection.Row;
			firstRow.style.height = 23;               

            secondRow = new VisualElement();
            
            secondRow.style.flexDirection = FlexDirection.Column;
            secondRow.style.backgroundColor = DatabrainHelpers.colorLightGrey;
            DatabrainHelpers.SetPadding(secondRow, 5, 5, 5, 5);
            secondRow.style.display = DisplayStyle.None;


            runtimeToggle = new ToggleSwitchElement("Runtime", DatabrainHelpers.colorRuntime, (evt) => 
            {
                if (evt)
                {
                    dataObjectInspector.Add(DrawDataObjectInspector(property, true));
                }
                else
                {
                    dataObjectInspector.Add(DrawDataObjectInspector(property, false));
                }
            });
            runtimeToggle.tooltip = "View runtime DataObject";

            secondRow.Add(runtimeToggle);

            
            dataObjectInspector = new VisualElement();
            dataObjectInspector.RegisterCallback<GeometryChangedEvent>((evt) => 
            {
                DataObjectInspectorHeightCheck(attribute);
            });
            dataObjectInspector.name = "DataObjectInspector";
            dataObjectInspector.style.flexGrow = 1;
            secondRow.Add(dataObjectInspector);


            rootDropdown = new VisualElement();
            rootDropdown.style.flexDirection = FlexDirection.Row;
            rootDropdown.name = "rootDropdown";
			rootButtons = new VisualElement();
			rootButtons.style.flexDirection = FlexDirection.Row;
            rootButtons.name = "rootButtons";
 
            firstRow.Add(rootDropdown);
			firstRow.Add(rootButtons);
			

            Build(property);
            BuildButtons(property);


            root.Add(firstRow);
            root.Add(secondRow);

			return root;
		}

		async void WaitForLibrary(SerializedProperty property)
		{
			while (libraryNotAvailable)
			{
				await Task.Delay(100);
                try
                {
                    if (container != null)
                    {
                        if ((container.objectReferenceValue as DataLibrary) != null)
                        {
                            libraryNotAvailable = false;
                        }
                        
                    }
                }
                catch { }
            }

			Build(property);
		}

        public void BuildDelayed(SerializedProperty property)
        {
            firstRow.schedule.Execute(() => { Build(property); }).ExecuteLater(10);
        }


        public void Build(SerializedProperty property)
		{
			var _attribute = (DataObjectDropdownAttribute)attribute;


            rootDropdown.Clear();
            rootDropdown.style.flexDirection = FlexDirection.Row;
            rootDropdown.style.flexGrow = 1;
            rootDropdown.style.backgroundColor = DatabrainHelpers.colorLightGrey;
            rootDropdown.tooltip = _attribute == null ? "" : _attribute.tooltip;
			

			var _indicator = new VisualElement();
			_indicator.name = "Indicator";
			_indicator.style.width = 5;
            _indicator.style.flexShrink = 0;
			DatabrainHelpers.SetMargin(_indicator, 0, 4, 0, 0);


            dataFoldout = new FoldoutButtonElement((value) => 
            {
                if (value)
                {
                    dataObjectInspector.Add(DrawDataObjectInspector(property, runtimeToggle.Value));
                    UpdateRuntimeUIState(property);
                }
                
                if (!runtimeToggle.Value)
                {
                    DatabrainHelpers.SetBorder(secondRow, 0);
                }
                else
                {
                    DatabrainHelpers.SetBorder(secondRow, 2, DatabrainHelpers.colorRuntime);
                }

                secondRow.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            });

            if (dataObjectInspector.resolvedStyle.height > 0)
            {
                dataFoldout.SetOpenWithoutCallback();
            }
            
            rootDropdown.Add(_indicator);
            rootDropdown.Add(dataFoldout);


			var _fieldType = fieldInfo.FieldType;

			if (fieldInfo.FieldType.IsGenericType && (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
			{
				_fieldType = fieldInfo.FieldType.GetGenericArguments().Single();
			}
            if (fieldInfo.FieldType.IsArray)
            {
                _fieldType = fieldInfo.FieldType.GetElementType();
            }

			List<DataObject> availableTypesList = new List<DataObject>();

			var _dataLibraryName = "relatedLibraryObject";

			if (searchIcon == null)
			{
				searchIcon = DatabrainHelpers.LoadIcon("search");
			}

            if (property == null)
            {
                dataFoldout.SetEnabled (false);
                return;
            }

			// Find dataLibrary
            container = property.serializedObject.FindProperty(_dataLibraryName);

            if (_attribute != null)
            {
                if (container == null)
                {
                    if (!string.IsNullOrEmpty(_attribute.dataLibraryFieldName))
                    {
                        container = property.serializedObject.FindProperty(_attribute.dataLibraryFieldName);
                    }
                }

                // Find property (property must have [field:SerializeField] attribute
                if (container == null)
                {
                    container = property.serializedObject.FindProperty("<" + _attribute.dataLibraryFieldName + ">k__BackingField");
                }
            }


            dataLibrary = null;
            if (container != null)
            {
                dataLibrary = (container.objectReferenceValue as DataLibrary);
            }
            else
            {  
                if (_attribute != null)
                {
                    // Try to get Data Library from returning method
                    Component c = property.serializedObject.targetObject as Component;
                    var _methods = c.GetType().GetMethods();
                    foreach (var _m in _methods)
                    {
                        if (_m.Name == _attribute.dataLibraryFieldName)
                        {
                            var _return = _m.Invoke(property.serializedObject.targetObject, null);
                            dataLibrary = (_return as DataLibrary);
                        }
                    }
                }
            }



            // Still couldn't find library
            if (dataLibrary == null)
            {
                dataFoldout.SetEnabled (false);

                var _label = new UnityEngine.UIElements.Label();
                _label.text = property.displayName + "<color=#FFA000><size=10>    Please assign DataLibrary</size></color>";
                _label.style.unityTextAlign = TextAnchor.MiddleLeft;

                rootDropdown.Add(_label);

                libraryNotAvailable = true;

                WaitForLibrary(property);

                return;
            }





            if (dataLibrary != null)
			{

				// Check if related library object has been changed by the user
				if ((property.objectReferenceValue as DataObject) != null)
				{
					if (((property.objectReferenceValue as DataObject).relatedLibraryObject) != dataLibrary)
					{
						property.objectReferenceValue = null;
					}

                    // If user has assigned a runtime data object to this data object we need to set it back to the initial data object
                    // and assign the runtime data object to the runtime clone of the initial data object.
                    if ((property.objectReferenceValue as DataObject).isRuntimeInstance)
                    {
                        var _runtimeDataObject = property.objectReferenceValue as DataObject;
                        var _initDataObject = dataLibrary.GetInitialDataObjectByGuid((property.objectReferenceValue as DataObject).initialGuid); 
                        _initDataObject.runtimeClone = _runtimeDataObject;
                        property.objectReferenceValue = _initDataObject;
                        // Show warning message to tell user not to assign a runtime data object directly.
                        Debug.LogWarning("Databrain: You are assigning a runtime DataObject directly to a public initial DataObject reference. This is not recommended as you would loose the initial DataObject. Please assign the runtime DataObject to the runtimeClone variable of the initial DataObject instead. - " + property.displayName, property.objectReferenceValue);
                    }
				}

				availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(_fieldType, _attribute == null ? false : _attribute.includeSubtypes);

                var _iconAdded = false;
#if DATABRAIN_LOGIC
                // filter by scene component types
                if (_attribute != null &&_attribute.sceneComponentType != null )
                {

					var _iconT = new VisualElement();
					_iconT.name = "Icon";
					_iconT.style.width = 20;
					_iconT.style.minWidth = 20;
                    _iconT.style.flexShrink = 0;
					Texture _iconTexture = null;
					var _res = EditorGUIUtility.Load(_attribute.sceneComponentType.Name + " Icon");
					if (_res != null)
					{
						_iconTexture = EditorGUIUtility.IconContent(_attribute.sceneComponentType.Name + " Icon").image;
						_iconAdded = true;

					}

					_iconT.style.backgroundImage = (Texture2D)_iconTexture;

                    rootDropdown.Add(_iconT);
                }
#endif

                if (availableTypesList != null)
				{
                    rootButtons.SetEnabled(true);

                    var _data = (property.objectReferenceValue as DataObject);

					if (_data != null)
					{
						// Search index of selected object
						for (int i = 0; i < availableTypesList.Count; i++)
						{
							if (_data.guid == availableTypesList[i].guid)
							{
								selectedIndex = i + 1;
							}
						}
					}
					// No object is selected
					else
					{
						if (availableTypesList.Count > 0)
						{
							selectedIndex = 0;
						}
					}

					if (availableTypesList.Count > 0)
					{

                        var tlist = availableTypesList.Select(x => x.title.ToString()).ToList(); //.Where(x => !string.IsNullOrEmpty(x.title)).Select(x => x.title.ToString()).ToList();
						tlist.Insert(0, "- none -");

						DataObject _obj = null;

                        // show icon
                        if (selectedIndex > 0 && selectedIndex <= availableTypesList.Count)
						{
							_obj = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, fieldInfo.FieldType);

							if ((_obj as DataObject).icon != null && !_iconAdded)
							{
								var _icon = new VisualElement();
                                _icon.style.width = 20;
                                _icon.style.minWidth = 20;
                                _icon.style.flexShrink = 0;
                                _icon.style.backgroundImage = (_obj as DataObject).icon.texture;

                                if ((_obj as DataObject).icon.texture != null)
                                {
                                    rootDropdown.Add(_icon);
							    }
                            }
						}

						if (selectedIndex == 0)
						{
                            dataFoldout.SetEnabled(false);
							_indicator.style.backgroundColor = colorRed;                            
                        }
						else
						{
                            dataFoldout.SetEnabled(true);
							_indicator.style.backgroundColor = colorGreen;
						}


                        var _button = new Button();
                        _button.text = tlist[selectedIndex];
                        _button.style.flexGrow = 1;
                        _button.style.alignItems = Align.FlexEnd;

                        var _dropdownIcon = new VisualElement();
                        _dropdownIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("arrowDown");
                        _dropdownIcon.style.width = 20;
                        _dropdownIcon.style.height = 20;
                        
                        _button.Add(_dropdownIcon);

                        _button.RegisterCallback<ClickEvent>(x =>
                        {

                            var _panel = new DataObjectSelectionPopup(_fieldType, dataLibrary, property, (x) => 
                            { 
                                selectedIndex = x; 
                                if ((selectedIndex + 1) < tlist.Count)
                                {
                                    UpdateDelayed(firstRow, tlist[selectedIndex + 1], _button, property);
                                }
                                else
                                {
                                    UpdateDelayed(firstRow, tlist[selectedIndex], _button, property);
                                }

                                if (selectedIndex == -1)
                                {
                                    dataFoldout.Value = false;
                                    dataFoldout.SetEnabled(false);
                                    dataObjectInspector.Clear();
                                    runtimeToggle.SetEnabled(false);
                                }
                                else
                                {
                                    dataObjectInspector.Add(DrawDataObjectInspector(property, false));
                                }

                            },  _attribute.includeSubtypes);
                            
                            DataObjectSelectionPopup.ShowPanel(Event.current.mousePosition, _panel);
                           
                        });

                        var _label = new UnityEngine.UIElements.Label();
						_label.text = property.displayName;
						_label.style.unityTextAlign = (TextAnchor.MiddleLeft);
						_label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        _label.style.overflow = Overflow.Hidden;
                        _label.style.flexShrink = 1;
                        _label.style.maxWidth = 250;

                        rootDropdown.Add(_label);

						var _space = new VisualElement();
						_space.style.flexDirection = FlexDirection.Row;
						_space.style.flexGrow = 1;

                        rootDropdown.Add(_button);

					}
					else
					{
						rootButtons.SetEnabled(false);

                        _indicator.style.backgroundColor = colorRed;

						var _label = new UnityEngine.UIElements.Label();
						_label.text = property.displayName;
						_label.style.unityTextAlign = TextAnchor.MiddleLeft;
						_label.style.unityFontStyleAndWeight = FontStyle.Bold;
						_label.tooltip = "No entries of type " + _fieldType.Name.ToString();

						var _createButton = new Button();
						_createButton.style.alignSelf = Align.FlexStart;
						_createButton.text = "create new";
						_createButton.RegisterCallback<ClickEvent>(click =>
						{
                            var _callback = new Action<SerializedProperty>(x => { Build(property); });

                            var _panel = new ShowCreateDataObjectPopup(dataLibrary, property, _fieldType, _attribute, _callback);
							ShowCreateDataObjectPopup.ShowPanel(Event.current.mousePosition, _panel);
						});

						var _space = new VisualElement();
						_space.style.flexDirection = FlexDirection.Row;
						_space.style.flexGrow = 1;

                        rootDropdown.Add(_label);
                        rootDropdown.Add(_space);
                        rootDropdown.Add(_createButton);
					}
				}
				else
				{
					rootButtons.SetEnabled(false);

                    _indicator.style.backgroundColor = colorRed;

					var _label = new UnityEngine.UIElements.Label();
					_label.text = property.displayName;
					_label.style.unityTextAlign = TextAnchor.MiddleLeft;
					_label.style.unityFontStyleAndWeight = FontStyle.Bold;
					_label.tooltip = "No entries of type " + _fieldType.Name.ToString();

                    var _createButton = new Button();
					_createButton.text = "create new";
					_createButton.style.alignSelf = Align.FlexStart;
					_createButton.RegisterCallback<ClickEvent>(x =>
					{
                        var _callback = new Action<SerializedProperty>(x => { Build(property); });

                        var _panel = new ShowCreateDataObjectPopup(dataLibrary, property, _fieldType, _attribute, _callback);
						ShowCreateDataObjectPopup.ShowPanel(Event.current.mousePosition, _panel);

					});

					var _space = new VisualElement();
					_space.style.flexDirection = FlexDirection.Row;
					_space.style.flexGrow = 1;


                    rootDropdown.Add(_label);
                    rootDropdown.Add(_space);
                    rootDropdown.Add(_createButton);
				}
			}

            property.serializedObject.ApplyModifiedProperties();
			
        }

        async void UpdateDelayed(VisualElement _root, string _title, Button _button, SerializedProperty property)
        {
            await Task.Delay(50);
           

            _button.text = _title;

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            // Reassign correct root elements
            // Need to do this because of a weird behaviour when displaying multiple 
            // property drawers.
            firstRow = _button.parent.parent;
            rootDropdown = _button.parent;

            Build(property);
            BuildButtons(property);
        }

        public void BuildButtons(SerializedProperty property)
		{
			rootButtons.Clear();
			rootButtons.style.marginTop = 3;
			rootButtons.style.height = 17;

            var _attribute = (DataObjectDropdownAttribute)attribute;

            if (dataLibrary == null)
            {
                return;
            }

            var availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(fieldInfo.FieldType, _attribute != null ? _attribute.includeSubtypes : false);

			if (availableTypesList != null)
			{
				
				var _data = (property.objectReferenceValue as DataObject);
				var _editor = Editor.CreateEditor(_data);


				var _addButton = SmallButton(addIcon);
				_addButton.tooltip = "Create new object";
				_addButton.RegisterCallback<ClickEvent>(click =>
				{
					var _callback = new Action<SerializedProperty>(x => { Build(property); });

					var _panel = new ShowCreateDataObjectPopup(dataLibrary, property, fieldInfo.FieldType, _attribute, _callback);
					ShowCreateDataObjectPopup.ShowPanel(Event.current.mousePosition, _panel);
				});


                var _showInitialInspector = SmallButton(quickAccessIcon);
                _showInitialInspector.RegisterCallback<ClickEvent>(click =>
                {
                    DataObject _db = property.objectReferenceValue as DataObject;
					if (_db != null)
					{
						var _popup = new ShowExposeToInspectorPopup(_db, property);
						ShowExposeToInspectorPopup.ShowPanel(Event.current.mousePosition, _popup);
					}
                });

                
                if (_editor != null)
                {
                    // EXPOSE TO INSPECTOR IS DEPRECATED
                    var _hasExposeToInspector = PropertyUtility.HasExposeToInspector(_editor);
					var _objs = _data.CollectObjects();
					if (_objs != null)
					{
                        _hasExposeToInspector = true;
					}
                    // _expInspector.SetEnabled(_hasExposeToInspector);
                    // _expInspector.tooltip = _hasExposeToInspector ? "View data with the [ExposeToInspector] attribute" : "No fields with [ExposeToInspector] attribute assigned";
				}

                var _searchButton = SmallButton(searchIcon);
				_searchButton.tooltip = "Select object in Databrain editor";

				_searchButton.RegisterCallback<ClickEvent>(click =>
				{
                    if (availableTypesList == null || availableTypesList.Count == 0)
                    {
                        availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(fieldInfo.FieldType, _attribute.includeSubtypes);
                    }
                    if (availableTypesList == null || availableTypesList.Count == 0)
                    {
                        return;
                    }
                    if (selectedIndex == 0)
						return;

					if (selectedIndex >= availableTypesList.Count)
					{
						availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(fieldInfo.FieldType, _attribute.includeSubtypes);
					}

                    var _go = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, _fieldType);

					if (dataLibrary != null)
					{
						var _window = DatabrainHelpers.OpenEditor(dataLibrary.GetInstanceID(), false);

						_window.SelectDataObject(_go);
					}
				});


				rootButtons.Add(_addButton);
                rootButtons.Add(_showInitialInspector);
              
                rootButtons.Add(_searchButton);
				
                UpdateRuntimeUIState(property);
				// if (_data != null)
				// {
				// 	if (Application.isPlaying) // && _data.runtimeClone != null)
				// 	{
                //         // Search runtime data object   
                //         // var _dataObject = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, _fieldType);
                //         runtimeDataObject = null;
                //         Component _sceneComponent = property.serializedObject.targetObject as Component;
                //         if (_sceneComponent != null)
                //         {
                //             runtimeDataObject = _data.GetRuntimeDataObject(_sceneComponent.gameObject);
                //         }
                //         if (runtimeDataObject == null)
                //         {
                //             // else use default runtime clone
                //             if (_data.runtimeClone != null)
                //             {
                //                 runtimeDataObject = _data.runtimeClone;
                //             }
                //         }

                //         if (runtimeDataObject != null)
                //         {
                //             var _searchRuntimeObject = SmallButton(searchRuntimeIcon);
                //             _searchRuntimeObject.tooltip = "Select runtime object";
                //             _searchRuntimeObject.RegisterCallback<ClickEvent>(click =>
                //             {

                //                 if ((container.objectReferenceValue as DataLibrary).runtimeLibrary != null)
                //                 {
                //                     var _window = DatabrainHelpers.OpenEditor(dataLibrary.runtimeLibrary.GetInstanceID(), false);
                //                     _window.SelectDataObject(runtimeDataObject);
                //                 }
                //             });
                            
    			// 			rootButtons.Add(_searchRuntimeObject);
                //             runtimeToggle.SetEnabled(true);
                //         }
                //         else
                //         {
                //             runtimeToggle.SetEnabled(false);
                //             runtimeToggle.tooltip = "No runtime data object available";
                //         }
				// 	}
                //     else
                //     {
                //         runtimeToggle.SetEnabled(false);
                //         runtimeToggle.tooltip = "Not in playmode";
                //     }
				// }
                // else
                // {
                //     runtimeToggle.SetEnabled(false);
                //     dataFoldout.SetEnabled(false);
                // }
			}
		}


        void UpdateRuntimeUIState(SerializedProperty _property)
        {
            var _data = (_property.objectReferenceValue as DataObject);
            if (_data != null)
            {
                if (Application.isPlaying) // && _data.runtimeClone != null)
                {
                    // Search runtime data object   
                    // var _dataObject = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, _fieldType);
                    runtimeDataObject = null;
                    Component _sceneComponent = _property.serializedObject.targetObject as Component;
                    if (_sceneComponent != null)
                    {
                        runtimeDataObject = _data.GetRuntimeDataObject(_sceneComponent.gameObject);
                    }
                    if (runtimeDataObject == null)
                    {
                        // else use default runtime clone
                        if (_data.runtimeClone != null)
                        {
                            runtimeDataObject = _data.runtimeClone;
                        }
                    }

                    if (runtimeDataObject != null)
                    {
                            var _rtButton = rootButtons.Q<Button>("searchRuntimeButton");
                            if (_rtButton != null)
                            {
                                rootButtons.Remove(_rtButton);
                            }

                            searchRuntimeObjectButton = SmallButton(searchRuntimeIcon);
                            searchRuntimeObjectButton.name = "searchRuntimeButton";
                            searchRuntimeObjectButton.tooltip = "Select runtime object";
                            
                            searchRuntimeObjectButton.RegisterCallback<ClickEvent>(click =>
                            {

                                if ((container.objectReferenceValue as DataLibrary).runtimeLibrary != null)
                                {
                                    var _window = DatabrainHelpers.OpenEditor(dataLibrary.runtimeLibrary.GetInstanceID(), false);
                                    _window.SelectDataObject(runtimeDataObject);
                                }
                            });
                            
                        rootButtons.Add(searchRuntimeObjectButton);
                        runtimeToggle.SetEnabled(true);
                    }
                    else
                    {
                        runtimeToggle.SetEnabled(false);
                        
                        runtimeToggle.tooltip = "No runtime data object available";
                    }
                }
                else
                {
                    runtimeToggle.SetEnabled(false);
                    runtimeToggle.tooltip = "Not in playmode";
                }
            }
            else
            {
                runtimeToggle.SetEnabled(false);
                dataFoldout.SetEnabled(false);
            }
        }



        // void BuildDataProperties(SerializedProperty property)
        // {
        //     dataPropertiesContainer = new VisualElement();

        //     var _dbObject = property.objectReferenceValue as DataObject;


        //     // Debug.Log(property.serializedObject.GetType().Name);
        //     if (_dbObject == null)
        //         return;

        //     Editor _dbEditor = Editor.CreateEditor(_dbObject);
            
        //     if (_dbObject.dataProperties == null)
        //         return;


        //     for (int i = 0; i < _dbObject.dataProperties.Count; i++)
        //     {
        //         var _prop = new PropertyField();
        //         _prop.BindProperty(_dbEditor.serializedObject.FindProperty("dataProperties").GetArrayElementAtIndex(i));

        //         dataPropertiesContainer.Add(_prop);
        //     }
        // }

        // this is now OBSOLETE as Odin Inspector now supports UIToolkit
        // Only for downward compatibility when Odin inspector is installed.
        /* 
         * Odin completely inserts itself into the editor generation process to take over with it's own system (the PropertyTree) that replaces Unity's drawer back end.
         * Thus if Odin is present, anything is going to be drawn with IMGUI. 
         */
        #region IMGUI 
        /*
        private void InitStyles()
        {
            if (searchIcon == null)
            {
                searchIcon = DatabrainHelpers.LoadIcon("search");
            }

            if (quickAccessIcon == null)
            {
                quickAccessIcon = DatabrainHelpers.LoadIcon("eye");
            }

            if (whiteTexture == null)
            {
                whiteTexture = EditorGUIUtility.whiteTexture;
            }

            if (searchRuntimeIcon == null)
            {
                searchRuntimeIcon = DatabrainHelpers.LoadIcon("searchRuntime");
            }
            
            if (imguiStyle == null)
            {
                imguiStyle = new GUIStyle(GUI.skin.box);
                imguiStyle.normal.background = EditorGUIUtility.whiteTexture; // (2, 2, new Color(0f, 1f, 0f, 0.5f));
                
            }
        }



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            InitStyles();

            var _fieldType = fieldInfo.FieldType;

            if (fieldInfo.FieldType.IsGenericType && (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                _fieldType = fieldInfo.FieldType.GetGenericArguments().Single();
            }


            Dictionary<string, DataObject> availableTypes = new Dictionary<string, DataObject>();

            List<DataObject> availableTypesList = new List<DataObject>();

            var _dataLibraryName = "relatedLibraryObject";

     

            var _attribute = (DataObjectDropdownAttribute)attribute;

            // Find dataLibrary
            container = property.serializedObject.FindProperty(_dataLibraryName);
            if (container == null)
            {
                if (!string.IsNullOrEmpty(_attribute.dataLibraryFieldName))
                {
                    container = property.serializedObject.FindProperty(_attribute.dataLibraryFieldName);
                }
            }



            // Find property (property must have [field:SerializeField] attribute
            if (container == null)
            {
                container = property.serializedObject.FindProperty("<" + _attribute.dataLibraryFieldName + ">k__BackingField");
            }



            dataLibrary = null;
            if (container != null)
            {
                dataLibrary = (container.objectReferenceValue as DataLibrary);
            }
            else
            {
                // Try to get Data Library from returning method
                Component c = property.serializedObject.targetObject as Component;
                var _methods = c.GetType().GetMethods();
                foreach (var _m in _methods)
                {
                    if (_m.Name == _attribute.dataLibraryFieldName)
                    {
                        var _return = _m.Invoke(property.serializedObject.targetObject, null);
                        dataLibrary = (_return as DataLibrary);
                    }
                }
            }


            //if (dataLibrary == null)
            //    return;


            if (dataLibrary == null)
            {
                EditorGUI.LabelField(position, label);
                return;
            }

            // Check if related library object has been changed by the user
            if ((property.objectReferenceValue as DataObject) != null)
            {
                if (((property.objectReferenceValue as DataObject).relatedLibraryObject) != dataLibrary)
                {
                    property.objectReferenceValue = null;
                }
            }

            availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(_fieldType, _attribute.includeSubtypes);


            var _data = (property.objectReferenceValue as DataObject);

            if (availableTypesList != null)
            {
                if (_data != null)
                {
                    // Search index of selected object
                    for (int i = 0; i < availableTypesList.Count; i++)
                    {
                        if (_data.guid == availableTypesList[i].guid)
                        {
                            selectedIndex = i;
                        }
                    }
                }
                // No object is selected
                else
                {
                    if (availableTypesList.Count > 0)
                    {
                        selectedIndex = -1;
                    }
                }

            }

            if (availableTypesList != null && availableTypesList.Count > 0)
            {
                GUI.color = selectedIndex == -1 ? colorRed : colorGreen;
                GUI.Box(new Rect(position.x, position.y, 5, position.height), "", imguiStyle);
                GUI.color = DatabrainColor.Grey.GetColor();
                GUI.Box(new Rect(position.x + 5, position.y, position.width, position.height), "", imguiStyle); // "Toolbar");
                GUI.color = Color.white;

                var tlist = availableTypesList.Select(x => x.title.ToString()).ToArray();

                DataObject _obj = null;
                var _xOffset = 0;

                if (selectedIndex == -1)
                {

                }
                else
                {
                    // show icon
                    if (selectedIndex < availableTypesList.Count)
                    {
                        _obj = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex].guid); //, fieldInfo.FieldType);


                        if ((_obj as DataObject).icon != null)
                        {
                            GUI.Label(new Rect(position.x + 5, position.y, position.width, position.height), (_obj as DataObject).icon.texture);
                            _xOffset = 30;
                        }
                    }
                }

                var _additionalOffset = 0;

                if (_data != null)
                {
                    if (Application.isPlaying && _data.runtimeClone != null)
                    {
                        _additionalOffset = 20;
                    }
                }


                EditorGUI.BeginChangeCheck();

                EditorGUI.LabelField(new Rect(position.x + 5 + _xOffset + 2, position.y - 2, position.width - 100, position.height), label, "boldLabel");
                selectedIndex = EditorGUI.Popup(new Rect(position.x + _xOffset + 153, position.y + 1, position.width - 235 - _xOffset - _additionalOffset, EditorGUIUtility.singleLineHeight), selectedIndex, tlist);


                if (EditorGUI.EndChangeCheck())
                {

                    property.objectReferenceValue = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex].guid); //, fieldInfo.FieldType);
                    property.serializedObject.ApplyModifiedProperties();
                }

              


                if (GUI.Button(new Rect(position.x + position.width - 80 - _additionalOffset, position.y, 20, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_Toolbar Minus", "Unassign data object"), "ToolbarButton"))
                {
                    property.objectReferenceValue = null;
                }

                if (GUI.Button(new Rect(position.x + position.width - 60 - _additionalOffset, position.y, 20, EditorGUIUtility.singleLineHeight), EditorGUIUtility.IconContent("d_Toolbar Plus"), "ToolbarButton"))
                {
                    var _callback = new Action<SerializedProperty>(x => { });
                    var _panel = new ShowCreateDataObjectPopup(dataLibrary, property, fieldInfo.FieldType, _attribute, _callback);
                    ShowCreateDataObjectPopup.ShowPanel(Event.current.mousePosition, _panel);
                }

                GUI.enabled = selectedIndex == -1 ? false : true;
                if (GUI.Button(new Rect(position.x + position.width - 40 - _additionalOffset, position.y, 20, EditorGUIUtility.singleLineHeight), quickAccessIcon, "ToolbarButton"))
                {
                    DataObject _db = property.objectReferenceValue as DataObject;
                    if (_db != null)
                    {
                        var _popup = new ShowExposeToInspectorPopup(_db, property);
                        ShowExposeToInspectorPopup.ShowPanel(Event.current.mousePosition, _popup);
                    }
                }

                if (GUI.Button(new Rect(position.x + position.width - 20 - _additionalOffset, position.y, 20, EditorGUIUtility.singleLineHeight), searchIcon, "ToolbarButton"))
                {
                    if (availableTypesList == null || availableTypesList.Count == 0)
                        return;
                    if (selectedIndex < 0)
                        return;

                    var _go = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex].guid);

                    if (dataLibrary != null)
                    {
                        var _window = DatabrainHelpers.OpenEditor(dataLibrary.GetInstanceID(), false);

                        _window.SelectDataObject(_go);
                    }
                }


                if (Application.isPlaying && _data.runtimeClone != null)
                {
                    if (GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, EditorGUIUtility.singleLineHeight), searchRuntimeIcon, "ToolbarButton"))
                    {
                        var _dataObject = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex].guid); //, _fieldType);
                        DataObject _rtDataObject = null;
                        Component _sceneComponent = property.serializedObject.targetObject as Component;
                        if (_sceneComponent != null)
                        {
                            _rtDataObject = _dataObject.GetRuntimeDataObject(_sceneComponent.gameObject);
                        }
                        if (_rtDataObject == null)
                        {
                            // else use default runtime clone
                            _rtDataObject = _dataObject.runtimeClone;
                        }

                        if (_rtDataObject != null)
                        {
                            if (dataLibrary.runtimeLibrary != null)
                            {
                                var _window = DatabrainHelpers.OpenEditor(dataLibrary.runtimeLibrary.GetInstanceID(), false);
                                _window.SelectDataObject(_rtDataObject);
                            }
                        }
                    }

                }


                GUI.enabled = true;

            }
            else
            {
                GUI.Box(new Rect(position.x, position.y, position.width, position.height), "", "Toolbar");
                EditorGUI.LabelField(position, label, "boldLabel");
                EditorGUI.LabelField(new Rect(position.x + 153, position.y, position.width - 183, EditorGUIUtility.singleLineHeight), new GUIContent("No entries", "No entries in " + _fieldType.Name.ToString() + " available."));


                if (GUI.Button(new Rect(position.x + position.width - 100, position.y, 100, EditorGUIUtility.singleLineHeight), "create new", "ToolbarButton"))
                {
                    var _callback = new Action<SerializedProperty>(x => { });
                    var _panel = new ShowCreateDataObjectPopup(dataLibrary, property, fieldInfo.FieldType, _attribute, _callback);
                    ShowCreateDataObjectPopup.ShowPanel(Event.current.mousePosition, _panel);
                }
            }

        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var _h = base.GetPropertyHeight(property, label);
            return _h + 4;
        }
        */

 #endregion


    VisualElement DrawDataObjectInspector(SerializedProperty property, bool _showRuntime)
    {
        DataObject _dbToInspect = null;

        if (Application.isPlaying && _showRuntime)
        {
            Component _sceneComponent = property.serializedObject.targetObject as Component;
            if (_sceneComponent != null)
            {
                _dbToInspect = (property.objectReferenceValue as DataObject).GetRuntimeDataObject(_sceneComponent.gameObject);
            }
            if (_dbToInspect == null)
            {
                // else use default runtime clone
                _dbToInspect = (property.objectReferenceValue as DataObject).runtimeClone;
            }

            DatabrainHelpers.SetBorder(secondRow, 2, DatabrainHelpers.colorRuntime);
        }
        else
        {
            _dbToInspect = property.objectReferenceValue as DataObject;

            DatabrainHelpers.SetBorder(secondRow, 0);
        }

        

        if (_dbToInspect != null)
        {
            dataObjectInspector.Clear();

            var _editor = Editor.CreateEditor(_dbToInspect);
            var _useIMGUIInspector = _dbToInspect.GetType().GetCustomAttribute(typeof(DataObjectIMGUIInspectorAttribute));
            var _useOdinInspector = _dbToInspect.GetType().GetCustomAttribute(typeof(UseOdinInspectorAttribute)) as UseOdinInspectorAttribute;
            IMGUIContainer inspectorIMGUI = null;
            if (_useIMGUIInspector != null)
            {
                inspectorIMGUI = new IMGUIContainer(() =>
                {
                    DrawDefaultInspectorUIElements.DrawIMGUIInspectorWithoutScriptField(_editor, null);

                });

                var _customGUI = (property.objectReferenceValue as DataObject).EditorGUI(_editor.serializedObject, null);
                if (_customGUI != null)
                {
                    inspectorIMGUI.Add(_customGUI);
                }

                return(inspectorIMGUI);
            }
            else if (_useOdinInspector != null)
            {
                #if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
                Sirenix.OdinInspector.Editor.OdinEditor odinEditor = Sirenix.OdinInspector.Editor.OdinEditor.CreateEditor(_dbToInspect) as Sirenix.OdinInspector.Editor.OdinEditor;

                inspectorIMGUI = new IMGUIContainer(() =>
                {
                    DrawDefaultInspectorUIElements.DrawInspectorWithOdin(odinEditor, null);

                });
                #else

                inspectorIMGUI = new IMGUIContainer(() =>
                {

                    GUILayout.Label("Odin Inspector not installed");

                });

                #endif

                var _customGUI = (property.objectReferenceValue as DataObject).EditorGUI(_editor.serializedObject, null);
                if (_customGUI != null)
                {
                    inspectorIMGUI.Add(_customGUI);
                }

                return(inspectorIMGUI);
            }
            else
            {
                var _uiElementsInspector = DrawDefaultInspectorUIElements.DrawInspector(_editor, _dbToInspect.GetType(), false);
                _uiElementsInspector.style.flexGrow = 1;

                var _customGUI = (property.objectReferenceValue as DataObject).EditorGUI(_editor.serializedObject, null);

                if (_customGUI != null)
                {
                    _uiElementsInspector.Add(_customGUI);
                }

                return(_uiElementsInspector);
            }

            
        }

        return null;

    }

    async void DataObjectInspectorHeightCheck(PropertyAttribute attribute)
    {
        await Task.Delay(200);

        if (!dataFoldout.Value)
        {
            return;
        }

        var _attribute = (DataObjectDropdownAttribute)attribute;

        if (_attribute.customHeight > -1)
        {
            dataObjectInspector.style.height = _attribute.customHeight;
        }
        else
        {
            if (dataObjectInspector.resolvedStyle.height == 0 || float.IsNaN(dataObjectInspector.style.height.value.value) || dataObjectInspector.style.height == null)
            {
                dataObjectInspector.style.height = 500;
            }
        }
    }
}


    public class ShowExposeToInspectorPopup : PopupWindowContent
    {
        private static DataObject dataObject;
        private static Editor editor;
        private static ScrollView scrollView;
        private static SerializedProperty property;

        public static void ShowPanel(Vector2 _pos, ShowExposeToInspectorPopup _panel)
        {
            UnityEditor.PopupWindow.Show(new Rect(_pos.x-300, _pos.y, 0, 0), _panel);
        }

        public override void OnGUI(Rect rect) 
        {
            editorWindow.minSize = new Vector2(300, 100);
        }

        public override void OnOpen()
        {
            var _root = new VisualElement();
            DatabrainHelpers.SetPadding(_root, 5, 5, 5, 5);
            var _toolbar = new Toolbar();
            var _initialButton = new ToolbarToggle(); 
            var _runtimeButton = new ToolbarToggle();

            _initialButton.text = "Initial";
            _initialButton.value = true;
            _initialButton.RegisterCallback<ClickEvent>(click =>
            {
                _initialButton.value = true;
                _runtimeButton.value = false;
                _initialButton.style.borderBottomWidth = 2;
                _initialButton.style.borderBottomColor = Color.white;
                _runtimeButton.style.borderBottomWidth = 0;

                DatabrainHelpers.SetBorder(_root, 0);

                editor = Editor.CreateEditor(dataObject);

                scrollView.Clear();
                var _gui = DrawDefaultInspectorUIElements.DrawInspector(editor, dataObject.GetType(), true);
                scrollView.Add(_gui);

                DrawInspectorForSubObjects(dataObject);     
            });

            _initialButton.style.borderBottomWidth = 2;
            _initialButton.style.borderBottomColor = Color.white;

            DataObject _rtDataObject = null;

            // first try to get the clone from the game object instance id
            Component _sceneComponent = property.serializedObject.targetObject as Component;
            if (_sceneComponent != null)
            {
                _rtDataObject = dataObject.GetRuntimeDataObject(_sceneComponent.gameObject);
            }
            if (_rtDataObject == null)
            {
                // else use default runtime clone
                _rtDataObject = dataObject.runtimeClone;
            }		

            _runtimeButton.text = "Runtime";
            _runtimeButton.value = false;
            _runtimeButton.RegisterCallback<ClickEvent>(click =>
            {
                _initialButton.value = false;
                _runtimeButton.value = true;
                _runtimeButton.style.borderBottomWidth = 2;
                _runtimeButton.style.borderBottomColor = Color.white;
                _initialButton.style.borderBottomWidth = 0;

                DatabrainHelpers.SetBorder(_root, 2, DatabrainHelpers.colorRuntime);

                editor = Editor.CreateEditor(_rtDataObject);

                scrollView.Clear();
                var _gui = DrawDefaultInspectorUIElements.DrawInspector(editor, _rtDataObject.GetType(), true);
                scrollView.Add(_gui);

                DrawInspectorForSubObjects(_rtDataObject);
            });
            if (_rtDataObject != null)
            {
                _runtimeButton.SetEnabled(true);
            }
            else
            {
                _runtimeButton.SetEnabled(false);
            }


            var _gui = DrawDefaultInspectorUIElements.DrawInspector(editor, dataObject.GetType(), true);
            scrollView.Add(_gui);

            DrawInspectorForSubObjects(dataObject);


            _toolbar.Add(_initialButton);
            _toolbar.Add(_runtimeButton);
            _root.Add(_toolbar);
            _root.Add(scrollView);

            editorWindow.rootVisualElement.Add(_root);          
        }

        void DrawInspectorForSubObjects(DataObject _dataObject)
        {
            // Draw quick access fields for sub scriptable objects (for example logic nodes)
            var _objects = _dataObject.CollectObjects();
            if (_objects != null)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    var _ed = Editor.CreateEditor(_objects[i].so);

                    var _objGui = DrawDefaultInspectorUIElements.DrawInspector(_ed, _objects[i].GetType(), true);
                    if (_objGui.childCount > 0)
                    {
                        var _item = new VisualElement();
                        DatabrainHelpers.SetBorder(_item, 1, DatabrainColor.Grey.GetColor());
                        var _label = new Label();
                        _label.text = _objects[i].title;
                        _label.style.unityFontStyleAndWeight = FontStyle.Bold;


                        _item.Add(_label);
                        _item.Add(_objGui);

                        scrollView.Add(_item);
                    }
                }
            }
        }



        public override void OnClose(){}

        public ShowExposeToInspectorPopup(DataObject _dataObject, SerializedProperty _property)
        {
            if (scrollView != null)
            {
                scrollView.Clear();
            }
            else
            {
                scrollView = new ScrollView();
            }

            property = _property;
            editor = Editor.CreateEditor(_dataObject);
            dataObject = _dataObject;
        }

    }

    public class ShowCreateDataObjectPopup : PopupWindowContent
    {

        static Vector2 position;

        private string dataObjectName;
        private string guid;

        private Type dataType;	
        private SerializedProperty property;
        //private SerializedProperty container;
        private DataLibrary dataLibrary;
        private DataObjectDropdownAttribute dropdownAttribute;

        private Vector2 scrollPosition = Vector2.zero;

        private List<string> types = new List<string>();
        private List<string> typeNames = new List<string>();
        private int selectedIndex;
        private Action<SerializedProperty> addCallback;

        public static void ShowPanel(Vector2 _pos, ShowCreateDataObjectPopup _panel)
        {		
            position = _pos;
            UnityEditor.PopupWindow.Show(new Rect(_pos.x, _pos.y, 0, 0), _panel);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 22);
        }

        public override void OnGUI(Rect rect)
        {	
            var _currentEvent = Event.current;

            using (new GUILayout.HorizontalScope())
            {
                dataObjectName = EditorGUILayout.TextField(dataObjectName);

                if (typeNames.Count > 1)
                {
                    selectedIndex = EditorGUILayout.Popup(selectedIndex, typeNames.ToArray());
                }
                else
                {
                    selectedIndex = 0;
                }

                GUI.enabled = !string.IsNullOrEmpty(dataObjectName);
                if (GUILayout.Button("add"))
                {
                    var _newObject = DataObjectCreator.CreateNewDataObject(dataLibrary, Type.GetType(types[selectedIndex])); // dataType);
                    _newObject.title = dataObjectName;

        #if DATABRAIN_LOGIC
                    if (dropdownAttribute.sceneComponentType != null)
                    {
                        _newObject.OnAddCallback(dropdownAttribute.sceneComponentType);
                    }
        #endif

                    addCallback?.Invoke(property);

                    //container.serializedObject.ApplyModifiedProperties();

                    editorWindow.Close();
                }

                GUI.enabled = true;
            }		
        }



        public override void OnOpen()	{	}

        public override void OnClose()
        {
            var _datacoreObject = dataLibrary; // (container.objectReferenceValue as DataLibrary);
            if (!string.IsNullOrEmpty(guid))
            {
                property.objectReferenceValue = _datacoreObject.GetInitialDataObjectByGuid(guid, dataType);

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public ShowCreateDataObjectPopup(DataLibrary _dataLibrary, SerializedProperty _property, Type _objectDataType, DataObjectDropdownAttribute _dataObjectDropdownAttribute, Action<SerializedProperty> _addCallback)
        {
            types = new List<string>();
            typeNames = new List<string>();
            property = _property;
            dataLibrary = _dataLibrary;
            dataType = _objectDataType;
            dropdownAttribute = _dataObjectDropdownAttribute;
            addCallback = _addCallback;

           // Is data type a generic list? Then we get the generic type of the list.
            if ((dataType.IsGenericType && (dataType.GetGenericTypeDefinition() == typeof(List<>))))
            {
                var _listType = dataType.GetGenericArguments()[0];

                types.Add(_listType.AssemblyQualifiedName.ToString());
                typeNames.Add(_listType.Name.ToString());

                var _subtypes = TypeCache.GetTypesDerivedFrom(_listType);
        
                for (int i = 0; i < _subtypes.Count; i ++)
                {
                    if (_subtypes[i].GetCustomAttribute<HideDataObjectTypeAttribute>() == null)
                    {
                        types.Add(_subtypes[i].AssemblyQualifiedName.ToString());
                        typeNames.Add(_subtypes[i].Name.ToString());
                    }
                }
            }
            else
            {
                if (dataType.GetCustomAttribute<HideDataObjectTypeAttribute>() == null)
                {
                    types.Add(dataType.AssemblyQualifiedName.ToString());
                    typeNames.Add(dataType.Name.ToString());
                }

                var _subtypes = TypeCache.GetTypesDerivedFrom(dataType);
        
                for (int i = 0; i < _subtypes.Count; i ++)
                {
                    if (_subtypes[i].GetCustomAttribute<HideDataObjectTypeAttribute>() == null)
                    {
                        types.Add(_subtypes[i].AssemblyQualifiedName.ToString());
                        typeNames.Add(_subtypes[i].Name.ToString());
                    }
                }
            }
        }

    }
}
#endif