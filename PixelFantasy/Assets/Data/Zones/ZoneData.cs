using System.Collections.Generic;
using Databrain;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;

namespace Data.Zones
{
    [DataObjectAddToRuntimeLibrary]
    public abstract class ZoneData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize]
        public string ZoneName;
        
        [ExposeToInspector, DatabrainSerialize]
        public List<Vector2> Cells = new List<Vector2>();
        
        [ExposeToInspector, DatabrainSerialize]
        public List<ZoneCellObject> ZoneCellObjects = new List<ZoneCellObject>();

        [ExposeToInspector, DatabrainSerialize]
        public int AssignedLayer;

        [ExposeToInspector, DatabrainSerialize]
        public bool IsVisible;

        public int NumCells => Cells.Count;
    }
}
