/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine.UIElements;

namespace Databrain.Blackboard
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectIcon("string", DatabrainColor.Black)]
    [DataObjectTypeName("String")]
    [DataObjectHideAllFields]
    public class StringVariable : BlackboardGenericVariable<string>
    {
        public override SerializableDataObject GetSerializedData()
        {
            var _rt = new StringVariableRuntime(_value);
            return _rt;
        }

        public override void SetSerializedData(object _data)
        {
            var _string = _data as StringVariableRuntime;
            _value = _string.value;
        }
#if UNITY_EDITOR
        public override VisualElement EditorGUI(SerializedObject _serializedObject, DatabrainEditorWindow _editorWindow)
        {
            var _root = new VisualElement();
            
            var _event = new PropertyField();
            _event.BindProperty(_serializedObject.FindProperty("onValueChanged"));

            var _prop = new TextField();
            _prop.multiline = true;
            _prop.label = "Value";
            _prop.BindProperty(_serializedObject.FindProperty("_value"));

            _root.Add(_event);
            _root.Add(_prop);

            return _root;
        }
#endif
    }


    public class StringVariableRuntime : SerializableDataObject
    {
        public string value;
        
        public StringVariableRuntime(string _string)
        {
            value = _string;
        }
    }

}
