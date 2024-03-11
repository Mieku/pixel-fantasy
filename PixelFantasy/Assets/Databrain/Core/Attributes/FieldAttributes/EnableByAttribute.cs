/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;

namespace Databrain.Attributes
{
    public class EnableByAttribute : PropertyAttribute
    {
        public string fieldName;

        /// <summary>
        /// Enable/disable a field depending on a bool fields value (fieldName) 
        /// </summary>
        /// <param name="_fieldName"></param>
        public EnableByAttribute(string _fieldName)
        {
            fieldName = _fieldName;
        }
    }
}