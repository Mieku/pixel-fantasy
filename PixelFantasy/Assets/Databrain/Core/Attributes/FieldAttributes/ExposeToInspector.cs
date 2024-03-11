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
    /// <summary>
    /// Show this field in the data object dropdown viewer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)] 
	public class ExposeToInspector : PropertyAttribute 
	{
		
        public ExposeToInspector(){}
	}
}