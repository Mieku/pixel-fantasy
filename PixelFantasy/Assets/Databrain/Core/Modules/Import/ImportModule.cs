/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */

using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using Databrain.UI.Elements;
#endif

using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;


namespace Databrain.Modules.Import
{
	#if UNITY_EDITOR
	[DatabrainModuleAttribute("Import", 1, "import.png")]
	#endif
	public class ImportModule : DatabrainModuleBase
	{
		#if UNITY_EDITOR
		private static DataLibrary dataLibrary;
        private static VisualElement contentVE;
		private static VisualElement worksheetVE;

		private DatabrainEditorWindow editorWindow;


		[System.Serializable]
		public class GoogleImportSettings
		{
			public string nameSpace;
			public string googleShareURL;

			public enum ImportType
			{
				Append,
				Replace
			}

			public ImportType importType;

			public enum OrderType
			{
				normal,
				reversed
			}

			public OrderType orderType;

			public GoogleImportSettings(string _namespace)
			{
				nameSpace = _namespace;
				googleWorksheets = new List<GoogleWorksheets>();
			}

			[System.Serializable]
			public class GoogleWorksheets
			{
				public string worksheetTypeName;
				public string worksheetID;


				public GoogleWorksheets(string _name)
				{
					worksheetTypeName = _name;
				}
			}


			public List<GoogleWorksheets> googleWorksheets = new List<GoogleWorksheets>();

		}

		public List<GoogleImportSettings> googleImportSettings = new List<GoogleImportSettings>();


		[System.Serializable]
		public class CSVImportSettings
		{

			public string nameSpace;

			public enum ImportType
			{
				Append,
				Replace
			}

			public ImportType importType;

			public CSVImportSettings(string _namespace)
			{
				nameSpace = _namespace;
			}

			[System.Serializable]
			public class CSVTypesheets
			{
				public string typeName;
				public string path;

				public CSVTypesheets(string _name)
				{
					typeName = _name;
				}
			}

			public List<CSVTypesheets> csvTypesheets = new List<CSVTypesheets>();
		}

		public List<CSVImportSettings> csvImportSettings = new List<CSVImportSettings>();


        public override void Initialize(DataLibrary _dataLibrary)
        {
            DatabrainHelpers.UpdateNamespaces(_dataLibrary, out System.Collections.Generic.List<DatabrainHelpers.SortedTypes> ads);
        }

        public override VisualElement DrawGUI(DataLibrary _dataLibrary, DatabrainEditorWindow _editorWindow)
		{
            DatabrainHelpers.UpdateNamespaces(_dataLibrary, out System.Collections.Generic.List<DatabrainHelpers.SortedTypes> ads);

            editorWindow = _editorWindow;

            var _scrollView = new ScrollView();
			var _root = new VisualElement();

			var _options = new List<string>();
			_options.Add("Google Import");
			_options.Add("CSV Import");

			var _actions = new List<System.Action>();

			System.Action action1 = () =>
			{
				contentVE.Clear();
				var _content = GoogleImportUI();
				contentVE.Add(_content);
			};


			System.Action action2 = () =>
			{
				contentVE.Clear();
				var _content = CSVImportUI();
				contentVE.Add(_content);
			};

			_actions.Add(action1);
			_actions.Add(action2);


			var _tabs = new TabsElement(_options, _actions);

			contentVE = new VisualElement();

			dataLibrary = _dataLibrary;


			contentVE.Clear();
			var _content = GoogleImportUI();
			contentVE.Add(_content);

			_root.Add(_tabs);
			_root.Add(contentVE);

			_scrollView.Add(_root);

			return _scrollView;
		}

		private VisualElement GoogleImportUI()
		{

			var _root = new VisualElement();


    //        var _updateButton = new Button();
    //        _updateButton.text = "Update namespace list";
    //        _updateButton.style.height = 30;
    //        _updateButton.style.marginBottom = 10;
    //        _updateButton.RegisterCallback<ClickEvent>(click =>
    //        {
               
				//DatabrainHelpers.UpdateNamespaces(dataLibrary);

				//editorWindow.UpdateData();
                
    //        });




            var _helpContent = new VisualElement();
			_helpContent.style.backgroundColor = DatabrainHelpers.colorDarkGrey;


			/// Help content
			var _step1Container = new VisualElement();
			_step1Container.style.flexDirection = FlexDirection.Row;
			DatabrainHelpers.SetPadding(_step1Container, 4, 4, 4, 4);

			var _step1 = new Label();
			_step1.text = "1";
			_step1.style.unityFontStyleAndWeight = FontStyle.Bold;
            _step1.style.width = 20;
            _step1.style.height = 20;
			_step1.style.unityTextAlign = TextAnchor.MiddleCenter;
            DatabrainHelpers.SetBorderRadius(_step1, 10, 10, 10, 10);
			DatabrainHelpers.SetBorder(_step1, 2, Color.white);

			var _step1Desc = new Label();
			_step1Desc.style.marginLeft = 5;
			_step1Desc.style.unityTextAlign = TextAnchor.MiddleLeft;
			_step1Desc.style.whiteSpace = WhiteSpace.Normal;
			_step1Desc.text = "In Google Sheets use the share as link function and copy&paste the link in the Google Share link field.";

			_step1Container.Add(_step1);
			_step1Container.Add(_step1Desc);

            var _step2Container = new VisualElement();
            _step2Container.style.flexDirection = FlexDirection.Row;
            DatabrainHelpers.SetPadding(_step2Container, 4, 4, 4, 4);

            var _step2 = new Label();
            _step2.text = "2";
            _step2.style.unityFontStyleAndWeight = FontStyle.Bold;
            _step2.style.width = 20;
            _step2.style.height = 20;
            _step2.style.unityTextAlign = TextAnchor.MiddleCenter;
            DatabrainHelpers.SetBorderRadius(_step2, 10, 10, 10, 10);
            DatabrainHelpers.SetBorder(_step2, 2, Color.white);


            var _step2Desc = new Label();
            _step2Desc.style.marginLeft = 5;
            _step2Desc.style.unityTextAlign = TextAnchor.MiddleLeft;
            _step2Desc.style.whiteSpace = WhiteSpace.Normal;
            _step2Desc.text = "Each worksheet corresponds to a type inside of the selected namespace in Databrain. In Google sheets, select the worksheet and Copy the worksheet ID from the link in the browser" +
                " (the number after #gid=)";


            _step2Container.Add(_step2);
            _step2Container.Add(_step2Desc);

            var _step3Container = new VisualElement();
            _step3Container.style.flexDirection = FlexDirection.Row;
            DatabrainHelpers.SetPadding(_step3Container, 4, 4, 4, 4);

            var _step3 = new Label();
            _step3.text = "3";
            _step3.style.unityFontStyleAndWeight = FontStyle.Bold;
            _step3.style.width = 20;
            _step3.style.height = 20;
            _step3.style.unityTextAlign = TextAnchor.MiddleCenter;
            DatabrainHelpers.SetBorderRadius(_step3, 10, 10, 10, 10);
            DatabrainHelpers.SetBorder(_step3, 2, Color.white);


            var _step3Desc = new Label();
            _step3Desc.style.marginLeft = 5;
            _step3Desc.style.unityTextAlign = TextAnchor.MiddleLeft;
            _step3Desc.style.whiteSpace = WhiteSpace.Normal;
            _step3Desc.text = "Click import";


            _step3Container.Add(_step3);
            _step3Container.Add(_step3Desc);

        
			_helpContent.Add(_step1Container);
			_helpContent.Add(_step2Container);
			_helpContent.Add(_step3Container);

            var _helpFoldout = new Foldout();
			_helpFoldout.text = "? Help";
			_helpFoldout.value = false;
			_helpFoldout.style.unityFontStyleAndWeight = FontStyle.Bold;
			_helpFoldout.style.paddingBottom = 5;

			_helpFoldout.Add(_helpContent);


			worksheetVE = new VisualElement();

			var _namespaceContainer = new VisualElement();

			var _label = new Label();
			_label.text = "Select namespace";
			_label.style.fontSize = 12;
			_label.style.unityFontStyleAndWeight = FontStyle.Bold;
			_label.style.paddingBottom = 10;

			_namespaceContainer.Add(_label);

            //_namespaceContainer.Add(_updateButton);

            // Update list
            for (int i = 0; i < dataLibrary.existingNamespaces.Count; i++)
			{

                for (int t = 0; t < dataLibrary.existingNamespaces[i].existingTypes.Count; t++)
				{
					var _extNamespace = googleImportSettings.Where(x => x.nameSpace == dataLibrary.existingNamespaces[i].namespaceName).FirstOrDefault();
					if (_extNamespace != null)
					{
						var _extType = _extNamespace.googleWorksheets.Where(x => x.worksheetTypeName == dataLibrary.existingNamespaces[i].existingTypes[t].typeName).FirstOrDefault();

						if (_extType == null)
						{
							_extNamespace.googleWorksheets.Add(new GoogleImportSettings.GoogleWorksheets(dataLibrary.existingNamespaces[i].existingTypes[t].typeName));
						}

					}
					else
					{
						googleImportSettings.Add(new GoogleImportSettings(dataLibrary.existingNamespaces[i].namespaceName));
						googleImportSettings[googleImportSettings.Count - 1].googleWorksheets.Add(new GoogleImportSettings.GoogleWorksheets(dataLibrary.existingNamespaces[i].existingTypes[t].typeName));
					}
				}
			}

            // cleanup
            for (int g = 0; g < googleImportSettings.Count; g++)
            {
                var _namespaceExists = false;
                var _index = g;

                for (int i = 0; i < dataLibrary.existingNamespaces.Count; i++)
                {
                    if (googleImportSettings[g].nameSpace == dataLibrary.existingNamespaces[i].namespaceName)
                    {
                        _namespaceExists = true;
                    }
                }

                if (!_namespaceExists)
                {
                    googleImportSettings.RemoveAt(_index);
                }
                else
                {
                    for (int s = 0; s < googleImportSettings[g].googleWorksheets.Count; s++)
                    {
                        var _typeExists = false;
                        var _windex = 0;
                        for (int n = 0; n < dataLibrary.existingNamespaces.Count; n++)
                        {
                         
                            for (int t = 0; t < dataLibrary.existingNamespaces[n].existingTypes.Count; t++)
                            {
                                if (googleImportSettings[g].googleWorksheets[s].worksheetTypeName == dataLibrary.existingNamespaces[n].existingTypes[t].typeName)
                                {
									
                                    _typeExists = true;
                                    
                                }
                            }

                          
                        }

                        if (!_typeExists)
                        {
                            if (_windex < googleImportSettings[g].googleWorksheets.Count)
                            {
                                googleImportSettings[g].googleWorksheets.RemoveAt(_windex);
                            }
                        }
                    }
                }
            }


            var _editor = UnityEditor.Editor.CreateEditor(this);

			// build UI
			for (int a = 0; a < googleImportSettings.Count; a++)
			{
				var _foldout = NamespaceFoldoutElement.Foldout(_namespaceContainer, googleImportSettings[a].nameSpace);
				_foldout.value = false;

				var _item = new VisualElement();
				_item.style.marginLeft = -20;
				_item.style.paddingBottom = 5;
				_item.style.paddingTop = 5;
				_item.style.paddingLeft = 5;
				_item.style.paddingRight = 5;

				var _title = new Label();
				_title.text = googleImportSettings[a].nameSpace;
				_title.style.fontSize = 12;
				_title.style.unityFontStyleAndWeight = FontStyle.Bold;
				_title.style.paddingBottom = 10;

				var _worksheetTitle = new Label();
				_worksheetTitle.text = "Worksheet IDs";
				_worksheetTitle.style.fontSize = 12;
				_worksheetTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
				_worksheetTitle.style.paddingBottom = 5;
				_worksheetTitle.style.paddingTop = 10;

				var _googleLinkItem = new VisualElement();
				_googleLinkItem.style.borderLeftWidth = 2;
				_googleLinkItem.style.borderLeftColor = Color.grey;
				_googleLinkItem.style.paddingLeft = 10;

				var _googleLinkTitle = new Label();
				_googleLinkTitle.text = "Google Share Link";

				var _googleLink = new TextField();
				_googleLink.BindProperty(_editor.serializedObject.FindProperty(nameof(googleImportSettings) + ".Array").GetArrayElementAtIndex(a).FindPropertyRelative("googleShareURL"));
				_googleLink.RegisterCallback<ChangeEvent<string>>(evt =>
				{
					_editor.serializedObject.ApplyModifiedProperties();
				});

				_googleLinkItem.Add(_googleLinkTitle);
				_googleLinkItem.Add(_googleLink);



				_item.Add(_title);
				_item.Add(_googleLinkItem);

				_item.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));

				var _worksheetIds = new VisualElement();
				_worksheetIds.style.marginTop = 5;
				_worksheetIds.style.borderLeftWidth = 2;
				_worksheetIds.style.borderLeftColor = Color.grey;
				_worksheetIds.style.paddingLeft = 10;

				_item.Add(_worksheetTitle);

				for (int b = 0; b < googleImportSettings[a].googleWorksheets.Count; b++)
				{
					var _typeName = new Label();
					_typeName.text = googleImportSettings[a].googleWorksheets[b].worksheetTypeName;

					var _idTextField = new TextField();
					_idTextField.BindProperty(_editor.serializedObject.FindProperty(nameof(googleImportSettings) + ".Array").GetArrayElementAtIndex(a).FindPropertyRelative("googleWorksheets.Array").GetArrayElementAtIndex(b).FindPropertyRelative("worksheetID"));
					_idTextField.RegisterCallback<ChangeEvent<string>>(evt =>
					{
						_editor.serializedObject.ApplyModifiedProperties();
					});


					_worksheetIds.Add(_typeName);
					_worksheetIds.Add(_idTextField);

					_item.Add(_worksheetIds);

				}

				var _importType = new EnumField();
				_importType.BindProperty(_editor.serializedObject.FindProperty(nameof(googleImportSettings) + ".Array").GetArrayElementAtIndex(a).FindPropertyRelative("importType"));
				_importType.label = "Import Type";
				_importType.style.marginTop = 10;

				var _importButton = new Button();
				_importButton.text = "Import";
				_importButton.style.height = 40;
				_importButton.style.marginTop = 5;

				int _index = a;

				_importButton.RegisterCallback<ClickEvent>(click =>
				{
					_importButton.SetEnabled(false);
					_importButton.text = "Downloading...";

					System.Action action = () =>
					{
						_importButton.SetEnabled(true);
						_importButton.text = "Import";

                        editorWindow.UpdateSelectedDataTypeList();
					};

					GoogleImportSettings.ImportType _it = (GoogleImportSettings.ImportType)_importType.value;
					DatabrainGoogleSheetDownloader.Download(dataLibrary, googleImportSettings[_index].nameSpace, _it, action);
				});

				_item.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));
				_item.Add(_importType);
				_item.Add(_importButton);

				_foldout.Add(_item);

				_namespaceContainer.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));
			}


			var _console = new VisualElement();
			_console.style.marginBottom = 5;
			_console.style.marginTop = 5;
			_console.style.marginLeft = 5;
			_console.style.marginRight = 5;
			_console.style.borderBottomWidth = 1;
			_console.style.borderTopWidth = 1;
			_console.style.borderLeftWidth = 1;
			_console.style.borderRightWidth = 1;
			_console.style.backgroundColor = DatabrainHelpers.colorDarkGrey;


			var _consoleLabel = new Label();
			_consoleLabel.text = "Status...";

			_console.Add(_consoleLabel);

			//_root.Add(_recreateButton);
			_root.Add(_helpFoldout);
			_root.Add(_namespaceContainer);
			_root.Add(_console);

			//_container.Add(_root);

			return _root;

		}

		public void OnDownloadComplete()
		{

		}


		public VisualElement CSVImportUI()
		{
			var _root = new VisualElement();

			var _namespaceContainer = new VisualElement();

			var _label = new Label();
			_label.text = "Select namespace";
			_label.style.fontSize = 12;
			_label.style.unityFontStyleAndWeight = FontStyle.Bold;
			_label.style.paddingBottom = 10;

			_namespaceContainer.Add(_label);


			var _editor = UnityEditor.Editor.CreateEditor(this);

			// Update list
			for (int i = 0; i < dataLibrary.existingNamespaces.Count; i++)
			{
				for (int t = 0; t < dataLibrary.existingNamespaces[i].existingTypes.Count; t++)
				{
					var _extNamespace = csvImportSettings.Where(x => x.nameSpace == dataLibrary.existingNamespaces[i].namespaceName).FirstOrDefault();
					if (_extNamespace != null)
					{
						var _extType = _extNamespace.csvTypesheets.Where(x => x.typeName == dataLibrary.existingNamespaces[i].existingTypes[t].typeName).FirstOrDefault();

						if (_extType == null)
						{
							_extNamespace.csvTypesheets.Add(new CSVImportSettings.CSVTypesheets(dataLibrary.existingNamespaces[i].existingTypes[t].typeName));
						}

					}
					else
					{
						csvImportSettings.Add(new CSVImportSettings(dataLibrary.existingNamespaces[i].namespaceName));
					}
				}
			}


			// build UI
			for (int a = 0; a < csvImportSettings.Count; a++)
			{
				var _foldout = NamespaceFoldoutElement.Foldout(_namespaceContainer, csvImportSettings[a].nameSpace);
				_foldout.value = false;

				var _item = new VisualElement();
				_item.style.marginLeft = -20;
				_item.style.paddingBottom = 5;
				_item.style.paddingTop = 5;
				_item.style.paddingLeft = 5;
				_item.style.paddingRight = 5;


				for (int t = 0; t < csvImportSettings[a].csvTypesheets.Count; t++)
				{
					var _subItem = new VisualElement();

					_subItem.style.paddingBottom = 5;
					_subItem.style.paddingTop = 5;
					_subItem.style.paddingLeft = 5;
					_subItem.style.paddingRight = 5;
					_subItem.style.marginBottom = 10;

					_subItem.style.borderBottomWidth = 1;
					_subItem.style.borderTopWidth = 1;
					_subItem.style.borderLeftWidth = 1;
					_subItem.style.borderRightWidth = 1;
					_subItem.style.borderBottomColor = DatabrainHelpers.colorLightGrey;
					_subItem.style.borderTopColor = DatabrainHelpers.colorLightGrey;
					_subItem.style.borderLeftColor = DatabrainHelpers.colorLightGrey;
					_subItem.style.borderRightColor = DatabrainHelpers.colorLightGrey;



					var _title = new Label();
					_title.text = csvImportSettings[a].csvTypesheets[t].typeName;
					_title.style.fontSize = 14;
					_title.style.unityFontStyleAndWeight = FontStyle.Bold;
					_title.style.paddingBottom = 10;



					var _selectFileButton = new Button();
					_selectFileButton.text = "Select CSV file";

					var _index1 = a;
					var _index2 = t;
					_selectFileButton.RegisterCallback<ClickEvent>(click =>
					{
						csvImportSettings[_index1].csvTypesheets[_index2].path = EditorUtility.OpenFilePanel("Select CSV file", "", "");
					});

					var _importType = new EnumField();
					_importType.BindProperty(_editor.serializedObject.FindProperty(nameof(csvImportSettings) + ".Array").GetArrayElementAtIndex(a).FindPropertyRelative("importType"));
					_importType.label = "Import Type";
					_importType.style.marginTop = 10;

					var _importButton = new Button();
					_importButton.text = "Import";
					_importButton.style.height = 40;
					_importButton.style.marginTop = 5;
					_importButton.RegisterCallback<ClickEvent>(click =>
					{
						if (System.IO.File.Exists(csvImportSettings[_index1].csvTypesheets[_index2].path))
						{
							var _csvString = ReadFile(csvImportSettings[_index1].csvTypesheets[_index2].path);

							List<DatabrainCSVConverter.Entry> _entries = DatabrainCSVConverter.ConvertCSV(_csvString);
                            DatabrainCSVConverter.BuildDataFromCSV(dataLibrary, _entries, csvImportSettings[_index1].csvTypesheets[_index2].typeName, csvImportSettings[_index1].importType.ToString());
						}
					});


					_subItem.Add(_title);


					if (!string.IsNullOrEmpty(csvImportSettings[a].csvTypesheets[t].path))
					{
						var _path = new Label();
						_path.text = csvImportSettings[a].csvTypesheets[t].path;
						_path.style.backgroundColor = DatabrainHelpers.colorDarkGrey;
						_path.style.whiteSpace = WhiteSpace.Normal;

						_subItem.Add(_path);
					}


					_subItem.Add(_selectFileButton);
					_subItem.Add(_importType);
					_subItem.Add(_importButton);

					_item.Add(_subItem);

				}



				_foldout.Add(_item);

				_namespaceContainer.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorDarkGrey));
			}

			_root.Add(_namespaceContainer);


			return _root;
		}

		string ReadFile(string _path)
		{
			var _sr = new System.IO.StreamReader(_path);

			var _out = _sr.ReadToEnd();

			_sr.Close();

			return _out;
		}
		#endif
    }
	
}
