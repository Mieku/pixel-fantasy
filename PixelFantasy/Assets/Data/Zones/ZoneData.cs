using System.Collections.Generic;
using Databrain;
using Databrain.Attributes;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Data.Zones
{
    [DataObjectAddToRuntimeLibrary]
    public abstract class ZoneData : DataObject
    {
        [ExposeToInspector, DatabrainSerialize]
        public string ZoneName;
        
        [ExposeToInspector, DatabrainSerialize]
        public List<Vector3Int> Cells = new List<Vector3Int>();
        
        [ExposeToInspector, DatabrainSerialize]
        public List<ZoneCellObject> ZoneCellObjects = new List<ZoneCellObject>();

        [ExposeToInspector, DatabrainSerialize]
        public int AssignedLayer;

        [ExposeToInspector, DatabrainSerialize]
        public bool IsVisible;

        public int NumCells => Cells.Count;
        
        public abstract Color ZoneColour { get; }
        public abstract TileBase DefaultTiles { get; }
        public abstract TileBase SelectedTiles { get; }
        
        public virtual void SelectZone()
        {
            ZoneManager.Instance.SelectZone(this);
        }
    }
}
