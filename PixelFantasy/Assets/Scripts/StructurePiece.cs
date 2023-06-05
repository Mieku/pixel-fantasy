using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using CodeMonkey.Utils;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TaskSystem;
using UnityEngine;
using UnityEngine.AI;

public class StructurePiece : Construction
{
    [SerializeField] private Color _blueprintColour;
    [SerializeField] private Color _canPlaceColour;
    [SerializeField] private Color _cantPlaceColour;

    public bool IsLeftAligned;

    private GameObject _wall;
    private WallData.Neighbours _priorNeighbours;
    private WallData _wallData;
    private bool _isPlanning;
    private List<string> _invalidPlacementTags => _wallData.InvalidPlacementTags;

    [Button("Refresh Wall")]
    private void TriggerRefresh()
    {
        Refresh(true, true, true, true);
    }

    [Button("Clear Data")]
    private void ClearData()
    {
        _priorNeighbours = null;
    }

    [Button("Clear Prefab")]
    private void ClearPrefab()
    {
        if (_wall != null)
        {
            DestroyImmediate(_wall);
        }
    }

    public void Init(WallData wallData)
    {
        _wallData = wallData;
        _isBuilt = false;
        _remainingResourceCosts = new List<ItemAmount> (_wallData.GetResourceCosts());
        _pendingResourceCosts = new List<ItemAmount>();
        Refresh(true, true, true, true);
        PrepForConstruction();
    }
    
    private void PrepForConstruction()
    {
        // TODO: Clear anything below this first
        
        CreateConstructionHaulingTasks();
    }
    
    private void CreateConstructionHaulingTasks()
    {
        var resourceCosts = _wallData.GetResourceCosts();
        CreateConstuctionHaulingTasksForItems(resourceCosts);
    }
    
    protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
    {
        Task task = new Task
        {
            TaskId = "Withdraw Item",
            Requestor = this,
            Payload = resourceData.ItemName,
            Profession = _wallData.CraftersProfession,
        };
        TaskManager.Instance.AddTask(task);
    }
    
    public override void CreateConstructTask(bool autoAssign = true)
    {
        Task constuctTask = new Task()
        {
            TaskId = "Build Construction",
            Requestor = this,
            Profession = _wallData.CraftersProfession,
        };
        constuctTask.Enqueue();
    }
    
    public override void CompleteConstruction()
    {
        base.CompleteConstruction();
        ColourWall(Color.white);
        _isBuilt = true;
        EnableObstacle(true);
    }

    private NavMeshObstacle _obstacle;
    private void EnableObstacle(bool isEnabled)
    {
        if (_obstacle == null)
        {
            _obstacle = _wall.GetComponent<NavMeshObstacle>();
        }

        if (_obstacle != null)
        {
            _obstacle.enabled = isEnabled;
        }
    }

    private void Update()
    {
        if (!_isBuilt)
        {
            ColourWall(_blueprintColour);
        }
        else // Is Built
        {
            EnableObstacle(true);
        }
    }

    private void ColourWall(Color color)
    {
        var wallRenderer = _wall.GetComponent<SpriteRenderer>();
        if (wallRenderer != null)
        {
            wallRenderer.color = color;
        }
    }
 
    public void Refresh(bool informUp, bool informRight, bool informDown, bool informLeft)
    {
        if (_wall != null)
        {
            DestroyImmediate(_wall);
        }
        
        var neighbours = FindNeighbours();
        var wall = _wallData.GetWall(neighbours);
        IsLeftAligned = wall.IsLeftAligned;
        _wall = Instantiate(wall.Prefab, transform);
        
        if (neighbours.IsEqualTo(_priorNeighbours)) return;
        _priorNeighbours = neighbours;

        if (neighbours.TopPiece != null && informUp)
        {
            neighbours.TopPiece.Refresh(true, true, false, true);
        }
        
        if (neighbours.RightPiece != null && informRight)
        {
            neighbours.RightPiece.Refresh(true, true, true, false);
        }
        
        if (neighbours.BottomPiece != null && informDown)
        {
            neighbours.BottomPiece.Refresh(false, true, true, true);
        }
        
        if (neighbours.LeftPiece != null && informLeft)
        {
            neighbours.LeftPiece.Refresh(true, false, true, true);
        }
    }

    private WallData.Neighbours FindNeighbours()
    {
        Vector2 leftPos = new Vector2(transform.position.x - 1, transform.position.y);
        Vector2 rightPos = new Vector2(transform.position.x + 1, transform.position.y);
        Vector2 upPos = new Vector2(transform.position.x, transform.position.y + 1);
        Vector2 downPos = new Vector2(transform.position.x, transform.position.y - 1);

        StructurePiece leftPiece = null;
        var left = Helper.GetGameObjectsOnTile(leftPos, "Structure");
        if (left != null && left.Count > 0)
        {
            leftPiece = left[0].GetComponent<StructurePiece>();
        }
        
        StructurePiece rightPiece = null;
        var right = Helper.GetGameObjectsOnTile(rightPos, "Structure");
        if (right != null && right.Count > 0)
        {
            rightPiece = right[0].GetComponent<StructurePiece>();
        }
        
        StructurePiece upPiece = null;
        var up = Helper.GetGameObjectsOnTile(upPos, "Structure");
        if (up != null && up.Count > 0)
        {
            upPiece = up[0].GetComponent<StructurePiece>();
        }
        
        StructurePiece downPiece = null;
        var down = Helper.GetGameObjectsOnTile(downPos, "Structure");
        if (down != null && down.Count > 0)
        {
            downPiece = down[0].GetComponent<StructurePiece>();
        }

        WallData.Neighbours results = new WallData.Neighbours
        {
            LeftPiece = leftPiece,
            RightPiece = rightPiece,
            TopPiece = upPiece,
            BottomPiece = downPiece,
        };

        return results;
    }
}
