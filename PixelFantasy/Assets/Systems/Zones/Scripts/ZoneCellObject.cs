using System;
using Data.Zones;
using UnityEngine;

namespace Systems.Zones.Scripts
{
    public class ZoneCellObject : MonoBehaviour
    {
        private ZoneData _data;

        public Vector3Int CellPos;

        public void Init(ZoneData data, Vector3Int cellPos)
        {
            _data = data;
            CellPos = cellPos;
        }

        private void OnMouseDown()
        {
            ZoneManager.Instance.SelectZone(_data);
        }
    }
}
