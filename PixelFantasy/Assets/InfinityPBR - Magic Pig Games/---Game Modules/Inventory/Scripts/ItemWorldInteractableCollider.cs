using UnityEngine;

namespace InfinityPBR.Modules.Inventory
{
    public class ItemWorldInteractableCollider : MonoBehaviour, IAmInteractable
    {
        public ItemWorldInteractable parent;

        public virtual bool Interact() => parent.Interact();
        public float InteractionRange() => parent.InteractionRange();
    }
}
