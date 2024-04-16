using System.Collections.Generic;
using InfinityPBR.Modules;

namespace InfinityPBR
{
    public interface IHandleLootBoxes
    {
        IEnumerable<GameItemObject> GenerateItems(LootBoxItemsSettings itemsSetting);
        float SpawnChance(int itemToSpawnIndex, LootBoxItemsSettings itemsSetting);
        GameItemAttributeList GenerateAttributes(LootBoxItemsSettings itemsSetting, ItemObject newItemObject, LootBoxItemToSpawn itemToSpawn);
    }
}