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
    [DataObjectTypeName("Vector2")]
    [DataObjectIcon("vector2", DatabrainColor.Black)]
    public class Vector2Variable : BlackboardGenericVariable<Vector2>
    {
        public Vector2Variable()
        {
            _value = Vector2.zero;
        }

        public override SerializableDataObject GetSerializedData()
        {
            return new Vector2VariableRuntime(_value);
        }

        public override void SetSerializedData(object _data)
        {
            var _v2 = (Vector2VariableRuntime)_data;
            _value = _v2.value;
        }
    }

    public class Vector2VariableRuntime : SerializableDataObject
    {
        public Vector2 value;

        public Vector2VariableRuntime(Vector2 value)
        {
            this.value = value;
        }
    }
}