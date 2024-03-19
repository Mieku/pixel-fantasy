/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;

namespace Databrain.Attributes
{
    public class EnumFlagsAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagsAttribute() { }

        public EnumFlagsAttribute(string name)
        {
            enumName = name;
        }
    }
}