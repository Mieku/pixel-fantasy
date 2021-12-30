using System.Collections;
using System.Collections.Generic;
using Gods;
using Items;
using UnityEngine;

public class ItemSpawner : God<ItemSpawner>
{
    [SerializeField] private Transform _itemsParent;
    [SerializeField] private GameObject _itemPrefab;

    public void SpawnItem(ItemData itemData, Vector2 spawnPosition, bool canBeHauled)
    {
        var item = Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
        item.transform.SetParent(_itemsParent);
        var itemScript = item.GetComponent<Item>();
        itemScript.InitializeItem(itemData, canBeHauled);
    }
}
