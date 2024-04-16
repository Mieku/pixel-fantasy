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
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class DataObjectTypeNameAttribute : Attribute
	{
		public string typeName;
		public DataObjectTypeNameAttribute(string typeName)
		{
			this.typeName = typeName;
		}
	}

	[AttributeUsage(AttributeTargets.Class)] 
	public class DataObjectIconAttribute : Attribute
	{
		public string iconPath;
		public Color iconColor;
		public string iconColorHex;

		public DataObjectIconAttribute(string _iconPath = "typeIcon", DatabrainColor _iconColor = DatabrainColor.White, string _iconColorHex = "")
		{
			iconPath = _iconPath;
			iconColor = _iconColor.GetColor();
			iconColorHex = _iconColorHex;
		}
	}
	

	[AttributeUsage(AttributeTargets.Class)] 
	public class DataObjectOrderAttribute : Attribute
	{
		public int order;
		
		public DataObjectOrderAttribute(int _order)
		{
			order = _order;
		}
	}
	
	/// <summary>
	/// Only allow for a certain amount of data objects of this type. 
	/// Useful for managers for example, where we only need one.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)] 
	public class DataObjectMaxObjectsAttribute : Attribute
	{
		public int maxObjects;
		
		public DataObjectMaxObjectsAttribute(int _maxObjects)
		{
			maxObjects = _maxObjects;
		}
	}
	
	/// <summary>
	/// Hide certain fields from the base class (Icon, Title, Description)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)] 
	public class DataObjectHideBaseFieldsAttribute : Attribute
	{
		
		public bool hideIconField;
		public bool hideColorField;
		public bool hideTitleField;
		public bool hideDescriptionField;
		
		public DataObjectHideBaseFieldsAttribute(bool _hideIconField = false, bool _hideColorField = false, bool _hideTitleField = false, bool _hideDescriptionField = false)
		{
			hideIconField = _hideIconField;
			hideColorField = _hideColorField;
			hideTitleField = _hideTitleField;
			hideDescriptionField = _hideDescriptionField;
		}
	}
	
	/// <summary>
	/// Hide all variable fields. Useful when creating custom GUI
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)] 
	public class DataObjectHideAllFieldsAttribute : Attribute {}

	/// <summary>
	/// Add this type to the runtime library by default
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DataObjectAddToRuntimeLibrary : Attribute { }


	/// <summary>
	/// [Deprecated] Display the list item in a smaller version
	/// </summary>
	
	[AttributeUsage(AttributeTargets.Class)]
	public class DataObjectSmallListItem : Attribute { }
}