using System;
using Data.Zones;
using UnityEngine;

namespace Systems.Zones.Scripts
{
    public class ZoneCellObject : MonoBehaviour
    {
        private ZoneData _data;

        public void Init(ZoneData data)
        {
            _data = data;
        }

        private void OnMouseDown()
        {
            ZoneManager.Instance.SelectZone(_data);
        }
    }
}
