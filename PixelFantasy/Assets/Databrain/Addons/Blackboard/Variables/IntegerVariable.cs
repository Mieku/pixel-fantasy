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
    [DataObjectIcon("integer", DatabrainColor.Black )]
    [DataObjectTypeName("Integer")]
    public class IntegerVariable : BlackboardGenericVariable<int>
    {
        public override SerializableDataObject GetSerializedData()
        {
            return new IntegerVariableRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _int = (IntegerVariableRuntime)_data;
            _value = _int.value;
        }
    }

    public class IntegerVariableRuntime : SerializableDataObject
    {
        public int value;

        public IntegerVariableRuntime(int value)
        {
            this.value = value;
        }
    }
}