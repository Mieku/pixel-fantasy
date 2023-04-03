using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Characters
{
    public class UnitInventory : MonoBehaviour
    {
        [SerializeField] private ItemSlot _held;
        [SerializeField] private ItemSlot _pocket;
        [SerializeField] private List<ItemSlot> _debugImmediateDrop = new List<ItemSlot>();

        private Family Family => FamilyManager.Instance.GetFamily(GetComponent<UnitState>());

        private void Start()
        {
            CheckImmediateDrop();
        }

        private void CheckImmediateDrop()
        {
            if (_debugImmediateDrop.Count > 0)
            {
                List<ItemData> itemsToDrop = new List<ItemData>();
                foreach (var dropSlot in _debugImmediateDrop)
                {
                    itemsToDrop.AddRange(dropSlot.GetEntriesAsItemDatas());
                }

                itemsToDrop = CreateDropCrate(itemsToDrop);
                
                // while (itemsToDrop.Count > 0)
                // {
                //     // Check the ground, if can place a crate, make it
                //     if (!Helper.DoesGridContainTag(transform.position, "Storage"))
                //     {
                //         itemsToDrop = CreateDropCrate(itemsToDrop);
                //     }
                // }
            }
        }

        private List<ItemData> CreateDropCrate(List<ItemData> itemsForCrate)
        {
            var crateData = Librarian.Instance.GetItemData("Crate") as StorageItemData;
            var crate = Spawner.Instance.SpawnStorageContainer(crateData, transform.position, Family);
            var remainingItems = crate.AddItems(itemsForCrate);
            return remainingItems;
        }
    }
}
