#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Databrain.Helpers;

namespace Databrain.Attributes
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {


        [System.Flags]
        enum TriBool
        {
            Unset = 0,
            False = 1,
            True = 2,
            Both = 3
        }

        struct Entry
        {
            public string label;
            public int mask;
            public TriBool currentValue;
        }

        List<SerializedProperty> _properties;
        List<Entry> _entries;


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (_properties == null)
                Initialize(property);


            var _root = new VisualElement();
            _root.style.flexDirection = FlexDirection.Row;

            var _label = new Label();
            _label.text = property.displayName;


            _root.Add(_label);

            var _toggleGroup = new VisualElement();
            _toggleGroup.style.flexDirection = FlexDirection.Row;
            _toggleGroup.style.flexWrap = Wrap.Wrap;

            for (int i = 0; i < _entries.Count; i++)
            {
                var _index = i;        
                var entry = _entries[i];
                var _toggle = new Toggle();
                DatabrainHelpers.SetBorder(_toggle, 2, DatabrainColor.LightGrey.GetColor());
                _toggle.style.marginLeft = 2;
                _toggle.style.paddingRight = 4;
                _toggle.value = entry.currentValue == TriBool.True;
                _toggle.label = entry.label;

                _toggle.RegisterCallback<ChangeEvent<bool>>(evt =>  //.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        foreach (var prop in _properties)
                            prop.intValue |= entry.mask;
                        entry.currentValue = TriBool.True;
                    }
                    else
                    {
                        foreach (var prop in _properties)
                            prop.intValue &= ~entry.mask;
                        entry.currentValue = TriBool.False;
                    }

                    _entries[_index] = entry;
   

                    foreach (var prop in _properties)
                    {
                        prop.serializedObject.ApplyModifiedProperties();
                    }
                });

                _toggleGroup.Add(_toggle);
            }

            _root.Add(_toggleGroup);

            return _root;
        }


        void Initialize(SerializedProperty property)
        {
            var allTargetObjects = property.serializedObject.targetObjects;
            _properties = new List<SerializedProperty>(allTargetObjects.Length);
            foreach (var targetObject in allTargetObjects)
            {
                SerializedObject iteratedObject = new SerializedObject(targetObject);
                SerializedProperty iteratedProperty = iteratedObject.FindProperty(property.propertyPath);
                if (iteratedProperty != null)
                    _properties.Add(iteratedProperty);
            }

            var parentType = property.serializedObject.targetObject.GetType();
            var fieldInfo = parentType.GetField(property.propertyPath);
            var enumType = fieldInfo.FieldType;
            var trueNames = System.Enum.GetNames(enumType);

            var typedValues = GetTypedValues(property, enumType);
            var display = property.enumDisplayNames;
            var names = property.enumNames;

            _entries = new List<Entry>();

            for (int i = 0; i < names.Length; i++)
            {
                int sortedIndex = System.Array.IndexOf(trueNames, names[i]);
                int value = typedValues[sortedIndex];
                int bitCount = 0;

                for (int temp = value; (temp != 0 && bitCount <= 1); temp >>= 1)
                    bitCount += temp & 1;


                if (bitCount != 1)
                    continue;

                TriBool consensus = TriBool.Unset;
                foreach (var prop in _properties)
                {
                    if ((prop.intValue & value) == 0)
                        consensus |= TriBool.False;
                    else
                        consensus |= TriBool.True;
                }

                _entries.Add(new Entry { label = display[i], mask = value, currentValue = consensus });
            }

        }

        int[] GetTypedValues(SerializedProperty property, System.Type enumType)
        {
            var values = System.Enum.GetValues(enumType);
            var underlying = System.Enum.GetUnderlyingType(enumType);

            if (underlying == typeof(int))
                return ConvertFrom<int>(values);
            else if (underlying == typeof(uint))
                return ConvertFrom<uint>(values);
            else if (underlying == typeof(short))
                return ConvertFrom<short>(values);
            else if (underlying == typeof(ushort))
                return ConvertFrom<ushort>(values);
            else if (underlying == typeof(sbyte))
                return ConvertFrom<sbyte>(values);
            else if (underlying == typeof(byte))
                return ConvertFrom<byte>(values);
            else
                throw new System.InvalidCastException("Cannot use enum backing types other than byte, sbyte, ushort, short, uint, or int.");
        }

        int[] ConvertFrom<T>(System.Array untyped) where T : System.IConvertible
        {
            var typedValues = new int[untyped.Length];

            for (int i = 0; i < typedValues.Length; i++)
                typedValues[i] = System.Convert.ToInt32((T)untyped.GetValue(i));

            return typedValues;
        }
    }
}
#endif