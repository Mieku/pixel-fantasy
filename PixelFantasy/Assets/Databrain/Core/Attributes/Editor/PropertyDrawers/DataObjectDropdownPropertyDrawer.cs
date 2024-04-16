/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using Databrain.Helpers;
using Databrain.UI;

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

		private Color colorRed = new Color(240f / 255f, 110f / 255f, 110f / 255f, 255f / 255f);
		private Color colorGreen = new Color(50f / 255f, 255f / 255f, 140f / 255f, 255f / 255f);

		private VisualElement root;
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
			_iconElement.style.width = 12;
			_iconElement.style.height = 12;
			_iconElement.style.marginTop = 5;
			_iconElement.style.backgroundImage = _icon;


			DatabrainHelpers.SetMargin(_newButton, 0, 0, -3, -3);
			DatabrainHelpers.SetBorderRadius(_newButton, 0, 0, 0, 0);
			_newButton.style.width = 25;

			_newButton.Add(_iconElement);

			return _newButton;
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			searchIcon = DatabrainHelpers.LoadIcon("search");
			searchRuntimeIcon = DatabrainHelpers.LoadIcon("searchRuntime");
			addIcon = DatabrainHelpers.LoadIcon("add");
			quickAccessIcon = DatabrainHelpers.LoadIcon("eye");

			root = new VisualElement();
			root.style.flexDirection = FlexDirection.Row;
			root.style.height = 23;               
            //DatabrainHelpers.SetPadding(root, 2, 2, 2, 2);
            DatabrainHelpers.SetMargin(root, 0, 0, 1, 1);

            rootDropdown = new VisualElement();
            rootDropdown.style.flexDirection = FlexDirection.Row;
            rootDropdown.name = "rootDropdown";
			rootButtons = new VisualElement();
			rootButtons.style.flexDirection = FlexDirection.Row;
            rootButtons.name = "rootButtons";
 
            root.Add(rootDropdown);
			root.Add(rootButtons);
			
			//root.schedule.Execute(() => Build(property)).Every(500); // poll 10 times a second
			

            Build(property);
            BuildButtons(property);

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
            root.schedule.Execute(() => { Build(property); }).ExecuteLater(10);
        }


        public void Build(SerializedProperty property)
		{
			var _attribute = (DataObjectDropdownAttribute)attribute;

            rootDropdown.Clear();
            rootDropdown.style.flexDirection = FlexDirection.Row;
            rootDropdown.style.flexGrow = 1;
            rootDropdown.style.backgroundColor = DatabrainHelpers.colorLightGrey;
            rootDropdown.tooltip = _attribute.tooltip;
			DatabrainHelpers.SetBorder(rootDropdown, 1, Color.black);
			

			var _indicator = new VisualElement();
			_indicator.name = "Indicator";
			_indicator.style.width = 5;
			DatabrainHelpers.SetMargin(_indicator, 0, 4, 0, 0);

            rootDropdown.Add(_indicator);

			//Debug.Log("Build: " + property.serializedObject.targetObject.GetInstanceID());


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

            //Debug.Log(_attribute.dataLibraryFieldName);
            if (property == null)
            return;
            
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



            // Still couldn't find library
            if (dataLibrary == null)
            {
                var _label = new UnityEngine.UIElements.Label();
                _label.text = property.displayName;
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
				}

				availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(_fieldType, _attribute.includeSubtypes);


                var _iconAdded = false;
#if DATABRAIN_LOGIC
                // filter by scene component types
                if (_attribute.sceneComponentType != null)
                {
                    
                    // var _ttList = availableTypesList.Where(x => x.CheckForType(_attribute.sceneComponentType)).ToList();
                    // availableTypesList = _ttList;

					var _iconT = new VisualElement();
					_iconT.name = "Icon";
					_iconT.style.width = 20;
					_iconT.style.minWidth = 20;

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
                        if (selectedIndex > 0 && selectedIndex < availableTypesList.Count)
						{
							_obj = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, fieldInfo.FieldType);


							if ((_obj as DataObject).icon != null && !_iconAdded)
							{

								var _icon = new VisualElement();
                                _icon.style.width = 20;
                                _icon.style.minWidth = 20;

                                _icon.style.backgroundImage = (_obj as DataObject).icon.texture;

                                rootDropdown.Add(_icon);
							}
						}

						if (selectedIndex == 0)
						{
							_indicator.style.backgroundColor = colorRed;                            
                        }
						else
						{
							_indicator.style.backgroundColor = colorGreen;
						}


                        //DropdownField _dropdown = new DropdownField(tlist, selectedIndex);
                        //_dropdown.index = selectedIndex;
                        //_dropdown.style.flexGrow = 1;
                        //_dropdown.style.marginRight = 5;

                        //// Hide the dropdown field as we won't need it
                        //// we only need the value change callback to make sure UIToolkit updates all visible property drawer instances.
                        //// This is a very hacky way to force update. Because of some incomprehensible things going on with the Event system 
                        //// and some differences between the RegisterValueChangedCallback and RegisterCallback<T> I'm using the dropdownfields value change callback instead.
                        //_dropdown.style.width = 0;
                        //_dropdown.style.maxWidth = 0;
                        //_dropdown.RegisterValueChangedCallback(x =>
                        //{
                        //    selectedIndex = _dropdown.index;

                        //    //if (selectedIndex == 0)
                        //    //{
                        //    //    property.objectReferenceValue = null; //, fieldInfo.FieldType);
                        //    //}
                        //    //else
                        //    //{
                        //    //    property.objectReferenceValue = (container.objectReferenceValue as DataLibrary).GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, fieldInfo.FieldType);

                        //    //}

                        //    property.serializedObject.ApplyModifiedProperties();
                        //    property.serializedObject.Update();
                        //    Debug.Log("build");
                        //    //Build(property);
                        //});

                        

                        var _button = new Button();
                        _button.text = tlist[selectedIndex];
                        _button.style.flexGrow = 1;

                        _button.RegisterCallback<ClickEvent>(x =>
                        {
                            //Action action = () => 
                            //{
                                //Debug.Log("hello " + tlist.Count + " _ " + selectedIndex);
                                //BuildDelayed(property);
                                
                                //UpdateDelayed(tlist[selectedIndex+1], _button, property);
                                //root.schedule.Execute(() =>
                                //{
                                //    Debug.Log("execute");
                                //    _dropdown.index = selectedIndex;
                                //    _dropdown.value = tlist[selectedIndex];
                                //    _button.text = tlist[selectedIndex];

                                //    property.serializedObject.ApplyModifiedProperties();
                                //    property.serializedObject.Update();
                                //    Build(property);

                                //}).ExecuteLater(200);
                            //};

                            var _panel = new DataObjectSelectionPopup(_fieldType, dataLibrary, property, (x) => 
                            { 
                                selectedIndex = x; 
                                if ((selectedIndex + 1) < tlist.Count)
                                {
                                    UpdateDelayed(root, tlist[selectedIndex + 1], _button, property);
                                }
                                else
                                {
                                    // Debug.Log(tlist.Count + " _ "  + selectedIndex);
                                    // if (selectedIndex >= tlist.Count)
                                    // {
                                    //     selectedIndex = tlist.Count - 1;
                                    // }
                                    UpdateDelayed(root, tlist[selectedIndex], _button, property);
                                }

                            },  _attribute.includeSubtypes);
                            
                            DataObjectSelectionPopup.ShowPanel(Event.current.mousePosition, _panel);
                           
                        });

                        var _label = new UnityEngine.UIElements.Label();
						_label.text = property.displayName;
						_label.style.unityTextAlign = (TextAnchor.MiddleLeft);
						_label.style.unityFontStyleAndWeight = FontStyle.Bold;
                        rootDropdown.Add(_label);

						var _space = new VisualElement();
						_space.style.flexDirection = FlexDirection.Row;
						_space.style.flexGrow = 1;


						//rootDropdown.Add(_space);
                        //rootDropdown.Add(_dropdown);
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
			//else
			//{
			//	_indicator.style.backgroundColor = colorRed;

			//	var _label = new UnityEngine.UIElements.Label();
			//	_label.text = "No data library defined";
   //             _label.style.unityTextAlign = TextAnchor.MiddleLeft;

   //             rootDropdown.Add(_label);
			//}


			//if (!buttonsBuilt)
			//{
			//	BuildButtons(property);
			//}

            property.serializedObject.ApplyModifiedProperties();
			
        }

        //void UpdateDelayed(VisualElement _root, string _title, Button _button, SerializedProperty property)
        async void UpdateDelayed(VisualElement _root, string _title, Button _button, SerializedProperty property)
        {
            await Task.Delay(50);
           

            _button.text = _title;

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            // Reassign correct root elements
            // Need to do this because of a weird behaviour when displaying multiple 
            // property drawers.
            root = _button.parent.parent;
            rootDropdown = _button.parent;

            Build(property);
        }

        public void BuildButtons(SerializedProperty property)
		{
			//var rootButtons = new VisualElement();
			rootButtons.Clear();
			rootButtons.style.marginTop = 3;
			rootButtons.style.height = 17;

            var _attribute = (DataObjectDropdownAttribute)attribute;

            if (dataLibrary == null)
            {
                return;
            }
   //         if (container == null)
			//{
   //             var _dataLibraryName = "relatedLibraryObject";
   //             container = property.serializedObject.FindProperty(_dataLibraryName);
   //             if (container == null)
   //             {
   //                 if (!string.IsNullOrEmpty(_attribute.dataLibraryFieldName))
   //                 {
   //                     container = property.serializedObject.FindProperty(_attribute.dataLibraryFieldName);
   //                 }
   //             }
			
			
			//	if (container == null)
			//	{
			//		return;
			//	}
			//}

   //         if (container == null)
   //         {
   //             return;
   //         }

			//try
			//{
			//	if (container.objectReferenceValue == null)
			//	{
			//		return;
			//	}
			//}
			//catch
			//{
			//	return;
			//}


            //buttonsBuilt = true;

            var availableTypesList = dataLibrary.GetAllInitialDataObjectsByType(fieldInfo.FieldType, _attribute.includeSubtypes);

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



				var _expInspector = SmallButton(quickAccessIcon);
                _expInspector.RegisterCallback<ClickEvent>(click =>
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
                    var _hasExposeToInspector = PropertyUtility.HasExposeToInspector(_editor);
					var _objs = _data.CollectObjects();
					if (_objs != null)
					{
                        _hasExposeToInspector = true;
					}
                    _expInspector.SetEnabled(_hasExposeToInspector);
                    _expInspector.tooltip = _hasExposeToInspector ? "View data with the [ExposeToInspector] attribute" : "No fields with [ExposeToInspector] attribute assigned";
				}

                var _searchButton = SmallButton(searchIcon);
				_searchButton.tooltip = "Select object in Databrain editor";
				//_searchButton.style.marginRight = -2;

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


				//rootButtons.Add(_unassignButton);
				rootButtons.Add(_addButton);
                rootButtons.Add(_expInspector);
                rootButtons.Add(_searchButton);
				

				if (_data != null)
				{
					if (Application.isPlaying && _data.runtimeClone != null)
					{
						var _searchRuntimeObject = SmallButton(searchRuntimeIcon);
						_searchRuntimeObject.tooltip = "Select runtime object";
						_searchRuntimeObject.RegisterCallback<ClickEvent>(click =>
						{
                            // var _dataObject = (container.objectReferenceValue as DataLibrary).GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, _fieldType);
                            var _dataObject = dataLibrary.GetInitialDataObjectByGuid(availableTypesList[selectedIndex - 1].guid); //, _fieldType);
                            
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
								if ((container.objectReferenceValue as DataLibrary).runtimeLibrary != null)
								{
									// var _window = DatabrainHelpers.OpenEditor((container.objectReferenceValue as DataLibrary).runtimeLibrary.GetInstanceID(), false);
                                    var _window = DatabrainHelpers.OpenEditor(dataLibrary.runtimeLibrary.GetInstanceID(), false);
									_window.SelectDataObject(_rtDataObject);
								}
							}


                        });


						rootButtons.Add(_searchRuntimeObject);
					}
				}
			}
		}


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

                DatabrainHelpers.SetBorder(_root, 2, new Color(209f / 255f, 89f / 255f, 89f / 255f, 255f / 255f));

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
#endif