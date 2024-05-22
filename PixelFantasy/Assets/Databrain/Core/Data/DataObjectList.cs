/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Databrain.Data
{
	[System.Serializable]
	public class DataObjectList
	{
		public static bool isRuntime = false;

		public DataObjectList() { }
		public DataObjectList(List<DataType> _objects) 
		{
			_objectList = new List<DataType>(_objects);
		}
		
		[System.Serializable]
		public class DataType
        {
			public string type;

            public DataType(Type _type, DataObject _object)
            {
                type = _type.AssemblyQualifiedName;
				dataObjects.Add(_object);
            }

            public List<DataObject> dataObjects	= new List<DataObject>();
		}
		
		[SerializeField]
		private List<DataType> _objectList = new List<DataType>();

		[SerializeField]
		private List<DataType> _favoriteList = new List<DataType>();
		
		public List<DataType> ObjectList
		{
			get 
			{
				return _objectList;
			}
		}

		public List<DataType> FavoriteList
		{
			get
			{
				return _favoriteList;
			}
		}

		// Runtime data dictionary
		public Dictionary <Type, Dictionary<string, DataObject>> runtimeDictionary = new Dictionary<Type, Dictionary<string, DataObject>>();

		/// <summary>
		/// Populates the runtime dictionary which will be used at runtime for faster access times
		/// </summary>
		internal void PopulateRuntimeDictionary()
		{
			isRuntime = true;
			runtimeDictionary = new Dictionary<Type, Dictionary<string, DataObject>>();

			for (int i = 0; i < _objectList.Count; i++)
			{
				Type _type = Type.GetType(_objectList[i].type);
				if (_type != null)
				{
					if (runtimeDictionary.ContainsKey(_type) == false)
					{
						Dictionary<string, DataObject> newDictionary = new Dictionary<string, DataObject>();
						runtimeDictionary.Add(_type, newDictionary);
					}
					
					
					for (int j = 0; j < _objectList[i].dataObjects.Count; j ++)
					{
						if (_objectList[i].dataObjects[j] != null)
						{
							runtimeDictionary[_type].Add(_objectList[i].dataObjects[j].guid, _objectList[i].dataObjects[j]);
						}	
					}				
				}
			}
		}

		internal void ChangeRuntimeGUID(Type _type, string _oldGuid, string _newGuid)
		{
			if (runtimeDictionary.ContainsKey(_type))
			{
				var _dataObject = runtimeDictionary[_type][_oldGuid];
				runtimeDictionary[_type].Remove(_oldGuid);
				runtimeDictionary[_type].Add(_newGuid, _dataObject);
			}
		}


		internal DataObject GetDataObjectByGuid(string _guid, Type _type = null)
		{
			if (isRuntime)
			{
				return GetDataObjectByGuidRuntime(_guid, _type);
			}

			if (_type != null)
			{
				for (int i = 0; i < _objectList.Count; i++)
				{
					if (_objectList[i].type == _type.AssemblyQualifiedName)
					{
						for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
						{
							if (_objectList[i].dataObjects[j].guid == _guid)
							{
								return _objectList[i].dataObjects[j];
							}
						}
					}
				}
            }
			else
			{
				for (int i = 0; i < _objectList.Count; i++)
				{
					for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
					{
						if (_objectList[i].dataObjects[j] == null)
							continue;

						if (_objectList[i].dataObjects[j].guid == _guid)
						{
							return _objectList[i].dataObjects[j];
						}
					}
				}
			}

			return null;
		}

		internal T GetDataObjectByGuid<T>(string _guid) where T : DataObject
		{
			if (isRuntime)
			{
				return (T)GetDataObjectByGuidRuntime<T>(_guid);
			}

			var _an = typeof(T).AssemblyQualifiedName;

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_objectList[i].type == _an)
				{
					for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
					{
						if (_objectList[i].dataObjects[j].guid == _guid)
						{
							return (T)_objectList[i].dataObjects[j];
						}
					}
				}
			} 

			return  null;
		}

		
		
		internal DataObject GetDataObjectByTitle(string _title, Type _type = null)
		{
			if (isRuntime)
			{
				return GetDataObjectByTitleRuntime(_title, _type);
			}


			if (_type != null)
			{
				for (int i = 0; i < _objectList.Count; i++)
				{
					if (_objectList[i].type == _type.AssemblyQualifiedName)
					{
						for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
						{
							if (_objectList[i].dataObjects[j].title == _title)
							{
                                return _objectList[i].dataObjects[j];
							}
						}
					}
				}       
            }
			else
			{
				for (int i = 0; i < _objectList.Count; i++)
				{
					for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
					{
						if (_objectList[i].dataObjects[j].title == _title)
						{
							return _objectList[i].dataObjects[j];
						}
					}
				}
            }
			
			return null;
		}

		internal T GetDataObjectByTitle<T>(string _title) where T : DataObject
		{
			if (isRuntime)
			{
				return GetDataObjectByTitleRuntime<T>(_title);
			}

			var _an = typeof(T).AssemblyQualifiedName;

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_objectList[i].type == _an)
				{
					for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
					{
						if (_objectList[i].dataObjects[j].title == _title)
						{
							return (T)_objectList[i].dataObjects[j];
						}
					}
				}
			}       

			return null;
		}

	
		internal DataObject GetFirstDataObjectByType(Type _type)
		{
			if (isRuntime)
			{
				return GetFirstDataObjectByTypeRuntime(_type);
			}

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_objectList[i].type == _type.AssemblyQualifiedName)
				{
					return _objectList[i].dataObjects.FirstOrDefault();
				}
			}

            return null;
        }

		internal T GetFirstDataObjectByType<T>() where T : DataObject
		{
			if (isRuntime)
			{
				return GetFirstDataObjectByTypeRuntime<T>();
			}

			var _an = typeof(T).AssemblyQualifiedName;

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_objectList[i].type == _an)
				{
					return (T)_objectList[i].dataObjects.FirstOrDefault();
				}
			}

            return null;
        }

		internal List<T> GetAllDataObjectsByType<T>(bool _includeSubTypes) where T : DataObject
		{
			if (isRuntime)
			{
				return GetAllDataObjectsByTypeRuntime<T>(_includeSubTypes);
			}

			List<T> list = new List<T>();
			var _an = typeof(T).AssemblyQualifiedName;

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_includeSubTypes)
				{
					if (Type.GetType(_objectList[i].type) != null)
					{

						var _derived = Type.GetType(_objectList[i].type);
						var _foundType = Type.GetType(_objectList[i].type);
                        if (_derived.BaseType != typeof(DataObject))
						{
							do
							{
							
								_derived = _derived.BaseType;
								if (_derived != null)
								{
									if (_derived.AssemblyQualifiedName == _an)
									{
										_foundType = _derived;
									}
								}
                               
                            } while (_derived.BaseType != typeof(DataObject));

						}
					
						if (_foundType.AssemblyQualifiedName == _an)
                        {
							for (int o = 0; o < _objectList[i].dataObjects.Count; o++)
							{
								if (_objectList[i].dataObjects[o] == null)
								{
									// Cleanup object list
									_objectList[i].dataObjects.RemoveAt(o);
									continue;
								}

								list.Add((T)_objectList[i].dataObjects[o]);
							}
						}
					}
				}

				if (_objectList[i].type == _an)
				{
					for (int o = 0; o < _objectList[i].dataObjects.Count; o++)
					{
						if (_objectList[i].dataObjects[o] == null)
						{
							// Cleanup object list
							_objectList[i].dataObjects.RemoveAt(o);
							continue;
						}

						if (!list.Contains(_objectList[i].dataObjects[o]))
						{
							list.Add((T)_objectList[i].dataObjects[o]);
						}
					}
                }
			}


			return list;
		}
		
		internal List<DataObject> GetAllDataObjectsByType(Type _type, bool _includeSubTypes)
		{
			if (isRuntime)
			{
				return GetAllDataObjectsByTypeRuntime(_type, _includeSubTypes);
			}

			List<DataObject> list = new List<DataObject>();
			
			if (_type == null)
			{
				return null;
			}

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_includeSubTypes)
				{
					if (Type.GetType(_objectList[i].type) != null)
					{

						var _derived = Type.GetType(_objectList[i].type);
						var _foundType = Type.GetType(_objectList[i].type);
                        if (_derived.BaseType != typeof(DataObject))
						{
							do
							{
							
								_derived = _derived.BaseType;
								if (_derived != null)
								{
									if (_derived.AssemblyQualifiedName == _type.AssemblyQualifiedName)
									{
										_foundType = _derived;
									}
								}
                               
                            } while (_derived.BaseType != typeof(DataObject));

						}
					
						if (_foundType.AssemblyQualifiedName == _type.AssemblyQualifiedName)
                        {
							for (int o = 0; o < _objectList[i].dataObjects.Count; o++)
							{
								if (_objectList[i].dataObjects[o] == null)
								{
									// Cleanup object list
									_objectList[i].dataObjects.RemoveAt(o);
									continue;
								}

								list.Add(_objectList[i].dataObjects[o]);
							}
						}
					}
				}

				if (_objectList[i].type == _type.AssemblyQualifiedName)
				{
					for (int o = 0; o < _objectList[i].dataObjects.Count; o++)
					{
						if (_objectList[i].dataObjects[o] == null)
						{
							// Cleanup object list
							_objectList[i].dataObjects.RemoveAt(o);
							continue;
						}

						if (!list.Contains(_objectList[i].dataObjects[o]))
						{
							list.Add(_objectList[i].dataObjects[o]);
						}
					}
                }
			}


			return list;
        }

		internal List<DataObject> GetAllDataObjectsByTags(string _tag, params string[] _tags)
		{
			if (isRuntime)
			{
				return GetAllDataObjectsByTagsRuntime(_tag, _tags);
			}

			List<DataObject> list = new List<DataObject>();
			for (int i = 0; i < _objectList.Count; i++)
			{
				for(var d = 0; d < _objectList[i].dataObjects.Count; d ++)
				{
					if (_objectList[i].dataObjects[d].tags !=null && _objectList[i].dataObjects[d].tags.Count > 0)
					{
						if (_objectList[i].dataObjects[d].tags.Contains(_tag) || _objectList[i].dataObjects[d].tags.Intersect(_tags).Count() > 0)
						{
							list.Add(_objectList[i].dataObjects[d]);
						}
					}
				}
			}

			return list;
		}


        internal void AddDataObject(Type _type, DataObject _dataObject)
		{
			if (isRuntime)
			{
				AddDataObjectRuntime(_type, _dataObject);
			}

			// check if type already exists in the object list
			var _typeExists = -1;
			for (int t = 0; t < _objectList.Count; t++)
			{
				if (_objectList[t].type == _type.AssemblyQualifiedName)
				{
					_typeExists = t;
				}
			}

			if (_typeExists > -1)
			{
				_objectList[_typeExists].dataObjects.Insert(0, _dataObject);
			}
			else
			{
				_objectList.Add(new DataType(_type, _dataObject));
			}
		}
		
		internal void RemoveDataObject(Type _type, DataObject _dataObject)
		{
			if (isRuntime)
			{
				RemoveDataObjectRuntime(_type, _dataObject);
			}

			for (int i = 0; i < _objectList.Count; i ++)
			{
				if (_objectList[i].type == _type.AssemblyQualifiedName)
				{
					for (int j = 0; j < _objectList[i].dataObjects.Count; j++)
					{
						if (_objectList[i].dataObjects[j].guid == _dataObject.guid)
						{
							_objectList[i].dataObjects.RemoveAt(j);
						}
					}
				}
			}
		}


		internal void SetFavorite(DataObject _dataObject)
		{
			_favoriteList.Add(new DataType(_dataObject.GetType(), _dataObject));
		}

		internal void RemoveFromFavorite(DataObject _dataObject)
		{
			for (int f = 0; f < _favoriteList.Count; f++)
			{
				if (_favoriteList[f].dataObjects[0] != null)
				{
					if (_favoriteList[f].dataObjects[0].guid == _dataObject.guid)
					{
						_favoriteList.RemoveAt(f);
					}
				}
			}
		}

		internal bool IsFavorite(DataObject _dataObject)
		{
			for (int f = 0; f < _favoriteList.Count; f++)
			{
				if (_favoriteList[f].dataObjects[0] != null)
				{
					if (_favoriteList[f].dataObjects[0].guid == _dataObject.guid)
					{
						return true;
					}
				}
			}

			return false;
		}


#region RUNIMTE_DATA_ACCESS

		// Runtime data access uses the runtime dictionary for faster data access.
		// measured 10x - 100x faster and nearly zero garbage collection.

		private List<DataObject> GetAllDataObjectsByTagsRuntime(string _tag, params string[] _tags)
		{
			List<DataObject> list = new List<DataObject>();
			foreach(var _key in runtimeDictionary.Keys)
			{
				foreach(var _data in runtimeDictionary[_key].Keys)
				{
					if (runtimeDictionary[_key][_data].tags.Contains(_tag) || runtimeDictionary[_key][_data].tags.Intersect(_tags).Count() > 0)
					{
						list.Add(runtimeDictionary[_key][_data]);
					}
				}
			}

			return list;
		}


		private DataObject GetDataObjectByGuidRuntime(string _guid, Type _type = null)
		{
			if (string.IsNullOrEmpty(_guid))
				return null;

			if (_type != null)
			{
				if (runtimeDictionary.ContainsKey(_type))
				{
					if (runtimeDictionary[_type].ContainsKey(_guid))
					{
						return runtimeDictionary[_type][_guid];
					}
				}
			}
			else
			{
				foreach(var _key in runtimeDictionary.Keys)
				{
					if (runtimeDictionary[_key].ContainsKey(_guid))
					{
						return runtimeDictionary[_key][_guid];
					}
				}
			}

			return null;
		}



		private T GetDataObjectByGuidRuntime<T>(string _guid) where T : DataObject
		{
			if (string.IsNullOrEmpty(_guid))
				return null;
				
			// var _an = typeof(T).AssemblyQualifiedName;
			if (runtimeDictionary.ContainsKey(typeof(T)))
			{
				if (runtimeDictionary[typeof(T)].ContainsKey(_guid))
				{
					return (T)runtimeDictionary[typeof(T)][_guid];
				}
			}

			return null;
		}

		private DataObject GetDataObjectByTitleRuntime(string _title, Type _type = null)
		{
			if (_type != null)
			{
				if (runtimeDictionary.ContainsKey(_type))
				{
					foreach(var _key in runtimeDictionary[_type].Keys)
					{
						if (runtimeDictionary[_type][_key].title == _title)
						{
							return runtimeDictionary[_type][_key];
						}
					}
				}
			}
			else
			{
				foreach(var _typeKey in runtimeDictionary.Keys)
				{
					foreach(var _key in runtimeDictionary[_typeKey].Keys)
					{
						if (runtimeDictionary[_typeKey][_key].title == _title)
						{
							return runtimeDictionary[_typeKey][_key];
						}
					}
				}
			}

			return null;
		}


		private T GetDataObjectByTitleRuntime<T>(string _title) where T : DataObject
		{
			// var _an = typeof(T).AssemblyQualifiedName;
			if (runtimeDictionary.ContainsKey(typeof(T)))
			{
				foreach (var _objKeys in runtimeDictionary[typeof(T)].Keys)
				{
					if (runtimeDictionary[typeof(T)][_objKeys].title == _title)
					{
						return (T)runtimeDictionary[typeof(T)][_objKeys];
					}
				}
			}

			return null;
		}

		private DataObject GetFirstDataObjectByTypeRuntime(Type _type)
		{
			if (runtimeDictionary.ContainsKey(_type))
			{
				return runtimeDictionary[_type].Values.FirstOrDefault();
			}

			return null;
		}


		private T GetFirstDataObjectByTypeRuntime<T>() where T : DataObject
		{
			// var _an = typeof(T).AssemblyQualifiedName;
			if (runtimeDictionary.ContainsKey(typeof(T)))
			{
				return (T)runtimeDictionary[typeof(T)].Values.FirstOrDefault();
			}

			return null;
		}

		private List<DataObject> GetAllDataObjectsByTypeRuntime(Type _type, bool _includeSubTypes)
		{
			List<DataObject> list = new List<DataObject>();
			

			if (_type == null)
			{
				return null;
			}

			foreach (var _key in runtimeDictionary.Keys)
			{
				List<string> _cleanupKeys = new List<string>();

				if (_includeSubTypes)
				{
					if (_key != null)
					{

						var _derived = _key;
						var _foundType = _key;
                        if (_derived.BaseType != typeof(DataObject))
						{
							do
							{
							
								_derived = _derived.BaseType;
								if (_derived != null)
								{
									if (_derived.AssemblyQualifiedName == _type.AssemblyQualifiedName)
									{
										_foundType = _derived;
									}
								}
                               
                            } while (_derived.BaseType != typeof(DataObject));

						}
					
						if (_foundType.AssemblyQualifiedName == _type.AssemblyQualifiedName)
                        {
							foreach(var _guid in runtimeDictionary[_key].Keys)
							{
								if (runtimeDictionary[_key][_guid] == null)
								{
									// Cleanup object list
									// runtimeDictionary[_key].Remove(_guid);
									_cleanupKeys.Add(_guid);
									continue;
								}

								list.Add(runtimeDictionary[_key][_guid]);
							}
						}
					}
				}

				if (_key.AssemblyQualifiedName == _type.AssemblyQualifiedName)
				{
					foreach (var _guid in runtimeDictionary[_key].Keys)
					{
						if (runtimeDictionary[_key][_guid] == null)
						{
							// Cleanup object list
							// runtimeDictionary[_key].Remove(_guid);
							_cleanupKeys.Add(_guid);
							continue;
						}

						if (!list.Contains(runtimeDictionary[_key][_guid]))
						{
							list.Add(runtimeDictionary[_key][_guid]);
						}
					}
                }

				for (int c = 0; c < _cleanupKeys.Count; c ++)
				{
					runtimeDictionary[_key].Remove(_cleanupKeys[c]);
				}
			}

		

			return list;
		}
		
		private List<T> GetAllDataObjectsByTypeRuntime<T>(bool _includeSubTypes) where T : DataObject
		{
			List<T> list = new List<T>();
			var _an = typeof(T).AssemblyQualifiedName;

			foreach (var _key in runtimeDictionary.Keys)
			{
				List<string> _cleanupKeys = new List<string>();
				
				if (_includeSubTypes)
				{
					if (_key!= null)
					{

						var _derived = _key;
						var _foundType = _key;
                        if (_derived.BaseType != typeof(DataObject))
						{
							do
							{
							
								_derived = _derived.BaseType;
								if (_derived != null)
								{
									if (_derived.AssemblyQualifiedName == _an)
									{
										_foundType = _derived;
									}
								}
                               
                            } while (_derived.BaseType != typeof(DataObject));

						}
					
						if (_foundType.AssemblyQualifiedName == _an)
                        {
							foreach(var _guid in runtimeDictionary[_key].Keys)
							{
								if (runtimeDictionary[_key][_guid] == null)
								{
									// Cleanup object list
									// runtimeDictionary[_key].Remove(_guid);
									_cleanupKeys.Add(_guid);
									continue;
								}

								list.Add((T)runtimeDictionary[_key][_guid]);
							}
						}
					}
				}
				
				if (_key.AssemblyQualifiedName == typeof(T).AssemblyQualifiedName)
				{
					foreach (var _guid in runtimeDictionary[_key].Keys)
					{
						if (runtimeDictionary[_key][_guid] == null)
						{
							// Cleanup object list
							// runtimeDictionary[_key].Remove(_guid);
							_cleanupKeys.Add(_guid);
							continue;
						}

						if (!list.Contains(runtimeDictionary[_key][_guid]))
						{
							list.Add((T)runtimeDictionary[_key][_guid]);
						}
					}
                }

				for (int c = 0; c < _cleanupKeys.Count; c ++)
				{
					runtimeDictionary[_key].Remove(_cleanupKeys[c]);
				}
			}


			return list;
		}


		private void AddDataObjectRuntime(Type _type, DataObject _dataObject)
		{
			if (runtimeDictionary.ContainsKey(_type))
			{
				if (runtimeDictionary[_type].ContainsKey(_dataObject.guid))
				{
					runtimeDictionary[_type][_dataObject.guid] = _dataObject;
				}
				else
				{
					runtimeDictionary[_type].Add(_dataObject.guid, _dataObject);
				}
			}
			else
			{
				var _newDictionary = new Dictionary<string, DataObject>();
				_newDictionary.Add(_dataObject.guid, _dataObject);
				runtimeDictionary.Add(_type, _newDictionary);
			}
		}

		private void RemoveDataObjectRuntime(Type _type, DataObject _dataObject)
		{
			if (runtimeDictionary.ContainsKey(_type))
			{
				runtimeDictionary[_type].Remove(_dataObject.guid);
			}
		}
#endregion
	}
}