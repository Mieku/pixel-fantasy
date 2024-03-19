/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using System.Collections.Generic;

namespace Databrain.Blackboard
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectIcon("floatlist", DatabrainColor.Black)]
    [DataObjectTypeName("Float List")]
    public class FloatList : BlackboardGenericVariable<List<float>>
    {
        public override SerializableDataObject GetSerializedData()
        {
            return new FloatListRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _list = (FloatListRuntime)_data;
            _value = _list.value;
        }
    }

    public class FloatListRuntime : SerializableDataObject
    {
        public List<float> value;

        public FloatListRuntime (List<float> value)
        {
            this.value = value;
        }
    }
}
