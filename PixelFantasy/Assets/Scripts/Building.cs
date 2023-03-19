using System;
using System.Collections;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using Items;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;

public class Building : Interactable
{
    protected List<ItemAmount> _remainingResourceCosts = new List<ItemAmount>();
    protected List<ItemAmount> _pendingResourceCosts = new List<ItemAmount>();
    
    private Material _material;
    private int _fadePropertyID;
    
    [SerializeField] private Transform _exteriorRoot;

    public Interior Interior => _buildingData.Interior;

    private BuildingExterior _exterior;
    private bool _isPlanning;
    private BuildingData _buildingData;
    protected float _remainingWork;

    public List<string> InvalidPlacementTags => _buildingData.InvalidPlacementTags;
    
    private void Start()
    {
        //_material = GetComponent<SpriteRenderer>().material;
        //_fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
    }

    public void Plan(BuildingData buildingData)
    {
        _buildingData = buildingData;
        _remainingWork = _buildingData.WorkCost;
        // Follows cursor
        _isPlanning = true;
        _exterior = Instantiate(_buildingData.Exterior, _exteriorRoot).GetComponent<BuildingExterior>();
        _exterior.Init(this);
    }

    public void PrepareToBuild()
    {
        // Stop Following cursor, set build task
        _isPlanning = false;
        _exterior.SetBlueprint();
        _remainingResourceCosts = new List<ItemAmount> (_buildingData.GetResourceCosts());
        CreateConstructionHaulingTasks();
    }
    
    private void CreateConstructionHaulingTasks()
    {
        var resourceCosts = _buildingData.GetResourceCosts();
        CreateConstuctionHaulingTasksForItems(resourceCosts);
    }
    
    public void CreateConstuctionHaulingTasksForItems(List<ItemAmount> remainingResources)
    {
        foreach (var resourceCost in remainingResources)
        {
            for (int i = 0; i < resourceCost.Quantity; i++)
            {
                EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
            }
        }
    }

    protected void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
    {
        Task task = new Task
        {
            TaskId = "Withdraw Item",
            Category = TaskCategory.Hauling,
            Requestor = this,
            Payload = resourceData.ItemName,
        };
        TaskManager.Instance.AddTask(task);
    }
    
    public override void ReceiveItem(Item item)
    {
        var itemData = item.GetItemData();
        Destroy(item.gameObject);
        RemoveFromPendingResourceCosts(itemData);
            
        foreach (var cost in _remainingResourceCosts)
        {
            if (cost.Item == itemData && cost.Quantity > 0)
            {
                cost.Quantity--;
                if (cost.Quantity <= 0)
                {
                    _remainingResourceCosts.Remove(cost);
                }

                break;
            }
        }
            
        if (_remainingResourceCosts.Count == 0)
        {
            CreateConstructTask();
        }
    }
    
    public void CreateConstructTask()
    {
        Task constuctTask = new Task()
        {
            Category = TaskCategory.Construction,
            TaskId = "Build Building",
            Requestor = this,
        };
        constuctTask.Enqueue();
    }
    
    public bool DoConstruction(float workAmount)
    {
        _remainingWork -= workAmount;
        if (_remainingWork <= 0)
        {
            CompleteConstruction();
            return true;
        }
            
        return false;
    }

    private void CompleteConstruction()
    {
        _remainingWork = _buildingData.WorkCost;
        _exterior.ColourArt(BuildingExterior.ColourStates.Built);
        InteriorsManager.Instance.GenerateInterior(this);
    }
    
    public void RemoveFromPendingResourceCosts(ItemData itemData, int quantity = 1)
    {
        foreach (var cost in _pendingResourceCosts)
        {
            if (cost.Item == itemData)
            {
                cost.Quantity -= quantity;
                if (cost.Quantity <= 0)
                {
                    _pendingResourceCosts.Remove(cost);
                }

                return;
            }
        }
    }

    private void Update()
    {
        FollowCursor();
        CheckPlacement();
    }

    private void FollowCursor()
    {
        if(!_isPlanning) return;
        
        var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
        gameObject.transform.position = cursorPos;
    }

    public bool CheckPlacement()
    {
        if(!_isPlanning) return false;

        if (_exterior.CheckPlacement())
        {
            _exterior.ColourArt(BuildingExterior.ColourStates.CanPlace);
            return true;
        }
        else
        {
            _exterior.ColourArt(BuildingExterior.ColourStates.CantPlace);
            return false;
        }
    }

    public void LinkEntrance(Transform interiorEntrancePos)
    {
        _exterior.EntranceLink.endTransform = interiorEntrancePos;
    }

    private void TriggerOutline(bool showOuline)
    {
        if (showOuline)
        {
            _material.SetFloat(_fadePropertyID, 1);
        }
        else
        {
            _material.SetFloat(_fadePropertyID, 0);
        }
    }

    private void OnMouseEnter()
    {
        TriggerOutline(true);
    }

    private void OnMouseExit()
    {
        TriggerOutline(false);
    }

    private void OnMouseDown()
    {
        
    }
}
