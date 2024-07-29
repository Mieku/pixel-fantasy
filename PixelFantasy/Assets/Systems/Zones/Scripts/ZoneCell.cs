using UnityEngine;

namespace Systems.Zones.Scripts
{
    public class ZoneCell : MonoBehaviour
    {
        protected ZoneData _data;

        public Vector3Int CellPos;

        public virtual void Init(ZoneData data, Vector3Int cellPos)
        {
            _data = data;
            CellPos = cellPos;
        }

        public virtual void DeleteCell()
        {
            Destroy(gameObject);
        }

        /// Deletes the cell without triggering any events, used for loading / clearing the game
        public void ClearCell()
        {
            Destroy(gameObject);
        }

        public virtual void TransferOwner(ZoneData zoneData)
        {
            _data = zoneData;
        }

        private void OnMouseDown()
        {
            ZonesDatabase.Instance.SelectZone(_data);
        }
    }
}
