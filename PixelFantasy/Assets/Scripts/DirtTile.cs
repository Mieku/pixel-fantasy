using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Gods;
using Items;
using Pathfinding;
using Tasks;
using Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DirtTile : MonoBehaviour
{
    public bool IsBuilt = false;
    
    [SerializeField] private int _workCost;
    [SerializeField] private List<string> _invalidPlacementTags;
    [SerializeField] private List<Order> _options;
    [SerializeField] private Sprite _icon;
    [SerializeField] private GraphUpdateScene _graphUpdateScene;
    [SerializeField] private RuleTile _dirtRuleTile;

    private TaskMaster taskMaster => TaskMaster.Instance;
    private UnitTaskAI _incomingUnit;
    private List<int> _assignedTaskRefs = new List<int>();
    private Structure _requestedStructure;
    private Floor _requestedFloor;
    private Tilemap _flooringTilemap;
    private Action _onDirtDug;

    public Sprite Icon => _icon;

    public List<string> InvalidPlacementTags
    {
        get
        {
            List<string> clone = new List<string>();
            foreach (var invalidPlacementTag in _invalidPlacementTags)
            {
                clone.Add(invalidPlacementTag);
            }

            return clone;
        }
    }
    public int WorkCost => _workCost;

    public List<Order> Options
    {
        get
        {
            List<Order> clone = new List<Order>();
            foreach (var option in _options)
            {
                clone.Add(option);
            }

            return clone;
        }
    }

    private void Awake()
    {
        _flooringTilemap =
            TilemapController.Instance.GetTilemap(TilemapLayer.Ground);
    }

    private Tilemap FindTilemapByName(string name)
    {
        var maps = FindObjectsOfType<Tilemap>(true);
        foreach (var map in maps)
        {
            if (map.gameObject.name == name)
            {
                return map;
            }
        }

        return null;
    } 

    public void CancelTasks()
    {
        if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

        foreach (var taskRef in _assignedTaskRefs)
        {
            taskMaster.FellingTaskSystem.CancelTask(taskRef);
            taskMaster.FarmingTaskSystem.CancelTask(taskRef);
        }
        _assignedTaskRefs.Clear();
            
        if (_incomingUnit != null)
        {
            _incomingUnit.CancelTask();
        }
    }
    
    public void Init(Structure requestedStructure = null)
    {
        _requestedStructure = requestedStructure;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
    }

    public void Init(Action onDirtDug)
    {
        _onDirtDug = onDirtDug;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
    }
    
    public void Init(Floor requestedFloor)
    {
        _requestedFloor = requestedFloor;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
    }

    public void ClearPlantsForClearingGrass()
    {
        // Check if there are any plants on the tile, if so, cut them down first
        if (Helper.DoesGridContainTag(transform.position, "Nature"))
        {
            ClearNatureFromTile();
            return;
        }
        
        // if clear, clear the grass
        CreateClearGrassTask();
    }

    private void ClearNatureFromTile()
    {
        var objectsOnTile = Helper.GetGameObjectsOnTile(transform.position);
        foreach (var tileObj in objectsOnTile)
        {
            var growResource = tileObj.GetComponent<GrowingResource>();
            if (growResource != null)
            {
                growResource.TaskRequestors.Add(gameObject);

                if (!growResource.QueuedToCut)
                {
                    growResource.CreateCutPlantTask();
                }
            }
        }
    }

    private void ShowBlueprint(bool showBlueprint)
    {
        if (showBlueprint)
        {
            ColourRenderers(Librarian.Instance.GetColour("Blueprint"));
            _graphUpdateScene.enabled = false;
        }
        else
        {
            ColourRenderers(Color.white);
            _graphUpdateScene.enabled = true;
        }
    }

    private void ColourRenderers(Color colour)
    {
        var cell = _flooringTilemap.WorldToCell(transform.position);
        _flooringTilemap.SetColor(cell, colour);
    }

    public void CreateClearGrassTask()
    {
        var task = new FarmingTask.ClearGrass()
        {
            claimDirt = (UnitTaskAI unitTaskAI) =>
            {
                _incomingUnit = unitTaskAI;
            },
            grassPosition = Helper.ConvertMousePosToGridPos(transform.position),
            workAmount = _workCost,
            completeWork = BuiltDirt
        };
        
        _assignedTaskRefs.Add(task.GetHashCode());
        taskMaster.FarmingTaskSystem.AddTask(task);
    }

    public void CancelClearGrass()
    {
        if (IsBuilt) return;
        
        CancelTasks();
        Destroy(gameObject);
    }

    private void BuiltDirt()
    {
        ShowBlueprint(false);
        _incomingUnit = null;
        IsBuilt = true;

        if (_requestedStructure != null)
        {
            _requestedStructure.InformDirtReady();
        }

        if (_requestedFloor != null)
        {
            _requestedFloor.InformDirtReady();
        }

        if (_onDirtDug != null)
        {
            _onDirtDug.Invoke();
        }
    }
    
    public void UpdateSprite(bool informNeighbours)
    {
        var cell = _flooringTilemap.WorldToCell(transform.position);
        _flooringTilemap.SetTile(cell, _dirtRuleTile);
    }
} 
