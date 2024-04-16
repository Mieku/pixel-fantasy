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

namespace InfinityPBR.Modules.Loot
{
    public class LootBoxes : MonoBehaviour
    {
        public static LootBoxes lootBoxes; // Static reference to this script
        
        [Header("Required")]
        public List<LootBox> boxes = new List<LootBox>(); // A list of all your LootBox scriptable Ojbects
        
        /// <summary>
        /// GetLootBox, given a uid of a loot box, will return that from the List<LootBox> boxes if the uid is found, or
        /// will return null if not found. Remember to populate the list!
        /// </summary>
        /// <param name="lootBoxUid"></param>
        /// <returns></returns>
        public LootBox GetLootBox(string lootBoxUid) => boxes.FirstOrDefault(box => box.Uid().Equals(lootBoxUid));
        
        private void Awake()
        {
            if (!lootBoxes)
                lootBoxes = this;
            else if (lootBoxes != this)
                Destroy(gameObject);

            CheckLootBoxesContent();
        }

        /// <summary>
        /// This will check to ensure there is at least one LootBox in the List<>, or it will give a Debug Warning, reminding
        /// you to add your Loot Box scriptable objects.
        /// </summary>
        private void CheckLootBoxesContent()
        {
            if (boxes.Count > 0) return;
            
            Debug.LogWarning("Warning: There are no Loot Box scriptable objects in this list. Don't forget to add all" +
                      "your Loot Box scriptable objects to this list.");
        }
    }
}
