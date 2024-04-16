using UnityEngine;

namespace InfinityPBR.Modules.Inventory
{
    public class ItemWorldInteractable : ItemWorld, IAmInteractable
    {
        public virtual bool Interact()
        {
            Debug.LogWarning("You're calling Interact on a ItemWorldInteractable object. You should create a " +
                             "new class which inherits from ItemWorldInteractable, and then override the Interact() " +
                             "method, to create custom logic for your project.");
            return false;
        }

        public virtual float InteractionRange() => 1.2f;
    }
}
