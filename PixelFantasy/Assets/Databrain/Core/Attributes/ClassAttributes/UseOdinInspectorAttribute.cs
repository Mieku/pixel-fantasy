/*
 *	DATABRAIN
 *	(c) 2024 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;

namespace Databrain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UseOdinInspectorAttribute : Attribute
    {
        public UseOdinInspectorAttribute(){}
    }
}