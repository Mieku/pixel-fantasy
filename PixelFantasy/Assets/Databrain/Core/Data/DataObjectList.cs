/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Databrain.Data
{
	[System.Serializable]
	public class DataObjectList
	{
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


		public DataObject GetDataObjectByGuid(string _guid, Type _type = null)
		{

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

		public T GetDataObjectByGuid<T>(string _guid) where T : DataObject
		{
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
		
		public DataObject GetDataObjectByTitle(string _title, Type _type = null)
		{
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

		public T GetDataObjectByTitle<T>(string _title) where T : DataObject
		{
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
		
		public DataObject GetFirstDataObjectByType(Type _type)
		{

			for (int i = 0; i < _objectList.Count; i++)
			{
				if (_objectList[i].type == _type.AssemblyQualifiedName)
				{
					return _objectList[i].dataObjects.FirstOrDefault();
				}
			}

            return null;
        }

		public T GetFirstDataObjectByType<T>() where T : DataObject
		{
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

		public List<T> GetAllDataObjectsByType<T>(bool _includeSubTypes) where T : DataObject
		{
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
		
		public List<DataObject> GetAllDataObjectsByType(Type _type, bool _includeSubTypes)
		{
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

  

        public void AddDataObject(Type _type, DataObject _dataObject)
		{
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
		
		public void RemoveDataObject(Type _type, DataObject _dataObject)
		{
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


		public void SetFavorite(DataObject _dataObject)
		{
			_favoriteList.Add(new DataType(_dataObject.GetType(), _dataObject));
		}

		public void RemoveFromFavorite(DataObject _dataObject)
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

		public bool IsFavorite(DataObject _dataObject)
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
	}
}