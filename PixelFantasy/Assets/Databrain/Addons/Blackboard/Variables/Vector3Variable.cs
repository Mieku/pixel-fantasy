/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using UnityEngine;

namespace Databrain.Blackboard
{
    [DataObjectAddToRuntimeLibrary]
    [DataObjectTypeName("Vector3")]
    [DataObjectIcon("vector3", DatabrainColor.Black)]
    public class Vector3Variable : BlackboardGenericVariable<Vector3>
    {
        public Vector3Variable()
        {
            _value = Vector3.zero;
        }

        public override SerializableDataObject GetSerializedData()
        {
            return new Vector3VariableRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _v3 = (Vector3VariableRuntime)_data;
            _value = _v3.value;
        }
    }

    public class Vector3VariableRuntime : SerializableDataObject
    {
        public Vector3 value;

        public Vector3VariableRuntime(Vector3 value)
        {
            this.value = value;
        }
    }
}