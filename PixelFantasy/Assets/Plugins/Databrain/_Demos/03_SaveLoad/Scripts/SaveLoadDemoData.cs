using Databrain.Attributes;
using UnityEngine;

namespace Databrain.Examples
{
    public class SaveLoadDemoData : SaveLoadDemoDataBase
    {
        public GameObject prefab;
        //[DatabrainSerialize]  -> Alternatively you can just add the DatabrainSerialize attribute
        public Vector3 position;


        /*
         *  Demonstration of how to serialize data by using a separate serilizable class
         *  in this example: RuntimeSaveLoadDemoData derived from SerializeableDataObject.
         *  Alternatively and simpler: use the DatabrainSerialize attribute on the fields you 
         *  want to serialize.
         * 
         */
        
        // 1.
        // To make sure our data gets serialized at runtime we have to create a runtime class which only contains the
        // serializable data. In this example: "position"
        // The class must inherit from SerializableDataObject.
        // 2.
        // GetSerializedData then returns the SerializableDataObject with the serializable values  
        public override SerializableDataObject GetSerializedData()
        {
            var _runtimeData = new RuntimeSaveLoadDemoData();

            _runtimeData.position = position;

            return _runtimeData;
        }

        // 3.
        // SetSerializedData gets called when loading the data back.
        // We can then get the serialized values (position) and set it back to our original DataObject position value.
        public override void SetSerializedData(object _data)
        {

            var _rt = (_data as SerializableDataObject);

            position = ((RuntimeSaveLoadDemoData)_data as RuntimeSaveLoadDemoData).position;
        }
    }

    // The actual runtime data class which only contains serializable data types.
    public class RuntimeSaveLoadDemoData : SerializableDataObject
    {
        public Vector3 position;
    }
}