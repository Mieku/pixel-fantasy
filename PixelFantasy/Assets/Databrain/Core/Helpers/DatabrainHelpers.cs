/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using Databrain.Attributes;

namespace Databrain.Helpers
{
	public static class DatabrainHelpers
	{
#if UNITY_EDITOR
		public static Color colorNormal = new Color(113f / 255f, 230f / 255f, 255f / 255f);
		public static Color colorRuntime = new Color(42f / 255f, 137f / 255f, 223f / 255f);
		public static Color colorError = new Color(209f / 255f, 89f / 255f, 89f / 255f);
		public static Color colorRed = new Color(175f / 255f, 50f / 255f, 50f / 255f);
		public static Color colorDarkGrey = new Color(50f / 255f, 50f / 255f, 50f / 255f);
		public static Color colorLightGrey = new Color(80f / 255f, 80f / 255f, 80f / 255f);

        public class SortedTypes
        {
            public int index;
            public System.Type type;

            public SortedTypes(int _index, System.Type _type)
            {
                index = _index;
                type = _type;
            }
        }

        public static void SetCursor(this VisualElement element, MouseCursor cursor)
		{
			object objCursor = new UnityEngine.UIElements.Cursor();
			PropertyInfo fields = typeof(UnityEngine.UIElements.Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
			fields.SetValue(objCursor, (int)cursor);
			element.style.cursor = new StyleCursor((UnityEngine.UIElements.Cursor)objCursor);
		}

		public static Texture2D LoadTexture(string _fileName, string _rootFile)
		{
			var _res = Directory.EnumerateFiles("Assets/", _rootFile, SearchOption.AllDirectories);
			var _found = _res.FirstOrDefault();
			var _path = "";
			if (!string.IsNullOrEmpty(_found))
			{
				_path = _found.Replace(_rootFile, "").Replace("\\", "/");
			}

            return (Texture2D)(AssetDatabase.LoadAssetAtPath(_path + "/" + _fileName, typeof(Texture2D)));
        }


		public static string GetRelativeIconPath()
		{
			return System.IO.Path.Combine(DatabrainHelpers.GetRelativeResPath(), "Icons");
		}

        public static string GetRelativeRootPath(string _rootFile = "DatabrainRoot.cs")
        {
            var _res = System.IO.Directory.EnumerateFiles("Assets/", _rootFile, System.IO.SearchOption.AllDirectories);

            var _path = "";

            var _found = _res.FirstOrDefault();
            if (!string.IsNullOrEmpty(_found))
            {
                _path = _found.Replace(_rootFile, "").Replace("\\", "/");
            }

            return _path;
        }

        public static string GetRelativeResPath()
		{
            //var _theme = "Light";

            //	#if UNITY_EDITOR
            //if (EditorGUIUtility.isProSkin)
            //{
            //	_theme = "Dark";
            //}
            //	#endif


            var _path = EditorPrefs.GetString("DATABRAIN_RESPATH");
			if (!AssetDatabase.IsValidFolder(_path))
			{
				_path = "";
			}

            if (string.IsNullOrEmpty(_path))
			{
				var _res = System.IO.Directory.EnumerateFiles("Assets/", "DatabrainResPath.cs", System.IO.SearchOption.AllDirectories);

				var _found = _res.FirstOrDefault();
				if (!string.IsNullOrEmpty(_found))
				{
					_path = _found.Replace("DatabrainResPath.cs", "").Replace("\\", "/");
					//_path = Path.Combine(_path, _theme);

					// Cache resource path
					EditorPrefs.SetString("DATABRAIN_RESPATH", _path);
				}
			}

			return _path;
		}


		public static Texture2D LoadIcon(string _name)
		{

			// Check if _name is a complete path ex.(Assets/MyNodes/Icons/nodeIcon.png)
			var _dataPath = Application.dataPath;
			_dataPath = _dataPath.Replace("/Assets", "");
			var _completePath = Path.Combine(_dataPath, _name);

			if (File.Exists(_completePath))
			{
#if UNITY_EDITOR
				return (Texture2D)(AssetDatabase.LoadAssetAtPath(_name, typeof(Texture2D)));
#else
				return null;
#endif
			}
			else
			{

				if (!Path.HasExtension(_name))
				{
					_name = _name + ".png";
				}

#if UNITY_EDITOR
				return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeIconPath() + "/" + _name, typeof(Texture2D)));
#else
				return null;
#endif

			}
		}

        public static Texture2D LoadHierarchyLogoIcon()
        {
            return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/databrain_hierarchy_icon.png", typeof(Texture2D)));
        }

        public static Texture2D LoadLogoIcon()
		{
			return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/databrain_icon.png", typeof(Texture2D)));
		}

		public static Texture2D LoadLogoLarge()
		{
			return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/databrain_logo.png", typeof(Texture2D)));
		}

		public static Texture2D LoadGGLogo()
		{
			return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/GiantGrey.png", typeof(Texture2D)));
		}

		public static Texture2D LoadResourceTexture(string _name)
		{
			return (Texture2D)(AssetDatabase.LoadAssetAtPath(GetRelativeResPath() + "/" + _name, typeof(Texture2D)));

		}


		static string GetUIAssetPath(bool _forceRefresh = false)
		{
            var _path = EditorPrefs.GetString("DATABRAIN_UIASSETPATH");
            if (!AssetDatabase.IsValidFolder(_path))
            {
                _path = "";
            }

			if (_forceRefresh)
			{
                _path = "";
            }

            if (string.IsNullOrEmpty(_path))
            {
                var _res = System.IO.Directory.EnumerateFiles("Assets/", "DatabrainUIAssetPath.cs", System.IO.SearchOption.AllDirectories);
                var _found = _res.FirstOrDefault();

                if (!string.IsNullOrEmpty(_found))
                {
                    _path = _found.Replace("DatabrainUIAssetPath.cs", "").Replace("\\", "/");
                    //_path = Path.Combine(_path, _theme);

                    EditorPrefs.SetString("DATABRAIN_UIASSETPATH", _path);
                }

            }

			return _path;
        }


		public static VisualTreeAsset GetVisualAsset(string _name)
		{
			var _finalPath = Path.Combine(GetUIAssetPath(), _name);
			var _visualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_finalPath);

            if (_visualElement == null)
            {
                _finalPath = Path.Combine(GetUIAssetPath(true), _name);
                _visualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_finalPath);
            }

            return _visualElement;

		}

		public static VisualTreeAsset GetVisualAsset(string _scriptName, string _assetName)
		{
			var _path = EditorPrefs.GetString("DATABRAIN_" + _scriptName);
            if (!AssetDatabase.IsValidFolder(_path))
            {
                _path = "";
            }

			if (string.IsNullOrEmpty(_path))
			{
				var _res = System.IO.Directory.EnumerateFiles("Assets/", _scriptName, System.IO.SearchOption.AllDirectories);


				var _found = _res.FirstOrDefault();
				if (!string.IsNullOrEmpty(_found))
				{
					_path = _found.Replace(_scriptName, "").Replace("\\", "/");

                    EditorPrefs.SetString("DATABRAIN_" + _scriptName, _path);
                }
			}

			var _finalPath = Path.Combine(_path, _assetName);

			var _visualElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_finalPath);

			return _visualElement;

		}

		public static StyleSheet GetStyleSheet(string _name)
		{

			var _finalPath = Path.Combine(GetUIAssetPath(), _name);
			var _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_finalPath);

            if (_styleSheet == null)
            {
                _finalPath = Path.Combine(GetUIAssetPath(true), _name);
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_finalPath);
            }

            return _styleSheet;
		}

		public static VisualElement VerticalSpace(int _space)
		{
			var _s = new VisualElement();
			_s.style.height = _space;

			return _s;
		}

        public static VisualElement HorizontalSpace()
        {
            var _s = new VisualElement();
			_s.style.flexDirection = FlexDirection.Row;
			_s.style.flexGrow = 1;

            return _s;
        }

        public static VisualElement Separator(int _height, Color? _color = null)
		{
			var _s = new VisualElement();
			_s.style.flexGrow = 1;
			_s.style.height = _height;
			_s.style.marginTop = 5;
			_s.style.marginBottom = 5;
			_s.style.backgroundColor = _color ?? Color.black;

			return _s;
		}

		public static VisualElement SetBorder(VisualElement _target, int _width, Color? _color = null)
		{
			_target.style.borderLeftWidth = _width;
			_target.style.borderRightWidth = _width;
			_target.style.borderTopWidth = _width;
			_target.style.borderBottomWidth = _width;

			_target.style.borderRightColor = _color ?? colorLightGrey;
			_target.style.borderLeftColor = _color ?? colorLightGrey;
			_target.style.borderBottomColor = _color ?? colorLightGrey;
			_target.style.borderTopColor = _color ?? colorLightGrey;

			return _target;
		}

		public static void SetMargin(VisualElement _target, int _left = 0, int _right = 0, int _top = 0, int _bottom = 0)
		{
			_target.style.marginLeft = _left;
			_target.style.marginRight = _right;
			_target.style.marginTop = _top;
			_target.style.marginBottom = _bottom;
		}

		public static void SetPadding(VisualElement _target, int _left = 0, int _right = 0, int _top = 0, int _bottom = 0)
		{
			_target.style.paddingLeft = _left;
			_target.style.paddingRight = _right;
			_target.style.paddingTop = _top;
			_target.style.paddingBottom = _bottom;
		}

		public static void SetBorderRadius(VisualElement _target, int _bottomLeftRadius, int _bottomRightRadius, int _topLeftRadius, int _topRightRadius)
		{
			_target.style.borderBottomLeftRadius = _bottomLeftRadius;
			_target.style.borderBottomRightRadius = _bottomRightRadius;
			_target.style.borderTopLeftRadius = _topLeftRadius;
			_target.style.borderTopRightRadius = _topRightRadius;
		}

		public static void SetTitle(Label _target, string _text)
		{
			_target.style.fontSize = 14;
			_target.style.unityFontStyleAndWeight = FontStyle.Bold;
			_target.style.marginTop = 5;
			_target.text = _text;
		}

		public static Button DatabrainButton(string _title)
		{
			var _b = new Button();
			_b.text = _title;
			SetBorderRadius(_b, 0, 0, 0, 0);
			SetMargin(_b, 0, 0, 0, 0);
			SetPadding(_b, 0, 0, 0, 0);
			SetBorder(_b, 0);

			return _b;
		}

		public static void SetScriptingDefineSymbols(string[] _symbols)
        {
            #if UNITY_6000_0_OR_NEWER
            string definesString = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
			
			 List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(_symbols.Except(allDefines));

			PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),  definesString);

            #else
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
			List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(_symbols.Except(allDefines));
			
			PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
            #endif
        }


		public static DatabrainEditorWindow OpenEditor(int instanceID, bool _newWindow, DataLibrary _initialLibrary = null, bool _forceRefresh = false)
		{
            DataLibrary _container = EditorUtility.InstanceIDToObject(instanceID) as DataLibrary;
			DatabrainEditorWindow window = null;


            if (_container != null)
            {
                DatabrainEditorWindow[] _w = Resources.FindObjectsOfTypeAll<DatabrainEditorWindow>();
				if (_w.Length > 0)
				{
					for (int i = 0; i < _w.Length; i++)
					{
						if (_w[i].container == _container)
						{
							window = _w[i];
						}
					}


					if (window != null && !_newWindow)
					{
						window.titleContent = new GUIContent("Databrain - " + _container.name, DatabrainHelpers.LoadLogoIcon());
						if (_forceRefresh)
						{
							window.SetupForceRebuild(_container);

						}
						else
						{
							window.Setup(_container, _initialLibrary == null ? _container.initialLibrary : _initialLibrary);
						}
						if (!Application.isPlaying)
						{
							window.Focus();
						}
					}
					else
					{
						if (_newWindow)
						{
							var _nw = EditorWindow.CreateWindow<DatabrainEditorWindow>(typeof(DatabrainEditorWindow));
							_nw.titleContent = new GUIContent("Databrain - " + _container.name, DatabrainHelpers.LoadLogoIcon());
							if (_forceRefresh)
							{
								_nw.SetupForceRebuild(_container);

							}
							else
							{
								_nw.Setup(_container, _initialLibrary == null ? _container.initialLibrary : _initialLibrary);
							}
							if (!Application.isPlaying)
							{
								_nw.Focus();
							}
							window = _nw;
						}
						else
						{
							window = EditorWindow.CreateWindow<DatabrainEditorWindow>(typeof(DatabrainEditorWindow));
							window.titleContent = new GUIContent("Databrain - " + _container.name, DatabrainHelpers.LoadLogoIcon());
							if (_forceRefresh)
							{
								window.SetupForceRebuild(_container);

							}
							else
							{
								window.Setup(_container, _initialLibrary == null ? _container.initialLibrary : _initialLibrary);
							}
							if (!Application.isPlaying)
							{
								window.Focus();
							}
						}
					}
				}
				else
				{
					if (_newWindow)
					{
						var _nw = EditorWindow.CreateWindow<DatabrainEditorWindow>(typeof(DatabrainEditorWindow));
						_nw.titleContent = new GUIContent("Databrain - " + _container.name, DatabrainHelpers.LoadLogoIcon());
						if (_forceRefresh)
						{
							_nw.SetupForceRebuild(_container);

						}
						else
						{
							_nw.Setup(_container, _initialLibrary == null ? _container.initialLibrary : _initialLibrary);
						}
						if (!Application.isPlaying)
						{
							_nw.Focus();
						}
                            window = _nw;
					}
					else
					{
						window = EditorWindow.CreateWindow<DatabrainEditorWindow>(typeof(DatabrainEditorWindow));
						window.titleContent = new GUIContent("Databrain - " + _container.name, DatabrainHelpers.LoadLogoIcon());
						if (_forceRefresh)
						{
							window.SetupForceRebuild(_container);

						}
						else
						{
							window.Setup(_container, _initialLibrary == null ? _container.initialLibrary : _initialLibrary);
						}

						if (!Application.isPlaying)
						{
							window.Focus();
						}
					}
				}

            }
            else 
            {
                //Debug.Log("container is null");
            }

			return window;
        }

		public static DatabrainEditorWindow GetOpenEditor(int instanceID)
		{
			DataLibrary _container = EditorUtility.InstanceIDToObject(instanceID) as DataLibrary;
			DatabrainEditorWindow window = null;
			if (_container != null)
			{
				DatabrainEditorWindow[] _w = Resources.FindObjectsOfTypeAll<DatabrainEditorWindow>();
				if (_w.Length > 0)
				{
					for (int i = 0; i < _w.Length; i++)
					{
						if (_w[i].container == _container)
						{
							window = _w[i];
						}
					}

					if (window != null)
					{
						return window;
					}
				}
				else
				{
                    return null;
                }
			}
			else
			{
				return null;
			}

            return null;
        }

        public static string LoadChangelog(string _rootFile)
        {
			if (File.Exists(Path.Combine(GetRelativeRootPath(_rootFile), "Changelog.txt")))
			{
				var _log = File.ReadAllText(Path.Combine(GetRelativeRootPath(_rootFile), "Changelog.txt"));
				if (_log != null)
				{
					return _log;
				}
				else
				{
					return "";
				}
			}
			else
			{
				return "";
			}
        }

        public static System.Version GetEditorVersionNumber()
        {
            var _changelog = LoadChangelog("DatabrainRoot.cs");
            var _lines = _changelog.Split(new string[3] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            string[] digits = System.Text.RegularExpressions.Regex.Split(_lines[0], @"\D+");
            string _buildVersion = "";
            foreach (string value in digits)
            {
                int number;
                if (int.TryParse(value, out number))
                {
                    _buildVersion += value.ToString() + ".";
                }
            }

            _buildVersion = _buildVersion.Remove(_buildVersion.Length - 1);
            var _version = new System.Version(_buildVersion);



            return _version;
        }


		public static void UpdateNamespaces(DataLibrary container, out List<DatabrainHelpers.SortedTypes> _sortedTypes)
		{

			if (container.existingNamespaces == null)
			{
                container.existingNamespaces = new List<DataLibrary.ExistingNamespace>();
            }


            // First get all existing data object types
            var _types = TypeCache.GetTypesDerivedFrom<DataObject>().ToList();

			_sortedTypes = new List<DatabrainHelpers.SortedTypes>();
			var sortedTypes = _sortedTypes;

            // Add types to the sortedlist. This list will be used to sort types based on the order attribute
            for (int i = 0; i < _types.Count; i++)
            {
                sortedTypes.Add(new DatabrainHelpers.SortedTypes(i, _types[i]));
            }


            // Get order attribute
            for (int t = 0; t < sortedTypes.Count; t++)
            {
                var _orderAttribute = _types[t].GetCustomAttribute(typeof(DataObjectOrderAttribute)) as DataObjectOrderAttribute;


                if (_orderAttribute != null)
                {
                    // Get order number
                    var _order = (_orderAttribute as DataObjectOrderAttribute).order;
                    sortedTypes[t].index = _order;
                }
                else
                {
                    sortedTypes[t].index = -1;
                }
            }


            // Sort by order attribute
            sortedTypes = sortedTypes.OrderBy(x => x.index).ToList();



            // NAMESPACE LIST BASED ON HIERARCHY TEMPLATE
            if (container.hierarchyTemplate != null)
			{
				// Groups / Namespace
				for (int i = 0; i < container.hierarchyTemplate.rootDatabrainTypes.subTypes.Count; i++)
				{
					bool _groupExists = false;
					for (int j = 0; j < container.existingNamespaces.Count; j++)
					{
						if (container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].name == container.existingNamespaces[j].namespaceName)
						{
							_groupExists = true;
						}
					}

					if (!_groupExists)
					{
						container.existingNamespaces.Add(new DataLibrary.ExistingNamespace(container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].name));
					}
				}

				// Types
				for (int i = 0; i < container.hierarchyTemplate.rootDatabrainTypes.subTypes.Count; i++)
				{
					for (int j = 0; j < container.existingNamespaces.Count; j++)
					{
						if (container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].name == container.existingNamespaces[j].namespaceName)
						{
							var groupTypes = container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].CollectTypes();
							for (int s = 0; s < groupTypes.Count; s++)
							{
								bool _typeExists = false;
								for (int t = 0; t < container.existingNamespaces[j].existingTypes.Count; t++)
								{
									if (container.existingNamespaces[j].existingTypes[t].typeName == groupTypes[s].type)
									{
										_typeExists = true;
									}
								}

								if (!_typeExists)
								{
                                    container.existingNamespaces[j].existingTypes.Add(new DataLibrary.ExistingNamespace.ExistingTypes(groupTypes[s].type, groupTypes[s].assemblyQualifiedTypeName));
								}
							}
						}
					}
				}

				// Cleanup
				for (int j = container.existingNamespaces.Count - 1; j >= 0; j--)
				{

					bool _namespaceExists = false;
					for (int i = 0; i < container.hierarchyTemplate.rootDatabrainTypes.subTypes.Count; i++)
					{
						if (container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].name == container.existingNamespaces[j].namespaceName)
						{
							_namespaceExists = true;


							for (int s = container.existingNamespaces[j].existingTypes.Count - 1; s >= 0; s--)
							{
								var groupTypes = container.hierarchyTemplate.rootDatabrainTypes.subTypes[i].CollectTypes();
								bool _typeExists = false;
								for (int t = 0; t < groupTypes.Count; t++)
								{
									if (groupTypes[t].type == container.existingNamespaces[j].existingTypes[s].typeName)
									{
										_typeExists = true;
									}
								}

								if (!_typeExists)
								{
									container.existingNamespaces[j].existingTypes.RemoveAt(s);
								}
							}
						}
					}

					if (!_namespaceExists)
					{
						container.existingNamespaces.RemoveAt(j);
					}



				}


			}
			// DEFAULT NAMESPACE LIST
			else
			{

				// Build namespace list
				var _namespaceList = new List<DataLibrary.ExistingNamespace>();

				for (int t = 0; t < sortedTypes.Count; t++)
				{
					var _namespace = "Global";
					if (sortedTypes[t].type.Namespace != null)
					{
						_namespace = sortedTypes[t].type.Namespace;
					}


					var _extN = _namespaceList.Where(x => x.namespaceName == _namespace).FirstOrDefault();

					if (_extN != null)
					{
						var _extT = _extN.existingTypes.Where(x => x.typeName == sortedTypes[t].type.Name).FirstOrDefault();

						if (_extT == null)
						{
							// Add type
							_extN.existingTypes.Add(new DataLibrary.ExistingNamespace.ExistingTypes(sortedTypes[t].type.Name, sortedTypes[t].type.AssemblyQualifiedName));
						}
					}
					else
					{
						// Add namespace and type
						_namespaceList.Add(new DataLibrary.ExistingNamespace(_namespace));
						// Add type
						_namespaceList[_namespaceList.Count-1].existingTypes.Add(new DataLibrary.ExistingNamespace.ExistingTypes(sortedTypes[t].type.Name, sortedTypes[t].type.AssemblyQualifiedName));
                    }


				}

				
                // Compare existing namespace list from data libray with newly build one
                // Add new Namespaces
                for (int i = 0; i < _namespaceList.Count; i++)
				{
					
					bool _namespaceExists = false;
					for (int j = 0; j < container.existingNamespaces.Count; j++)
					{
						// check if namespace exists in old list
						if (container.existingNamespaces[j].namespaceName == _namespaceList[i].namespaceName)
						{
							_namespaceExists = true;
						}
					}

					if (!_namespaceExists)
					{
						container.existingNamespaces.Add(new DataLibrary.ExistingNamespace(_namespaceList[i].namespaceName));
					}
				}

				// Add new Types
				for (int i = 0; i < _namespaceList.Count; i++)
				{
					for (int j = 0; j < container.existingNamespaces.Count; j++)
					{
						if (_namespaceList[i].namespaceName == container.existingNamespaces[j].namespaceName)
						{
							for (int s = 0; s < _namespaceList[i].existingTypes.Count; s++)
							{
								bool _typeExists = false;
								for (int t = 0; t < container.existingNamespaces[j].existingTypes.Count; t++)
								{
									if (container.existingNamespaces[j].existingTypes[t].typeName == _namespaceList[i].existingTypes[s].typeName)
									{
										_typeExists = true;
									}
								}

								if (!_typeExists)
								{
									container.existingNamespaces[j].existingTypes.Add(new DataLibrary.ExistingNamespace.ExistingTypes(_namespaceList[i].existingTypes[s].typeName, _namespaceList[i].existingTypes[s].typeAssemblyQualifiedName));
								}
							}
						}
					}
				}

				// Cleanup namespaces and types which don't exist anymore

				for (int j = container.existingNamespaces.Count - 1; j >= 0; j--)
				{
					bool _namespaceExists = false;
					for (int i = 0; i < _namespaceList.Count; i++)
					{
						if (_namespaceList[i].namespaceName == container.existingNamespaces[j].namespaceName)
						{
							_namespaceExists = true;


							for (int s = container.existingNamespaces[j].existingTypes.Count - 1; s >= 0; s--)
							{
								bool _typeExists = false;
								for (int t = 0; t < _namespaceList[i].existingTypes.Count; t++)
								{
									if (_namespaceList[i].existingTypes[t].typeName == container.existingNamespaces[j].existingTypes[s].typeName)
									{
										_typeExists = true;
									}
								}

								if (!_typeExists)
								{
									container.existingNamespaces[j].existingTypes.RemoveAt(s);
								}
							}
						}
					}

					if (!_namespaceExists)
					{
						container.existingNamespaces.RemoveAt(j);
					}
				}


			}

			_sortedTypes = sortedTypes;


        }

#endif
    }
}
