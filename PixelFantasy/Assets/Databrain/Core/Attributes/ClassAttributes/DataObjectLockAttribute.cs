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
    /// Lock data object creation for this class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataObjectLockAttribute : Attribute { }
}