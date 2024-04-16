using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Item Object Reward...", menuName = "Game Modules/Quest Reward/Item Object Reward", order = 1)]
    [Serializable]
    public class ItemObjectReward : QuestReward
    {
        public List<LootBox> lootBoxes = new List<LootBox>();

        public override void GiveReward(IUseGameModules owner)
        {
            Debug.Log("Item Objet Reward: GiveReward()");
            var generatedItems = new GameItemObjectList();
            foreach (var lootBox in lootBoxes)
                generatedItems.list.AddRange(lootBox.GenerateLoot().list);
            
            Debug.Log($"There are {generatedItems.Count()} Generated Items in this reward.");
            
            // Pass the items to the Handler on the quest Repository -- this is something you'll need to create for
            // your game!!
            ModulesHelper.Instance.HandleItemObjectReward(generatedItems);
        }

        public void AddLootBox(LootBox value) => lootBoxes.Add(value);
        public void RemoveLootBox(int index) => lootBoxes.RemoveAt(index);
    }
}