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
    [DataObjectIcon("float", DatabrainColor.Black)]
    [DataObjectTypeName("Float")]
    public class FloatVariable : BlackboardGenericVariable<float>
    {

        public override SerializableDataObject GetSerializedData()
        {
            return new FloatVariableRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _float = (FloatVariableRuntime)_data;
            _value = _float.value;
        }
    }

    public class FloatVariableRuntime : SerializableDataObject
    {
        public float value;

        public FloatVariableRuntime(float value)
        {
            this.value = value;
        }
    }
}
