/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;

namespace Databrain.Blackboard
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectIcon("string", DatabrainColor.Black)]
    [DataObjectTypeName("String")]
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
