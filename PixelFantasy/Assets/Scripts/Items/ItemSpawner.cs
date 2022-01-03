using System.Collections;
using System.Collections.Generic;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

public class ItemSpawner : God<ItemSpawner>
{
    [SerializeField] private Transform _itemsParent;
    [SerializeField] private GameObject _itemPrefab;
    
    [SerializeField] private Transform _structureParent;
    [SerializeField] private GameObject _structurePrefab;

    public void SpawnItem(ItemData itemData, Vector2 spawnPosition, bool canBeHauled)
    {
        var item = Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
        item.transform.SetParent(_itemsParent);
        var itemScript = item.GetComponent<Item>();
        itemScript.InitializeItem(itemData, canBeHauled);
    }

    public void SpawnStructure(StructureData structureData, Vector2 spawnPosition)
    {
        var structureObj = Instantiate(_structurePrefab, spawnPosition, Quaternion.identity);
        structureObj.transform.SetParent(_structureParent);
        var structure = structureObj.GetComponent<Wall>();
        structure.Init(structureData);
    }
}
