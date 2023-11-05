using System.Collections.Generic;
using Controllers;
using Items;
using DataPersistence;
using Managers;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;
using Action = System.Action;

public class DirtTile : PlayerInteractable, IPersistent
{
    public bool IsBuilt = false;
    
    [SerializeField] private int _workCost;
    [SerializeField] private Sprite _placementSprite;
    [SerializeField] private List<string> _invalidPlacementTags;
    [SerializeField] private RuleTile _dirtRuleTile;
    
    private Structure _requestedStructure;
    private Floor _requestedFloor;
    private Tilemap _dirtTilemap;
    private Action _onDirtDug;
    protected float _remainingWork;

    public Sprite PlacementIcon => _placementSprite;

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

    public override int GetWorkAmount()
    {
        return _workCost;
    }
    
    public float WorkDone(float workAmount)
    {
        _remainingWork -= workAmount;
        return _remainingWork;
    }
    
    public virtual bool DoWork(float workAmount)
    {
        _remainingWork -= workAmount;
        if (_remainingWork <= 0)
        {
            BuiltDirt();
            return true;
        }
            
        return false;
    }
    
    private void Awake()
    {
        _dirtTilemap =
            TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
    }

    public void Init(Structure requestedStructure = null)
    {
        _requestedStructure = requestedStructure;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
        _remainingWork = GetWorkAmount();
    }

    public void Init(Action onDirtDug)
    {
        _onDirtDug = onDirtDug;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
        _remainingWork = GetWorkAmount();
    }
    
    public void Init(Floor requestedFloor)
    {
        _requestedFloor = requestedFloor;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
        _remainingWork = GetWorkAmount();
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

    private void CreateClearGrassTask()
    {
        Task task = new Task("Clear Grass", this);
        task.Enqueue();
    }

    private void ClearNatureFromTile()
    {
        var objectsOnTile = Helper.GetGameObjectsOnTile(transform.position);
        foreach (var tileObj in objectsOnTile)
        {
            var growResource = tileObj.GetComponent<GrowingResource>();
            if (growResource != null)
            {
                Debug.LogError("This still needs to be built");
                // growResource.TaskRequestors.Add(gameObject);
                //
                // // if (!growResource.QueuedToCut)
                // // {
                // //     growResource.CreateTaskById("Cut Plant");
                // // }
                // growResource.CreateTaskById("Cut Plant");
            }
        }
    }

    private void ShowBlueprint(bool showBlueprint)
    {
        if (showBlueprint)
        {
            ColourRenderers(Librarian.Instance.GetColour("Blueprint"));
        }
        else
        {
            ColourRenderers(Color.white);
        }
    }

    private void ColourRenderers(Color colour)
    {
        var cell = _dirtTilemap.WorldToCell(transform.position);
        _dirtTilemap.SetColor(cell, colour);
    }

    public void BuiltDirt()
    {
        ShowBlueprint(false);
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
        var cell = _dirtTilemap.WorldToCell(transform.position);
        _dirtTilemap.SetTile(cell, _dirtRuleTile);
    }

    public object CaptureState()
    {
        return new DirtData
        {
            UID = UniqueId,
            IsBuilt = IsBuilt,
            Position = transform.position,
            RemainingWork = _remainingWork,
        };
    }

    public void RestoreState(object data)
    {
        var state = (DirtData)data;
        UniqueId = state.UID;
        IsBuilt = state.IsBuilt;
        transform.position = state.Position;
        _remainingWork = state.RemainingWork;

        ShowBlueprint(!IsBuilt);
    }

    public struct DirtData
    {
        public string UID;
        public bool IsBuilt;
        public Vector2 Position;
        public float RemainingWork;
    }
} 
