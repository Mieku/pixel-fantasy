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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class HorizontalLineAttribute : PropertyAttribute
    {
        public const float defaultHeight = 2.0f;
	    public const DatabrainColor defaultColor = DatabrainColor.Grey;

        public float height;
        public DatabrainColor color;

        public HorizontalLineAttribute(float height = defaultHeight, DatabrainColor color = defaultColor)
        {
            this.height = height;
            this.color = color;
        }
    }
}
