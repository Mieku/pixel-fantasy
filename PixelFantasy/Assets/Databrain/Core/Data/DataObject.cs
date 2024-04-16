/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Databrain.Attributes;
using Databrain.Modules.Import;
using Databrain.Blackboard;

using System.Linq;

namespace Databrain
{
	public class DataObject : DataObjectBase
	{
		
		[CopyGuid]
		[OdinHide]
		public string guid;
        [CopyGuid]
		[OdinHide]
        public string initialGuid;
		[OdinHide]
		public string runtimeIndexID;
		[IconField]
		[OdinHide]
		public Sprite icon;
		[ColorField]
		[OdinHide]
		public Color color;
		[OdinHide]
		public string title;
		[Textfield]
		[OdinHide]
		public string description;

		[Hide]
		[OdinHide]
        public List<string> tags = new List<string>();

        [Hide]
		[OdinHide]
		public int index;
		[Hide]
		[OdinHide]
		// The data library object this data object belongs to
		public DataLibrary relatedLibraryObject;
		[Hide]
		[OdinHide]
		public DataLibrary runtimeLibraryObject;
		[Hide]
		[OdinHide]
		public string assignedGuid;
		[Hide]
		[OdinHide]
		// if true, this object only lives during runtime and will be destroyed after.
		public bool isRuntimeInstance;

		[Hide]
		[OdinHide]
		[SerializeField]
		private bool boxFoldout;


		private bool _isRuntimeSerialization;
		public bool isRuntimeSerialization => _isRuntimeSerialization;

		/// <summary>
		/// Simple class to collect data for the override method collect obects.
		/// </summary>
		public class ScriptableObjectData
        {
			public ScriptableObject so;
			public string title;

			public ScriptableObjectData(ScriptableObject so, string title)
			{
				this.so = so;
				this.title = title;
			}
		}


		/// <summary>
		/// Get the first runtime clone object
		/// </summary>
		[DatabrainNonSerialize]
        public DataObject runtimeClone 
		{  
			get 
			{
				if (_runtimeClone == null)
				{
					_runtimeClone = GetRuntimeDataObject();
				}

				return _runtimeClone;  
			} 
			set
			{
				_runtimeClone = value;
			}
		}

		[DatabrainNonSerialize]
		private DataObject _runtimeClone;

		// EXPERIMENTAL
		#region dataproperties_experimental
		// [Hide]
		// [OdinHide]
		// internal List<DataPropertyBase> dataProperties = new List<DataPropertyBase>();
		// private Dictionary<string, DataPropertyBase> dataPropertiesDictionary = new Dictionary<string, DataPropertyBase>();


		// internal void AddDataProperty(DataPropertyBase _dataProperty)
		// {
		// 	dataProperties.Add(_dataProperty);
		// }

		// internal void RemoveDataProperty(DataPropertyBase _dataProperty)
		// {
		// 	if (dataProperties.Contains(_dataProperty))
		// 	{
		// 		dataProperties.Remove(_dataProperty);
		// 	}
		// }
		#endregion

		/// <summary>
		/// EXPERIMENTAL
		/// Get the data property object
		/// </summary>
		/// <param name="_propertyName"></param>
		/// <returns></returns>
		// public DataPropertyBase GetDataProperty(string _propertyName)
		// {
		// 	foreach ( var _key in dataPropertiesDictionary.Keys)
		// 	{
		// 		if (dataPropertiesDictionary[_key].title == _propertyName)
		// 		{
		// 			return dataPropertiesDictionary[_key];
		// 		}
		// 	}
		
		// 	return null;
		// }

		/// <summary>
		/// Get the runtime clone of this data object. A data object can have more than one runtime clone.
		/// Provide the owner game object if the runtime data object has been cloned manually at runtime, otherwise first in the list will be returned.
		/// </summary>
		/// <param name="_ownerGameObject"></param>
		public DataObject GetRuntimeDataObject(GameObject _ownerGameObject = null)
		{
			if (_ownerGameObject == null)
			{
				if (relatedLibraryObject == null)
				{
					return null;
				}

				var _rt = relatedLibraryObject.GetAllRuntimeDataObjectsByType(this.GetType());
				if (_rt == null)
				{
					return null;
				}

				// Get all runtime objects with initial guid = this guid
				var _allBelongingRt = _rt.Where(x => x.initialGuid == guid && x != null).ToList();
				if (_allBelongingRt != null) 
				{
					// Return first or defaul in list 
					return _allBelongingRt.FirstOrDefault();
				}
			}
			else 
			{
                var _rt = relatedLibraryObject.GetAllRuntimeDataObjectsByType(this.GetType());

                // Get all runtime objects with initial guid = this guid
                var _allBelongingRt = _rt.Where(x => x.initialGuid == guid).ToList();
				var _instanceID = _ownerGameObject.GetInstanceID().ToString();

				for (int i = 0; i < _allBelongingRt.Count; i ++)
				{
					if (_allBelongingRt[i].guid == _instanceID)
					{
						return _allBelongingRt[i];
					}
				}
            }
			
            return null;
		}

		internal void SetRuntimeSerialization(bool _value)
		{
			_isRuntimeSerialization = _value;
		}

        /// <summary>
        /// Clone this DataObject directly to its related runtime-DataLibrary.
		/// An initial data object can have multiple runtime clones. When passing the owner game object,
		/// the runtime clone can be linked to this owner game object which makes it possible to retrieve the runtime clone
		/// by the owner game object.
        /// </summary>
        /// <param name="_ownerGameObject"></param>
        public virtual void CloneToRuntimeLibrary(GameObject _ownerGameObject = null)
		{
            if (relatedLibraryObject != null)
			{
                var _isRuntimeSerialization = relatedLibraryObject.IsRuntimeSerialization(this.GetType().Name);
				if (!_isRuntimeSerialization)
                {
                    return;
                }

                if (!isRuntimeInstance)
				{
                    _runtimeClone = relatedLibraryObject.CloneDataObjectToRuntime(this, _ownerGameObject);
				}
			}
		}
		
		/// <summary>
		/// Called during initialization
		/// </summary>
		internal void Initialize()
		{
			
			// if (dataPropertiesDictionary != null)
			// {
			// 	dataPropertiesDictionary.Clear();
			// }
			// else
			// {
			// 	dataPropertiesDictionary = new Dictionary<string, DataPropertyBase>();
			// }

			// foreach (var _dataProperty in dataProperties)
			// {
			// 	if (_dataProperty == null)
			// 		continue;

			// 	dataPropertiesDictionary.Add(_dataProperty.guid, _dataProperty);
			// }
		}

		/// <summary>
		/// Called on start after DataLibrary has been initialized
		/// </summary>
		public virtual void OnStart(){}
		
		/// <summary>
		/// Called after DataLibrary has been disabled
		/// </summary>
		public virtual void OnEnd()
		{
			if (isRuntimeInstance)
			{
				DestroyImmediate(this, true);
            }
        }	

		/// <summary>
		/// Override this method to return the serialized data object class of your custom DataObject to make sure
		/// the data gets serialized as json when saving to a file.
		/// </summary>
		/// <returns></returns>
		public virtual SerializableDataObject GetSerializedData(){ return null; }

		/// <summary>
		/// Gets called after the DataLibrary has loaded the json file. The _data object contains the serialized data object.
		/// </summary>
		/// <param name="_data"></param>
		public virtual void SetSerializedData(object _data){ }


		/// <summary>
		/// Return all runtime DataObjects from the same type of this DataObject
		/// </summary>
		/// <returns></returns>
		public List<DataObject> GetAllRuntimeDataObjectsFromThisType()
        {
            return relatedLibraryObject.GetAllRuntimeDataObjectsByType(this.GetType());
        }
		/// <summary>
		/// Return all runtime DataObject which has been cloned from this data object
		/// </summary>
		/// <returns></returns>
		public List<DataObject> GetAllRuntimeDataObjectFromThis()
		{
			List<DataObject> _returnList = new List<DataObject>();

            var _rt = relatedLibraryObject.GetAllRuntimeDataObjectsByType(this.GetType());

            // Get all runtime objects with initial guid = this guid
            _returnList = _rt.Where(x => x.initialGuid == guid).ToList();


			return _returnList;
		}

        /// <summary>
        /// Return the initial data object
        /// </summary>
        /// <returns></returns>
        public DataObject GetInitialDataObject()
        {
            if (isRuntimeInstance)
            {
                //Debug.Log("_ initial guid: " + initialGuid);
                return relatedLibraryObject.GetInitialDataObjectByGuid(initialGuid);
            }
            else
            {
                return relatedLibraryObject.GetInitialDataObjectByGuid(guid);
            }
        }


		/// <summary>
		/// Set a new guid. Only for runtime data objects.
		/// </summary>
		/// <param name="_guid"></param>
        public void SetNewGuid(string _guid)
		{
			if (isRuntimeInstance)
			{
				// update runtime dictionary
				relatedLibraryObject.runtimeLibrary.data.ChangeRuntimeGUID(this.GetType(), guid, _guid);

				guid = _guid;
			}
			else
			{
				Debug.LogWarning("Changing guid for initial data object is not allowed");
			}
		}

		/// <summary>
		/// Can be used to return a runtime data GUI using UIElements
		/// </summary>
		/// <returns></returns>
		public virtual VisualElement RuntimeGUI() { return null; }

        #region EditorMethods
#if UNITY_EDITOR
        /// <summary>
        /// Create custom GUI for this DataObject using UIElements
        /// </summary>
        /// <param name="_serializedObject"></param>
        /// <param name="_editorWindow"></param>
        /// <returns></returns>
        public virtual VisualElement EditorGUI(SerializedObject _serializedObject, DatabrainEditorWindow _editorWindow){return null;}

#endif

		/// <summary>
		/// Custom Icon override instead of using an attribute. (Used by the scene component types of the Logic add-on)
		/// </summary>
		/// <returns></returns>
		public virtual Texture2D ReturnIcon() { return null;  }

		public virtual void OnAddCallback(System.Type _componentType) { }
        /// <summary>
        /// Editor method. Resets the DataObject. Must be overriden to reset all custom values.
        /// </summary>
        public virtual void Reset()
        {
            icon = null;
			color = default;
            title = "New Data entry";
            description = "This is a description of the newly created data entry.";
        }

        /// <summary>
        /// Editor method which gets called when DataObject gets selected in the Databrain editor
        /// </summary>
        public virtual void Selected() { }

        /// <summary>
        /// Editor method which gets called when a DataObject gets deleted in the Databrain editor
        /// </summary>
        public virtual void OnDelete() { }

		/// <summary>
		/// Editor method which gets called when DataObject gets duplicated
		/// </summary>
		public virtual void OnDuplicate(DataObject _duplicateFrom) { }

        /// <summary>
        /// Editor method where you can return true or false if this DataObject is valid.
        /// When returning false, an exclamation mark is being showed in the Databrain object list view.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid(){return true;}

		/// <summary>
		/// Override this method to manually convert CSV string data to your custom DataObject
		/// </summary>
		/// <param name="_data"></param>
		public virtual void ConvertFromCSV(DatabrainCSVConverter.Entry _data){}

		/// <summary>
		/// Collect sub objects for displaying fields in the quick access data view.
		/// This is mostly being used for the logic node editor where node fields also
		/// can have quick access fields.
		/// </summary>
		/// <returns></returns>
        public virtual List<ScriptableObjectData> CollectObjects(){return null;}

		/// <summary>
		/// Used by Logic add-on. Return true or false if _type matches the sceneComponentType filter in the DataObjectDropdown property drawer.
		/// </summary>
		/// <param name="_type"></param>
		/// <returns></returns>
		public virtual bool CheckForType(System.Type _type) { return false; }
        #endregion
    }
}