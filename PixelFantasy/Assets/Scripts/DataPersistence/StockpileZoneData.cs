using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Newtonsoft.Json;
using ScriptableObjects;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StockpileZoneData : ZoneData, IStorage
{
    public string UniqueID { get; set; }
    public string StockpileSettingsID;

    public List<StorageCellData> StorageCellDatas = new List<StorageCellData>();
    

    [JsonIgnore] public StockpileZoneSettings StockpileSettings => (StockpileZoneSettings) GameSettings.Instance.LoadZoneSettings(StockpileSettingsID);
        
    [JsonIgnore] public List<StockpileCell> StockpileCells => ZoneCells.Cast<StockpileCell>().ToList();
        
    // Specific storage player settings chosen for area 
    [JsonRequired] private StorageConfigs _storageConfigs;

    [JsonIgnore] public StorageConfigs StorageConfigs => _storageConfigs;


    public void InitData(StockpileZoneSettings settings)
    {
        StockpileSettingsID = settings.name;
        UniqueID = CreateUID();
        _storageConfigs = new StorageConfigs();
        _storageConfigs.PasteConfigs(settings.DefaultConfigs);
        IsEnabled = true;

        ZoneName = $"Stockpile {AssignedLayer}";
    }

    public void CopyData(StockpileZoneData dataToCopy)
    {
        StockpileSettingsID = dataToCopy.StockpileSettingsID;
        _storageConfigs = new StorageConfigs();
        _storageConfigs.PasteConfigs(dataToCopy.StorageConfigs);
        IsEnabled = dataToCopy.IsEnabled;
            
        ZoneName = $"Stockpile {AssignedLayer}";
    }

    public void PrepSave()
    {
        StorageCellDatas = CollectStockpileZoneDatas();
    }

    public void RestoreDefaultStockpileSettings()
    {
        _storageConfigs.PasteConfigs(StockpileSettings.DefaultConfigs);
    }
        
    [JsonIgnore] public override Color ZoneColour => Settings.ZoneColour;
    [JsonIgnore] public override TileBase DefaultTiles => Settings.DefaultTiles;
    [JsonIgnore] public override TileBase SelectedTiles => Settings.SelectedTiles;
    [JsonIgnore] public override ZoneSettings.EZoneType ZoneType => ZoneSettings.EZoneType.Stockpile;
    [JsonIgnore] public override ZoneSettings Settings => StockpileSettings;

    [JsonIgnore] 
    public List<ItemData> Stored
    {
        get
        {
            List<ItemData> results = new List<ItemData>();
            foreach (var stockpileCell in StockpileCells)
            {
                results.AddRange(stockpileCell.Stored);
            }
            return results;
        }
    }

    [JsonIgnore] 
    public List<ItemData> Incoming
    {
        get
        {
            List<ItemData> results = new List<ItemData>();
            foreach (var stockpileCell in StockpileCells)
            {
                results.AddRange(stockpileCell.Incoming);
            }
            return results;
        }
    }
        
    [JsonIgnore] 
    public List<ItemData> Claimed
    {
        get
        {
            List<ItemData> results = new List<ItemData>();
            foreach (var stockpileCell in StockpileCells)
            {
                results.AddRange(stockpileCell.Claimed);
            }
            return results;
        }
    }

    public List<StorageCellData> CollectStockpileZoneDatas()
    {
        List<StorageCellData> results = new List<StorageCellData>();
        foreach (var stockpileCell in StockpileCells)
        {
            if (stockpileCell.Stored.Count > 0)
            {
                var cellData = new StorageCellData();
                cellData.Cell = stockpileCell.CellPos;

                List<string> itemsList = new List<string>();
                foreach (var itemData in stockpileCell.Stored)
                {
                    itemsList.Add(itemData.UniqueID);
                }

                cellData.StoredItemsData = itemsList;
                results.Add(cellData);
            }
        }

        return results;
    }
        
    public List<InventoryAmount> GetInventoryAmounts()
    {
        List<InventoryAmount> results = new List<InventoryAmount>();

        foreach (var storedItem in Stored)
        {
            var recorded = results.Find(i => i.ItemSettings == storedItem.Settings);
            if (recorded == null)
            {
                recorded = new InventoryAmount(storedItem.Settings);
                results.Add(recorded);
            }

            recorded.AddStored(storedItem);
        }

        foreach (var claimedItem in Claimed)
        {
            var recorded = results.Find(i => i.ItemSettings == claimedItem.Settings);
            if (recorded == null)
            {
                recorded = new InventoryAmount(claimedItem.Settings);
                results.Add(recorded);
            }
                
            recorded.AddClaimed(claimedItem);
        }
            
        foreach (var incomingItem in Incoming)
        {
            var recorded = results.Find(i => i.ItemSettings == incomingItem.Settings);
            if (recorded == null)
            {
                recorded = new InventoryAmount(incomingItem.Settings);
                results.Add(recorded);
            }
                
            recorded.AddIncoming(incomingItem);
        }

        return results;
    }
        
    public void SetIncoming(ItemData itemData)
    {
        if (!StorageConfigs.IsItemValidToStore(itemData))
        {
            Debug.LogError("Attempting to store the wrong item category");
            return;
        }

        var availableSpace = AmountCanBeDeposited(itemData.Settings);
        if (availableSpace <= 0)
        {
            Debug.LogError("Attempted to set incoming with no space available");
            return;
        }
            
        // Find the cell that contains the item
        StockpileCell cell = GetAvailableCellForItem(itemData.Settings);
        cell.Incoming.Add(itemData);
            
        GameEvents.Trigger_RefreshInventoryDisplay();
    }

    public void CancelIncoming(ItemData itemData)
    {
        if (IsSpecificItemDataIncoming(itemData))
        {
            Incoming.Remove(itemData);
        }
            
        GameEvents.Trigger_RefreshInventoryDisplay();
    }

    public void DepositItems(Items.Item item)
    {
        var runtimeData = item.RuntimeData;
            
        if (!IsSpecificItemDataIncoming(runtimeData))
        {
            Debug.LogError("Tried to deposit an item that was not set as incoming");
            return;
        }

        runtimeData.AssignedStorageID = UniqueID;
        runtimeData.State = EItemState.Stored;

        var cell = GetAssignedCellForSpecificItem(runtimeData);
        cell.DepositItem(runtimeData);
            
        //Destroy(item.gameObject);
        OnZoneChanged?.Invoke();
    }

    public void LoadInItem(ItemData itemData)
    {
        var cellData = StorageCellDatas.Find(s => s.StoredItemsData.Contains(itemData.UniqueID));
        var cell = StockpileCells.Find(c => c.CellPos == cellData.Cell);
        cell.LoadInItemData(itemData);
    }

    public Item WithdrawItem(ItemData itemData)
    {
        itemData.State = EItemState.Carried;
        var cell = GetAssignedCellForSpecificItem(itemData);
        var item = cell.WithdrawItem(itemData);
        OnZoneChanged?.Invoke();
        return item;
    }

    public bool ClaimItem(ItemData itemToClaim)
    {
        var cell = GetAssignedCellForSpecificItem(itemToClaim);
        if (!cell.Stored.Contains(itemToClaim))
        {
            Debug.LogError($"Could not find {itemToClaim.ItemName} in storage");
            return false;
        }

        if (cell.Claimed.Contains(itemToClaim))
        {
            Debug.LogError($"Attempted to Claim {itemToClaim.ItemName}, but it was already claimed");
            return false;
        }
            
        cell.Claimed.Add(itemToClaim);
        GameEvents.Trigger_RefreshInventoryDisplay();
        return true;
    }

    public void RestoreClaimed(ItemData itemData)
    {
        var cell = GetAssignedCellForSpecificItem(itemData);
        if (cell != null)
        {
            if (!cell.Claimed.Contains(itemData))
            {
                Debug.LogError($"Item Claim: {itemData.ItemName} was not restored, was not found in claimed");
                return;
            }

            cell.Claimed.Remove(itemData);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
    }

    public Vector2? AccessPosition(Vector2 requestorPosition, ItemData specificItem)
    {
        var cell = GetAssignedCellForSpecificItem(specificItem);

        if (cell == null)
        {
            Debug.LogError($"{specificItem} is not stored or incoming in any cells");
            return null;
        }
            
        return cell.Position;
    }

    [JsonIgnore] public bool IsAvailable => IsEnabled;
    public bool IsItemInStorage(ItemSettings itemSettings)
    {
        foreach (var cell in StockpileCells)
        {
            if (cell.StoredItemSettings == itemSettings && cell.Claimed.Count < cell.Stored.Count)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsSpecificItemInStorage(ItemData specificItem)
    {
        var cell = GetAssignedCellForSpecificItem(specificItem);
        if (cell == null) return false;

        return true;
    }

    public ItemData GetItemDataOfType(ItemSettings itemSettings)
    {
        int amountClaimable = AmountCanBeWithdrawn(itemSettings);
        if (amountClaimable <= 0) return null;

        foreach (var cell in StockpileCells)
        {
            if (cell.StoredItemSettings == itemSettings)
            {
                ItemData item = cell.GetUnclaimedItem(itemSettings);
                if (item != null)
                {
                    return item;
                }
            }
        }
            
        return null;
    }

    public int AmountCanBeWithdrawn(ItemSettings itemSettings)
    {
        int result = 0;
        foreach (var cell in StockpileCells)
        {
            result += cell.AmountCanBeWithdrawn(itemSettings);
        }

        return result;
    }

    public int AmountCanBeDeposited(ItemSettings itemSettings)
    {
        if (!StorageConfigs.IsItemTypeAllowed(itemSettings)) return 0;

        int result = 0;
        foreach (var cell in StockpileCells)
        {
            result += cell.AmountCanBeDeposited(itemSettings);
        }

        return result;
    }

    public List<ToolData> GetAllToolItems(bool includeIncoming = false)
    {
        throw new System.NotImplementedException(); // Going to get rid of this.
    }
        
    private bool IsSpecificItemDataIncoming(ItemData itemData)
    {
        foreach (var incoming in Incoming)
        {
            if (incoming == itemData)
            {
                return true;
            }
        }

        return false;
    }

    private StockpileCell GetAvailableCellForItem(ItemSettings itemSettings)
    {
        // Check if any cells can accept more
        foreach (var cell in StockpileCells)
        {
            if (cell.StoredItemSettings == itemSettings && cell.SpaceAvailable)
            {
                return cell;
            }
        }

        foreach (var cell in StockpileCells)
        {
            if (cell.IsEmpty)
            {
                return cell;
            }
        }

        return null;
    }

    private StockpileCell GetAssignedCellForSpecificItem(ItemData itemData)
    {
        foreach (var cell in StockpileCells)
        {
            if (cell.Stored.Contains(itemData) || cell.Incoming.Contains(itemData))
            {
                return cell;
            }
        }
            
        return null;
    }

    [JsonIgnore] public int MaxCapacity => StockpileCells.Count;

    [JsonIgnore] 
    public int TotalAmountStored
    {
        get
        {
            int result = 0;
            foreach (var cell in StockpileCells)
            {
                if (!cell.IsEmpty)
                {
                    result++;
                }
            }

            return result;
        }
    }

    public bool IsCategoryAllowed(EItemCategory category)
    {
        return StockpileSettings.AcceptedCategories.Contains(category);
    }
        
    protected string CreateUID()
    {
        return $"StockpileZone_{Guid.NewGuid()}";
    }
}

public class StorageCellData
{
    [JsonRequired] private int _cellX;
    [JsonRequired] private int _cellY;
    [JsonRequired] public List<string> StoredItemsData = new List<string>();

    [JsonIgnore]
    public Vector3Int Cell
    {
        get => new(_cellX, _cellY);
        set
        {
            _cellX = value.x;
            _cellY = value.y;
        }
    }
}