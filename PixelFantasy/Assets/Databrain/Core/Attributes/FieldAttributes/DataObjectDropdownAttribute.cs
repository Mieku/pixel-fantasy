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

		public Type sceneComponentType;

		public DataObjectDropdownAttribute(){}

        
        public DataObjectDropdownAttribute(bool includeSubtypes = false, string tooltip = "", Type sceneComponentType = null)
		{
			this.includeSubtypes = includeSubtypes;
			this.tooltip = tooltip;
			this.sceneComponentType = sceneComponentType;
		}

		public DataObjectDropdownAttribute(string dataLibraryFieldName, bool includeSubtypes = false, string tooltip = "")
		{
            this.dataLibraryFieldName = dataLibraryFieldName;
			this.includeSubtypes = includeSubtypes;
			this.tooltip = tooltip;
		}
    }
}