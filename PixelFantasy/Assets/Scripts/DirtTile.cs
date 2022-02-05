using System;
using System.Collections;
using System.Collections.Generic;
using Controllers;
using Gods;
using Interfaces;
using Items;
using Pathfinding;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

public class DirtTile : MonoBehaviour
{
    public bool IsBuilt = false;
    
    [SerializeField] private int _workCost;
    [SerializeField] private List<string> _invalidPlacementTags;
    [SerializeField] private List<Option> _options;
    [SerializeField] private Sprite _icon;
    [SerializeField] private GraphUpdateScene _graphUpdateScene;
    
    [SerializeField] private Sprite _centerSpr;
    [SerializeField] private Sprite _cornerTLSpr, _cornerTRSpr, _cornerBLSpr, _cornerBRSpr;
    [SerializeField] private Sprite _cornerInnerTLSpr, _cornerInnerTRSpr, _cornerInnerBLSpr, _cornerInnerBRSpr;
    [SerializeField] private Sprite _leftSpr, _rightSpr, _topSpr, _botSpr;
    
    [SerializeField] private SpriteRenderer _center, _topLeft, _topRight, _botLeft, _botRight;

    private List<GameObject> _neighbours = new List<GameObject>();
    private int _origLayer;
    private TaskMaster taskMaster => TaskMaster.Instance;
    private UnitTaskAI _incomingUnit;
    private List<int> _assignedTaskRefs = new List<int>();
    private Structure _requestedStructure;
    private Floor _requestedFloor;

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

    public List<Option> Options
    {
        get
        {
            List<Option> clone = new List<Option>();
            foreach (var option in _options)
            {
                clone.Add(option);
            }

            return clone;
        }
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
        _origLayer = _center.sortingOrder;
        UpdateSprite(true);
        ShowBlueprint(true);
        ClearPlantsForClearingGrass();
    }
    
    public void Init(Floor requestedFloor)
    {
        _requestedFloor = requestedFloor;
        _origLayer = _center.sortingOrder;
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
        _center.color = colour;
        _botLeft.color = colour;
        _botRight.color = colour;
        _topLeft.color = colour;
        _topRight.color = colour;
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
    }

    #region Sprite Displaying

    public void UpdateSprite(bool informNeighbours)
    {
        // collect data on connections
        var neighbourData = GetNeighbourData();
        _neighbours = neighbourData.Neighbours;
        var connectData = neighbourData.WallNeighbourConnectionInfo;

        UpdateTiles(connectData);

        // If inform neighbours, tell neighbours to UpdateSprite (but they shouldn't inform their neighbours
        if (informNeighbours)
        {
            RefreshNeighbours();
        }
    }
    
    private void RefreshNeighbours()
    {
        foreach (var neighbour in _neighbours)
        {
            var dirt = neighbour.GetComponent<DirtTile>();
            if (dirt != null)
            {
                dirt.UpdateSprite(false);
            }
        }
    }

    private NeighbourData GetNeighbourData()
    {
        NeighbourData neighbourData = new NeighbourData();
        var pos = transform.position;
            
        neighbourData.Neighbours.Clear();
        var connectionInfo = new WallNeighbourConnectionInfo();
        
        Vector2 topPos = new Vector2(pos.x, pos.y + 1);
        Vector2 botPos = new Vector2(pos.x, pos.y - 1);
        Vector2 leftPos = new Vector2(pos.x - 1, pos.y);
        Vector2 rightPos = new Vector2(pos.x + 1, pos.y);
        
        Vector2 topLeftPos = new Vector2(pos.x - 1, pos.y + 1);
        Vector2 topRightPos = new Vector2(pos.x + 1, pos.y + 1);
        Vector2 botLeftPos = new Vector2(pos.x - 1, pos.y - 1);
        Vector2 botRightPos = new Vector2(pos.x + 1, pos.y - 1);
        
        var allHitTop = Physics2D.RaycastAll(topPos, Vector2.down, 0.4f);
        var allHitBot = Physics2D.RaycastAll(botPos, Vector2.up, 0.4f);
        var allHitLeft = Physics2D.RaycastAll(leftPos, Vector2.right, 0.4f);
        var allHitRight = Physics2D.RaycastAll(rightPos, Vector2.left, 0.4f);
        
        var allHitTopLeft = Physics2D.RaycastAll(topLeftPos, Vector2.down + Vector2.right, 0.4f);
        var allHitTopRight = Physics2D.RaycastAll(topRightPos, Vector2.down + Vector2.left, 0.4f);
        var allHitBotLeft = Physics2D.RaycastAll(botLeftPos, Vector2.up + Vector2.right, 0.4f);
        var allHitBotRight = Physics2D.RaycastAll(botRightPos, Vector2.up + Vector2.left, 0.4f);

        // Top
        foreach (var hit in allHitTop)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.Top = true;
                break;
            }
        }
        // Bottom
        foreach (var hit in allHitBot)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.Bottom = true;
                break;
            }
        }
        // Left
        foreach (var hit in allHitLeft)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.Left = true;
                break;
            }
        }
        // Right
        foreach (var hit in allHitRight)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.Right = true;
                break;
            }
        }
        // tl
        foreach (var hit in allHitTopLeft)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.TopLeft = true;
                break;
            }
        }
        
        // tr
        foreach (var hit in allHitTopRight)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.TopRight = true;
                break;
            }
        }
        
        // bl
        foreach (var hit in allHitBotLeft)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.BottomLeft = true;
                break;
            }
        }
        
        // br
        foreach (var hit in allHitBotRight)
        {
            if (hit.transform.CompareTag("Dirt"))
            {
                neighbourData.Neighbours.Add(hit.transform.gameObject);
                connectionInfo.BottomRight = true;
                break;
            }
        }

        neighbourData.WallNeighbourConnectionInfo = connectionInfo;

        return neighbourData;
    }

    private void UpdateTiles(WallNeighbourConnectionInfo cI)
    {
        HideAllRenderers();
        ResetLayer();

        // Surrounded
        if (cI.Top && cI.Bottom && cI.Left && cI.Right)
        {
            MoveUpLayer();
            AssignCenter(_centerSpr);
            return;
        }
        
        // Alone
        if (!cI.Top && !cI.Bottom && !cI.Left && !cI.Right)
        {
            AssignTL(_cornerTLSpr);
            AssignTR(_cornerTRSpr);
            AssignBL(_cornerBLSpr);
            AssignBR(_cornerBRSpr);
            return;
        }
        
        // End left
        if (!cI.Top && !cI.Bottom && !cI.Left && cI.Right)
        {
            AssignTL(_cornerTLSpr);
            AssignTR(_topSpr);
            AssignBL(_cornerBLSpr);
            AssignBR(_botSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerTRSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerTLSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerBRSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerBLSpr);
            }
            
            return;
        }
        
        // End right
        if (!cI.Top && !cI.Bottom && cI.Left && !cI.Right)
        {
            AssignTL(_topSpr);
            AssignTR(_cornerTRSpr);
            AssignBL(_botSpr);
            AssignBR(_cornerBRSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerTRSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerTLSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerBRSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerBLSpr);
            }
            
            return;
        }
        
        // End up
        if (!cI.Top && cI.Bottom && !cI.Left && !cI.Right)
        {
            AssignTL(_cornerTLSpr);
            AssignTR(_cornerTRSpr);
            AssignBL(_leftSpr);
            AssignBR(_rightSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerBRSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerBLSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerTLSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerTRSpr);
            }
            
            return;
        }
        
        // End down
        if (cI.Top && !cI.Bottom && !cI.Left && !cI.Right)
        {
            AssignTL(_leftSpr);
            AssignTR(_rightSpr);
            AssignBL(_cornerBLSpr);
            AssignBR(_cornerBRSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerBLSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerBRSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerTLSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerTRSpr);
            }
            
            return;
        }
        
        // Hor
        if (!cI.Top && !cI.Bottom && cI.Left && cI.Right)
        {
            AssignTL(_topSpr);
            AssignTR(_topSpr);
            AssignBL(_botSpr);
            AssignBR(_botSpr);

            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerTRSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerTLSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerBRSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerBLSpr);
            }
            
            return;
        }
        
        // vert
        if (cI.Top && cI.Bottom && !cI.Left && !cI.Right)
        {
            AssignTL(_leftSpr);
            AssignTR(_rightSpr);
            AssignBL(_leftSpr);
            AssignBR(_rightSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerBLSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerBRSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerTLSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerTRSpr);
            }
            
            return;
        }
        
        // corner tl
        if (!cI.Top && cI.Bottom && !cI.Left && cI.Right)
        {
            MoveUpLayer();
            AssignTL(_cornerTLSpr);
            AssignTR(_topSpr);
            AssignBL(_leftSpr);
            AssignBR(_centerSpr);
            
            if (!cI.BottomRight)
            {
                AssignBR(_cornerInnerBRSpr);
            }

            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerTLSpr);
            }

            if (cI.TopRight)
            {
                AssignTR(_cornerInnerTLSpr);
            }
            
            return;
        }
        
        // corner tr
        if (!cI.Top && cI.Bottom && cI.Left && !cI.Right)
        {
            MoveUpLayer();
            AssignTL(_topSpr);
            AssignTR(_cornerTRSpr);
            AssignBR(_rightSpr);
            AssignBL(_centerSpr);

            if (!cI.BottomLeft)
            {
                AssignBL(_cornerInnerBLSpr);
            }

            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerTRSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerTRSpr);
            }
            
            return;
        }
        
        // corner bl
        if (cI.Top && !cI.Bottom && !cI.Left && cI.Right)
        {
            MoveUpLayer();
            AssignTL(_leftSpr);
            AssignBL(_cornerBLSpr);
            AssignBR(_botSpr);
            AssignTR(_centerSpr);

            if (!cI.TopRight)
            {
                AssignTR(_cornerInnerTRSpr);
            }

            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerBLSpr);
            }

            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerBLSpr);
            }
            
            return;
        }
        
        // corner br
        if (cI.Top && !cI.Bottom && cI.Left && !cI.Right)
        {
            MoveUpLayer();
            AssignTR(_rightSpr);
            AssignBL(_botSpr);
            AssignBR(_cornerBRSpr);
            AssignTL(_centerSpr);

            if (!cI.TopLeft)
            {
                AssignTL(_cornerInnerTLSpr);
            }

            if (cI.TopRight)
            {
                AssignTR(_cornerInnerBRSpr);
            }

            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerBRSpr);
            }
            
            return;
        }
        
        // left edge
        if (cI.Top && cI.Bottom && !cI.Left && cI.Right)
        {
            MoveUpLayer(2);
            AssignTL(_leftSpr);
            AssignTR(_centerSpr);
            AssignBL(_leftSpr);
            AssignBR(_centerSpr);

            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerBLSpr);
            }
            
            if (!cI.TopRight)
            {
                AssignTR(_cornerInnerTRSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerTLSpr);
            }
            
            if (!cI.BottomRight)
            {
                AssignBR(_cornerInnerBRSpr);
            }
            
            return;
        }
        
        // right edge
        if (cI.Top && cI.Bottom && cI.Left && !cI.Right)
        {
            MoveUpLayer(2);
            AssignTL(_centerSpr);
            AssignTR(_rightSpr);
            AssignBL(_centerSpr);
            AssignBR(_rightSpr);
            
            if (!cI.TopLeft)
            {
                AssignTL(_cornerInnerTLSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerBRSpr);
            }
            
            if (!cI.BottomLeft)
            {
                AssignBL(_cornerInnerBLSpr);
            }
            
            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerTRSpr);
            }
            
            return;
        }
        
        // top edge
        if (!cI.Top && cI.Bottom && cI.Left && cI.Right)
        {
            MoveUpLayer(2);
            AssignTL(_topSpr);
            AssignTR(_topSpr);
            AssignBL(_centerSpr);
            AssignBR(_centerSpr);
            
            if (cI.TopLeft)
            {
                AssignTL(_cornerInnerTRSpr);
            }
            
            if (cI.TopRight)
            {
                AssignTR(_cornerInnerTLSpr);
            }
            
            if (!cI.BottomLeft)
            {
                AssignBL(_cornerInnerBLSpr);
            }
            
            if (!cI.BottomRight)
            {
                AssignBR(_cornerInnerBRSpr);
            }
            
            return;
        }
        
        // bottom edge
        if (cI.Top && !cI.Bottom && cI.Left && cI.Right)
        {
            MoveUpLayer(2);
            AssignTL(_centerSpr);
            AssignTR(_centerSpr);
            AssignBL(_botSpr);
            AssignBR(_botSpr);
            
            if (!cI.TopLeft)
            {
                AssignTL(_cornerInnerTLSpr);
            }
            
            if (!cI.TopRight)
            {
                AssignTR(_cornerInnerTRSpr);
            }
            
            if (cI.BottomLeft)
            {
                AssignBL(_cornerInnerBRSpr);
            }
            
            if (cI.BottomRight)
            {
                AssignBR(_cornerInnerBLSpr);
            }
            
            return;
        }
    }

    private void HideAllRenderers()
    {
        _center.gameObject.SetActive(false);
        _botLeft.gameObject.SetActive(false);
        _botRight.gameObject.SetActive(false);
        _topLeft.gameObject.SetActive(false);
        _topRight.gameObject.SetActive(false);
    }

    private void AssignCenter(Sprite sprite)
    {
        _center.gameObject.SetActive(true);
        _center.sprite = sprite;
    }
    
    private void AssignTL(Sprite sprite)
    {
        _topLeft.gameObject.SetActive(true);
        _topLeft.sprite = sprite;
    }
    
    private void AssignTR(Sprite sprite)
    {
        _topRight.gameObject.SetActive(true);
        _topRight.sprite = sprite;
    }
    
    private void AssignBR(Sprite sprite)
    {
        _botRight.gameObject.SetActive(true);
        _botRight.sprite = sprite;
    }
    
    private void AssignBL(Sprite sprite)
    {
        _botLeft.gameObject.SetActive(true);
        _botLeft.sprite = sprite;
    }

    private void MoveUpLayer(int amount = 1)
    {
        _center.sortingOrder = _origLayer + amount;
        _topLeft.sortingOrder = _origLayer + amount;
        _topRight.sortingOrder = _origLayer + amount;
        _botLeft.sortingOrder = _origLayer + amount;
        _botRight.sortingOrder = _origLayer + amount;
    }

    private void ResetLayer()
    {
        _center.sortingOrder = _origLayer;
        _topLeft.sortingOrder = _origLayer;
        _topRight.sortingOrder = _origLayer;
        _botLeft.sortingOrder = _origLayer;
        _botRight.sortingOrder = _origLayer;
    }

    #endregion
    
} 
