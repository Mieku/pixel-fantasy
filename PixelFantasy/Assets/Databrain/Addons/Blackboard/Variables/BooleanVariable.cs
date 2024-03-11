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
    [DataObjectIcon("boolean", DatabrainColor.Black)]
    [DataObjectTypeName("Boolean")]
    public class BooleanVariable : BlackboardGenericVariable<bool>
    {
        public override SerializableDataObject GetSerializedData()
        {
            return new BooleanVariableRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _bool = (BooleanVariableRuntime)_data;
            _value = _bool.value;
        }
      
    }

    public class BooleanVariableRuntime : SerializableDataObject
    {
        public bool value;

        public BooleanVariableRuntime(bool value)
        {
            this.value = value;
        }
    }
}
