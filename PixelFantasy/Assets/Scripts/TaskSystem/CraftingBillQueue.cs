using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Characters;
using Items;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class CraftingBillQueue
    {
        [SerializeField] private List<CraftingBill> _bills = new List<CraftingBill>();

        public void Add(CraftingBill bill)
        {
            _bills.Add(bill);
        }

        public void Cancel(CraftingBill billToCancel)
        {
            CraftingBill target = null;
            foreach (var bill in _bills)
            {
                if (bill.Equals(billToCancel))
                {
                    target = bill;
                    break;
                }
            }

            if (target != null)
            {
                target.OnCancelled.Invoke();
                _bills.Remove(target);
            }
            else
            {
                Debug.LogError($"Error trying to cancel bill: {billToCancel}");
            }
        }

        public CraftingBill GetNextCraftingBillByBuilding(ProductionBuilding building)
        {
            for (int i = 0; i < _bills.Count; i++)
            {
                var potentialBill = _bills[i];
                if (potentialBill.HasCorrectCraftingTable(building) && potentialBill.IsPossible())
                {
                    _bills.RemoveAt(i);
                    return potentialBill;
                }
            }

            return null;
        }

        public CraftingBill GetNextBillByCraftingTable(CraftingTable craftingTable)
        {
            for (int i = 0; i < _bills.Count; i++)
            {
                var potentialBill = _bills[i];
                if (potentialBill.ItemToCraft.RequiredCraftingTable == craftingTable.FurnitureItemData && potentialBill.IsPossible())
                {
                    _bills.RemoveAt(i);
                    return potentialBill;
                }
            }

            return null;
        }
    }
}