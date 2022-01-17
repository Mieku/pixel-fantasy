
using System.Collections.Generic;
using System.Linq;
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
    /// Converts the starting grid position and current grid position into a list of all the grid positions between them
    /// </summary>
    public static List<Vector2> GetGridPositionsBetweenPoints(Vector2 startGridPos, Vector2 currentGridPos)
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
    /// Determines if the grid position is a valid position to build on
    /// </summary>
    public static bool IsGridPosValidToBuild(Vector2 gridPos, List<string> listOfInvalidTags)
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

        return listOfInvalidTags.All(invalidTag => !detectedTags.Contains(invalidTag));
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
}
