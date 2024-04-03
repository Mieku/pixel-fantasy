/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;
using Databrain.Attributes;

namespace Databrain.Blackboard
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectIcon("integerlist", DatabrainColor.Black )]
    [DataObjectTypeName("Integer List")]
    public class IntegerList : BlackboardGenericVariable<List<int>>
    {
        public override SerializableDataObject GetSerializedData()
        {
            return new IntegerListRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _list = (IntegerListRuntime)_data;
            _value = _list.value;
        }
    }

    public class IntegerListRuntime : SerializableDataObject
    {
        public List<int> value;

        public IntegerListRuntime(List<int> value)
        {
            this.value = value;
        }
    }
}