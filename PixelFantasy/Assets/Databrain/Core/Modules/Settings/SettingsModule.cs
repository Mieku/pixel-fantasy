/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Helpers;
using Databrain.UI;
using System.Linq;

namespace Databrain.Modules.Settings
{

    #if UNITY_EDITOR
	[DatabrainModuleAttribute("Settings", 2, "settings.png")]
    #endif
	public class SettingsModule : DatabrainModuleBase
	{
        #if UNITY_EDITOR
		private VisualElement tagContainer;

		public override VisualElement DrawGUI(DataLibrary _dataLibrary, DatabrainEditorWindow _editorWindow)
		{
			var _root = new VisualElement();

			var _editor = Editor.CreateEditor(_dataLibrary);


            #region tags
            /// TAGS
            ////////////////////////////////
            var _tagsSettings = new VisualElement();
			DatabrainHelpers.SetPadding(_tagsSettings, 5, 5, 5, 5);

			var _tagsLabel = new Label();
			_tagsLabel.text = "Tags";
            _tagsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _tagsLabel.style.fontSize = 14;

			var _tagsDescription = new Label();
			_tagsDescription.text = "Tags can be searched by using t: in the search bar (example: t:enemy npc)";

            var _tagField = new TextField();
            _tagField.label = "Tag";

			var _addTag = new Button();
			_addTag.text = "Add tag";
			_addTag.RegisterCallback<ClickEvent>(evt =>
			{
                if (!_dataLibrary.tags.Contains(_tagField.value))
                {
                    _dataLibrary.tags.Add(_tagField.value);
                    DatabrainTags.ShowTagsDataObject( tagContainer, _dataLibrary.tags, _dataLibrary.tags, null);
                    _editorWindow.SetupForceRebuild(_dataLibrary, true);
                }
            });

            _tagsSettings.Add(_tagsLabel);		
            _tagsSettings.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorLightGrey));
            _tagsSettings.Add(_tagsDescription);
            _tagsSettings.Add(_tagField);
			_tagsSettings.Add(_addTag);
            _tagsSettings.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorLightGrey));

			tagContainer = new VisualElement();
            tagContainer.style.flexDirection = FlexDirection.Row;
            tagContainer.style.flexWrap = Wrap.Wrap;

            _tagsSettings.Add(tagContainer);


            DatabrainTags.ShowTagsDataObject( tagContainer, _dataLibrary.tags, _dataLibrary.tags, (type) => 
            {
                _editorWindow.SetupForceRebuild(_dataLibrary, true);
            });
            #endregion


            #region themetemplate
            /// THEME TEMPLATE
            ////////////////////////////////
            var _themeSettings = new VisualElement();
            DatabrainHelpers.SetPadding(_themeSettings, 5, 5, 5, 5);

            var _themeLabel = new Label();
            _themeLabel.text = "Theme";
            _themeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _themeLabel.style.fontSize = 14;

            _themeSettings.Add(_themeLabel);
            _themeSettings.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorLightGrey));

            var _themeDescription = new Label();
            _themeDescription.style.whiteSpace = WhiteSpace.Normal;
            _themeDescription.text = "Here you can assign a custom theme template.";
            _themeDescription.style.marginBottom = 10;

            _themeSettings.Add(_themeDescription);

            var _themetemplateSettings = new VisualElement();
            _themetemplateSettings.style.flexDirection = FlexDirection.Row;
            _themetemplateSettings.style.flexGrow = 1;

            DatabrainHelpers.SetBorder(_themetemplateSettings, 1, DatabrainHelpers.colorLightGrey);

            var _baseFoldoutTemplate = new PropertyField();
            _baseFoldoutTemplate.style.flexGrow = 1;
            _baseFoldoutTemplate.BindProperty(_editor.serializedObject.FindProperty("themeTemplate"));

            var _createVisualTreeTemplate = new Button();
            _createVisualTreeTemplate.text = "Create new template";
            _createVisualTreeTemplate.RegisterCallback<ClickEvent>(evt =>
            {
                var _path = EditorUtility.SaveFilePanelInProject("Create new template", "DatabrainThemeTemplate", "asset", "Create new Databrain visual tree template");
                if (_path.Length != 0)
                {
                    var _instance = ScriptableObject.CreateInstance<DatabrainThemeTemplate>();

                    AssetDatabase.CreateAsset(_instance, _path);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    //_dataLibrary.visualTreeTemplate = _instance;
                }
            });

            var _reloadTheme = new Button();
            _reloadTheme.text = "Reload";
            _reloadTheme.RegisterCallback<ClickEvent>(evt =>
            {
                _editorWindow.SetupForceRebuild(_dataLibrary);
            });

            _themetemplateSettings.Add(_baseFoldoutTemplate);
            _themetemplateSettings.Add(_createVisualTreeTemplate);
            _themetemplateSettings.Add(_reloadTheme);

            _themeSettings.Add(_themetemplateSettings);
            #endregion


            #region hierarchy
            /// HIERARCHY & VISIBILITY
            ////////////////////////////////
            var _visibilitySettings = new VisualElement();
            DatabrainHelpers.SetPadding(_visibilitySettings, 5, 5, 5, 5);

            var _visibilityLabel = new Label();
			_visibilityLabel.text = "Hierarchy";
			_visibilityLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			_visibilityLabel.style.fontSize = 14;

			_visibilitySettings.Add(_visibilityLabel);
			_visibilitySettings.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorLightGrey));

			var _visibilityDescription = new Label();
            _visibilityDescription.style.whiteSpace = WhiteSpace.Normal;
			_visibilityDescription.text = "Here you can hide namespaces from the default hierarchy list or assign a custom hierarchy template.";
			_visibilityDescription.style.marginBottom = 10;

			_visibilitySettings.Add(_visibilityDescription);

            var _templateSettings = new VisualElement();
            _templateSettings.style.flexDirection = FlexDirection.Row;
            _templateSettings.style.flexGrow = 1;
            DatabrainHelpers.SetBorder(_templateSettings, 1, DatabrainHelpers.colorLightGrey);

            var _hierarchyTemplate = new PropertyField();
            _hierarchyTemplate.BindProperty(_editor.serializedObject.FindProperty("hierarchyTemplate"));

            var _createTemplate = new Button();
            _createTemplate.text = "Create new template";
            _createTemplate.RegisterCallback<ClickEvent>(evt =>
            {
                var _path = EditorUtility.SaveFilePanelInProject("Create new template", "HierarchyTemplate", "asset", "Create new Databrain hierarchy template");
                if (_path.Length != 0)
                {
                    var _instance = ScriptableObject.CreateInstance<DatabrainHierarchyTemplate>();

                    AssetDatabase.CreateAsset(_instance, _path);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    _dataLibrary.hierarchyTemplate = _instance;
                }
            });

            var _reloadEditor = new Button();
            _reloadEditor.text = "Reload";
            _reloadEditor.RegisterCallback<ClickEvent>(evt =>
            {
                _editorWindow.SetupForceRebuild(_dataLibrary);
            });

            _templateSettings.Add(_hierarchyTemplate);
            _templateSettings.Add(_createTemplate);
            _templateSettings.Add(_reloadEditor);

            _visibilitySettings.Add(_templateSettings);

            var _defaultHierarchyLabel = new Label();
            _defaultHierarchyLabel.text = "Visibility";
            _defaultHierarchyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            _visibilitySettings.Add(DatabrainHelpers.VerticalSpace(10));
            _visibilitySettings.Add(_defaultHierarchyLabel);

            for (int i = 0; i < _dataLibrary.existingNamespaces.Count; i++)
			{
				var _hideNamespaceToggle = new Toggle();
				_hideNamespaceToggle.text = "Hide " + _dataLibrary.existingNamespaces[i].namespaceName;
				_hideNamespaceToggle.BindProperty(_editor.serializedObject.FindProperty("existingNamespaces.Array").GetArrayElementAtIndex(i).FindPropertyRelative("hidden"));

                _hideNamespaceToggle.RegisterValueChangedCallback(x =>
                {
                    DatabrainEditorWindow[] editors = (DatabrainEditorWindow[])Resources.FindObjectsOfTypeAll(typeof(DatabrainEditorWindow));
                    if (editors.Length > 0)
                    {
                        editors[0].UpdateData();
                    }
                });

                _visibilitySettings.Add(_hideNamespaceToggle);
			}
            #endregion


            #region debug
            /// DEBUG
            ////////////////////////////////
            var _debugSettings = new VisualElement();
			DatabrainHelpers.SetPadding(_debugSettings, 5, 5, 5, 5);

            var _debugLabel = new Label();
            _debugLabel.text = "Debug";
            _debugLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _debugLabel.style.fontSize = 14;

            var _unhideAllSubAssetsButton = new Button();
			_unhideAllSubAssetsButton.text = "Unhide sub-assets";
			_unhideAllSubAssetsButton.RegisterCallback<ClickEvent>(evt =>
			{
				UnhideAssets(_dataLibrary);
			});


            var _hideAllSubAssetsButton = new Button();
            _hideAllSubAssetsButton.text = "Hide sub-assets";
            _hideAllSubAssetsButton.RegisterCallback<ClickEvent>(evt =>
            {
                HideAssets(_dataLibrary);
            });

			_debugSettings.Add(_debugLabel);
			_debugSettings.Add(DatabrainHelpers.Separator(1, DatabrainHelpers.colorLightGrey));
			_debugSettings.Add(_unhideAllSubAssetsButton);
			_debugSettings.Add(_hideAllSubAssetsButton);
            #endregion



            _root.Add(_tagsSettings);
            _root.Add(_themeSettings);
			_root.Add(_visibilitySettings);
			_root.Add(_debugSettings);

			return _root;

		}

		void UnhideAssets(DataLibrary _dataLibrary)
		{
            string assetPath = AssetDatabase.GetAssetPath(_dataLibrary);
            UnityEngine.Object [] allAsset = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(asset => asset != null).ToArray();

			foreach (var _asset in allAsset)
			{
				_asset.hideFlags = HideFlags.None;
			}


            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            EditorUtility.SetDirty(assetImporter);
            assetImporter.SaveAndReimport();
        }

		void HideAssets(DataLibrary _dataLibrary)
		{
            string assetPath = AssetDatabase.GetAssetPath(_dataLibrary);
            UnityEngine.Object[] allAsset = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(asset => asset != null).ToArray();

            foreach (var _asset in allAsset)
            {
				if (_asset.GetType() == typeof(DataLibrary))
					continue;

                _asset.hideFlags = HideFlags.HideInHierarchy;
            }


            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            EditorUtility.SetDirty(assetImporter);
            assetImporter.SaveAndReimport();
        }
        #endif
    }
    
}
