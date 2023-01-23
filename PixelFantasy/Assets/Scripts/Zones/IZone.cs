using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public interface IZone
{
    public void ClickZone();
    public void UnclickZone();
    public void ExpandZone(List<Vector3Int> newCells);
    
    public string UID { get; set; }
    public string Name { get; set; }
    public ZoneType ZoneType { get; }
    public ZoneTypeData ZoneTypeData { get; }
    public LayeredRuleTile LayeredRuleTile { get; set; }
    public List<Vector3Int> GridPositions { get; set; }
}
