/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Attributes;
using Databrain.Modules;
using Databrain.Helpers;
using Databrain.UI.Elements;
using Databrain.UI;
using Databrain.Modules.SaveLoad;

namespace Databrain
{
#pragma warning disable 0162
	public class DatabrainEditorWindow : EditorWindow
	{
		 
		public DataLibrary container;

        // UI ASSETS
        public VisualTreeAsset visualAsset;
		public VisualTreeAsset typeListElementAsset;
		public VisualTreeAsset dataListElementAsset;
		public VisualTreeAsset foldoutAsset;
		public VisualTreeAsset searchFieldAsset;
		public VisualTreeAsset moduleButtonAsset;
		public StyleSheet styleSheet;
		
		// UI ELEMENTS
		private ScrollView typeListSV;
		private VisualElement dataTypeListContainer;
		private VisualElement dataInspectorVE;
		private VisualElement dataInspectorBaseVE;
		private VisualElement colorIndicatorVE;
		private VisualElement searchFieldVE;
		private VisualElement searchResultContainer;
		private VisualElement dataFilterContainer;
		private ScrollView dataViewScrollView;
		private SplitView splitView1;
		private SplitView splitView2;

		private ToolbarButton createDataTypeButton;
		private ToolbarButton clearButton;
		private ToolbarButton resetDataButton;
		private ToolbarButton openFileButton;
        private ToolbarButton gotoRuntimeObjectButton;

		private TextField filterInput;
        
		private Label statusLabel;
		private Label titleLabel;
		private Button typeListButton;
		private Label dataTitle;
		private VisualElement logoIconVE;
        private Foldout favoritesFoldout;
        private VisualElement runtimeOverlayBlock;
        public ListView dataTypelistView;

		// EDITOR
		public Action<KeyCode> DatacoreKeyUpEditorEvent;

		private bool windowDestroyed = true;
        private static  Type selectedDataType;
		private static string selectedGuid;
        private int selectedTypeIndex;
		private int selectedObjectIndex;
		
		private Texture2D logoLarge;
		private Texture2D logoIcon;
		
		private Dictionary<string, List<DataTypes>> rootDataTypes = new Dictionary<string, List<DataTypes>>();
		private List<Button> typeButtons = new List<Button>();
		private List<Foldout> namespaceFoldouts = new List<Foldout>();		
		private int selectedSearchResultIndex;

		
		public List<DatabrainHelpers.SortedTypes> sortedTypes = new List<DatabrainHelpers.SortedTypes>();
		public List<DataObject> selectedDataObjects;
		List<DataObject> availableObjsList;


        public class SearchResultData
		{
			public Type type;
			public string guid;
			
			public SearchResultData (Type _type, string _guid)
			{
				type = _type;
				guid = _guid;
			}
		}
		
		public class DataTypes
		{
			public Type type;
			
			public List<DataTypes> dataTypes = new List<DataTypes>();
	
			public DataTypes(){}
			public DataTypes(Type _type)
			{
				type = _type;	
			}

			public bool AddChildType(Type _childType)
			{

				if (_childType.BaseType.IsGenericType)
				{
					var _isAssignable = _childType.BaseType.GetGenericTypeDefinition().IsAssignableFrom(type);
					if (_isAssignable)
					{
						dataTypes.Add(new DataTypes(_childType));
						return true;
					}
					else
					{
						for (int i = 0; i < dataTypes.Count; i++)
						{
                            var _return = dataTypes[i].AddChildType(_childType);
                            if (_return)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    return false;

                }
				else
				{
					
					if (_childType.BaseType == type)
					{
						dataTypes.Add(new DataTypes(_childType));
						return true;
					}
					else
					{
						for (int i = 0; i < dataTypes.Count; i++)
						{
							var _return = dataTypes[i].AddChildType(_childType);
							if (_return)
							{
								return true;
							}
						}
						return false;
                    }
				}
				
				return false;
			}
			
			public void SetWarningIcon(DataLibrary _container, List<Button> _typeButtons)
			{
				if (_container.isRuntimeContainer)
					return;
				
			

				var _isValid = true;
				for(var i = 0; i < _container.data.ObjectList.Count; i ++)
				{
					if (_container.data.ObjectList[i].type == type.AssemblyQualifiedName)
					{
						for (int j = 0; j < _container.data.ObjectList[i].dataObjects.Count; j++)
						{
                            if (_container.data.ObjectList[i].dataObjects[j] == null)
                                continue;

                            if (_isValid)
							{
								_isValid = _container.data.ObjectList[i].dataObjects[j].IsValid();
							}
						}
					}
				}

				
				
				for (int i = 0; i < _typeButtons.Count; i ++)
				{
					if (_typeButtons[i].parent.name == type.Name)
					{
						var _listElement = _typeButtons[i].parent;
						var _warningIcon = _listElement.Q<VisualElement>("warningIcon");
								
						_warningIcon.style.display = _isValid ? DisplayStyle.None : DisplayStyle.Flex;
					}
				}		
				
				for (int i = 0; i < dataTypes.Count; i++)
				{
					dataTypes[i].SetWarningIcon(_container, _typeButtons);
				}
				
			}
			
			public void SetSaveIcon(DataLibrary _container, List<Button> _typeButtons)
			{
				if (_container.isRuntimeContainer)
					return;
					
				
					
				var _runtimeSerialization = _container.IsRuntimeSerialization(type.Name);
				
				
				
				for (int i = 0; i < _typeButtons.Count; i ++)
				{
					if (_typeButtons[i].parent.name == type.Name)
					{
						var _listElement = _typeButtons[i].parent;
						var _warningIcon = _listElement.Q<VisualElement>("saveIcon");
								
						_warningIcon.style.display = _runtimeSerialization ? DisplayStyle.Flex : DisplayStyle.None;
					}
				}		
				
				
				for (int i = 0; i < dataTypes.Count; i++)
				{
					dataTypes[i].SetSaveIcon(_container, _typeButtons);
				}
			}
			
			public void Build(Action<Type> _action, VisualElement _parentElement, VisualTreeAsset _listElement, Foldout _foldout, List<Button> _typeButtons, int _depth)
			{
				bool _skip = false;
				
				var _attributes = type.GetCustomAttributes().ToList();
				for (int a = 0; a < _attributes.Count; a ++)
				{
					if (_attributes[a].GetType() == typeof (HideDataObjectTypeAttribute))
					{
						_skip = true;			
					}

					//if (_attributes[a].GetType() == typeof(DataObjectUseNamespace))
					//{
					//	_skip = true;
					//}
				}



				if (_skip)
				{
					//Debug.Log("SKIP: " + type.Name);
				}


				if (dataTypes.Count > 0)
				{
					
					if (type.BaseType != typeof(DataObject) && !_skip)
					{
						_listElement.CloneTree(_parentElement);
						var typeListElement = _parentElement.Q<VisualElement>("typeListElement");
						var _typeButton = _parentElement.Query<VisualElement>("typeListElement").Children<Button>("typeListButton").First();
						var _typeIcon = _parentElement.Query<VisualElement>("typeListElement").Children<VisualElement>("typeIcon").First();
		    	
						var _objectBoxIconAttribute = type.GetCustomAttribute(typeof(DataObjectIconAttribute)) as DataObjectIconAttribute;
		
						if (_objectBoxIconAttribute != null)
						{
							if (!string.IsNullOrEmpty(_objectBoxIconAttribute.iconColorHex))
							{
								Color _hexColor = Color.white;
								ColorUtility.TryParseHtmlString(_objectBoxIconAttribute.iconColorHex, out _hexColor);
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_hexColor);
							}
							else
							{
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_objectBoxIconAttribute.iconColor);
							}
							_typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon(_objectBoxIconAttribute.iconPath);
						}
		    	
						_typeButton.name = "button" + type.Name;
						typeListElement.name = type.Name;
						_typeButton.userData = type;

						var _customTypeNameAttribute = type.GetCustomAttribute(typeof(DataObjectTypeNameAttribute), false) as DataObjectTypeNameAttribute;
                        if (_customTypeNameAttribute != null)
						{
							_typeButton.text = _customTypeNameAttribute.typeName;
						}
						else
						{
							_typeButton.text = type.Name;
						}

						_typeButton.tooltip = "Type: " + type.Name;

						//typeListElement.style.marginLeft = _depth * 15;
						
						_typeButtons.Add(_typeButton);
						
						_typeButton.RegisterCallback<ClickEvent>((clickEvent) => 
						{
							selectedDataType = type;
                            selectedGuid = "";
                            //DataLibrary.selectedType = type;
							
							_action.Invoke(type);	
							
							
							for (int i = 0; i < _typeButtons.Count; i ++)
							{
								if (_typeButtons[i].name == _typeButton.name)
									continue;
					
								_typeButtons[i].EnableInClassList("listElement--checked", false);
							}
							

							
							_typeButton.EnableInClassList("listElement--checked", true);
							
						});
						
						if (_foldout != null)
						{
							//typeListElement.style.marginLeft = (_depth - 1) * 15;
							_foldout.Add(typeListElement);
						}
						else
						{
							_parentElement.Add(typeListElement);
						}
						
					}
					
					_depth ++;


					Foldout _subFoldout = _foldout;
					if (dataTypes.Count > 0 && _depth > 1)
					{
						// Create foldout element and add it to the build method
						_subFoldout = new Foldout();
						_subFoldout.style.fontSize = 10;

						var _subTypeName = type.Name;
						if (_subTypeName.Contains("`1"))
						{
							_subTypeName = _subTypeName.Replace("`1", "");
						}

						_subFoldout.text = "Subtypes: " + _subTypeName;
						_subFoldout.name = "Subtypes: " + _subTypeName;

						_foldout.Add(_subFoldout);
					}


					for (int c = 0; c < dataTypes.Count; c ++)
					{
                        dataTypes[c].Build(_action, _parentElement, _listElement, _subFoldout, _typeButtons, _depth);
					}
				}
				else
				{
					if (type.BaseType != typeof(DataObject) && !_skip)
					{
	
						_listElement.CloneTree(_parentElement);	
						var _typeListElement = _parentElement.Q<VisualElement>("typeListElement");
						var _typeButton = _parentElement.Query<VisualElement>("typeListElement").Children<Button>("typeListButton").First();
						var _typeIcon = _parentElement.Query<VisualElement>("typeListElement").Children<VisualElement>("typeIcon").First();
		    	
						var _datacoreIconDataAttribute = type.GetCustomAttribute(typeof(DataObjectIconAttribute)) as DataObjectIconAttribute;
		
						if (_datacoreIconDataAttribute != null)
						{
							if (!string.IsNullOrEmpty(_datacoreIconDataAttribute.iconColorHex))
							{
								Color _hexColor = Color.white;
								ColorUtility.TryParseHtmlString(_datacoreIconDataAttribute.iconColorHex, out _hexColor);
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_hexColor);
							}
							else
							{
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_datacoreIconDataAttribute.iconColor);
							}
							_typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon(_datacoreIconDataAttribute.iconPath);
						}
						
						_typeButton.name = "button" + type.Name;
						_typeListElement.name = type.Name;
						_typeButton.userData = type;

                        var _customTypeNameAttribute = type.GetCustomAttribute(typeof(DataObjectTypeNameAttribute), false) as DataObjectTypeNameAttribute;
						if (_customTypeNameAttribute != null)
						{
                            _typeButton.text = _customTypeNameAttribute.typeName;
                        }
						else
						{
							_typeButton.text = type.Name;
						}

                        _typeButton.tooltip = "Type: " + type.Name;

                        //_typeListElement.style.marginLeft = _depth * 15;
						
						_typeButtons.Add(_typeButton);
						
						_typeButton.RegisterCallback<ClickEvent>((clickEvent) => 
						{
							selectedDataType = type;
                            selectedGuid = "";
							
                            //DataLibrary.selectedType = type;
							
							_action.Invoke(type);
							
							
							for (int i = 0; i < _typeButtons.Count; i ++)
							{
								if (_typeButtons[i].name == _typeButton.name)
									continue;
					
								_typeButtons[i].EnableInClassList("listElement--checked", false);
							}
							
							_typeButton.EnableInClassList("listElement--checked", true);
						});
						
						
						if (_foldout != null)
						{
							//_typeListElement.style.marginLeft = (_depth -1) * 15;
							_foldout.Add(_typeListElement);
						}
						else
						{
                            _parentElement.Add(_typeListElement);
						}
						
					}


                    _depth++;


                    Foldout _subFoldout = _foldout;
                    if (dataTypes.Count > 0 && _depth > 1)
                    {
                        // Create foldout element and add it to the build method
                        _subFoldout = new Foldout();
                        _subFoldout.style.fontSize = 10;

                        var _subTypeName = type.Name;
                        if (_subTypeName.Contains("`1"))
                        {
                            _subTypeName = _subTypeName.Replace("`1", "");
                        }

                        _subFoldout.text = "Subtypes: " + _subTypeName;
                        _subFoldout.name = "Subtypes: " + _subTypeName;

                        _foldout.Add(_subFoldout);
                    }


                    for (int c = 0; c < dataTypes.Count; c++)
                    {
                        dataTypes[c].Build(_action, _parentElement, _listElement, _subFoldout, _typeButtons, _depth);
                    }




                }
				
			}
	
		}
		
		void OnEnable()
		{
			this.SetAntiAliasing(4);
			DataLibrary.ResetEvent += Reset;

            EditorApplication.playModeStateChanged -= PlayModeChanged;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            
        }

		private void OnDisable()
		{
            EditorApplication.playModeStateChanged -= PlayModeChanged;
            DataLibrary.ResetEvent -= Reset;
            windowDestroyed = true;          
        }

		void PlayModeChanged(PlayModeStateChange _state)
		{
			if (_state == PlayModeStateChange.EnteredPlayMode)
			{
				if (!container.isRuntimeContainer)
					return;

				if (runtimeOverlayBlock != null)
				{
					runtimeOverlayBlock.visible = false;
                }				
			}

			if (_state == PlayModeStateChange.ExitingPlayMode)
			{
				if (!container.isRuntimeContainer)
					return;

				if (runtimeOverlayBlock != null)
				{
					runtimeOverlayBlock.visible = true;
				}
            }
        }

		public void Reset()
		{
			if (dataInspectorVE != null)
			{
				dataInspectorVE.Clear();
			}
			if (dataInspectorBaseVE != null)
			{
				dataInspectorBaseVE.Clear();
			}
		}

		public void DisableEditing()
		{
			createDataTypeButton.SetEnabled(false);
			resetDataButton.SetEnabled(false);
			clearButton.SetEnabled(false);
		}
		
		[DidReloadScripts]
		public static void Reload() 
		{
			if (HasOpenInstances<DatabrainEditorWindow>())
			{
				// Get container ids before closing all windows
                //var _containerID = EditorPrefs.GetInt("Databrain_containerID");
                //var _runtimeContainerID = EditorPrefs.GetInt("Databrain_runtimeContainerID");

                // OBSOLETE
                // Instead of closing all windows -> which would lead to unwanted behaviour when windows are docked
                // we simply re-open existing windows to make sure they get re-initialized.
                // _containerID + _runtimeContainerID are therefore also obsolete as they would only allow one databrain editor.
                //---------------------------------------------------
                // Close all windows
                //DatabrainEditorWindow[] _w = Resources.FindObjectsOfTypeAll<DatabrainEditorWindow>();
                //for (int i = 0; i < _w.Length; i++)
                //{
                //	_w[i].Close();
                //}

                // reopen and initialize them
                //if (_containerID != 0)
                //{
                //	DataLibrary _container = EditorUtility.InstanceIDToObject(_containerID) as DataLibrary;
                //	if (_container != null)
                //	{
                //		DatabrainHelpers.OpenEditor(_containerID, true);
                //	}
                //}


                //if (_runtimeContainerID != 0)
                //{
                //	DataLibrary _runtimeContainer = EditorUtility.InstanceIDToObject(_runtimeContainerID) as DataLibrary;
                //	if (_runtimeContainer != null)
                //	{
                //		DatabrainHelpers.OpenEditor(_runtimeContainerID, true);
                //	}
                //}
                //---------------------------------------------------

				// Re-Open existing databrain windows
                DatabrainEditorWindow[] _w = Resources.FindObjectsOfTypeAll<DatabrainEditorWindow>();
                for (int i = 0; i < _w.Length; i++)
                {             
                    DataLibrary _container = EditorUtility.InstanceIDToObject(_w[i].container.GetInstanceID()) as DataLibrary;
                    if (_container != null)
                    {
                        DatabrainHelpers.OpenEditor(_w[i].container.GetInstanceID(), false);


						DataErrorCheck(_container);
                    }
                }
            }
        }

		public void SetupForceRebuild(DataLibrary _library)
		{
			windowDestroyed = true;
			container.existingNamespaces = null;
			typeButtons = new List<Button>();
			typeListSV.Clear();
			rootDataTypes = new Dictionary<string, List<DataTypes>>();
			favoritesFoldout = null;
			searchFieldVE = null;
            Setup(_library);
		}

		public void Setup(DataLibrary _library, DataLibrary _initialLibrary = null) 
		{
#if DATABRAIN_DEBUG
			Debug.Log("SETUP " + windowDestroyed);
#endif
			container = _library;

			if (container.isRuntimeContainer)
			{
				if (_initialLibrary != null)
				{
					if (_initialLibrary.hierarchyTemplate != null)
					{
						container.hierarchyTemplate = _initialLibrary.hierarchyTemplate;
					}
				}


                EditorPrefs.SetInt("Databrain_runtimeContainerID", container.GetInstanceID()) ;
			}
			else
			{
                EditorPrefs.SetInt("Databrain_containerID", container.GetInstanceID());
            }

			// rebuild windows
			if (rootVisualElement.childCount == 0)
			{
				windowDestroyed = true;
			}

            if (windowDestroyed)
			{
                dataTypelistView = null;

				DataErrorCheck(container);
                PopulateView();
				SetupModules();
				UpdateData();

				windowDestroyed = false;
	
            }

			// Check if runtime library exists
			if (container.runtimeLibrary == null)
			{
				if (!string.IsNullOrEmpty(container.runtimeLibraryFolderPath))
				{
					var _saveLoadModule = container.modules[0];
					if (_saveLoadModule != null)
					{
						(_saveLoadModule as SaveLoadModule).CreateRuntimeDataLibraryObjectAtPath(container, container.runtimeLibraryFolderPath);
					}
				}
			}

            ShowLastSelected();
        } 


		private void OnDestroy()
		{
			if (container.isRuntimeContainer)
			{
				EditorPrefs.DeleteKey("Databrain_runtimeContainerID");
			}
			else
			{
                EditorPrefs.DeleteKey("Databrain_containerID");
            }

			try
			{
				if (splitView1 != null && container != null)
				{
					container.firstColumnWidth = splitView1.fixedPane.style.width.value.value;
				}

				if (splitView2 != null && container != null)
				{
					container.secondColumnWidth = splitView2.fixedPane.style.width.value.value;
				}
			}
			catch { }

            windowDestroyed = true;
		}

		private void SetupModules()
		{
			if (container.isRuntimeContainer)
				return;
			

			// Gather all available modules
			var _modules = TypeCache.GetTypesDerivedFrom<DatabrainModuleBase>();
			List<DatabrainHelpers.SortedTypes> _sortedModules = new List<DatabrainHelpers.SortedTypes>();
			
			for (int i = 0; i < _modules.Count; i ++)
			{
				_sortedModules.Add(new DatabrainHelpers.SortedTypes(i, _modules[i]));
			}
			
			for (int i = 0; i < _modules.Count; i ++)
			{
				// Sort by attribute
				var _orderAttribute = _modules[i].GetCustomAttribute(typeof(DatacoreModuleAttribute)) as DatacoreModuleAttribute;
					
				if (_orderAttribute != null)
				{
					// Get order number
					var _order = (_orderAttribute as DatacoreModuleAttribute).order;	
					_sortedModules[i].index = _order;		
				}
				else
				{
					_sortedModules[i].index = -1;
				}
			}
			
			// Sort by order attribute
			_sortedModules = _sortedModules.OrderBy(x => x.index).ToList();
			
			
			for (int t = 0; t < _sortedModules.Count; t ++)
			{
				bool _moduleExists = false;
				for (int j = 0; j < container.modules.Count; j ++)
				{
					if (container.modules[j] != null)
					{
						if (container.modules[j].GetType() == _sortedModules[t].type)
						{
							_moduleExists = true;
						}
					}
				}
				
				if (!_moduleExists)
				{
					var _module = ScriptableObject.CreateInstance(_sortedModules[t].type);
					_module.name = _sortedModules[t].type.Name;

					_module.hideFlags = HideFlags.HideInHierarchy;

					container.modules.Add(_module as DatabrainModuleBase);
					
					AssetDatabase.AddObjectToAsset(_module, container);
						
					EditorUtility.SetDirty(_module); 
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();


                    // Do some initialization on the module if needed
                    (_module as DatabrainModuleBase).Initialize(container);

                }
			}
			
		
			// Build UI Module buttons
			var _topbar = rootVisualElement.Q<VisualElement>("topBar");
			for (int m = 0; m < container.modules.Count; m ++)
			{
				if (container.modules[m] != null)
				{
					moduleButtonAsset.CloneTree(_topbar);
					var _moduleButton = _topbar.Q<VisualElement>("moduleButton");

					_moduleButton.name = container.modules[m].name;

					var _attribute = container.modules[m].GetType().GetCustomAttribute(typeof(DatacoreModuleAttribute)) as DatacoreModuleAttribute;

					if (_attribute != null)
					{
						if (!string.IsNullOrEmpty(_attribute.icon))
						{
							var _icon = DatabrainHelpers.LoadIcon(_attribute.icon);
							var _iconElement = _moduleButton.Q<VisualElement>("icon");
							_iconElement.style.backgroundImage = _icon;
						}
					}


					int _mi = m;
					_moduleButton.RegisterCallback<ClickEvent>(click =>
					{
						ShowModule(_mi);
					});


					_topbar.Add(_moduleButton);
				}
			}
		}
		  
		
		private void PopulateView()
		{


            if (container.themeTemplate != null)
            {
                // Load Visual Assets
                visualAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.visualAsset : container.themeTemplate.serializedGroup.light.visualAsset;
                foldoutAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.foldoutAsset : container.themeTemplate.serializedGroup.light.foldoutAsset;
                typeListElementAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.typeListElementAsset : container.themeTemplate.serializedGroup.light.typeListElementAsset;
                dataListElementAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.dataListElementAsset : container.themeTemplate.serializedGroup.light.dataListElementAsset;
                searchFieldAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.searchFieldAsset : container.themeTemplate.serializedGroup.light.searchFieldAsset;
                moduleButtonAsset = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.moduleButtonAsset : container.themeTemplate.serializedGroup.light.moduleButtonAsset;

                // Load stylesheet
                styleSheet = EditorGUIUtility.isProSkin ? container.themeTemplate.serializedGroup.dark.styleSheet : container.themeTemplate.serializedGroup.light.styleSheet;
            }
            else
            {
                // Load Visual Assets
                visualAsset = DatabrainHelpers.GetVisualAsset("DatabrainVisualAsset.uxml");
                foldoutAsset = DatabrainHelpers.GetVisualAsset("BaseFoldout.uxml");
                typeListElementAsset = DatabrainHelpers.GetVisualAsset("TypeListElementVisualAsset.uxml");
                dataListElementAsset = DatabrainHelpers.GetVisualAsset("DataListElementVisualAsset.uxml");
                searchFieldAsset = DatabrainHelpers.GetVisualAsset("SearchFieldAsset.uxml");
                moduleButtonAsset = DatabrainHelpers.GetVisualAsset("ModuleButtonVisualAsset.uxml");

                // Load stylesheet
                styleSheet = DatabrainHelpers.GetStyleSheet("DatabrainStyleSheet.uss");
            }


            //dataTypesVisualElements = new List<VisualElement>();
            namespaceFoldouts = new List<Foldout>();
			
	        // Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

            rootVisualElement.Clear();
			
		    visualAsset.CloneTree(root);
		   
			root.styleSheets.Add(styleSheet);
		    
	    	
			colorIndicatorVE = root.Q<VisualElement>("colorIndicator");
	    	typeListSV = root.Q<ScrollView>("typeList");
	    	dataTypeListContainer = root.Q<VisualElement>("dataTypeList");
			dataInspectorVE = root.Q<VisualElement>("dataInspector");
			dataInspectorBaseVE = root.Q<VisualElement>("dataInspectorBase");
			dataFilterContainer = root.Q<VisualElement>("dataFilter");
            createDataTypeButton = root.Q<ToolbarButton>("createDataTypeButton");		
            resetDataButton = root.Q<ToolbarButton>("resetDataButton");
			openFileButton = root.Q<ToolbarButton>("openFileButton");	
			gotoRuntimeObjectButton = root.Q<ToolbarButton>("gotoRuntimeObjectButton");
            statusLabel = root.Q<Label>("statusInfoLabel");
			titleLabel = root.Q<Label>("titleLabel");
			dataTitle = root.Q<Label>("dataTitle");
			logoIconVE = root.Q<VisualElement>("logoIcon");
            splitView1 = root.Q<SplitView>("splitview1");
            splitView2 = root.Q<SplitView>("splitview2");

			titleLabel.text = container.name;

            logoIcon = DatabrainHelpers.LoadLogoIcon();
			logoLarge = DatabrainHelpers.LoadLogoLarge();
	    	
			logoIconVE.style.backgroundImage = logoIcon;
	

	    	createDataTypeButton.RegisterCallback<ClickEvent>(evt => 
	    	{
	    		if (selectedDataType != null)
			    {
			    	CreateNewDataObject();
			    }
	    	});


            resetDataButton.RegisterCallback<ClickEvent>((clickEvent) =>
			{
				ResetData();
			});
			
			openFileButton.RegisterCallback<ClickEvent>((clickEvent) => 
			{
				EditFile();
			});


            gotoRuntimeObjectButton.SetEnabled(false);
            gotoRuntimeObjectButton.RegisterCallback<ClickEvent>(click =>
			{

				var _obj = gotoRuntimeObjectButton.userData as DataObject;

                var _window = DatabrainHelpers.OpenEditor((_obj.runtimeClone.runtimeLibraryObject).GetInstanceID(), false);

                _window.Setup(_obj.runtimeClone.runtimeLibraryObject);
                _window.SelectDataObject(_obj.runtimeClone);
            });


			if (container.firstColumnWidth > 0)
			{
				if (splitView1 != null)
				{
					splitView1.fixedPaneInitialDimension = container.firstColumnWidth;
				}
			}
			if (container.secondColumnWidth > 0)
			{
				if (splitView2 != null)
				{
					splitView2.fixedPaneInitialDimension = container.secondColumnWidth;
				}
			}

			// Update the namespace list based on default hierarchy or template
			DatabrainHelpers.UpdateNamespaces(container, out sortedTypes);


			for (int n = 0; n < container.existingNamespaces.Count; n ++)
			{
                if (!rootDataTypes.ContainsKey(container.existingNamespaces[n].namespaceName))
                {
                    rootDataTypes.Add(container.existingNamespaces[n].namespaceName, new List<DataTypes>());
                }
            }


            foreach ( var _nameSpace in rootDataTypes.Keys)
			{
				List<DatabrainHelpers.SortedTypes> _cleanUpList = new List<DatabrainHelpers.SortedTypes>();
				
				for (int t = 0; t < sortedTypes.Count; t ++)
				{
					var _n = "Global";
					if (sortedTypes[t].type.Namespace != null)
					{
						_n = sortedTypes[t].type.Namespace;
					}


					if (_n == _nameSpace && sortedTypes[t].type.BaseType == typeof(DataObject))
					{
						rootDataTypes[_nameSpace].Add(new DataTypes(sortedTypes[t].type));
					
						_cleanUpList.Add(sortedTypes[t]);
					}
				}
				
				
				for (int c = 0; c < _cleanUpList.Count; c ++)
				{
					sortedTypes.Remove(_cleanUpList[c]);
				}
				
				
			}

			int _loopCount = 0;

			while (sortedTypes.Count > 0)
			{
				foreach( var _nameSpace in rootDataTypes.Keys)
				{
					List<DatabrainHelpers.SortedTypes> _cleanupList = new List<DatabrainHelpers.SortedTypes>();
					for (int t = 0; t < sortedTypes.Count; t ++)
					{	
						for (int r = 0; r < rootDataTypes[_nameSpace].Count; r ++)
						{
							var _return = rootDataTypes[_nameSpace][r].AddChildType(sortedTypes[t].type);
							if (_return)
							{
                                _cleanupList.Add(sortedTypes[t]);
							}							
						}				
					}
				
					for (int c = 0; c < _cleanupList.Count; c ++)
					{
						sortedTypes.Remove(_cleanupList[c]);
					}					
				}

			

				_loopCount++;
				if (_loopCount > 100)
				{
					break;
				}

            }

	    	typeListSV.Clear();
			

            typeButtons = new List<Button>();

            // Build favorites list
            BuildFavoritesList();

			if (container.hierarchyTemplate == null)
			{
				BuildTypeHierarchyDefault();
			}
			else
			{
				BuildTypeHierarchyFromTemplate();
			}


            if (searchFieldVE == null)
			{
				searchFieldAsset.CloneTree(root);
				searchFieldVE = root.Q<VisualElement>("searchBar");
				searchFieldVE.visible = false;
			}
			
			
			SetupSearchBar();	
			ShowIsRuntimeContainer();

			if (root.panel != null)
			{
				//rootVisualElement.UnregisterCallback<KeyUpEvent>(OnKeyUpShortcut);
				//rootVisualElement.RegisterCallback<KeyUpEvent>(OnKeyUpShortcut);
				root.panel.visualTree.UnregisterCallback<KeyUpEvent>(OnKeyUpShortcut);
				root.panel.visualTree.RegisterCallback<KeyUpEvent>(OnKeyUpShortcut);
			}

		}



		void BuildTypeHierarchyDefault()
		{
            // Build Types buttons
            foreach (var _namespace in rootDataTypes.Keys)
            {
                var _namespaceFoldout = SetupBaseFoldout(typeListSV, _namespace) as Foldout;

                _namespaceFoldout.value = false;
                _namespaceFoldout.RegisterCallback<ChangeEvent<bool>>(e =>
                {
                    SetNamespaceFoldoutValue(_namespace, e.newValue);
                });


                for (int d = 0; d < rootDataTypes[_namespace].Count; d++)
                {
                    // First create root types buttons
                    int _i = d;

                    var _datacoreDataIconAttribute = rootDataTypes[_namespace][_i].type.GetCustomAttribute(typeof(DataObjectIconAttribute)) as DataObjectIconAttribute;
                    var _hideDataObjectAttribute = rootDataTypes[_namespace][_i].type.GetCustomAttribute(typeof(HideDataObjectTypeAttribute)) as HideDataObjectTypeAttribute;

                    if (_hideDataObjectAttribute == null)
                    {

                        typeListElementAsset.CloneTree(typeListSV);

                        var _typeListElement = typeListSV.Q<VisualElement>("typeListElement");
                        var _rootButton = typeListSV.Query<VisualElement>("typeListElement").Children<Button>("typeListButton").First();
                        var _typeIcon = typeListSV.Query<VisualElement>("typeListElement").Children<VisualElement>("typeIcon").First();


                        _typeListElement.name = rootDataTypes[_namespace][_i].type.Name;


                        if (_datacoreDataIconAttribute != null)
                        {
							if (!string.IsNullOrEmpty(_datacoreDataIconAttribute.iconColorHex))
							{
								Color _hexColor = Color.white;
								ColorUtility.TryParseHtmlString(_datacoreDataIconAttribute.iconColorHex, out _hexColor);
								
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_hexColor);
							}
							else
							{
								_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_datacoreDataIconAttribute.iconColor);
							}
                            _typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon(_datacoreDataIconAttribute.iconPath);
                        }


                        _rootButton.name = "button" + rootDataTypes[_namespace][_i].type.Name;
                        _rootButton.userData = rootDataTypes[_namespace][_i].type;


                        var _customTypeNameAttribute = rootDataTypes[_namespace][_i].type.GetCustomAttribute<DataObjectTypeNameAttribute>();
                        if (_customTypeNameAttribute != null)
                        {
                            _rootButton.text = _customTypeNameAttribute.typeName;
                        }
                        else
                        {

                            var _typeName = rootDataTypes[_namespace][_i].type.Name;
                            if (_typeName.Contains("`1"))
                            {
                                _typeName = _typeName.Replace("`1", "");
                            }
                            _rootButton.text = _typeName;
                        }

                        _rootButton.tooltip = "Type: " + rootDataTypes[_namespace][_i].type.Name;


                        typeButtons.Add(_rootButton);

                        _rootButton.RegisterCallback<ClickEvent>((clickEvent) =>
                        {
                            selectedDataType = rootDataTypes[_namespace][_i].type;
                            selectedGuid = "";

                            //DataLibrary.selectedType = rootDataTypes[_namespace][_i].type;


                            PopulateDataTypeList(rootDataTypes[_namespace][_i].type);
                            ResetTypeButtonsHighlight(_rootButton.name);

                            _rootButton.EnableInClassList("listElement--checked", true);
                        });


                        _namespaceFoldout.Add(_typeListElement);
                        //typeListSV.Add(_typeListElement);

                        // Build list elements from inherited classes
                        Foldout _foldout = null;

                        if (rootDataTypes[_namespace][d].dataTypes.Count > 0)
                        {
                            // Create foldout element and add it to the build method
                            _foldout = new Foldout();
                            _foldout.style.fontSize = 10;

                            var _subTypeName = rootDataTypes[_namespace][d].type.Name;
                            if (_subTypeName.Contains("`1"))
                            {
                                _subTypeName = _subTypeName.Replace("`1", "");
                            }

                            _foldout.text = "Subtypes: " + _subTypeName;
                            _foldout.name = "Subtypes: " + _subTypeName;

                            _namespaceFoldout.Add(_foldout);

                        }

                        // Build sub types buttons
                        //for (int t = 0; t < rootDataTypes[_namespace][d].dataTypes.Count; t++)
                        //{
                        //Debug.Log(rootDataTypes[_namespace][d].dataTypes[t].type.Name);
                        rootDataTypes[_namespace][d].Build(new Action<Type>(PopulateDataTypeList), _namespaceFoldout, typeListElementAsset, _foldout, typeButtons, 0);
                        //}
                    }
                    else
                    {
                        // Build sub types buttons and add them directly to the namespace foldout
                        rootDataTypes[_namespace][d].Build(new Action<Type>(PopulateDataTypeList), _namespaceFoldout, typeListElementAsset, _namespaceFoldout, typeButtons, 0);
                    }
                }
            }

            for (int i = 0; i < container.existingNamespaces.Count; i++)
            {
                for (int e = 0; e < container.existingNamespaces[i].existingTypes.Count; e++)
                {
                    var _type = Type.GetType(container.existingNamespaces[i].existingTypes[e].typeAssemblyQualifiedName);
                    if (_type != null)
                    {
                        var _addToRuntimeLibAtt = _type.GetCustomAttribute(typeof(DataObjectAddToRuntimeLibrary));
                        if (_addToRuntimeLibAtt != null)
                        {
                            container.existingNamespaces[i].existingTypes[e].runtimeSerialization = true;
                        }
                    }
                }
            }
        }




		void BuildTypeHierarchyFromTemplate()
		{
            for (int g = 0; g < container.hierarchyTemplate.rootDatabrainTypes.subTypes.Count; g++)
            {
                var _groupIndex = g;
                var _namespaceFoldout = SetupBaseFoldout(typeListSV, container.hierarchyTemplate.rootDatabrainTypes.subTypes[g].name) as Foldout;

                _namespaceFoldout.value = false;
                _namespaceFoldout.RegisterCallback<ChangeEvent<bool>>(e =>
                {
                    SetNamespaceFoldoutValue(container.hierarchyTemplate.rootDatabrainTypes.subTypes[_groupIndex].name, e.newValue);
                });



                BuildSubTypesFromTemplate(container.hierarchyTemplate.rootDatabrainTypes.subTypes[g].name, _namespaceFoldout, container.hierarchyTemplate.rootDatabrainTypes.subTypes[g].subTypes);
			}

		}

		void BuildSubTypesFromTemplate(string _groupName, VisualElement _parentElement, List<DatabrainHierarchyTemplate.DatabrainTypes> _types)
		{

			for (int t = 0; t < _types.Count; t++)
			{
                var _extN = container.existingNamespaces.Where(x => x.namespaceName == _groupName).FirstOrDefault();
				if (_extN != null)
				{

					var _extT = _extN.existingTypes.Where(x => x.typeName == _types[t].type).FirstOrDefault();

					if (_extT == null)
					{
						// Add type
						_extN.existingTypes.Add(new DataLibrary.ExistingNamespace.ExistingTypes(_types[t].type, _types[t].assemblyQualifiedTypeName));
					}
				}
			}


            for (int t = 0; t < _types.Count; t++)
			{
				var _type = Type.GetType(_types[t].assemblyQualifiedTypeName);

				if (_type == null)
					continue;

				typeListElementAsset.CloneTree(_parentElement);
				var _typeListElement = _parentElement.Q<VisualElement>("typeListElement");
				var _typeButton = _parentElement.Query<VisualElement>("typeListElement").Children<Button>("typeListButton").First();
				var _typeIcon = _parentElement.Query<VisualElement>("typeListElement").Children<VisualElement>("typeIcon").First();

				var _objectBoxIconAttribute = _type.GetCustomAttribute(typeof(DataObjectIconAttribute)) as DataObjectIconAttribute;

				if (_objectBoxIconAttribute != null)
				{
					if (!string.IsNullOrEmpty(_objectBoxIconAttribute.iconColorHex))
					{
						Color _hexColor = Color.white;
						ColorUtility.TryParseHtmlString(_objectBoxIconAttribute.iconColorHex, out _hexColor);
                        _typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_hexColor);
					}
					else
					{
						_typeIcon.style.unityBackgroundImageTintColor = new StyleColor(_objectBoxIconAttribute.iconColor);
					}
					_typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon(_objectBoxIconAttribute.iconPath);
				}

				_typeButton.name = "button" + _type.Name;
                _typeListElement.name = _type.Name;
				_typeButton.userData = _type;

                _parentElement.Add(_typeListElement);


				if (string.IsNullOrEmpty(_types[t].name))
				{
					var _customTypeNameAttribute = _type.GetCustomAttribute<DataObjectTypeNameAttribute>();
					if (_customTypeNameAttribute != null)
					{
						_typeButton.text = _customTypeNameAttribute.typeName;
					}
					else
					{
                        _typeButton.text = _types[t].name;
                    }
				}
				else
				{
					_typeButton.text = _types[t].name;
				}

				_typeButton.tooltip = "Type: " + _type.Name;


                typeButtons.Add(_typeButton);

                _typeButton.RegisterCallback<ClickEvent>((clickEvent) =>
                {
                    selectedDataType = _type;
                    selectedGuid = "";

                    //DataLibrary.selectedType = _type;


                    PopulateDataTypeList(_type);
                    ResetTypeButtonsHighlight(_typeButton.name);

                    _typeButton.EnableInClassList("listElement--checked", true);
                });




                if (_types[t].subTypes.Count > 0)
				{
					// Create foldout element and add it to the build method
					var _foldout = new Foldout();
					_foldout.style.fontSize = 10;

                    var _subTypeName = _type.Name;
                    if (_subTypeName.Contains("`1"))
                    {
                        _subTypeName = _subTypeName.Replace("`1", "");
                    }

                    _foldout.text = "Subtypes: " + _subTypeName;
                    _foldout.name = "Subtypes: " + _subTypeName;

					_parentElement.Add(_foldout);


                    BuildSubTypesFromTemplate(_groupName, _foldout, _types[t].subTypes);
				} 
			}
		}



        public void BuildFavoritesList()
		{
		

			if (favoritesFoldout == null)
			{
				var _favoritesFoldout = SetupBaseFoldout(typeListSV, "Favorites") as Foldout;
				_favoritesFoldout.value = false;

				var _favoritesIcon = _favoritesFoldout.parent.Q<VisualElement>("foldoutIcon");
				_favoritesIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("favorite.png");
				_favoritesIcon.style.unityBackgroundImageTintColor = new StyleColor(new Color(248f/255f, 138f/255f, 137f/255f));

                favoritesFoldout = _favoritesFoldout;

                favoritesFoldout.BringToFront();
            }
			else
			{
				favoritesFoldout.Clear();
			}


            for (int f = 0; f < container.data.FavoriteList.Count; f++)
            {
                typeListElementAsset.CloneTree(typeListSV);
                var _typeListElement = typeListSV.Q<VisualElement>("typeListElement");
                var _rootButton = typeListSV.Query<VisualElement>("typeListElement").Children<Button>("typeListButton").First();
                var _typeIcon = typeListSV.Query<VisualElement>("typeListElement").Children<VisualElement>("typeIcon").First();

				if (container.data.FavoriteList[f].dataObjects[0] != null)
				{
					var _iconAttribute = container.data.FavoriteList[f].dataObjects[0].GetType().GetCustomAttribute(typeof(DataObjectIconAttribute));
					if (_iconAttribute != null)
					{
						_typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon((_iconAttribute as DataObjectIconAttribute).iconPath);
						if (!string.IsNullOrEmpty((_iconAttribute as DataObjectIconAttribute).iconColorHex))
						{
							Color _hexColor = Color.white;
							ColorUtility.TryParseHtmlString((_iconAttribute as DataObjectIconAttribute).iconColorHex, out _hexColor);
							
							_typeIcon.style.unityBackgroundImageTintColor = new StyleColor( _hexColor);
                        }
						else
						{
							_typeIcon.style.unityBackgroundImageTintColor = new StyleColor((_iconAttribute as DataObjectIconAttribute).iconColor);
						} 
					}
					else
					{
						_typeIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("favorite.png");
					}

					_typeListElement.name = container.data.FavoriteList[f].type;
					_rootButton.text = container.data.FavoriteList[f].dataObjects[0].title;

					int _index = f;
					_rootButton.RegisterCallback<ClickEvent>(click =>
					{
						var _type = container.data.FavoriteList[_index].dataObjects[0].GetType();
						var _guid = container.data.FavoriteList[_index].dataObjects[0].guid;

						selectedDataType = _type;
						selectedGuid = _guid;
						PopulateDataTypeList(_type);
						PopulateData(_guid);
						HighlightTypeButton(_type);

					});

					favoritesFoldout.Add(_typeListElement);
				}
				else
				{
					container.data.FavoriteList.RemoveAt(f);
                }
			}

          
        }
		void OnKeyUpShortcut(KeyUpEvent evt)
		{
			//Debug.Log(evt.imguiEvent.keyCode);
			//Debug.Log(evt.actionKey);
			//Debug.Log(evt.character);

            if (evt.imguiEvent.keyCode == KeyCode.Space && (evt.ctrlKey || evt.commandKey))
            {
				TextField _textField = searchFieldVE.Q<TextField>("searchTextfield");
				FocusSearchFieldDelayed(_textField);
			}
			
			if (evt.imguiEvent.type == EventType.MouseDown)
			{
				ClearSearchResult();
			}

			if (evt.imguiEvent.keyCode == KeyCode.Delete && (evt.ctrlKey || evt.commandKey))
			{
                if (!container.isRuntimeContainer)
                {
					
                    if (EditorUtility.DisplayDialog("Delete object", selectedDataObjects.Count > 1 ? "Do you really want to delete selected DataObjects?" : "Do you really want to delete selected DataObject?", "Yes", "No"))
                    {
                        ClearDataInspectors();
                        //databrainEditor.RemoveDataObject(dataObject);
                        for (int i = 0; i < selectedDataObjects.Count; i++)
                        {
                            RemoveDataObject(selectedDataObjects[i]);
                        }

                        dataTypelistView.selectedIndex = -1;
                    }
                }
            }

			if (evt.imguiEvent.keyCode == KeyCode.D && (evt.ctrlKey || evt.commandKey))
			{
                // Duplicate
                DuplicateDataObject();
			
            }

			DatacoreKeyUpEditorEvent?.Invoke(evt.keyCode);
		}
		
		void ShowIsRuntimeContainer()
		{
			if (!container.isRuntimeContainer)
				return;
			
			var _root = rootVisualElement;

			
			runtimeOverlayBlock = new VisualElement();
            runtimeOverlayBlock.style.position = Position.Absolute;
            runtimeOverlayBlock.style.height = new Length(100, LengthUnit.Percent);
            runtimeOverlayBlock.style.width = new Length(100, LengthUnit.Percent);
            runtimeOverlayBlock.style.backgroundColor = new Color(DatabrainColor.DarkGrey.GetColor().r, DatabrainColor.DarkGrey.GetColor().g, DatabrainColor.DarkGrey.GetColor().b, 200f/255f);   
            runtimeOverlayBlock.visible = !Application.isPlaying;

            _root.Add(runtimeOverlayBlock);


			DatabrainHelpers.SetBorder(_root, 4, new Color(209f/255f, 89f/255f, 89f/255f, 255f/255f));

            var _cover = new VisualElement();
			_cover.name = "cover";
			_cover.style.height = 50;
			_cover.style.backgroundColor = new Color(209f / 255f, 89f / 255f, 89f / 255f, 255f / 255f);

            var _a = new StyleEnum<Align>();
			_a.value = Align.Center;
			
			_cover.style.alignItems = _a;
			
			
			var _lbl = new Label();
			_lbl.text = "RUNTIME LIBRARY";
			_lbl.style.fontSize = 20;
			_lbl.style.color = Color.black;
			_lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
			_lbl.style.flexGrow = 1;

			_cover.Add(_lbl);
			_root.Add(_cover);
		}
		
		private bool GetNamespaceFoldoutValue(string _namespace)
		{
			if (container.existingNamespaces == null)
				return false;
				
			for (int i = 0; i < container.existingNamespaces.Count; i++)
			{
				if (container.existingNamespaces[i].namespaceName == _namespace)
				{
					return container.existingNamespaces[i].foldout;
				}
			}
			
			return false;
		}

		private bool GetNamespaceFoldoutVisibility(string _namespace)
		{
            if (container.existingNamespaces == null)
                return true;

            for (int i = 0; i < container.existingNamespaces.Count; i++)
            {
                if (container.existingNamespaces[i].namespaceName == _namespace)
                {
                    return !container.existingNamespaces[i].hidden;
                }
            }

            return true;
        }


        private void SetNamespaceFoldoutValue(string _namespace, bool _value)
		{
			for (int i = 0; i < container.existingNamespaces.Count; i++)
			{
				if (container.existingNamespaces[i].namespaceName == _namespace)
				{
					container.existingNamespaces[i].foldout = _value;
				}
			}
		}
		
		private void SetupSearchBar()
		{
	
			searchFieldVE.visible = !searchFieldVE.visible;
			
			if (!searchFieldVE.visible)
				return;
				
			selectedSearchResultIndex = 0;
			TextField _textField = searchFieldVE.Q<TextField>("searchTextfield");
			searchResultContainer = searchFieldVE.Q<VisualElement>("searchResult");
			
			
			_textField.RegisterValueChangedCallback(OnSearch);
			_textField.RegisterCallback<BlurEvent>(e => 
			{
				ClearSearchResult();
			});
			
			_textField.RegisterCallback<KeyDownEvent>((e)  =>
			{
				if (e.keyCode == KeyCode.DownArrow)
				{
					selectedSearchResultIndex ++;
					
					var _resultCount =  searchResultContainer.Query<Button>().ToList().Count;
					if (selectedSearchResultIndex >= _resultCount)
					{
						selectedSearchResultIndex = _resultCount - 1;
					}
					
					HighlightSearchResultButton();
				}
				
				if (e.keyCode == KeyCode.UpArrow)
				{
					selectedSearchResultIndex --;
					
					if (selectedSearchResultIndex < 0)
					{
						selectedSearchResultIndex = 0;
					}
					
					HighlightSearchResultButton();
				}
				
				if (e.keyCode == KeyCode.Return)
				{
					var _results = searchResultContainer.Query<Button>().ToList();

					if (selectedSearchResultIndex >= _results.Count)
						return;

					Button _firstSearchResultButton = _results[selectedSearchResultIndex];
					var _searchResultData = (SearchResultData)_firstSearchResultButton.userData;
#if DATABRAIN_DEBUG
					Debug.Log("search guid: " + _searchResultData.guid);
#endif
					if (!string.IsNullOrEmpty(_searchResultData.guid))
					{
						selectedDataType = _searchResultData.type;
						selectedGuid = _searchResultData.guid;
						PopulateDataTypeList(_searchResultData.type);
						PopulateData(_searchResultData.guid);
                        HighlightTypeButton(selectedDataType);
                        ClearSearchResult();
					}
					else
					{
						selectedDataType = _searchResultData.type;
						PopulateDataTypeList(_searchResultData.type);
                        HighlightTypeButton(selectedDataType);
                        ClearSearchResult();
					}
				}
			});	
		}
		
		private void HighlightSearchResultButton()
		{
			var _buttons = searchResultContainer.Query<Button>().ToList();
			
			for (int i = 0; i < _buttons.Count; i ++)
			{
				if (i == selectedSearchResultIndex)
				{
					_buttons[i].style.borderBottomWidth = 2;
					_buttons[i].style.borderTopWidth = 2;
					_buttons[i].style.borderLeftWidth = 2;
					_buttons[i].style.borderRightWidth = 2;
					
					_buttons[i].style.borderBottomColor = Color.white;
					_buttons[i].style.borderTopColor = Color.white;
					_buttons[i].style.borderLeftColor = Color.white;
					_buttons[i].style.borderRightColor = Color.white;
				}
				else
				{
					_buttons[i].style.borderBottomWidth = 0;
					_buttons[i].style.borderTopWidth = 0;
					_buttons[i].style.borderLeftWidth = 0;
					_buttons[i].style.borderRightWidth = 0;
					
				}
			}
		}
		
		
		private async void ClearSearchResult()
		{
			await System.Threading.Tasks.Task.Delay(100);
			
			searchResultContainer.Clear();
			TextField _textField = searchFieldVE.Q<TextField>("searchTextfield");
			_textField.value = "";
			
			selectedSearchResultIndex = 0;
		}
		
		private async void FocusSearchFieldDelayed(TextField _textField)
		{
			await System.Threading.Tasks.Task.Delay(100);
			_textField.Q("unity-text-input").Focus();			
		}
		
		private void OnSearch(ChangeEvent<string> evt)
		{
			
			if (string.IsNullOrEmpty(evt.newValue))
			{
				return;
			}			
			else
			{
				//Debug.Log(evt.newValue);
			}
			
			searchResultContainer.Clear();
				
			for(var i = 0; i < container.data.ObjectList.Count; i ++)
			{
				for (int j = 0; j < container.data.ObjectList[i].dataObjects.Count; j++)
				{
                    if(container.data.ObjectList[i].dataObjects[j] == null)
						continue;

					if (container.hierarchyTemplate != null)
					{
						bool _hasType = false;
						for (int g = 0; g < container.hierarchyTemplate.rootDatabrainTypes.subTypes.Count; g++)
						{
							_hasType = container.hierarchyTemplate.rootDatabrainTypes.subTypes[g].HasType(container.data.ObjectList[i].dataObjects[j]);
							if (_hasType)
							{
								break;
							}
						}
						
						if (!_hasType)
						{
							continue;
						}
					}

                    var _typeIndex = i;
					var _objectIndex = j;


					// Search for tags
					if (evt.newValue.Contains("t:"))
					{
						var _tagSearchString = evt.newValue.Substring(2, evt.newValue.Length - 2);
						var _tags = _tagSearchString.Split(" ", StringSplitOptions.None);

						var _tagFound = false;
						for (int t = 0; t < container.data.ObjectList[i].dataObjects[j].tags.Count; t++)
						{
							for (int s = 0; s < _tags.Length; s++)
							{
								if (!string.IsNullOrEmpty(_tags[s]))
								{
									if (container.data.ObjectList[i].dataObjects[j].tags[t].ToLower() == _tags[s].ToLower())
									{
										_tagFound = true;
									}
								}
							}
						}

						if (_tagFound)
						{
							var _resultButton = SearchResultButton(container.data.ObjectList[i].dataObjects[j]);
							_resultButton.Q<Label>("label").text = container.data.ObjectList[i].dataObjects[j].title + "\n<size=10>Data Object of type: " + container.data.ObjectList[_typeIndex].type.Split(',')[0].Trim() + "</size>";
                            //_resultButton.text = container.data.ObjectList[i].dataObjects[j].title + "\n<size=10>Data Object of type: " + container.data.ObjectList[_typeIndex].type.Split(',')[0].Trim() + "</size>";
                            _resultButton.name = container.data.ObjectList[_typeIndex].type;
							_resultButton.userData = new SearchResultData(Type.GetType(container.data.ObjectList[_typeIndex].type), container.data.ObjectList[_typeIndex].dataObjects[j].guid); // evt.newValue);



							_resultButton.RegisterCallback<ClickEvent>(click =>
							{
								selectedDataType = Type.GetType(container.data.ObjectList[_typeIndex].type);
								selectedGuid = container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid;

								selectedTypeIndex = _typeIndex;
								selectedObjectIndex = _objectIndex;

								PopulateDataTypeList(Type.GetType(container.data.ObjectList[_typeIndex].type));
								PopulateData(evt.newValue);
								HighlightTypeButton(selectedDataType);
								ClearSearchResult();

							});

							searchResultContainer.Add(_resultButton);
						}
					}


					// Search for guid in each type
					if (container.data.ObjectList[i].dataObjects[j].guid.Contains(evt.newValue))
					{
						var _resultButton = SearchResultButton(container.data.ObjectList[i].dataObjects[j]);
                        _resultButton.Q<Label>("label").text = container.data.ObjectList[i].dataObjects[j].title + "\n<size=10>Data Object of type: " + container.data.ObjectList[_typeIndex].type.Split(',')[0].Trim() + "</size>";
						_resultButton.name = container.data.ObjectList[_typeIndex].type;
						_resultButton.userData = new SearchResultData(Type.GetType(container.data.ObjectList[_typeIndex].type), container.data.ObjectList[_typeIndex].dataObjects[j].guid); // evt.newValue);
						


                        _resultButton.RegisterCallback<ClickEvent>(click =>
						{
							selectedDataType = Type.GetType(container.data.ObjectList[_typeIndex].type);
							selectedGuid = container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid;

							selectedTypeIndex = _typeIndex;
							selectedObjectIndex = _objectIndex;

							PopulateDataTypeList(Type.GetType(container.data.ObjectList[_typeIndex].type));
							PopulateData(evt.newValue);
							HighlightTypeButton(selectedDataType);
                            ClearSearchResult();
							
						});

						searchResultContainer.Add(_resultButton);

					}					
					else
					{
                        // Search for title
                        if (container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].title.ToLower().Contains(evt.newValue.ToLower()))
						{
							var _resultButton = SearchResultButton(container.data.ObjectList[i].dataObjects[j]);
                            _resultButton.Q<Label>("label").text = container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].title + "\n<size=10>Data Object of type: " + container.data.ObjectList[_typeIndex].type.Split(',')[0].Trim() + "</size>";
							_resultButton.name = container.data.ObjectList[_typeIndex].type;
							_resultButton.userData = new SearchResultData(Type.GetType(container.data.ObjectList[_typeIndex].type), container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid);

							_resultButton.RegisterCallback<ClickEvent>(click =>
							{

								
								selectedDataType = Type.GetType(container.data.ObjectList[_typeIndex].type);
								selectedGuid = container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid;
								selectedTypeIndex = _typeIndex;
								selectedObjectIndex = _objectIndex;
								//Debug.Log("resul: " + container.data.ObjectList[_typeIndex].type);
								PopulateDataTypeList(Type.GetType(container.data.ObjectList[_typeIndex].type));
								PopulateData(container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid);
                                HighlightTypeButton(selectedDataType);
                                ClearSearchResult();
                                
                            });

							searchResultContainer.Add(_resultButton);
						}
						else
                        {
							// Search for type name
                            if (container.data.ObjectList[_typeIndex].type.ToLower().Contains(evt.newValue.ToLower()))
                            {
                                var _resultButton = SearchResultButton(container.data.ObjectList[i].dataObjects[j]);
                                _resultButton.Q<Label>("label").text = container.data.ObjectList[_typeIndex].type.Split(',')[0].Trim() + "\n<size=10>Type</size>";
                                _resultButton.name = container.data.ObjectList[_typeIndex].type;
                                _resultButton.userData = new SearchResultData(Type.GetType(container.data.ObjectList[_typeIndex].type), container.data.ObjectList[i].dataObjects[j].guid);


                                _resultButton.RegisterCallback<ClickEvent>(click =>
                                {
                                    selectedDataType = Type.GetType(container.data.ObjectList[_typeIndex].type);
                                    selectedGuid = container.data.ObjectList[_typeIndex].dataObjects[_objectIndex].guid;
                                    selectedTypeIndex = _typeIndex;
                                    selectedObjectIndex = _objectIndex;
                                    PopulateDataTypeList(Type.GetType(container.data.ObjectList[_typeIndex].type));
                                    HighlightTypeButton(selectedDataType);
                                    ClearSearchResult();
                                    
                                });

                                searchResultContainer.Add(_resultButton);
                            }
                        }
						//}
					}




				}
			}
		
			HighlightSearchResultButton();
		}

		private Button SearchResultButton(DataObject _dataObject)
		{
			var _button = new Button();

			var _label = new Label();
			_label.name = "label";
			_label.text = "Button";

			var _tagContainer = new VisualElement();
			_tagContainer.style.flexDirection = FlexDirection.Row;
			_tagContainer.style.flexWrap = Wrap.Wrap;
			
			_button.style.borderBottomLeftRadius = 0;
			_button.style.borderBottomRightRadius = 0;
			_button.style.borderTopLeftRadius = 0;
			_button.style.borderTopRightRadius = 0;
			
			_button.style.marginBottom = 2;
			_button.style.marginTop = 2;
			_button.style.marginLeft = 10;
			_button.style.marginRight = 10;
			
			_button.style.paddingBottom = 6;
			_button.style.paddingTop = 6;
			_button.style.paddingRight = 6;
			_button.style.paddingLeft = 6;
			
			_button.enableRichText = true;
			
			
			_button.style.unityTextAlign = TextAnchor.MiddleLeft;

			_button.Add(_label);

			
			for (int i = 0; i < _dataObject.tags.Count; i++)
			{
                var _tagItem = new VisualElement();
                DatabrainHelpers.SetBorderRadius(_tagItem, 10, 10, 10, 10);
                DatabrainHelpers.SetBorder(_tagItem, 0);
                DatabrainHelpers.SetPadding(_tagItem, 5, 5, 0, 0);
                DatabrainHelpers.SetMargin(_tagItem, 2, 2, 2, 2);

                _tagItem.style.flexDirection = FlexDirection.Row;
                _tagItem.style.minHeight = 18;
                _tagItem.style.backgroundColor = DatabrainColor.DarkBlue.GetColor();

                var _tagIcon = new VisualElement();
                _tagIcon.style.backgroundImage = DatabrainHelpers.LoadIcon("tagoutline");
                _tagIcon.style.width = 14;
                _tagIcon.style.height = 14;
                _tagIcon.style.alignSelf = Align.Center;
                DatabrainHelpers.SetMargin(_tagIcon, 2, 2, 0, 0);

                var _tagItemLabel = new Label();
                _tagItemLabel.text = _dataObject.tags[i];
                _tagItemLabel.style.fontSize = 12;
                _tagItemLabel.style.flexGrow = 1;
                _tagItemLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _tagItemLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

				_tagItem.Add(_tagIcon);
				_tagItem.Add(_tagItemLabel);

				_tagContainer.Add(_tagItem);
			}

			_button.Add(_tagContainer);
			

			return _button;
		}
		
		private VisualElement SetupBaseFoldout(VisualElement target, string _name)
		{
			foldoutAsset.CloneTree(target);
			VisualElement _baseFoldout = target.Q<VisualElement>("baseFoldout");
			_baseFoldout.name = _name;
			Foldout _foldout = _baseFoldout.Q<Foldout>("foldout");
			_foldout.name = _name;
			VisualElement _foldoutChecked = _baseFoldout.Q<VisualElement>("foldoutChecked");
			
			_foldout.RegisterCallback<MouseOverEvent>(mouseOverEvent => 
			{
				_foldoutChecked.EnableInClassList("baseFoldout--checked", true);
			});
			_foldout.RegisterCallback<MouseLeaveEvent>(mouseLeaveEvent => 
			{
				_foldoutChecked.EnableInClassList("baseFoldout--checked", false);
			});
			_foldout.text = _name.Substring(_name.LastIndexOf('.') + 1); // _name;


            _baseFoldout.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));

            target.Add(_baseFoldout);

			namespaceFoldouts.Add(_foldout);
			
			return _foldout;
		}

	
		
		private void ShowModule(int index)
		{
            container.modules[index].OnOpen(container);

            dataInspectorVE.Clear();
			dataInspectorBaseVE.Clear();
			dataInspectorBaseVE.style.display = DisplayStyle.None;

            resetDataButton.SetEnabled(false);
			openFileButton.SetEnabled(false);
			
			var _attribute = container.modules[index].GetType().GetCustomAttribute(typeof(DatacoreModuleAttribute)) as DatacoreModuleAttribute;
			if (_attribute != null)
			{
				dataTitle.text = _attribute.title;
			}
			else
			{
				dataTitle.text = "";
			}
			
			var _gui = (container.modules[index]).DrawGUI(container, this);

			dataInspectorVE.Add(_gui);
        }

		
		public void SelectDataObject(DataObject _dataObject, bool _populateDataTypeList = true)
		{
			if (_dataObject == null)
				return;

            selectedDataType = _dataObject.GetType();
            selectedGuid = _dataObject.guid;
			if (_populateDataTypeList)
			{
				PopulateDataTypeList(selectedDataType);
			}
			PopulateData(_dataObject.guid);
            HighlightTypeButton(selectedDataType);
            HighlightDataTypeListDelayed(2000);
        }


		public void ShowLastSelected()
		{
            var _selectedGuidPref = EditorPrefs.GetString("DATABRAIN_SELECTEDGUID_" + container.GetInstanceID().ToString());

            if (!string.IsNullOrEmpty(_selectedGuidPref))
			{
				var _db = container.GetInitialDataObjectByGuid(_selectedGuidPref);

				SelectDataObject(_db, true);
			}
		}
     

        private void ResetTypeButtonsHighlight(string _except)
		{
			for (int i = 0; i < typeButtons.Count; i ++)
			{
				if (typeButtons[i].name == _except)
					continue;
					
				typeButtons[i].EnableInClassList("listElement--checked", false);
			}
		}

		private void HighlightTypeButton(Type _type)
		{
			for (int i = 0; i < typeButtons.Count; i++)
			{
				if (typeButtons[i].parent.name == _type.Name)
				{
					var _foldout = typeButtons[i].GetFirstAncestorOfType<Foldout>();
					_foldout.value = true;
					
                    typeButtons[i].EnableInClassList("listElement--checked", true);
				}
				else
				{
					typeButtons[i].EnableInClassList("listElement--checked", false);
				}
			}
        }
	    
	    
		private void CreateNewDataObject()
		{
#if DATABRAIN_DEBUG
			Debug.Log("create new data object of type: " + selectedDataType.Name);
#endif
			var _newObj = DataObjectCreator.CreateNewDataObject(container, selectedDataType);
			selectedGuid = _newObj.guid;
			//selectedDataObject = _newObj;
            selectedDataObjects = new List<DataObject>();
			selectedDataObjects.Add(_newObj);

            PopulateDataTypeList(selectedDataType);
		}
		
		public void RemoveDataObject(DataObject _object)
		{
			if (_object == null)
				return;
				
			var _path = AssetDatabase.GetAssetPath(_object);

			var _objGuid = _object.guid;

			_object.OnDelete();
#if DATABRAIN_DEBUG
			Debug.Log("remove obj: " + _objGuid);
#endif
			for (int i = container.data.ObjectList.Count-1; i >= 0; i --)
			{
				for (int j = container.data.ObjectList[i].dataObjects.Count-1; j >= 0; j--)
				{
					if (container.data.ObjectList[i].dataObjects[j] == null)
					{
						container.data.ObjectList[i].dataObjects.RemoveAt(j);
						continue;
					}

					if (container.data.ObjectList[i].dataObjects[j].guid == _objGuid)
					{
						container.data.ObjectList[i].dataObjects.RemoveAt(j);
					}
				}
			}
			
			
            AssetDatabase.RemoveObjectFromAsset(_object);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DestroyImmediate(_object);
        }
		
		public void ClearDataInspectors()
		{
			dataInspectorVE.Clear();
			dataInspectorBaseVE.Clear();
		}


		public void UpdateSelectedDataTypeList()
		{
			PopulateDataTypeList(selectedDataType);
		}


		private void PopulateDataTypeList(Type _type)
		{
			if (container == null)
				return;

			dataTypeListContainer.Clear();
			dataFilterContainer.Clear();
			resetDataButton.SetEnabled(true);
			openFileButton.SetEnabled(true);
			

			// Show tags and title filter
			dataFilterContainer.style.flexGrow = 0;
			dataFilterContainer.style.flexShrink = 0;
            var _tagParentContainer = new VisualElement();
			DatabrainHelpers.SetMargin(_tagParentContainer, 5, 5, 5, 5);
			DatabrainHelpers.SetPadding(_tagParentContainer, 5, 5, 5, 5);
			DatabrainHelpers.SetBorderRadius(_tagParentContainer, 5, 5, 5, 5);
			DatabrainHelpers.SetBorder(_tagParentContainer, 1);
			_tagParentContainer.style.flexDirection = FlexDirection.Column;

			var _filterInputContainer = new VisualElement();
			_filterInputContainer.style.flexDirection = FlexDirection.Row;
			_filterInputContainer.style.flexGrow = 1;
            
			var _filterLabel = new Label();
			_filterLabel.text = "Filter";
			_filterLabel.tooltip = "Filter either by tag or title.\nThe filter automatically finds if it is a tag or a title.\nHit return key to apply filter.\nWords can be comma separated.";

			var _tagContainer = new VisualElement();
			_tagContainer.style.flexDirection = FlexDirection.Row;

			var _titlesContainer = new VisualElement();
			_titlesContainer.style.flexDirection = FlexDirection.Row;

			if (filterInput == null)
			{
                filterInput = new TextField();
				filterInput.style.flexGrow = 1;
                filterInput.RegisterCallback<KeyDownEvent>(evt =>
				{
					if (evt.keyCode == KeyCode.Return)
					{

						var _split = filterInput.value.Split(",")
									.Select(p => p.Trim())
									.Where(p => !string.IsNullOrWhiteSpace(p))
									.ToList();

						var _assignedTags = container.GetAssignedTagsFromType(selectedDataType);
						var _foundTag = false;

						for (int i = _split.Count - 1; i >= 0; i--)
						{
							var _foundTagIndex = -1;
							for (int t = 0; t < container.tags.Count; t++)
							{
								if (container.tags[t].Contains(_split[i], StringComparison.OrdinalIgnoreCase))
								{
									_foundTagIndex = t;
									_foundTag = true;
								}
							}

							if (_assignedTags != null)
							{
								if (_foundTagIndex > -1 && !_assignedTags.Contains(_split[i], StringComparer.OrdinalIgnoreCase))
								{
									_assignedTags.Add(container.tags[_foundTagIndex]);
									_split.RemoveAt(i);
								}
							}
						}

						var _assignedTitlesFilter = container.GetAssignedTitlesFiltersFromType(selectedDataType);
						var _availableObjsList = container.GetAllInitialDataObjectsByType(selectedDataType);
						
						var _foundTitle = false;
						for (int i = _split.Count - 1; i >= 0; i--)
						{
							var _foundTitleIndex = -1;
							for (int t = 0; t < _availableObjsList.Count; t++)
							{
								//Debug.Log("Type: " + _type + " _ " + _availableObjsList[t].title + " _ " + _split[i]);
								if (_availableObjsList[t].title.Contains(_split[i], StringComparison.OrdinalIgnoreCase))
								{
									_foundTitleIndex = t;
									_foundTitle = true;
								}
							}

							if (_assignedTitlesFilter != null)
							{
								if (!_assignedTitlesFilter.Contains(_split[i], StringComparer.OrdinalIgnoreCase) && _foundTitleIndex > -1)
								{
									_assignedTitlesFilter.Add(_split[i]);
									_split.RemoveAt(i);
								}
							}
						}


						if (!_foundTag && !_foundTitle)
						{
							if (EditorUtility.DisplayDialog("Filter", "No DataObject with tag or title found", "ok"))
							{
								filterInput.schedule.Execute(() => { filterInput.Q("unity-text-input").Focus(); }).ExecuteLater(200);
                            }

                        }

						PopulateDataTypeList(selectedDataType);

                        filterInput.value = "";
                    }
				});
			}
		
			var _assignedTags = container.GetAssignedTagsFromType(selectedDataType);
			DatabrainTags.ShowTagsDataObject(_tagContainer, _assignedTags, (x) => { PopulateDataTypeList(selectedDataType); });

			var _assignedTitlesFilter = container.GetAssignedTitlesFiltersFromType(selectedDataType);
			DatabrainTags.ShowTitlesDataObject(_titlesContainer, _assignedTitlesFilter, (x) => { PopulateDataTypeList(selectedDataType); });

			_filterInputContainer.Add(_filterLabel);
			_filterInputContainer.Add(filterInput);

            _tagParentContainer.Add(_filterInputContainer);
            _tagParentContainer.Add(_tagContainer);
			_tagParentContainer.Add(_titlesContainer);

            dataFilterContainer.Add(_tagParentContainer);

            var _lockAttribute = _type.GetCustomAttribute<DataObjectLockAttribute>(false);
			if (_lockAttribute != null)
			{
				createDataTypeButton.SetEnabled(false);
            }
			else
			{
				createDataTypeButton.SetEnabled(true);
            }

			availableObjsList = new List<DataObject>();

			availableObjsList = container.GetAllInitialDataObjectsByType(_type);
	
			if (availableObjsList != null)
			{

				// Filter by tags and titles
				for (int a = availableObjsList.Count - 1; a >= 0; a--)
				{
					bool filterOK = false;
					if (_assignedTags != null && _assignedTags.Count > 0)
					{
	
                        for (int t = 0; t < _assignedTags.Count; t++)
						{
							if (availableObjsList[a].tags.Contains(_assignedTags[t]))
							{
								filterOK = true;
							}
						}
					}
					else
					{

						if (_assignedTitlesFilter != null)
						{
							if (_assignedTitlesFilter.Count == 0)
							{
								filterOK = true;
							}
						}
						else
						{
							filterOK = true;
						}
					}

					if (_assignedTitlesFilter != null && _assignedTitlesFilter.Count > 0)
					{

						for (int t = 0; t < _assignedTitlesFilter.Count; t++)
						{
							if (availableObjsList[a].title.Contains(_assignedTitlesFilter[t], StringComparison.OrdinalIgnoreCase))
							{
								filterOK = true;
							}
						}
					}
					else
					{
						if (_assignedTags != null)
						{
							if (_assignedTags.Count == 0)
							{
								filterOK = true;
							}
						}
						else
						{
							filterOK = true;
						}
					}


					if (!filterOK)
					{
						availableObjsList.RemoveAt(a);
					}
                }



                selectedDataObjects = new List<DataObject>();


                var _datacoreMaxObjectsAttribute = _type.GetCustomAttribute(typeof(DataObjectMaxObjectsAttribute)) as DataObjectMaxObjectsAttribute;
			
				if (_datacoreMaxObjectsAttribute != null)
				{
					if (availableObjsList.Count >= _datacoreMaxObjectsAttribute.maxObjects)
					{
						createDataTypeButton.SetEnabled(false);
						statusLabel.text = "available entries: " + availableObjsList.Count + " - max objects reached";
					}
				}
				
				statusLabel.text = "available entries: " + availableObjsList.Count;

				
				if (dataTypelistView == null)
				{
					dataTypelistView = new ListView();
					dataTypelistView.itemsSource = availableObjsList;
					dataTypelistView.makeItem = () => { return new DataObjectListItemElement(dataListElementAsset); };
					dataTypelistView.bindItem = (element, index) =>
					{
						(element as DataObjectListItemElement).Bind(availableObjsList[index], index, container, this, dataTypelistView);
					};

					dataTypelistView.reorderable = true;
					dataTypelistView.reorderMode = ListViewReorderMode.Simple;
					dataTypelistView.showBorder = false;
					dataTypelistView.fixedItemHeight = 40;
					//dataTypelistView.showBoundCollectionSize = false;
					dataTypelistView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;

					dataTypelistView.selectionType = SelectionType.Multiple;


                    dataTypelistView.selectionChanged += (elements) =>
					{
						
						selectedDataObjects = new List<DataObject>();


						foreach (var item in elements)
						{
							selectedDataObjects.Add((item as DataObject));

							var _listItem = dataTypeListContainer.Q((item as DataObject).guid);
							if (_listItem != null)
							{
								var _dataListItemButton = _listItem.Q<Button>("dataListButton");
								if (_dataListItemButton != null)
								{
									_dataListItemButton.EnableInClassList("typeListElementSelected--checked", true);
								}

							}
						}

						if (elements != null && elements.ToList().Count > 0)
						{
							var _do = (elements.First() as DataObject);
							PopulateData(_do.guid);
							_do.Selected();
						}

					};

					dataTypelistView.itemsChosen += elements =>
					{

                       
                        selectedDataObjects = new List<DataObject>();
                        foreach (var item in elements)
                        {
                            selectedDataObjects.Add((item as DataObject));

                            var _listItem = dataTypeListContainer.Q((item as DataObject).guid);
                            if (_listItem != null)
                            {
                                var _dataListItemButton = _listItem.Q<Button>("dataListButton");
                                if (_dataListItemButton != null)
                                {
                                    _dataListItemButton.EnableInClassList("typeListElementSelected--checked", true);
                                }

                            }
						}
						if (elements != null && elements.ToList().Count > 0)
						{
                            var _do = (elements.First() as DataObject);
                            PopulateData(_do.guid);
                            _do.Selected();
                        }
                    }; 

					dataTypelistView.itemIndexChanged += (int _from, int _to) =>
					{
						
						// Rearrange item
						for (int t = 0; t < container.data.ObjectList.Count; t++)
						{
							if (container.data.ObjectList[t].type == selectedDataType.AssemblyQualifiedName)
							{
								container.data.ObjectList[t].dataObjects[_from].index = _to;
								container.data.ObjectList[t].dataObjects[_to].index = _from;

								var _obj = container.data.ObjectList[t].dataObjects[_from];
								container.data.ObjectList[t].dataObjects.RemoveAt(_from);
								container.data.ObjectList[t].dataObjects.Insert(_to, _obj);

							} 
						}
					};



					dataTypelistView.style.flexGrow = 1.0f;
				}
				else
				{
                    dataTypelistView.itemsSource = availableObjsList;
                    dataTypelistView.RefreshItems();
				}


              
              


                dataTypeListContainer.Add(dataTypelistView);


				if (availableObjsList != null && availableObjsList.Count > 0)
				{
					// Check which guid is selected
					// if selected guid exists in current type select it.
					// otherwise use first guid in the list

					if (string.IsNullOrEmpty(selectedGuid))
					{
						var _foundGuid = false;
						for (int a = 0; a < availableObjsList.Count; a++)
						{
							if (availableObjsList[a] == null)
								continue;

							if (availableObjsList[a].guid == selectedGuid)
							{
								_foundGuid = true;
							}
						}
						if (!_foundGuid)
						{
							if (availableObjsList[0] != null)
							{
								selectedGuid = availableObjsList[0].guid;
							}
						}

#if DATABRAIN_DEBUG
						Debug.Log(selectedGuid);
#endif
						for (int a = 0; a < container.data.ObjectList.Count; a++)
						{
							for (int b = 0; b < container.data.ObjectList[a].dataObjects.Count; b++)
							{
								if (container.data.ObjectList[a].dataObjects[b] == null)
									continue;

								if (container.data.ObjectList[a].dataObjects[b].guid == selectedGuid)
								{
									selectedTypeIndex = a;
									selectedObjectIndex = b;
								}
							}
						}



						for (int i = 0; i < availableObjsList.Count; i++)
						{
							if (availableObjsList[i] == null)
								continue;

							if (availableObjsList[i].guid == selectedGuid)
							{
								availableObjsList[i].Selected();
							}
						}
					}

					PopulateData(selectedGuid);

					HighlightDataTypeListDelayed();

				}
				else
				{
					dataTitle.text = "";
					dataInspectorVE.Clear();
					dataInspectorBaseVE.Clear();
					dataInspectorBaseVE.style.display = DisplayStyle.None;
					if (_lockAttribute == null)
					{
						dataInspectorVE.Add(NoDataObjects());
					} 
				}
				
			}
			else
			{
                // No data objects

                dataTitle.text = "";
                dataInspectorVE.Clear();
                dataInspectorBaseVE.Clear();
                dataInspectorBaseVE.style.display = DisplayStyle.None;
				if (_lockAttribute == null)
				{
					dataInspectorVE.Add(NoDataObjects());
				}
            }


			if (_lockAttribute != null)
			{
				var _lockedLabel = new Label();
				_lockedLabel.text = "";

				dataInspectorVE.Add(_lockedLabel);


				splitView2.fixedPane.style.display = DisplayStyle.None;
				//splitView2.fixedPane.style.width = 0;
				container.secondColumnWidth = splitView2.fixedPane.style.width.value.value;
				splitView2.fixedPaneInitialDimension = 0;


				splitView2.Q<VisualElement>("unity-dragline-anchor").style.display = DisplayStyle.None;
			}
			else
			{
				if (splitView2.fixedPane != null)
				{
					if (splitView2.fixedPane.style.width != 0)
					{
						container.secondColumnWidth = splitView2.fixedPane.style.width.value.value;
					}

					splitView2.fixedPaneInitialDimension = container.secondColumnWidth;
					splitView2.fixedPane.style.width = container.secondColumnWidth;
					splitView2.fixedPane.style.display = DisplayStyle.Flex;
					splitView2.Q<VisualElement>("unity-dragline-anchor").style.display = DisplayStyle.Flex;
				}
			}
        }
		
		async void HighlightDataTypeListDelayed(int _milliseconds = 100)
		{
            await System.Threading.Tasks.Task.Delay(_milliseconds);

			//var _listItem = dataTypeListContainer.Q(selectedGuid);
			//if (_listItem != null)
			//{
			//	var _dataListItemButton = _listItem.Q<Button>("dataListButton");
			//	if (_dataListItemButton != null)
			//	{
			//		_dataListItemButton.EnableInClassList("typeListElementSelected--checked", true);
			//	}
			//}

			var _index = 0;
			var _items = dataTypeListContainer.Query<DataObjectListItemElement>().ToList();

            foreach (var _item in _items)
			{
				if (_item.name == selectedGuid)
				{
					dataTypelistView.selectedIndex = _index;
					//dataTypelistView.AddToSelection(_index);
					//dataTypelistView.SetSelection(_index);
				}

				_index++;
            }
        }


		VisualElement NoDataObjects()
		{
            var _infoBox = new VisualElement();
            _infoBox.style.marginBottom = 10;
            _infoBox.style.marginTop = 10;
            _infoBox.style.marginLeft = 10;
            _infoBox.style.marginRight = 10;
            _infoBox.style.borderBottomWidth = 1;
            _infoBox.style.borderTopWidth = 1;
            _infoBox.style.borderLeftWidth = 1;
            _infoBox.style.borderRightWidth = 1;
            _infoBox.style.borderBottomColor = DatabrainHelpers.colorLightGrey;
            _infoBox.style.borderTopColor = DatabrainHelpers.colorLightGrey;
            _infoBox.style.borderLeftColor = DatabrainHelpers.colorLightGrey;
            _infoBox.style.borderRightColor = DatabrainHelpers.colorLightGrey;

            var _infoText = new Label();
            _infoText.text = "No data objects of type " + selectedDataType.Name;
            _infoText.style.whiteSpace = WhiteSpace.Normal;
            _infoText.style.fontSize = 14;
            _infoText.style.marginBottom = 10;
            _infoText.style.marginTop = 10;
            _infoText.style.marginLeft = 10;
            _infoText.style.marginRight = 10;
            _infoText.style.unityTextAlign = TextAnchor.MiddleCenter;

            var _createObject = new Button();
            _createObject.text = "+ Create data object";
            _createObject.style.marginBottom = 10;
            _createObject.style.marginTop = 10;
            _createObject.style.marginLeft = 10;
            _createObject.style.marginRight = 10;
            _createObject.style.height = 40;
            _createObject.RegisterCallback<ClickEvent>(click =>
            {
                if (selectedDataType != null)
                {
                    CreateNewDataObject();
                }
            });


            _infoBox.Add(_infoText);
            _infoBox.Add(_createObject);

			return _infoBox;
        }
		
		public void UpdateData()
		{
			if (container == null)
				return;
				
			if (selectedDataType != null && !string.IsNullOrEmpty(selectedGuid))
			{
				for (int i = 0; i < container.data.ObjectList.Count; i++)
				{
					for (int j = 0; j < container.data.ObjectList[i].dataObjects.Count; j++)
					{
						if (container.data.ObjectList[i].dataObjects[j] == null)
							continue;

						if (container.data.ObjectList[i].dataObjects[j].guid == selectedGuid)
						{
							
							var _listElement = dataTypeListContainer.Q<VisualElement>(selectedGuid); //container.data.GuidDictionary[selectedDataType][selectedGuid].guid);
							if (_listElement != null)
							{

								var _button = _listElement.Q<Button>("dataListButton");
								var _icon = _listElement.Q<VisualElement>("dataListIcon");
								var _warningIcon = _listElement.Q<VisualElement>("warningIcon");

								_button.text = container.data.ObjectList[i].dataObjects[j].title;
								_button.tooltip = container.data.ObjectList[i].dataObjects[j].description;
								_icon.style.backgroundImage = new StyleBackground(container.data.ObjectList[i].dataObjects[j].icon);
								_icon.style.backgroundColor = new StyleColor(container.data.ObjectList[i].dataObjects[j].color);

								var _isValid = container.data.ObjectList[i].dataObjects[j].IsValid();

								_warningIcon.visible = !_isValid;
							}
						}
					}
				}
			}
			


			for (int i = 0; i < container.existingNamespaces.Count; i ++)
			{
				for (int s = 0; s < container.existingNamespaces[i].existingTypes.Count; s ++)
				{
                    if (container.isRuntimeContainer)
                        return;

					// Set Save Icon
                    var _runtimeSerialization = container.existingNamespaces[i].existingTypes[s].runtimeSerialization;

                    for (int t = 0; t < typeButtons.Count; t++)
                    {
                        if (typeButtons[t].parent.name == container.existingNamespaces[i].existingTypes[s].typeName)
                        {
                            var _listElement = typeButtons[t].parent;
                            var _warningIcon = _listElement.Q<VisualElement>("saveIcon");

                            _warningIcon.style.display = _runtimeSerialization ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                    }



					// Set warning icons
					var _isValid = true;
					for (var j = 0; j < container.data.ObjectList.Count; j++)
					{
						if (container.data.ObjectList[j].type == container.existingNamespaces[i].existingTypes[s].typeAssemblyQualifiedName)
						{
							for (int k = 0; k < container.data.ObjectList[j].dataObjects.Count; k++)
							{
								if (container.data.ObjectList[j].dataObjects[k] == null)
									continue;

								if (_isValid)
								{
									_isValid = container.data.ObjectList[j].dataObjects[k].IsValid();
								}
							}
						}
					}



					for (int l = 0; l < typeButtons.Count; l++)
					{
						if (typeButtons[l].parent.name == container.existingNamespaces[i].existingTypes[s].typeName)
						{
							var _listElement = typeButtons[l].parent;
							var _warningIcon = _listElement.Q<VisualElement>("warningIcon");

							_warningIcon.style.display = _isValid ? DisplayStyle.None : DisplayStyle.Flex;
						}
					}

				}
            }


			
			
			if (!container.isRuntimeContainer)
			{
			
				for(var i = 0; i < container.data.ObjectList.Count; i++)
				{
					for (int j = 0; j < container.data.ObjectList[i].dataObjects.Count; j++)
					{
						if (container.data.ObjectList[i] != null)
						{
							if (container.data.ObjectList[i].dataObjects[j] == null)
							{

							}
							else
							{
								container.data.ObjectList[i].dataObjects[j].SetRuntimeSerialization(container.IsRuntimeSerialization(container.data.ObjectList[i].type));
							}
						}
					}
				}
			}


			for (int i = 0; i < namespaceFoldouts.Count; i++)
			{
				namespaceFoldouts[i].value = GetNamespaceFoldoutValue(namespaceFoldouts[i].name);
				namespaceFoldouts[i].parent.parent.style.display = GetNamespaceFoldoutVisibility(namespaceFoldouts[i].name) ? DisplayStyle.Flex : DisplayStyle.None;
			}

			if (resetDataButton != null)
			{
				resetDataButton.SetEnabled(true);
			}
			if (openFileButton != null)
			{
				openFileButton.SetEnabled(true);
			}
			
			if (createDataTypeButton != null)
			{
				createDataTypeButton.SetEnabled(!container.isRuntimeContainer);
			}
			
			if (colorIndicatorVE != null)
			{
				colorIndicatorVE.style.backgroundColor = container.isRuntimeContainer ? DatabrainHelpers.colorRuntime : DatabrainHelpers.colorNormal;
			}


            for (int i = 0; i < container.existingNamespaces.Count; i++)
            {
                for (int e = 0; e < container.existingNamespaces[i].existingTypes.Count; e++)
                {
                    var _type = Type.GetType(container.existingNamespaces[i].existingTypes[e].typeAssemblyQualifiedName);
                    if (_type != null)
                    {
                        var _addToRuntimeLibAtt = _type.GetCustomAttribute(typeof(DataObjectAddToRuntimeLibrary));
                        if (_addToRuntimeLibAtt != null)
                        {
                            container.existingNamespaces[i].existingTypes[e].runtimeSerialization = true;
                        }
                    }
                }
            }


			// Make sure data object assets are named correctly
			for (int i = 0; i < container.data.ObjectList.Count; i++)
			{
				for (int j = 0; j < container.data.ObjectList[i].dataObjects.Count; j++)
				{
					if (container.data.ObjectList[i].dataObjects[j] != null)
					{
						container.data.ObjectList[i].dataObjects[j].name = container.data.ObjectList[i].dataObjects[j].title;

						if (container.data.ObjectList[i].dataObjects[j].icon != null)
						{
							EditorGUIUtility.SetIconForObject(container.data.ObjectList[i].dataObjects[j], container.data.ObjectList[i].dataObjects[j].icon.texture);

						}
					}

				}
			}
        }
		
		
		
		public void PopulateData(string _guid)
		{

            if (selectedDataType == null)
                return;

            dataInspectorVE.Clear();
			dataInspectorBaseVE.Clear();
			dataInspectorBaseVE.style.display = DisplayStyle.Flex;

            selectedGuid = _guid;


			//var _selectedGuidPref = EditorPrefs.GetString("DATABRAIN_SELECTEDGUID_" + container.GetInstanceID().ToString());
			//if (!string.IsNullOrEmpty(_selectedGuidPref))
			//{
			//	selected
			//}
			//else
			//{
			EditorPrefs.SetString("DATABRAIN_SELECTEDGUID_" + container.GetInstanceID().ToString(), _guid);

			//}


			//container.selectedGuid = _guid;

            DataObject _obj = container.GetInitialDataObjectByGuid(_guid);
           
			if (selectedDataObjects == null)
            {
                selectedDataObjects = new List<DataObject>();
            }

            if (selectedDataObjects.Count == 0)
			{
				selectedDataObjects.Add(_obj);
            }


            if (_obj == null)
				return;
			
			dataTitle.text = _obj.title;
			
			_obj.name = _obj.title;
			
		
			Editor editor = Editor.CreateEditor(_obj);

			var _useIMGUIInspector = selectedDataType.GetCustomAttribute(typeof(DataObjectIMGUIInspectorAttribute));


			IMGUIContainer inspectorIMGUI = null;
			VisualElement _uiElementsInspector = null;

            if (_useIMGUIInspector != null)
			{
				inspectorIMGUI = new IMGUIContainer(() =>
				{

					DrawDefaultInspectorWithoutScriptField(editor);

				});
			}

			else
			{
				_uiElementsInspector = DrawDefaultInspectorUIElements.DrawInspector(editor, selectedDataType, false);

            }

            var _uiBaseDataInspector = DrawDefaultBaseInspectorUIElements(editor);
           

            dataInspectorBaseVE.Add(_uiBaseDataInspector);

			//IMGUIContainer runtimeInspectorIMGUI = null;

			//if (selectedDataType != null)
			//{
			//	var _hideFieldsAttribute = selectedDataType.GetCustomAttribute(typeof(DataObjectHideAllFieldsAttribute)) as DataObjectHideAllFieldsAttribute;

			//	if (_hideFieldsAttribute == null)
			//	{
			//		var _rtObj = _obj.GetRuntimeDataObject();

			//		if (_rtObj != null)
			//		{
			//			Editor _rteditor = Editor.CreateEditor(_rtObj);


			//			runtimeInspectorIMGUI = new IMGUIContainer(() =>
			//			{
			//				try
			//				{
			//					DrawDefaultInspectorWithoutScriptField(_rteditor);
			//				}
			//				catch
			//				{
			//					Debug.Log("FAILED");
			//					var _ip = dataInspectorVE.Q<IMGUIContainer>("runtimeIMGUIInspector");
			//					dataInspectorVE.Clear();

			//				}
			//			});

			//			runtimeInspectorIMGUI.name = "runtimeIMGUIInspector";
			//		}
			//	}
			//}

			if (dataViewScrollView != null)
			{
				dataViewScrollView.Clear();
			}
			else
			{
                dataViewScrollView = new ScrollView();
				dataViewScrollView.name = "dataScrollView";
				dataViewScrollView.style.flexGrow = 1;
                var _content = dataViewScrollView.Q<VisualElement>("unity-content-container");
				_content.style.flexGrow = 1;

            }

			if (_obj.runtimeClone != null && Application.isPlaying)
			{
				gotoRuntimeObjectButton.userData = _obj;
				gotoRuntimeObjectButton.SetEnabled(true);
              
            }

			// Draw inspector using IMGUI
			if (_useIMGUIInspector != null)
			{
                dataViewScrollView.Add(inspectorIMGUI);
			}
			else
			{
                dataViewScrollView.Add(_uiElementsInspector);
			}


			var _customGUI = _obj.EditorGUI(editor.serializedObject, this);
			if (_customGUI != null)
			{
                dataViewScrollView.Add(_customGUI);

			}

			dataInspectorVE.Add(dataViewScrollView);
         
		}

		
		void DrawDefaultInspectorWithoutScriptField (Editor inspector)
		{
			
			if (inspector == null)
				return;
			if (inspector.serializedObject == null)
				return;

			EditorGUI.BeginChangeCheck();

			inspector.serializedObject.Update();


			SerializedProperty iterator = inspector.serializedObject.GetIterator();

			iterator.NextVisible(true);

			Type t = iterator.serializedObject.targetObject.GetType();


			while (iterator.NextVisible(false))
			{
				if (iterator.propertyPath != "guid" &&
                    iterator.propertyPath != "initialGuid" &&
					iterator.propertyPath != "runtimeIndexID" &&
                    iterator.propertyPath != "icon" &&
					iterator.propertyPath != "color" &&
					iterator.propertyPath != "title" &&
					iterator.propertyPath != "description" &&
					iterator.propertyPath != "skipRuntimeSerialization")
				{

					/////// CUSTOM ATTRIBUTES		
					FieldInfo f = null;
					Attribute _hideAttribute = null;
                 
                    f = t.GetField(iterator.propertyPath);
					if (f != null)
					{		
						_hideAttribute = f.GetCustomAttribute(typeof(HideAttribute), true);				
					}
					//////////////////////////////////////


					if (selectedDataType != null)
					{
						var _hideFieldsAttribute = selectedDataType.GetCustomAttribute(typeof(DataObjectHideAllFieldsAttribute)) as DataObjectHideAllFieldsAttribute;
                        

                        if (_hideFieldsAttribute == null)
						{
							if (_hideAttribute == null)
							{
								EditorGUILayout.PropertyField(iterator, true);
							}	
						}
					}
				}
			}

			inspector.serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				UpdateData();
			}
		}



        // Do some checks here to see if a data object class has been renamed or if it doesn't exist
		// in the DataLibrary
        static void DataErrorCheck(DataLibrary _container)
        {
            string assetPath = AssetDatabase.GetAssetPath(_container);
            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(asset => asset != null).ToArray();

            for (int g = 0; g < allAssets.Length; g++)
            {
                if ((allAssets[g] as DataObject) == null)
                    continue;

                var _exists = false;
                var _typeIndex = 0;
                for (int s = 0; s < _container.data.ObjectList.Count; s++)
                {
                    for (int c = 0; c < _container.data.ObjectList[s].dataObjects.Count; c++)
                    {
						
                        if ((allAssets[g] as DataObject) != null)
                        {
                            if (_container.data.ObjectList[s].dataObjects[c] != null)
                            {
                                if (_container.data.ObjectList[s].dataObjects[c].guid == (allAssets[g] as DataObject).guid)
                                {
                                    // Check if type has changed because class has been renamed
                                    if (_container.data.ObjectList[s].type != allAssets[g].GetType().AssemblyQualifiedName)
                                    {
                                        _container.data.ObjectList[s].type = allAssets[g].GetType().AssemblyQualifiedName;
                                    }
                                }
                            
                        

								if (_container.data.ObjectList[s].dataObjects[c].guid == (allAssets[g] as DataObject).guid)
								{
									_exists = true;
									_typeIndex = s; 
							
								}

                            }

                        }

                    }
                } 


                if (!_exists)
                {
#if DATABRAIN_DEBUG
					Debug.Log("DataObject: " + allAssets[g].name + " does not exist in the DataLibrary list. Adding it back to the DataLibrary");
#endif
                    var _db = allAssets[g] as DataObject;

					if (!_db.isRuntimeInstance && !_container.isRuntimeContainer)
					{
						try
						{
							_container.data.ObjectList[_typeIndex].dataObjects.Insert(_db.index, _db);
						}
						catch
						{
							_container.data.ObjectList[_typeIndex].dataObjects.Add(_db);
						}
					}
                }

            }
        }


        VisualElement DrawDefaultBaseInspectorUIElements(Editor inspector)
		{
			if (selectedObjectIndex >= container.data.ObjectList[selectedTypeIndex].dataObjects.Count)
				return null;

            if (container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex] == null)
            {
                return null;
            }


            var _root = new VisualElement();

            var _foldout = new Foldout();
            _foldout.text = "General";
			_foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            _foldout.BindProperty(inspector.serializedObject.FindProperty("boxFoldout"));

            _root.Add(_foldout);



            /// Class attributes
            var _datacoreHideBaseFieldsAttribute = inspector.serializedObject.targetObject.GetType().GetCustomAttribute(typeof(DataObjectHideBaseFieldsAttribute));


            var _iterator = inspector.serializedObject.GetIterator();
            _iterator.NextVisible(true);

		
            while (_iterator.NextVisible(false))
			{
                if (_iterator.propertyPath == "guid" ||
					_iterator.propertyPath == "initialGuid" ||
					_iterator.propertyPath == "runtimeIndexID" ||
                    _iterator.propertyPath == "icon" ||
					_iterator.propertyPath == "color" ||
                    _iterator.propertyPath == "title" ||
                    _iterator.propertyPath == "description" ||
                    _iterator.propertyPath == "skipRuntimeSerialization")
				{
                    var _skip = false;
                    if (_datacoreHideBaseFieldsAttribute != null)
                    {
                        if ((_datacoreHideBaseFieldsAttribute as DataObjectHideBaseFieldsAttribute).hideIconField && _iterator.propertyPath == "icon")
                        {
                            _skip = true;
                        }
						if ((_datacoreHideBaseFieldsAttribute as DataObjectHideBaseFieldsAttribute).hideColorField && _iterator.propertyPath == "color")
						{
							_skip = true;
						}
                        if ((_datacoreHideBaseFieldsAttribute as DataObjectHideBaseFieldsAttribute).hideTitleField && _iterator.propertyPath == "title")
                        {
                            _skip = true;
                        }

                        if ((_datacoreHideBaseFieldsAttribute as DataObjectHideBaseFieldsAttribute).hideDescriptionField && _iterator.propertyPath == "description")
                        {
                            _skip = true;
                        }
                    }

                    if (_iterator.propertyPath == "initialGuid" && !container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex].isRuntimeInstance)
					{
						_skip = true;
					}

                    if (_iterator.propertyPath == "runtimeIndexID")
                    {
#if !DATABRAIN_DEBUG
						_skip = true;
#endif
                    }

                    if (_iterator.propertyPath == "title")
                    {
                        container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex].name = _iterator.stringValue;
                    }

                    if (!_skip)
                    {
                        var _property = new PropertyField(_iterator);
						if (_iterator.propertyPath == "title")
						{
							_property.RegisterValueChangeCallback(x =>
							{
								var _index = inspector.serializedObject.FindProperty("index").intValue;
								this.dataTypelistView.RefreshItem(_index);
								container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex].name = container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex].title;

							});

							_property.RegisterCallback<FocusOutEvent>(x =>
							{
                                string assetPath = AssetDatabase.GetAssetPath(container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex]);
                                var _guidString = AssetDatabase.AssetPathToGUID(assetPath);
                                GUID _guid = GUID.Generate();
                                GUID.TryParse(_guidString, out _guid);
                                AssetDatabase.SaveAssetIfDirty(_guid);
                            });
						}

                        _property.BindProperty(_iterator);
                        _foldout.Add(_property);
                    }

                }
			}

			// show tags
			var _tagLabel = new Label();
			_tagLabel.text = "Tags";
			_tagLabel.style.width = 140;
			_tagLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			var _tagParentContainer = new VisualElement();
            _tagParentContainer.style.flexDirection = FlexDirection.Row;

            _tagParentContainer.Add(_tagLabel);

			var _tagContainer = new VisualElement();

            var _assignTag = DatabrainTags.ShowAssignTags(_tagContainer, selectedDataObjects.First());

            _tagContainer.Add(_assignTag);
            DatabrainTags.ShowTagsDataObject(_tagContainer, selectedDataObjects.First().tags);


            _tagParentContainer.Add(_tagContainer);

            //_foldout.Add(_tagLabel);
			_foldout.Add(_tagParentContainer);


            inspector.serializedObject.ApplyModifiedProperties();

            return _root;
        }
     

        void ResetData()
		{
			var data = container.data.ObjectList[selectedTypeIndex].dataObjects[selectedObjectIndex];
			data.Reset();
			
			UpdateData();
		}
		
		void EditFile()
		{
#if UNITY_EDITOR
            var script = MonoScript.FromScriptableObject(selectedDataObjects.First());
			var path = AssetDatabase.GetAssetPath( script );
			//Debug.Log(path);
			AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(path));
#endif
		}

		public void DuplicateDataObject(DataObject _dataObject = null)
		{
			if (_dataObject != null)
			{
                var _newObj = DataObjectCreator.DuplicateDataObject(container, _dataObject);
                selectedGuid = _newObj.guid;

                PopulateDataTypeList(selectedDataType);
            }
			else
			{

				if (selectedDataObjects != null && selectedDataObjects.Count > 0)
				{
					if (!container.isRuntimeContainer)
					{
						for (int i = 0; i < selectedDataObjects.Count; i++)
						{
							//selectedDataObject = selectedDataObjects[i];

							var _newObj = DataObjectCreator.DuplicateDataObject(container, selectedDataObjects[i]);
							selectedGuid = _newObj.guid;

							PopulateDataTypeList(selectedDataType);
						}
					}
				}
			}
		}


    }
#pragma warning restore 0162
}
#endif