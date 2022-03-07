
using System.Collections.Generic;
using System.Linq;
using Gods;
using Items;
using UnityEngine;

public static class Helper
{

    /// <summary>
    /// Converts the current mouse position to a grid position
    /// </summary>
    public static Vector2 ConvertMousePosToGridPos(Vector2 mousePos)
    {
        float xGrid, yGrid;

        xGrid = (int)mousePos.x + 0.5f;
        yGrid = (int)mousePos.y + 0.5f;
        
        return new Vector2(xGrid, yGrid);
    }

    
    /// <summary>
    /// Converts the starting grid position and current grid position into a list of all the grid positions between them as a rectangle
    /// </summary>
    public static List<Vector2> GetRectangleGridPositionsBetweenPoints(Vector2 startGridPos, Vector2 currentGridPos)
    {
        List<Vector2> result = new List<Vector2>();
        var lowerLeft = new Vector2(
            Mathf.Min(startGridPos.x, currentGridPos.x),
            Mathf.Min(startGridPos.y, currentGridPos.y)
        );
        var upperRight = new Vector2(
            Mathf.Max(startGridPos.x, currentGridPos.x),
            Mathf.Max(startGridPos.y, currentGridPos.y)
        );

        var xDelta = upperRight.x - lowerLeft.x;
        var yDelta = upperRight.y - lowerLeft.y;

        for (int x = 0; x <= xDelta; x++)
        {
            for (int y = 0; y <= yDelta; y++)
            {
                var gridPos = new Vector2(lowerLeft.x + x, lowerLeft.y + y);
                result.Add(gridPos);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts the starting grid position and current grid position into a list of all the grid positions between them as a line
    /// </summary>
    public static List<Vector2> GetLineGridPositionsBetweenPoints(Vector2 startGridPos, Vector2 currentPos)
    {
        List<Vector2> result = new List<Vector2>();
        var currentGridPos = ConvertMousePosToGridPos(currentPos);

        var diff = startGridPos - currentPos;
        diff = new Vector2(Mathf.Abs(diff.x), Mathf.Abs(diff.y));

        if (diff.x > diff.y) // horizontal
        {
            if (startGridPos.x < currentGridPos.x) // left -> right
            {
                var tiles = currentGridPos.x - startGridPos.x;
                for (int i = 0; i <= tiles; i++)
                {
                    var gridPos = new Vector2(startGridPos.x + i, startGridPos.y);
                    result.Add(gridPos);
                }
            }
            else // right -> left
            {
                var tiles = startGridPos.x - currentGridPos.x;
                for (int i = 0; i <= tiles; i++)
                {
                    var gridPos = new Vector2(startGridPos.x - i, startGridPos.y);
                    result.Add(gridPos);
                }
            }
        }
        else // vert
        {
            if (startGridPos.y < currentGridPos.y) // down -> up
            {
                var tiles = currentGridPos.y - startGridPos.y;
                for (int i = 0; i <= tiles; i++)
                {
                    var gridPos = new Vector2(startGridPos.x, startGridPos.y + i);
                    result.Add(gridPos);
                }
            }
            else // up -> down
            {
                var tiles = startGridPos.y - currentGridPos.y;
                for (int i = 0; i <= tiles; i++)
                {
                    var gridPos = new Vector2(startGridPos.x, startGridPos.y - i);
                    result.Add(gridPos);
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Determines if the grid position is a valid position to build on
    /// </summary>
    public static bool IsGridPosValidToBuild(Vector2 gridPos, List<string> listOfInvalidTags)
    {
        var detectedTags = GetTagsAtGridPos(gridPos);
        
        // Ensure On Ground
        if (!detectedTags.Contains("Ground"))
            return false;

        return listOfInvalidTags.All(invalidTag => !detectedTags.Contains(invalidTag));
    }

    /// <summary>
    /// Checks if the grid position contains the requested tag
    /// </summary>
    public static bool DoesGridContainTag(Vector2 gridPos, string tagToCheck)
    {
        var detectedTags = GetTagsAtGridPos(gridPos);
        return detectedTags.Contains(tagToCheck);
    }

    /// <summary>
    /// Returns a list of all the tags located at the specified grid position
    /// </summary>
    public static List<string> GetTagsAtGridPos(Vector2 gridPos)
    {
        var leftStart = new Vector2(gridPos.x - 0.45f, gridPos.y);
        var bottomStart = new Vector2(gridPos.x, gridPos.y - 0.45f);
        
        var allHitHor = Physics2D.RaycastAll(leftStart, Vector2.right, 0.9f);
        var allHitVert = Physics2D.RaycastAll(bottomStart, Vector2.up, 0.9f);

        List<string> detectedTags = new List<string>();
        foreach (var hitHor in allHitHor)
        {
            detectedTags.Add(hitHor.transform.tag);
        }
        foreach (var hitVert in allHitVert)
        {
            detectedTags.Add(hitVert.transform.tag);
        }

        return detectedTags;
    }

    /// <summary>
    /// Returns the GameObjects located on a specific tile position
    /// </summary>
    public static List<GameObject> GetGameObjectsOnTile(Vector2 gridPos)
    {
        var leftStart = new Vector2(gridPos.x - 0.45f, gridPos.y);
        var bottomStart = new Vector2(gridPos.x, gridPos.y - 0.45f);
        
        var allHitHor = Physics2D.RaycastAll(leftStart, Vector2.right, 0.9f);
        var allHitVert = Physics2D.RaycastAll(bottomStart, Vector2.up, 0.9f);

        List<GameObject> detected = new List<GameObject>();
        foreach (var hitHor in allHitHor)
        {
            detected.Add(hitHor.transform.gameObject);
        }
        foreach (var hitVert in allHitVert)
        {
            detected.Add(hitVert.transform.gameObject);
        }
        
        // Remove duplicates
        var result = detected.Distinct().ToList();
        
        return result;
    }

    /// <summary>
    /// The percent is in 100 format,
    /// True if win
    /// </summary>
    public static bool RollDice(float percentWin)
    {
        var roll = Random.Range(0f, 100f);
        return roll <= percentWin;
    }

    /// <summary>
    /// Provides the next direction in a clockwise motion
    /// </summary>
    public static PlacementDirection GetNextDirection(PlacementDirection curDirection)
    {
        switch (curDirection)
        {
            case PlacementDirection.Up:
                return PlacementDirection.Right;
            case PlacementDirection.Right:
                return PlacementDirection.Down;
            case PlacementDirection.Down:
                return PlacementDirection.Left;
            case PlacementDirection.Left:
                return PlacementDirection.Up;
        }

        return PlacementDirection.Down;
    }

    /// <summary>
    /// Provides the next direction in a counter clockwise motion
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static PlacementDirection GetPrevDirection(PlacementDirection curDirection)
    {
        switch (curDirection)
        {
            case PlacementDirection.Up:
                return PlacementDirection.Left;
            case PlacementDirection.Right:
                return PlacementDirection.Up;
            case PlacementDirection.Down:
                return PlacementDirection.Right;
            case PlacementDirection.Left:
                return PlacementDirection.Down;
        }

        return PlacementDirection.Down;
    }
}
