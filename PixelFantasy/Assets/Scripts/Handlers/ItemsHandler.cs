using Items;
using Managers;
using UnityEngine;

namespace Handlers
{
    public class ItemsHandler : Singleton<ItemsHandler>
    {
        public Item CreateItemObject(ItemData data, Vector2 pos, bool createHaulTask)
        {
            var prefab = Resources.Load<Items.Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data, createHaulTask);
            data.LinkedItem = itemObj;
            return itemObj;
        }
    }
}
