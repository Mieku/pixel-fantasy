using Data.Resource;
using Databrain;
using Databrain.Attributes;
using UnityEngine;

namespace Data.Item
{
    [DataObjectAddToRuntimeLibrary]
    public class CropData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize]  public CropSettings Settings;
    }
}
