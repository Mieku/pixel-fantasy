/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.Modules
{
	public class DatabrainModuleBase : ScriptableObject
	{
		public virtual VisualElement DrawGUI(DataLibrary _dataContainer, DatabrainEditorWindow _editorWindow)
		{
			return null;
		}
		
		public virtual void Initialize(DataLibrary _datacoreObject) {}
		
		public virtual void OnOpen(DataLibrary _datacoreObject) { }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DatacoreModuleAttribute : Attribute
	{
		public string title;
		public int order;
		public string icon;

		public DatacoreModuleAttribute(string _title, int _order = -1, string _icon = "")
		{
			title = _title;
			order = _order;
			icon = _icon;
		}
	}
}
#endif