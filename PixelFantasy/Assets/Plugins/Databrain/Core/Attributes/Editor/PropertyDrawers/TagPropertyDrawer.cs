/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
	public class TagPropertyDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var _root = new VisualElement();

            if (property.propertyType == SerializedPropertyType.String)
            {
                List<string> tagList = new List<string>();
                tagList.Add("(None)");
                tagList.Add("Untagged");
                tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

                string propertyString = property.stringValue;
                int index = 0;

                // check if there is an entry that matches the entry and get the index
                // we skip index 0 as that is a special custom case
                for (int i = 1; i < tagList.Count; i++)
                {
                    if (tagList[i].Equals(propertyString, System.StringComparison.Ordinal))
                    {
                        index = i;
                        break;
                    }
                }

                var _dropdown = new DropdownField(tagList, index);
                _dropdown.label = property.displayName;

                _dropdown.RegisterValueChangedCallback(x =>
                {
                    if (_dropdown.index > 0)
                    {
                        property.stringValue = x.newValue;
                    }
                    else
                    {
                        property.stringValue = string.Empty;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                });

                _root.Add(_dropdown);
            }

            return _root;
        }

    }
}
#endif