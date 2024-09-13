using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Zones.Scripts
{
    public class ZoneCell : MonoBehaviour
    {
        public ZoneData RuntimeData;

        public Vector3Int CellPos;

        public virtual void Init(ZoneData data, Vector3Int cellPos)
        {
            RuntimeData = data;
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
            RuntimeData = zoneData;
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            ZonesDatabase.Instance.SelectZone(RuntimeData);
        }
    }
}
