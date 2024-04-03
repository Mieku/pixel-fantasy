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

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class BorderAttribute : PropertyAttribute
    {
        public int borderWidth;
        public Color color;

        public BorderAttribute(int borderWidth, DatabrainColor color)
        {
            this.borderWidth = borderWidth;
            this.color = color.GetColor();
        }
    }
}