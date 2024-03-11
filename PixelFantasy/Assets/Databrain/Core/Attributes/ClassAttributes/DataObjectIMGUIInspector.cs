/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;

namespace Databrain.Attributes
{
    /// <summary>
    /// Force the inspector to draw fields with IMGUI.
    /// This is just to maintain downward compatibility for certain property drawers
    /// which do not work with UIElements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataObjectIMGUIInspectorAttribute : Attribute { }
}