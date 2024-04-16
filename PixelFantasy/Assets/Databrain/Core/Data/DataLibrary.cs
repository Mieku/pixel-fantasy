/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;

using Databrain.Attributes;
using Databrain.Data;
using Databrain.Modules;
using Databrain.Helpers;

using Newtonsoft.Json;

namespace Databrain
{

#pragma warning disable 0618
	[CreateAssetMenu(menuName= "Databrain / Data Library")]
	public class DataLibrary : ScriptableObject
	{

		public DatabrainThemeTemplate themeTemplate;

		[SerializeField]
		private bool isInitialized;
		public bool IsInitialized
		{
			get
			{
				return isInitialized;
			}
			set
			{
				isInitialized = value;
			}
		}
		public DataObjectList data = new DataObjectList();
        public DataObjectList copyData = new DataObjectList();
#pragma warning disable 0067
		public static event UnityAction ResetEvent;
#pragma warning restore 0067
        public Action OnSaved;
		public Action OnBeforeLoading;
		public Action OnLoaded;

		[Obsolete("Please use RegisterInitializationCallback to make sure event is called even after initialization.")]
		public Action OnDataInitialized;

		public List<Action> initializationCallbacks = new List<Action>();

		public float firstColumnWidth;
		public float secondColumnWidth;

        /// <summary>
        /// The runtime library where all runtime objects are being instantiated to.
        /// All objects in a runtime library are being serialized when calling the save method.
        /// </summary>
        public DataLibrary runtimeLibrary;
		public string runtimeLibraryGuid;
		public string runtimeLibraryFolderPath;
		public DataLibrary initialLibrary;

        public DataObject selectedDataObject;

#if UNITY_EDITOR
        /// <summary>
        /// All available modules
        /// </summary>
        public List<DatabrainModuleBase> modules = new List<DatabrainModuleBase>();
#endif

		/// <summary>
		/// Store all existing types in the according namespaces
		/// </summary>
		[System.Serializable]
		public class ExistingNamespace
		{
			
			public string namespaceName;
			public bool foldout;
			public bool hidden;
			public string icon;
			
			public ExistingNamespace(string _namespaceName)
			{
				namespaceName = _namespaceName;
			}
			
			[System.Serializable]
			public class ExistingTypes
			{
				public string typeName;
				public string typeAssemblyQualifiedName;
				public bool runtimeSerialization;
				public List<string> tagsFilter;
				public List<string> titlesFilter;
				public List<string> valuesFilter;

				public ExistingTypes (string _typeName, string _typeAssemblyQualifiedName)
				{
					typeName = _typeName;
					typeAssemblyQualifiedName = _typeAssemblyQualifiedName;
                }
			}
			
			public List<ExistingTypes> existingTypes = new List<ExistingTypes>();
		}
		
		public List<ExistingNamespace> existingNamespaces = new List<ExistingNamespace>();


		public List<string> GetAssignedTagsFromType(Type _type)
		{
			if (_type == null)
				return null;

			for (int n = 0; n < existingNamespaces.Count; n++)
			{
				//if (existingNamespaces[n].namespaceName == _type.Namespace)
				//{
					for (int t = 0; t < existingNamespaces[n].existingTypes.Count; t++)
					{
						if (existingNamespaces[n].existingTypes[t] == null)
							continue;

						if (existingNamespaces[n].existingTypes[t].typeName == _type.Name)
						{
							if (existingNamespaces[n].existingTypes[t].tagsFilter == null)
							{
								existingNamespaces[n].existingTypes[t].tagsFilter = new List<string>();
							}

							return existingNamespaces[n].existingTypes[t].tagsFilter;
						}
					}
				//}
			}

			return null;
		}


        public List<string> GetAssignedTitlesFiltersFromType(Type _type)
        {
			if (_type == null)
				return null;
				
            for (int n = 0; n < existingNamespaces.Count; n++)
            {
                //if (existingNamespaces[n].namespaceName == _type.Namespace)
                //{
                    for (int t = 0; t < existingNamespaces[n].existingTypes.Count; t++)
                    {
                        if (existingNamespaces[n].existingTypes[t].typeName == _type.Name)
                        {
                            if (existingNamespaces[n].existingTypes[t].titlesFilter == null)
                            {
                                existingNamespaces[n].existingTypes[t].titlesFilter = new List<string>();
                            }

                            return existingNamespaces[n].existingTypes[t].titlesFilter;
                        }
                    }
                //}
            }

            return null;
        }


        /// <summary>
        /// Runtime serializeable class
        /// </summary>
        public class JsonSerializeableClass
		{
			[System.Serializable]
			public class SaveableData
			{
				public string type;
				public string guid;
				public string initialGuid;
				public string runtimeIndexID;
				public string title;
				public string description;
				public string name;
				public SerializableDataObject data;
				
				public class SerializedFields
				{
					public string fieldName;
					public object data;

					public SerializedFields(string _fieldName, object _data)
					{
						fieldName = _fieldName;
						data = _data;
					}
				}

				public List<SerializedFields> serializedFields = new List<SerializedFields>();


				public SaveableData(string _type, string _guid, string _initialGuid, string _runtimeIndexID, string _title, string _description, string _name, SerializableDataObject _runtimeData, List<SerializedFields>  _serializedFields = null)
				{
					type = _type;
					guid = _guid;
					initialGuid = _initialGuid;
					runtimeIndexID = _runtimeIndexID;
					title = _title;
					description = _description; 
					name = _name;
					data = _runtimeData;
					serializedFields = _serializedFields;
				}
			}

            public List<SaveableData> serializeableList = new List<SaveableData>();

			public JsonSerializeableClass()
			{
				serializeableList = new List<SaveableData>();
			}
		}

		public JsonSerializeableClass jsonClass;
		
		// Save Settings
		public bool isRuntimeContainer;
		public bool useXOREncryption;
		public int encryptionKey;
		public Formatting jsonFormatting;

		// Search data object tags
		public List<string> tags = new List<string>();
		public DatabrainHierarchyTemplate hierarchyTemplate;

		public void OnEnable()
		{
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayStateChange;
			EditorApplication.playModeStateChanged += OnPlayStateChange;
#else
			OnBegin();
#endif
        }
		
		public void OnDisable()
		{	
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayStateChange;
#else
			OnEnd();
#endif
        }

		// On scene loaded is called after Awake and OnEnable BUT BEFORE Start
		// That's why we wait in the OnBegin method to make sure OnDataInitialized event is also called
		// when user has registered to the event on Start.
		// void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		// {	
		// 	OnBegin();
		// }

        public bool IsRuntimeSerialization(string _typename)
		{
			for (int n = 0; n < existingNamespaces.Count; n ++)
			{
				for (int t = 0; t < existingNamespaces[n].existingTypes.Count; t ++)
				{
					if (existingNamespaces[n].existingTypes[t].typeName == _typename)
					{
						return existingNamespaces[n].existingTypes[t].runtimeSerialization;
					}
				}
			}
		
			return false;
		}

#if UNITY_EDITOR
        void OnPlayStateChange(PlayModeStateChange _state)
		{
			if (_state == PlayModeStateChange.EnteredPlayMode)
			{
				OnBegin();
			}
			else if (_state == PlayModeStateChange.ExitingPlayMode)
			{
				OnEnd();
			}

            if (_state == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {

            }

            if (_state ==  UnityEditor.PlayModeStateChange.ExitingEditMode)
			{

            }

            if (_state is UnityEditor.PlayModeStateChange.ExitingPlayMode)
            { 
				ResetEvent?.Invoke();
			}
        }
#endif

        async void OnBegin()
		{
		

			if (isRuntimeContainer)
				return;

		

			data.PopulateRuntimeDictionary();

			if (runtimeLibrary != null)
			{
				if (!isRuntimeContainer)
				{
					runtimeLibrary.initialLibrary = this;
				}


                runtimeLibrary.data = new DataObjectList();
                runtimeLibrary.isRuntimeContainer = true;

				for (var i = 0; i < data.ObjectList.Count; i++)
				{
					for (int j = 0; j < data.ObjectList[i].dataObjects.Count; j++)
					{
						if (data.ObjectList[i].dataObjects[j] != null)
						{
							data.ObjectList[i].dataObjects[j].CloneToRuntimeLibrary();
						}
					}
				}

				for (var i = 0; i < data.ObjectList.Count; i++)
				{
					for (int j = 0; j < data.ObjectList[i].dataObjects.Count; j++)
					{
						if (data.ObjectList[i].dataObjects[j] != null)
						{
							data.ObjectList[i].dataObjects[j].Initialize();
						}
					}
				}

                int currentFrame = Time.frameCount;
				while (currentFrame + 5 >= Time.frameCount)
					await Task.Yield();
			
				if (OnDataInitialized != null)
				{
                    OnDataInitialized.Invoke();
				}

				for (int i = 0; i < initializationCallbacks.Count; i ++)
				{
					initializationCallbacks[i]?.Invoke();
				}
				

                IsInitialized = true;
            }
			else
			{
				// Debug.LogError("Warning no runtime DataLibrary object. Please create one in the save&load module");		
			}


			


            for (var i = 0; i < data.ObjectList.Count; i++)
            {
				for (int j = 0; j < data.ObjectList[i].dataObjects.Count; j++)
				{
					if (data.ObjectList[i].dataObjects[j] != null)
					{
						data.ObjectList[i].dataObjects[j].OnStart();
					}
				}
            }
        }

		
		void OnEnd()
		{
			DataObjectList.isRuntime = false;

            // Clear action subscriptions
            OnDataInitialized = null;
            OnSaved = null;
            OnLoaded = null;
			OnBeforeLoading = null;
			IsInitialized = false;
			initializationCallbacks = new List<Action>();

            for (var i = 0; i < data.ObjectList.Count; i ++)
			{
				for (int j = 0; j < data.ObjectList[i].dataObjects.Count; j++)
				{
					if (data.ObjectList[i].dataObjects[j] != null)
					{
						data.ObjectList[i].dataObjects[j].OnEnd();
					}
				}
			}

#if UNITY_EDITOR
            if (isRuntimeContainer)
			{
				CleanUp();
			}
#endif
        }


		/// <summary>
		/// Register to OnDataInitialization callback. If your script loads after the data has been initialized,
		/// make sure to register to the initialization like this to make sure the event gets called. 
		/// </summary>
		/// <param name="_callback"></param>
		public void RegisterInitializationCallback(Action _callback)
		{
			if (IsInitialized)
			{
				// Data is already initialized, call event directly
				_callback?.Invoke();
			}
			else
			{
				// otherwise, add callback to the list which gets called later.
				initializationCallbacks.Add(_callback);
			}
		}



#if UNITY_EDITOR
        async void CleanUp()
		{
            await Task.Delay(2000);
            var _assetPath = AssetDatabase.GetAssetPath(this);
			var _assets = AssetDatabase.LoadAllAssetsAtPath(_assetPath);
			for (int i = 0; i < _assets.Length; i++)
			{
				if (_assets[i] != null)
				{
					if (typeof(DataObject).IsAssignableFrom(_assets[i].GetType()))
					{
						if ((_assets[i] as DataObject).isRuntimeInstance)
						{
							DestroyImmediate(_assets[i], true);
						}
					}
				}
			}


			await Task.Delay(500);
            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(this));
            EditorUtility.SetDirty(assetImporter);
            //assetImporter.SaveAndReimport();
        }
#endif


		/// <summary>
		/// Cleans up all data objects in the runtime library and resets it back to the initial state
		/// </summary>
		public void ResetRuntimeDataLibrary()
		{
			if (runtimeLibrary != null)
			{
				for (int i = 0; i < runtimeLibrary.data.ObjectList.Count; i ++)
				{
					for (int j = 0; j < runtimeLibrary.data.ObjectList[i].dataObjects.Count; j ++)
					{
						DestroyImmediate(runtimeLibrary.data.ObjectList[i].dataObjects[j], true);
					}
				}
			

				runtimeLibrary.data = new DataObjectList();
				runtimeLibrary.isRuntimeContainer = true;

				for (var i = 0; i < data.ObjectList.Count; i++)
				{
					for (int j = 0; j < data.ObjectList[i].dataObjects.Count; j++)
					{
						if (data.ObjectList[i].dataObjects[j] != null)
						{
							data.ObjectList[i].dataObjects[j].CloneToRuntimeLibrary();
						}
					}
				}

			}
		}

#if UNITY_EDITOR
        public DatabrainModuleBase GetModule(string _moduleName)
		{
			for ( int i = 0; i < modules.Count; i ++)
			{
				if (modules[i] == null)
					continue;
					
				if (modules[i].name == _moduleName)
				{
					return modules[i];
				}
			}
			
			return null;
		}
#endif

		/// <summary>
		/// Save to json file
		/// </summary>
		/// <param name="_path"></param>
		public async void Save(string _path)
		{	
			if (runtimeLibrary == null)
			{
				Debug.LogError("Could not perform save, please make sure to assign a Runtime Container in the configuration");
				return;
			}


			jsonClass = GetSerializableData();

            var converters = new List<JsonConverter>();
			converters.Add(new DatabrainJsonConverter());

            string _jsonString = JsonConvert.SerializeObject(jsonClass, jsonFormatting,
			new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Auto,
				Converters = converters
			});


			if (useXOREncryption)
			{
				_jsonString = XOREncryptDecrypt(_jsonString, encryptionKey);
            }


			await File.WriteAllTextAsync(_path, _jsonString);
			
			if (OnSaved != null)
			{
				OnSaved.Invoke();
			}
		}


        /// <summary>
        /// Returns a JsonSerializeableClass class which can be serialized by a custom serializer.
        /// </summary>
        /// <returns></returns>
        public JsonSerializeableClass GetSerializableData()
		{
            var _jsonClass = new JsonSerializeableClass();

            for (var i = 0; i < runtimeLibrary.data.ObjectList.Count; i++)
            {
                for (int j = 0; j < runtimeLibrary.data.ObjectList[i].dataObjects.Count; j++)
                {

                    // First get serialized data class
                    var _serializedData = runtimeLibrary.data.ObjectList[i].dataObjects[j].GetSerializedData();


                    // Second get all fields with the DatabrainSerialize attribute
                    var _allFields = runtimeLibrary.data.ObjectList[i].dataObjects[j].GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    List<JsonSerializeableClass.SaveableData.SerializedFields> _serializedFields = new List<JsonSerializeableClass.SaveableData.SerializedFields>();
                    foreach (var _field in _allFields)
                    {
                        var _attributes = _field.GetCustomAttributes(true);
                        for (int a = 0; a < _attributes.Length; a++)
                        {
                            if (_attributes[a].GetType() == typeof(DatabrainSerializeAttribute))
                            {
                                var _obj = _field.GetValue(runtimeLibrary.data.ObjectList[i].dataObjects[j]);
                                var _serializedField = new JsonSerializeableClass.SaveableData.SerializedFields(_field.Name, _obj);
                                _serializedFields.Add(_serializedField);

                            }
                        }

                    }

				
                    // If both serialized datas are null do not create a save entry
                    if (_serializedData == null && _serializedFields.Count == 0)
                    {

                    }
                    else
                    {
                        var _s = new JsonSerializeableClass.SaveableData(
                            runtimeLibrary.data.ObjectList[i].type,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].guid,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].initialGuid,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].runtimeIndexID,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].title,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].description,
                            runtimeLibrary.data.ObjectList[i].dataObjects[j].name,
                            _serializedData,
                            _serializedFields);


                        _jsonClass.serializeableList.Add(_s);
                    }
                }
            }

            return _jsonClass;
		}

		
		/// <summary>
		/// Set serialized data back to the data library. Used in combination with the GetSerializedData method.
		/// </summary>
		/// <param name="_data"></param>
		public void SetSerializedData(JsonSerializeableClass _data)
		{
            for (int i = 0; i < _data.serializeableList.Count; i++)
            {
                for (int t = 0; t < runtimeLibrary.data.ObjectList.Count; t++)
                {
                    for (int j = runtimeLibrary.data.ObjectList[t].dataObjects.Count - 1; j >= 0; j--)
                    //for (int j = 0; j < runtimeLibrary.data.ObjectList[t].dataObjects.Count; j++)
                    {
                        if (runtimeLibrary.data.ObjectList[t].dataObjects[j].guid == _data.serializeableList[i].guid)
                        {
#if DATABRAIN_DEBUG
							Debug.Log("remove object " + runtimeLibrary.data.ObjectList[t].dataObjects[j].title);
#endif
                            DestroyImmediate(runtimeLibrary.data.ObjectList[t].dataObjects[j], true);
                            runtimeLibrary.data.ObjectList[t].dataObjects.RemoveAt(j);
                        }
                        else
                        {
                            for (int k = 0; k < runtimeLibrary.data.ObjectList[t].dataObjects.Count; k++)
                            {
                                if (runtimeLibrary.data.ObjectList[t].dataObjects[k].runtimeIndexID == _data.serializeableList[i].runtimeIndexID)
                                {
#if DATABRAIN_DEBUG
									Debug.Log("remove object because of same runtime index id " + runtimeLibrary.data.ObjectList[t].dataObjects[j].title);
#endif
                                    DestroyImmediate(runtimeLibrary.data.ObjectList[t].dataObjects[j], true);
                                    runtimeLibrary.data.ObjectList[t].dataObjects.RemoveAt(j);
                                }
                            }
                        }
                    }
                }
            }


            for (int i = 0; i < _data.serializeableList.Count; i++)
            {

#if DATABRAIN_DEBUG
				Debug.Log("create instance of type: " + _data.serializeableList[i].type);
#endif
                var _dbData = CreateInstance(System.Type.GetType(_data.serializeableList[i].type));

                (_dbData as DataObject).Reset();
                (_dbData as DataObject).guid = _data.serializeableList[i].guid;
                (_dbData as DataObject).initialGuid = _data.serializeableList[i].initialGuid;
                (_dbData as DataObject).runtimeIndexID = _data.serializeableList[i].runtimeIndexID;
                (_dbData as DataObject).name = _data.serializeableList[i].name;
                (_dbData as DataObject).title = _data.serializeableList[i].title;
                (_dbData as DataObject).description = _data.serializeableList[i].description;
                (_dbData as DataObject).isRuntimeInstance = true;
                (_dbData as DataObject).relatedLibraryObject = this;


                (_dbData as DataObject).SetSerializedData(_data.serializeableList[i].data);


                for (int s = 0; s < _data.serializeableList[i].serializedFields.Count; s++)
                {
                    var _allFields = _dbData.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    foreach (var _field in _allFields)
                    {
                        var _attributes = _field.GetCustomAttributes(true);
                        for (int a = 0; a < _attributes.Length; a++)
                        {
                            if (_attributes[a].GetType() == typeof(DatabrainSerializeAttribute) && _field.Name == _data.serializeableList[i].serializedFields[s].fieldName)
                            {

                                object value = _data.serializeableList[i].serializedFields[s].data;

                                if (value.GetType() == typeof(System.Int64))
                                {

                                    var temp = Convert.ToInt64(_data.serializeableList[i].serializedFields[s].data);
                                    if (temp <= Byte.MaxValue && temp >= Byte.MinValue)
                                        value = Convert.ToByte(_data.serializeableList[i].serializedFields[s].data);
                                    else if (temp >= Int16.MinValue && temp <= Int16.MaxValue)
                                        value = Convert.ToInt16(_data.serializeableList[i].serializedFields[s].data);
                                    else if (temp >= Int32.MinValue && temp <= Int32.MaxValue)
                                        value = Convert.ToInt32(_data.serializeableList[i].serializedFields[s].data);
                                    else
                                        value = temp;
                                }

                                if (value.GetType() == typeof(System.Double))
                                {
                                    value = Convert.ToSingle(_data.serializeableList[i].serializedFields[s].data);
                                }

                                _field.SetValue(_dbData, value);
                            }
                        }
                    }
                }

                runtimeLibrary.data.AddDataObject(System.Type.GetType(_data.serializeableList[i].type), (_dbData as DataObject));


#if UNITY_EDITOR
                AssetDatabase.AddObjectToAsset(_dbData, this);
#endif
			}


			// Populate the runtime dictionary.
			runtimeLibrary.data.PopulateRuntimeDictionary();
		

            if (OnLoaded != null)
            {
                OnLoaded.Invoke();
            }


#if UNITY_EDITOR
            // Refresh Databrain editor
            var _editorWindow = DatabrainHelpers.GetOpenEditor(runtimeLibrary.GetInstanceID());
            if (_editorWindow != null)
            {
                DatabrainHelpers.OpenEditor(runtimeLibrary.GetInstanceID(), false);
            }
#endif
        }


        /// <summary>
        /// Load json file
        /// </summary>
        /// <param name="_path"></param>
        public async void Load(string _path)
		{
			
			if (runtimeLibrary == null)
			{
				Debug.LogError("Could not perform load, please make sure to assign a Runtime Container in the configuration");
				return;
			}

			OnBeforeLoading?.Invoke();

			jsonClass = new JsonSerializeableClass();

			var _jsonString = await File.ReadAllTextAsync(_path);

            if (useXOREncryption)
            {
                _jsonString = XOREncryptDecrypt(_jsonString, encryptionKey);
            }

            jsonClass = JsonConvert.DeserializeObject<JsonSerializeableClass>(_jsonString,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            });

            SetSerializedData(jsonClass);
        }


        string XOREncryptDecrypt(string _plainText, int _encryptionKey)
        {
            System.Text.StringBuilder szInputStringBuild = new System.Text.StringBuilder(_plainText);
            System.Text.StringBuilder szOutStringBuild = new System.Text.StringBuilder(_plainText.Length);
            char textCh;
            for (int iCount = 0; iCount < _plainText.Length; iCount++)
            {
                textCh = szInputStringBuild[iCount];
                textCh = (char)(textCh ^ _encryptionKey);
                szOutStringBuild.Append(textCh);
            }


            return szOutStringBuild.ToString();
        }

		/// <summary>
		/// Returns a singleton data object. Tries to get the runtime data object first.
		/// If there isn't any, it will return the initial data object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetSingleton<T>(bool _forceReturnInitial = false) where T : DataObject
		{
			if (_forceReturnInitial)
			{
				return data.GetFirstDataObjectByType<T>();
			}

			if (runtimeLibrary == null)
			{
				Debug.LogError("No runtime Databrain object. Please create one in the save&load module");
				return null;
			}
			if (runtimeLibrary != null)
			{
				return runtimeLibrary.data.GetFirstDataObjectByType<T>();
			}
			
			return data.GetFirstDataObjectByType<T>();
		}

        /// <summary>
        /// Get a runtime-DataObject by guid. Please use generic version for better performance.
        /// </summary>
        /// <param name="_guid"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public DataObject GetRuntimeDataObjectByGuid(string _guid, Type _type = null)
		{
			if (runtimeLibrary == null)
			{
				Debug.LogError("No runtime Databrain object. Please create one in the save&load module");
				return null;
			}

			return runtimeLibrary.data.GetDataObjectByGuid(_guid, _type);
		}

		/// <summary>
		/// Get a runtime-DataObject by guid. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_guid"></param>
		/// <returns></returns>
		public T GetRuntimeDataObjectByGuid<T>(string _guid) where T : DataObject
		{
			if (runtimeLibrary == null)
			{
				Debug.LogError("No runtime Databrain object. Please create one in the save&load module");
				return null;
			}

			return runtimeLibrary.data.GetDataObjectByGuid<T>(_guid);
		}
		
		/// <summary>
		/// Get initial-DataObject by Guid. Please use generic version for better performance.
		/// </summary>
		/// <param name="_guid"></param>
		/// <param name="_type"></param>
		/// <returns></returns>
		public DataObject GetInitialDataObjectByGuid(string _guid, Type _type = null)
		{
			return data.GetDataObjectByGuid(_guid, _type);
		}

		/// <summary>
		/// Get initial-DataObject by Guid. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_guid"></param>
		/// <returns></returns>
		public T GetInitialDataObjectByGuid<T>(string _guid) where T : DataObject
		{
			return data.GetDataObjectByGuid<T>(_guid);
		}

		/// <summary>
		/// Get runtime-DataObject by title. Please use generic version for better performance.
		/// </summary>
		/// <param name="_title"></param>
		/// <param name="_type"></param>
		/// <returns></returns>
		public DataObject GetRuntimeDataObjectByTitle(string _title, Type _type = null)
		{
            if (runtimeLibrary == null)
            {
                Debug.LogError("No runtime Databrain object. Please create one in the save&load module");
                return null;
            }

            return runtimeLibrary.data.GetDataObjectByTitle(_title, _type);
		}

		/// <summary>
		/// Get runtime-DataObject by title. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_title"></param>
		/// <returns></returns>
		public T GetRuntimeDataObjectByTitle<T>(string _title) where T : DataObject
		{
			if (runtimeLibrary == null)
            {
                Debug.LogError("No runtime Databrain object. Please create one in the save&load module");
                return null;
            }

            return runtimeLibrary.data.GetDataObjectByTitle<T>(_title);
		}

		/// <summary>
		/// Get initial-DataObject by title. Please use generic version for better performance.
		/// </summary>
		/// <param name="_title"></param>
		/// <param name="_type"></param>
		/// <returns></returns>
		public DataObject GetInitialDataObjectByTitle( string _title, Type _type = null)
		{
			return data.GetDataObjectByTitle(_title, _type);
		}
		
		/// <summary>
		/// Get initial-DataObject by title. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_title"></param>
		/// <returns></returns>
		public T GetInitialDataObjectByTitle<T>( string _title) where T : DataObject
		{
			return data.GetDataObjectByTitle<T>(_title);
		}

		/// <summary>
		/// Get the first dataobject in the list by type. This is useful for manager types where we only have one object
        /// Please use generic version for better performance.
		/// </summary>
		/// <param name="_type"></param>
		/// <returns></returns>
		public DataObject GetInitialFirstDataObjectByType(Type _type)
		{
			return data.GetFirstDataObjectByType(_type);
		}

		/// <summary>
		/// Get the first dataobject in the list by type. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetInitialFirstDataObjectByType<T>() where T : DataObject
		{
			return data.GetFirstDataObjectByType<T>();
		}

		/// <summary>
		/// Returns a list of all runtime data objects of type. Please use generic version for better performance.
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_includeSubtypes"></param>
		/// <returns></returns>
		public List<DataObject> GetAllRuntimeDataObjectsByType(Type _type, bool _includeSubtypes = false)
		{
			if (runtimeLibrary == null)
			{
				return null;
			}
			else
			{
				return runtimeLibrary.data.GetAllDataObjectsByType(_type, _includeSubtypes);
			}
		}

		/// <summary>
		/// Returns a list of all runtime data objects of type. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_includeSubTypes"></param>
		/// <returns></returns>
		public List<T> GetAllRuntimeDataObjectsByType<T>(bool _includeSubTypes = false) where T : DataObject
		{
			if (runtimeLibrary == null)
			{
				return null;
			}
			else
			{
				return runtimeLibrary.data.GetAllDataObjectsByType<T>(_includeSubTypes);
			}
		}

		/// <summary>
		/// Returns a list of all initial data objects of type. Please use generic version for better performance.
		/// </summary>
		/// <param name="_type"></param>
		/// <param name="_includeSubtypes"></param>
		/// <returns></returns>
		public List<DataObject> GetAllInitialDataObjectsByType(Type _type, bool _includeSubtypes = false)
		{
			return data.GetAllDataObjectsByType(_type, _includeSubtypes);			
		}


		/// <summary>
		/// Returns a list of all initial data objects of type. Pass in the type to directly cast it.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="_includeSubTypes"></param>
		/// <returns></returns>
		public List<T> GetAllInitialDataObjectsByType<T>(bool _includeSubTypes = false) where T : DataObject
		{
			return data.GetAllDataObjectsByType<T>(_includeSubTypes);
		}

		/// <summary>
		/// Retrieves all initial data objects by given tags.
		/// </summary>
		/// <param name="_tag"></param>
		/// <param name="_tags"></param>
		/// <returns></returns>
		public List<DataObject> GetAllInitialDataObjectsByTags(string _tag, params string[] _tags)
		{
			return data.GetAllDataObjectsByTags(_tag, _tags);
		}

		/// <summary>
		/// Retrieves all runtime data objects by a given tag.
		/// </summary>
		/// <param name="_tag"></param>
		/// <param name="_tags"></param>
		/// <returns></returns>
		public List<DataObject> GetAllRuntimeDataObjectsByTags(string _tag, params string[] _tags)
		{
			if (runtimeLibrary == null)
			{
				return null;
			}
			else
			{
				return runtimeLibrary.data.GetAllDataObjectsByTags(_tag, _tags);
			}
		}

		/// <summary>
		/// Returns true or false wether a DataObject exists or not
		/// </summary>
		/// <param name="_guid"></param>
		/// <param name="_type"></param>
		/// <returns></returns>
		public bool RuntimeDataObjectWithGuidExists(string _guid, Type _type = null)
		{
            var _d = runtimeLibrary.data.GetDataObjectByGuid(_guid, _type);
            if (_d != null)
            {
                return true;
            }

			return false;
		}

		public bool RuntimeDataObjectWithGuidExists<T>(string _guid) where T : DataObject
		{
			var _d = runtimeLibrary.data.GetDataObjectByGuid<T>(_guid);
            if (_d != null)
            {
                return true;
            }

			return false;
		}

		/// <summary>
		/// Returns true or false wether a DataObject exists or not
		/// </summary>
		/// <param name="_guid"></param>
		/// <param name="_type"></param>
		/// <returns></returns>
		public bool InitialDataObjectWithGuidExists(string _guid, Type _type = null)
		{
			var _d = data.GetDataObjectByGuid(_guid, _type);
			if (_d != null)
			{
				return true;
			}
		
			return false;
		}

		public bool InitialDataObjectWithGuidExists<T>(string _guid) where T : DataObject
		{
			var _d = data.GetDataObjectByGuid<T>(_guid);
			if (_d != null)
			{
				return true;
			}
		
			return false;
		}


        /// <summary>
        /// This will make a new instance of the data object and add it to the runtime data library object.
        /// If the data object has a runtime data class it can then be serialized.
		/// When providing an owner game object the runtime clone will use the object instance id instead of a guid.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_ownerGameObject"></param>
		/// <returns></returns>
		public DataObject CloneDataObjectToRuntime(DataObject _data, GameObject _ownerGameObject = null)
		{
			if (isRuntimeContainer)
				return null;

			if (runtimeLibrary == null)
			{
				Debug.LogError("Warning no runtime DataLibrary object. Please create one in the save&load module");
				return null;
			}
			
			var _so = Instantiate(_data);

			// Get runtime index id
			var _allObjectsOfType = runtimeLibrary.data.GetAllDataObjectsByType(_so.GetType(), false);

			if (_ownerGameObject != null)
			{
				_so.guid = _ownerGameObject.GetInstanceID().ToString();
			}
			else
			{
                _so.guid = System.Guid.NewGuid().ToString();
            }
			
			_so.initialGuid = _data.guid;
			_so.runtimeIndexID = _data.guid + "_" + (_allObjectsOfType.Count);
            _so.isRuntimeInstance = true;
			_so.relatedLibraryObject = this;
            _so.runtimeLibraryObject = runtimeLibrary;

#if UNITY_EDITOR
			_so.hideFlags = HideFlags.HideInHierarchy;
#endif

			if (runtimeLibrary.data == null)
                runtimeLibrary.data = new DataObjectList();

            runtimeLibrary.data.AddDataObject(_so.GetType(), _so);

#if UNITY_EDITOR
            if (Application.isPlaying)
			{
				AssetDatabase.AddObjectToAsset(_so, runtimeLibrary);
			}
#endif

			_data.runtimeClone = _so;
			
			return _so;
		}
		
		/// <summary>
		/// Remove a DataObject from the runtime DataLibrary
		/// </summary>
		/// <param name="_data"></param>
		public bool RemoveDataObjectFromRuntime(DataObject _data)
		{

			var _guid = _data.guid;


			for(var i = 0; i < runtimeLibrary.data.ObjectList.Count; i ++)
			{
				for (int j = 0; j < runtimeLibrary.data.ObjectList[i].dataObjects.Count; j++)
				{
					if (runtimeLibrary.data.ObjectList[i].dataObjects[j].guid == _guid)
					{
						DestroyImmediate(runtimeLibrary.data.ObjectList[i].dataObjects[j], true);
						runtimeLibrary.data.ObjectList[i].dataObjects.RemoveAt(j);
						return true;
					}
				}
			}

			return false;
		}
	}
#pragma warning restore 0618
}