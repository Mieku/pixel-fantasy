using System.Collections;
using System.Collections.Generic;
using DataPersistence;
using Handlers;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Settings", menuName = "Settings/Item Settings")]
public class ItemSettings : Settings
{
    // Settings
    [SerializeField] protected string _itemName;
    [SerializeField] protected Sprite _itemSprite;
    [SerializeField] protected EItemCategory _category;
    [SerializeField] protected int _maxDurability;
    [SerializeField] protected int _maxStackSize = 25;
    [SerializeField] protected bool _hasQuality = true;
    [SerializeField] protected EItemQuality _defaultQuality = EItemQuality.Common;
        
    public EItemCategory Category => _category;
    public string ItemName => _itemName;
    public Sprite ItemSprite => _itemSprite;
    public int MaxDurability => _maxDurability;
    public int MaxStackSize => _maxStackSize;

    public bool CanBeStored => _maxStackSize > 0;
        
    public EItemQuality DefaultQuality
    {
        get
        {
            if (_hasQuality)
            {
                return _defaultQuality;
            }

            return EItemQuality.Common;
        }
    }
        
    public virtual string GetDetailsMsg(string headerColourCode = "#272736")
    {
        string msg = "";
        msg += $"<color={headerColourCode}>Durability:</color> <b>{MaxDurability}</b>\n";
        return msg;
    }

    public virtual ItemData CreateItemData(Vector2 spawnPos)
    {
        ItemData data = new ItemData();
        data.InitData(this, spawnPos);
        ItemsDatabase.Instance.RegisterItem(data);
        return data;
    }
}
