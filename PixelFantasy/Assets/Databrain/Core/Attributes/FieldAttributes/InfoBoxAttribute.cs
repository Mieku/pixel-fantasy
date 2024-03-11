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
	public enum InfoBoxType
    {
        Normal,
        Warning,
        Error
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string text { get; private set; }
        public InfoBoxType type { get; private set; }

        public InfoBoxAttribute(string text, InfoBoxType type = InfoBoxType.Normal)
        {
            this.text = text;
            this.type = type;
        }
    }
}
