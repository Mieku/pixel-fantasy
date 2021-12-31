using UnityEngine;

namespace Items
{
    public class StorageSlot : MonoBehaviour
    {
        private Transform itemTransform;
        private bool hasItemIncoming;
        
        public bool IsEmpty()
        {
            return itemTransform == null && !hasItemIncoming;
        }

        public void HasItemIncoming(bool hasItemIncoming)
        {
            this.hasItemIncoming = hasItemIncoming;
        }

        public void SetItemTransform(Transform itemTransform)
        {
            this.itemTransform = itemTransform;
            hasItemIncoming = false;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
