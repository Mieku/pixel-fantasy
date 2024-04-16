using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * This holds a list of ItemObjects, which will be used to populate the LootBox objects. Each LootItems object is meant
 * to hold potential items that could be spawned in a LootBox.
 */

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Loot Items", menuName = "Game Modules/Create/Loot Items", order = 1)]
    [Serializable]
    public class LootItems : ModulesScriptableObject, IHaveUid, IHaveDictionaries
    {
        public string description = ""; // Internal description to help users remember what the point of this was
        public List<ItemObject> itemObjects = new List<ItemObject>(); // List of the ItemObject objects in this LootItems

        public int AvailableItemsCount => itemObjects.Count; // Number of itemObjects in the list
        
        [HideInInspector] public bool hasBeenSetup; // True if this has been setup already
        [HideInInspector] public int menubarIndex;

        // Inspector/Editor
        [HideInInspector] public int typeIndex;
        [HideInInspector] public int itemIndex;
        [FormerlySerializedAs("availabletypes")] [HideInInspector] public string[] availableTypes;
        [HideInInspector] public string[] availableItems;
        [HideInInspector] public string[] availableItemNames;
        [HideInInspector] public bool displayDescription;
    }
}
