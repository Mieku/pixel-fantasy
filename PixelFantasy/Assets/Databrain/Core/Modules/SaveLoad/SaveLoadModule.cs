/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */

using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using Databrain.Helpers;
using Databrain.UI.Elements;
#endif
using System.Reflection;
using Databrain.Attributes;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Databrain.Modules.SaveLoad
{
	#if UNITY_EDITOR
	[DatabrainModuleAttribute("Save & Load", 0, "save.png")]
	#endif
	public class SaveLoadModule : DatabrainModuleBase
	{
		#if UNITY_EDITOR
		public DataLibrary dataLibrary;

		VisualElement root;
		Label runtimeLibraryPath;


		public override void Initialize(DataLibrary _dataLibrary)
		{
            // runtime databrain object
            if (_dataLibrary.runtimeLibrary == null && string.IsNullOrEmpty(_dataLibrary.runtimeLibraryFolderPath))
			{
				CreateRuntimeDataLibraryObject(_dataLibrary);
            }
			else
			{
				if (_dataLibrary.runtimeLibrary == null && !string.IsNullOrEmpty(_dataLibrary.runtimeLibraryFolderPath))
				{
					CreateRuntimeDataLibraryObjectAtPath(_dataLibrary, _dataLibrary.runtimeLibraryFolderPath);
				}
			}
        }

        public override void OnOpen(DataLibrary _dataLibrary)
        {
            if (_dataLibrary.runtimeLibrary == null && !string.IsNullOrEmpty(_dataLibrary.runtimeLibraryFolderPath))
            {
                CreateRuntimeDataLibraryObjectAtPath(_dataLibrary, _dataLibrary.runtimeLibraryFolderPath);
            }
        }


        private void CreateRuntimeDataLibraryObject(DataLibrary _dataLibrary)
		{
            if (_dataLibrary.runtimeLibrary != null)
            {
				try
				{
					// Debug.Log(Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary)) + " _ " + Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary)));

					// only remove runtime library if it was on the same path (parented) as the initial data library. To make sure we dont clutter the initial data library with multiple runtime data library
					// objects.
					if (Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary)) == Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary)))
					{
						AssetDatabase.RemoveObjectFromAsset(_dataLibrary.runtimeLibrary);
						if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary)))
						{
							AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary));
						}
						AssetDatabase.SaveAssets();
					}
				}
				catch{}
			}


            var _runtimeLibrary = ScriptableObject.CreateInstance<DataLibrary>();
            _runtimeLibrary.name = "RuntimeLibrary";

            AssetDatabase.AddObjectToAsset(_runtimeLibrary, this);

            _runtimeLibrary.isRuntimeContainer = true;
            _dataLibrary.runtimeLibrary = _runtimeLibrary;

			if (runtimeLibraryPath != null)
			{
				runtimeLibraryPath.text = "Path: " + AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary);
			}

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

		public void CreateRuntimeDataLibraryObjectAtPath(DataLibrary _dataLibrary, string _filePath)
		{

			// only remove runtime library if it was on the same path (parented) as the initial data library. To make sure we dont clutter the initial data library with multiple runtime data library
			// objects.
			if (_dataLibrary.runtimeLibrary != null)
			{
				if (Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary)) == Path.GetDirectoryName(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary)))
				{
					AssetDatabase.RemoveObjectFromAsset(_dataLibrary.runtimeLibrary);
					
					AssetDatabase.SaveAssets();
				}

				if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary)))
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary));
					
					AssetDatabase.SaveAssets();
				}
			}

			// Create new runtime library
			CreateRuntimeLibrary(_dataLibrary, _filePath);
		}

		private void CreateRuntimeLibrary(DataLibrary _dataLibrary, string _path) //, string _relativePathWithAssets)
		{
			
			var _fileName = Path.GetFileName(_path);

			// Create new runtime library
			var _runtimeLibrary = ScriptableObject.CreateInstance<DataLibrary>();
			_runtimeLibrary.name = _fileName; //_dataLibrary + "_Runtime";
			_runtimeLibrary.isRuntimeContainer = true;
			_dataLibrary.runtimeLibrary = _runtimeLibrary;
			_dataLibrary.runtimeLibraryFolderPath = _path; //Path.Combine(_folderPath, _fileName);

			if (runtimeLibraryPath != null)
			{
				runtimeLibraryPath.text = "Path: " + _path; //AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary);
			}

			AssetDatabase.CreateAsset(_runtimeLibrary, _path);
			// AssetDatabase.CreateAsset(_runtimeLibrary, System.IO.Path.Combine(_relativePathWithAssets, _dataLibrary.name + "_Runtime.asset"));
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			UpdateRuntimeLibraryGuid(_dataLibrary, _runtimeLibrary);
			
			EditorUtility.SetDirty(_dataLibrary);
		}

		// Apply original runtime library guid for version control systems.
		private void UpdateRuntimeLibraryGuid(DataLibrary _initialDataLibrary, DataLibrary _runtimeLibrary)
		{
			// Remove Assets/ from path
			var _dataPath = Application.dataPath;
			_dataPath = _dataPath.Substring(0, _dataPath.Length-7);

			var _pathCheck = Path.Combine(_dataPath, _initialDataLibrary.runtimeLibraryFolderPath + ".meta");
			var _content = File.ReadAllText(_pathCheck);
			var _guids = GetGuids(_content);

			
			if (!string.IsNullOrEmpty(_initialDataLibrary.runtimeLibraryGuid))
			{
				// Debug.Log("different guid. Old guid: " + _guids.FirstOrDefault() + " new guid: " + _initialDataLibrary.runtimeLibraryGuid);
				_content = _content.Replace("guid: " + _guids.FirstOrDefault(), "guid: " + _initialDataLibrary.runtimeLibraryGuid);
				File.WriteAllText(_pathCheck, _content);
			}
			else
			{
				// Debug.Log("assign guid " + _guids.FirstOrDefault());
				_initialDataLibrary.runtimeLibraryGuid = _guids.FirstOrDefault();
			}
		}

		private IEnumerable<string> GetGuids(string text) 
		{
            const string guidStart = "guid: ";
            const int guidLength = 32;
            int textLength = text.Length;
            int guidStartLength = guidStart.Length;
            List<string> guids = new List<string>();

            int index = 0;
            while (index + guidStartLength + guidLength < textLength) {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                string guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGuid(guid)) {
                    guids.Add(guid);
                }
            }

            return guids;
        }

		private bool IsGuid(string text)
		{
            for (int i = 0; i < text.Length; i++) 
			{
                char c = text[i];
                if (
                    !((c >= '0' && c <= '9') ||
					(c >= 'a' && c <= 'z'))
                    )
                    return false;
            }

            return true;
        }

		#region UI
		///////////////////
		// UI
		///////////////////
		public override VisualElement DrawGUI(DataLibrary _dataLibrary, DatabrainEditorWindow _editorWindow)
		{
            if (root != null)
            {
                root.Clear();
            }
            else
            {
                root = new VisualElement();
            }

			root.Add(BuildUI(_dataLibrary, _editorWindow));

			return root;
		}
		VisualElement BuildUI(DataLibrary _dataLibrary, DatabrainEditorWindow _editorWindow) 
		{
            DatabrainHelpers.UpdateNamespaces(_dataLibrary, out System.Collections.Generic.List<DatabrainHelpers.SortedTypes> ads);

            dataLibrary = _dataLibrary;

			var _scrollView = new ScrollView();
			_scrollView.style.flexGrow = 1;

            var _root = new VisualElement();


			if (dataLibrary.runtimeLibrary == null)
			{

				var _noContainer = new VisualElement();
				_noContainer.style.marginBottom = 10;
				_noContainer.style.marginTop = 10;
				_noContainer.style.marginLeft = 10;
				_noContainer.style.marginRight = 10;

				_noContainer.style.borderBottomWidth = 1;
				_noContainer.style.borderTopWidth = 1;
				_noContainer.style.borderLeftWidth = 1;
				_noContainer.style.borderRightWidth = 1;

				_noContainer.style.borderBottomColor = Color.grey;
				_noContainer.style.borderTopColor = Color.grey;
				_noContainer.style.borderLeftColor = Color.grey;
				_noContainer.style.borderRightColor = Color.grey;

				var _noContainerLabel = new Label();
				_noContainerLabel.text = "Please create a runtime library asset to enable runtime save & load feature.";
				_noContainerLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
				_noContainerLabel.style.whiteSpace = WhiteSpace.Normal;
				_noContainerLabel.style.fontSize = 14;
				_noContainerLabel.style.marginBottom = 10;
				_noContainerLabel.style.marginTop = 10;
				_noContainerLabel.style.marginLeft = 10;
				_noContainerLabel.style.marginRight = 10;

				var _createRuntimeContainerButton = new Button();
				_createRuntimeContainerButton.text = "Create";
				_createRuntimeContainerButton.style.height = 50;
				_createRuntimeContainerButton.style.marginBottom = 10;
				_createRuntimeContainerButton.style.marginTop = 10;
				_createRuntimeContainerButton.style.marginLeft = 10;
				_createRuntimeContainerButton.style.marginRight = 10;

				_createRuntimeContainerButton.RegisterCallback<ClickEvent>(click =>
				{
					CreateRuntimeDataLibraryObject(dataLibrary);
                    DrawGUI(dataLibrary, _editorWindow);
				});

				_noContainer.Add(_noContainerLabel);
				_noContainer.Add(_createRuntimeContainerButton);

				_root.Add(_noContainer);
			}
			else
			{

				var _runtimeDataLibraryContainer = new VisualElement();
				DatabrainHelpers.SetBorder(_runtimeDataLibraryContainer, 1, Color.grey);
				DatabrainHelpers.SetPadding(_runtimeDataLibraryContainer, 10, 10, 10, 10);
				DatabrainHelpers.SetMargin(_runtimeDataLibraryContainer, 0, 0, 0, 5);
				

				var _runtimeLabel = new Label();
				DatabrainHelpers.SetTitle(_runtimeLabel, "Runtime Data Library");

				var _openRuntimeDataLibrary = new Button();
				_openRuntimeDataLibrary.text = "Open Runtime Data Library";
				_openRuntimeDataLibrary.style.height = 30;
				DatabrainHelpers.SetMargin(_openRuntimeDataLibrary, 10, 10, 10, 10);

				var _runtimeLibraryPathContainer = new VisualElement();
                _runtimeLibraryPathContainer.style.backgroundColor = DatabrainColor.DarkGrey.GetColor();
				_runtimeLibraryPathContainer.style.flexDirection = FlexDirection.Row;

				runtimeLibraryPath = new Label();
				runtimeLibraryPath.text = "Path: " + AssetDatabase.GetAssetPath(dataLibrary.runtimeLibrary);
				runtimeLibraryPath.style.flexGrow = 1;

                runtimeLibraryPath.style.unityTextAlign = TextAnchor.MiddleLeft;
			
                var _selectRuntimeLibrary = new Button();
				_selectRuntimeLibrary.text = "Select";
				_selectRuntimeLibrary.style.width = 40;
				_selectRuntimeLibrary.RegisterCallback<ClickEvent>(evt =>
				{
					Selection.activeObject = (dataLibrary.runtimeLibrary);
					EditorGUIUtility.PingObject(dataLibrary.runtimeLibrary.GetInstanceID());
				});

                _runtimeLibraryPathContainer.Add(runtimeLibraryPath);
				_runtimeLibraryPathContainer.Add(_selectRuntimeLibrary);

                var _helpText = "By default, the runtime DataLibrary is parented under this (initial) DataLibrary. " +
                    "When using version control you should set a different folder for the runtime DataLibrary, " +
                    "so that the runtime asset file can be ignored by the version control.";
                var _runtimePathInfo = new HelpBox(_helpText, HelpBoxMessageType.Info);
			
				var _setDifferentRuntimeDataLibraryLocation = new Button();
				_setDifferentRuntimeDataLibraryLocation.text = "Set different runtime DataLibrary location";
				_setDifferentRuntimeDataLibraryLocation.RegisterCallback<ClickEvent>(evt =>
				{
					// var _folder = EditorUtility.OpenFolderPanel("Runtime DataLibrary location", "Assets", "");
					var _path  = EditorUtility.SaveFilePanelInProject("Select ", _dataLibrary.name + "_RuntimeLibrary", "asset", "asset");
					if (_path.Length != 0)
					{
                        // var _relativePath = _folder.Substring(Application.dataPath.Length + 1);
                        CreateRuntimeDataLibraryObjectAtPath(dataLibrary, _path);
                    }

                    root.Clear();
                    root.Add(BuildUI(_dataLibrary, _editorWindow));
                });

				var _setToInitialLibrary = new Button();
				_setToInitialLibrary.text = "Parent back to this DataLibray";
				_setToInitialLibrary.RegisterCallback<ClickEvent>(evt =>
				{
					CreateRuntimeDataLibraryObject(dataLibrary);

                    root.Clear();
                    root.Add(BuildUI(_dataLibrary, _editorWindow));
                });
			

				_openRuntimeDataLibrary.RegisterCallback<ClickEvent>(click =>
				{
                    var _window = DatabrainHelpers.OpenEditor(dataLibrary.runtimeLibrary.GetInstanceID(), false, dataLibrary);
                });


				_runtimeDataLibraryContainer.Add(_runtimeLabel);
                _runtimeDataLibraryContainer.Add(_runtimeLibraryPathContainer);
                _runtimeDataLibraryContainer.Add(_openRuntimeDataLibrary);
                _runtimeDataLibraryContainer.Add(_runtimePathInfo);
				_runtimeDataLibraryContainer.Add(_setDifferentRuntimeDataLibraryLocation);

				if (AssetDatabase.GetAssetPath(dataLibrary) != AssetDatabase.GetAssetPath(dataLibrary.runtimeLibrary))
				{
					_runtimeDataLibraryContainer.Add(_setToInitialLibrary);
                }

				if (runtimeLibraryPath != null)
				{
					runtimeLibraryPath.text = "Path: " + AssetDatabase.GetAssetPath(_dataLibrary.runtimeLibrary);
				}

                // GLOBAL SETTINGS
                #region globalsettings
                var _editor = Editor.CreateEditor(dataLibrary);

				var _globalSettings = new VisualElement();
				_globalSettings.style.borderBottomWidth = 1;
				_globalSettings.style.borderTopWidth = 1;
				_globalSettings.style.borderLeftWidth = 1;
				_globalSettings.style.borderRightWidth = 1;
				_globalSettings.style.borderBottomColor = Color.grey;
				_globalSettings.style.borderTopColor = Color.grey;
				_globalSettings.style.borderLeftColor = Color.grey;
				_globalSettings.style.borderRightColor = Color.grey;
				_globalSettings.style.paddingBottom = 10;
				_globalSettings.style.paddingTop = 10;
				_globalSettings.style.paddingLeft = 10;
				_globalSettings.style.paddingRight = 10;


				var _globalSettingsLabel = new Label();
				_globalSettingsLabel.text = "Global";
				_globalSettingsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
				_globalSettingsLabel.style.fontSize = 14;


				var _jsonFormatting = new PropertyField();
				_jsonFormatting.BindProperty(_editor.serializedObject.FindProperty(nameof(dataLibrary.jsonFormatting)));

				var _enableXOREncryption = new PropertyField();
				_enableXOREncryption.BindProperty(_editor.serializedObject.FindProperty(nameof(dataLibrary.useXOREncryption)));

				var _encryptionKey = new PropertyField();
				_encryptionKey.BindProperty(_editor.serializedObject.FindProperty(nameof(dataLibrary.encryptionKey)));



				_globalSettings.Add(_globalSettingsLabel);
				_globalSettings.Add(_jsonFormatting);
				_globalSettings.Add(_enableXOREncryption);
				_globalSettings.Add(_encryptionKey);

				//_globalSettings.Add(_eventsLabel);
				//_globalSettings.Add(_onSaveEvent);


				_root.Add(_runtimeDataLibraryContainer);
				_root.Add(_globalSettings);
				#endregion
				////

				#region setuptype

				var _updateButton = new Button();
				_updateButton.text = "Update namespace list";
				_updateButton.style.height = 30;
				_updateButton.style.marginBottom = 10;
				_updateButton.RegisterCallback<ClickEvent>(click =>
				{
					if (EditorUtility.DisplayDialog("Reset", "Warning this will reset all manually added data types from the list", "Ok", "Cancel"))
					{
                        DatabrainHelpers.UpdateNamespaces(dataLibrary, out _editorWindow.sortedTypes);

                        //_editorWindow.UpdateData();
					}
				});


				var _namespaceContainer = new VisualElement();
				_namespaceContainer.name = "namespaceContainer";
				var _namespaceTitle = new Label();
				_namespaceTitle.text = "Add to runtime data library";
				_namespaceTitle.style.marginTop = 10;
				_namespaceTitle.style.marginBottom = 5;
				_namespaceTitle.style.unityFontStyleAndWeight = FontStyle.Bold;

				var _desc = new Label();
				_desc.text = "Here you can define which types should be cloned to the runtime data library on start. Data types which have implemented either the GetSerializedData and SetSerializedData method or fields that are marked with the [DatabrainSerialize] attribute and are in the runtime data library are being serialized as JSON when calling save on the data library at runtime.";
				_desc.style.whiteSpace = WhiteSpace.Normal;
				_desc.style.marginBottom = 10;

				_namespaceContainer.Add(_namespaceTitle);
				_namespaceContainer.Add(_desc);
				_namespaceContainer.Add(_updateButton);

				for (int a = 0; a < dataLibrary.existingNamespaces.Count; a++)
				{
					var _foldout = NamespaceFoldoutElement.Foldout(_namespaceContainer, dataLibrary.existingNamespaces[a].namespaceName);
					//_foldout.text = dataContainer.googleImportSettings[a].nameSpace;
					_foldout.value = false;

					var _item = new VisualElement();
					_item.style.flexDirection = FlexDirection.Column;

					for (int t = 0; t < dataLibrary.existingNamespaces[a].existingTypes.Count; t++)
					{

						var _typeContainer = new VisualElement();
						_typeContainer.style.flexDirection = FlexDirection.Row;
						_typeContainer.style.backgroundColor = DatabrainHelpers.colorLightGrey;
						_typeContainer.style.marginBottom = 2;
						_typeContainer.style.paddingLeft = 5;
						_typeContainer.style.paddingTop = 5;
						_typeContainer.style.paddingBottom = 5;


						var _title = new Label();

						var _type = System.Type.GetType(dataLibrary.existingNamespaces[a].existingTypes[t].typeAssemblyQualifiedName);
						if (_type != null)
						{
							var _customNameAttribute = _type.GetCustomAttribute<DataObjectTypeNameAttribute>();
							if (_customNameAttribute != null)
							{
								_title.text = (_customNameAttribute as DataObjectTypeNameAttribute).typeName;
							}
							else
							{
								_title.text = dataLibrary.existingNamespaces[a].existingTypes[t].typeName;
							}
						}
						else
						{
							_title.text = dataLibrary.existingNamespaces[a].existingTypes[t].typeName;
						}

						_title.style.unityFontStyleAndWeight = FontStyle.Bold;

						var _space = new VisualElement();
						_space.style.flexGrow = 1;

						//var _lbl = new Label();
						//_lbl.text = "Enable";

						var _isSerializeable = new PropertyField();
						_isSerializeable.label = "Add to runtime data library";

                        _isSerializeable.BindProperty(
							_editor.serializedObject.FindProperty("existingNamespaces" + ".Array").GetArrayElementAtIndex(a)
							.FindPropertyRelative("existingTypes" + ".Array")
							.GetArrayElementAtIndex(t).FindPropertyRelative("runtimeSerialization")
						);

						//_isSerializeable.RegisterCallback<ChangeEvent<string>>(evt =>
						//{
						//	_inspector.serializedObject.ApplyModifiedProperties();
						//});

						_isSerializeable.RegisterValueChangeCallback(x =>
						{
                            DatabrainEditorWindow[] editors = (DatabrainEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(DatabrainEditorWindow));
							if (editors.Length > 0)
							{
								editors[0].UpdateData();
							}
						});

						_typeContainer.Add(_title);
						_typeContainer.Add(_space);
						//_typeContainer.Add(_lbl);
						_typeContainer.Add(_isSerializeable);

						_item.Add(_typeContainer);


					}


					_foldout.Add(_item);
					_namespaceContainer.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));

				}


				_root.Add(_namespaceContainer);

				#endregion

			}

			_scrollView.Add(_root);


            return _scrollView;
		}
		#endregion
		#endif
	}
}
