using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Add this script to your project, perhaps on your Game Controller or another object, even a new object, perhaps named
 * LootBoxes :)
 *
 * This script will hold a reference to all of your LootBox scriptable objects, since they can not be serialized, and must
 * be re-linked when the scene loads, or at another appropriate time.
 */

namespace InfinityPBR.Modules
{
    public class ItemDataObjects : MonoBehaviour
    {
        public static ItemDataObjects itemDataObjects; // Static reference to this script
        
        [Header("Required")]
        public List<ItemObject> itemObjects = new List<ItemObject>(); // A list of all your Item scriptable Ojbects
        public List<ItemAttribute> itemAttributes = new List<ItemAttribute>(); // A list of all your ItemAttribute scriptable Ojbects

        /// <summary>
        /// GetLootBox, given a uid of a loot box, will return that from the List<LootBox> boxes if the uid is found, or
        /// will return null if not found. Remember to populate the list!
        /// </summary>
        /// <param name="lootBoxUid"></param>
        /// <returns></returns>
        public ItemObject GetItemObject(string itemObjectUid) => itemObjects.FirstOrDefault(itemObject => itemObject.Uid().Equals(itemObjectUid));
        public ItemAttribute GetItemAttribute(string itemAttributeUid) => itemAttributes.FirstOrDefault(itemAttribute => itemAttribute.Uid().Equals(itemAttributeUid));
        
        private void Awake()
        {
            if (!itemDataObjects)
                itemDataObjects = this;
            else if (itemDataObjects != this)
                Destroy(gameObject);
        }
    }
}
