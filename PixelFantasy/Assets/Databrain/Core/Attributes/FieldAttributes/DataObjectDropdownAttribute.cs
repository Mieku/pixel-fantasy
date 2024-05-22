/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using UnityEngine;

namespace Databrain.Attributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)] 
	public class DataObjectDropdownAttribute : PropertyAttribute
	{
		public string dataLibraryFieldName;
		public bool includeSubtypes;
		public string tooltip;

		/// <summary>
		/// Set a custom height for the dropdown data view inspector
		/// </summary>
		public int customHeight = -1;

		public Type sceneComponentType;

		public DataObjectDropdownAttribute(){}

        
        public DataObjectDropdownAttribute(bool includeSubtypes = false, string tooltip = "", Type sceneComponentType = null)
		{
			this.includeSubtypes = includeSubtypes;
			this.tooltip = tooltip;
			this.sceneComponentType = sceneComponentType;
		}

		public DataObjectDropdownAttribute(string dataLibraryFieldName, bool includeSubtypes = false, string tooltip = "", int customHeight = -1)
		{
            this.dataLibraryFieldName = dataLibraryFieldName;
			this.includeSubtypes = includeSubtypes;
			this.tooltip = tooltip;
			this.customHeight = customHeight;
		}
    }
}