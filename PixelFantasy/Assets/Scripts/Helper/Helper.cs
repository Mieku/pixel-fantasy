
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Buildings;
using Items;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using Zones;
using Random = UnityEngine.Random;

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
    public static List<Vector2> GetRectangleGridPositionsBetweenPoints(Vector2 startGridPos, Vector2 currentPos)
    {
        var currentGridPos = ConvertMousePosToGridPos(currentPos);
        
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

    public static List<Vector2> GetBoxPositionsBetweenPoints(Vector2 startGridPos, Vector2 currentPos)
    {
        var currentGridPos = ConvertMousePosToGridPos(currentPos);
        
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
            var gridPos = new Vector2(lowerLeft.x + x, lowerLeft.y);
            var gridPos2 = new Vector2(lowerLeft.x + x, lowerLeft.y + yDelta);
            result.Add(gridPos);
            result.Add(gridPos2);
        }

        for (int y = 0; y <= yDelta; y++)
        {
            var gridPos = new Vector2(lowerLeft.x, lowerLeft.y + y);
            var gridPos2 = new Vector2(lowerLeft.x + xDelta, lowerLeft.y + y);
            result.Add(gridPos);
            result.Add(gridPos2);
        }
        
        return result.Distinct().ToList();
    }
    
    public static List<Vector2> GetSurroundingGridPositions(Vector2 referencePos, bool includeDiagonals, int radius, bool includeCenter = false)
    {
        var refGridPos = ConvertMousePosToGridPos(referencePos);
        HashSet<Vector2> results = new HashSet<Vector2>();

        if (includeCenter)
        {
            results.Add(refGridPos);
        }

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0 && !includeCenter)
                    continue;

                if (!includeDiagonals && (x != 0 && y != 0))
                    continue;

                Vector2 gridPos = refGridPos + new Vector2(x, y);
                results.Add(gridPos);
            }
        }

        return results.ToList();
    }
    
    public static bool IsPositionWithinRadius(Vector2 posToCheck, Vector2 radiusCenter, int radius)
    {
        var gridPosToCheck = ConvertMousePosToGridPos(posToCheck);
        var gridRadiusCenter = ConvertMousePosToGridPos(radiusCenter);

        if (gridPosToCheck == gridRadiusCenter)
            return true;

        Vector2 difference = gridPosToCheck - gridRadiusCenter;
        if (difference.magnitude <= radius)
            return true;

        return false;
    }
    
    /// <summary>
    /// Determines if the grid position is a valid position to build on
    /// </summary>
    public static bool IsGridPosValidToBuild(Vector2 gridPos, List<string> listOfInvalidTags)
    {
        var detectedTags = GetTagsAtGridPos(gridPos);
        
        // Ensure On Ground
        // if (!detectedTags.Contains("Ground"))
        //     return false;

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
        var leftStart = new Vector2(gridPos.x - 0.25f, gridPos.y);
        var bottomStart = new Vector2(gridPos.x, gridPos.y - 0.25f);
        
        var allHitHor = Physics2D.RaycastAll(leftStart, Vector2.right, 0.5f);
        var allHitVert = Physics2D.RaycastAll(bottomStart, Vector2.up, 0.5f);

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

    public static List<T> GetAllGenericOnGridPositions<T>(List<Vector2> tiles)
    {
        List<GameObject> allGameobjects = new List<GameObject>();
        foreach (var tile in tiles)
        {
            allGameobjects.AddRange(GetGameObjectsOnTile(tile));
        }

        var uniqueGOS = allGameobjects.Distinct().ToList();

        List<T> results = new List<T>();
        foreach (var uniqueGO in uniqueGOS)
        {
            var component = uniqueGO.GetComponent<T>();
            if (component != null)
            {
                results.Add(component);
            }
        }

        return results;
    }
    
    public static T GetObjectAtPosition<T>(Vector2 pos)
    {
        Collider2D[] colliders = new Collider2D[8];
        int colliderCount = Physics2D.OverlapPointNonAlloc(pos, colliders);
        for (int i = 0; i < colliderCount; i++)
        {
            var result = colliders[i].gameObject.TryGetComponent<T>(out var component);
            if (result)
            {
                return component;
            }
        }

        return default(T);
    }

    /// <summary>
    /// Returns the GameObjects located on a specific tile position
    /// </summary>
    public static List<GameObject> GetGameObjectsOnTile(Vector2 gridPos, string tagToGet = "")
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

        if (tagToGet != "")
        {
            result = result.FindAll(x => x.CompareTag(tagToGet));
        }
        
        return result;
    }
    
    public static List<ClickObject> GetClickObjectsAtPos(Vector2 pos)
    {
        var leftStart = new Vector2(pos.x - 0.20f, pos.y);
        var bottomStart = new Vector2(pos.x, pos.y - 0.20f);
        
        var allHitHor = Physics2D.RaycastAll(leftStart, Vector2.right, 0.4f);
        var allHitVert = Physics2D.RaycastAll(bottomStart, Vector2.up, 0.4f);
        
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
        var foundObjs = detected.Distinct().ToList();
        List<ClickObject> results = new List<ClickObject>();
        foreach (var foundObj in foundObjs)
        {
            var clickObj = foundObj.GetComponent<ClickObject>();
            if (clickObj != null)
            {
                results.Add(clickObj);
            }
        }

        return results;
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
            case PlacementDirection.North:
                return PlacementDirection.East;
            case PlacementDirection.East:
                return PlacementDirection.South;
            case PlacementDirection.South:
                return PlacementDirection.West;
            case PlacementDirection.West:
                return PlacementDirection.North;
        }

        return PlacementDirection.South;
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
            case PlacementDirection.North:
                return PlacementDirection.West;
            case PlacementDirection.East:
                return PlacementDirection.North;
            case PlacementDirection.South:
                return PlacementDirection.East;
            case PlacementDirection.West:
                return PlacementDirection.South;
        }

        return PlacementDirection.South;
    }
    
    public static Vector2 LerpByDistance(Vector3 a, Vector3 b, float x)
    {
        Vector2 p = x * Vector3.Normalize(b - a) + a;
        return p;
    }
    
    /// <summary>
    /// Calculates a 360 Deg Angle, 0 is top and increments counter-clockwise
    /// </summary>
    public static float CalculateAngle(Vector3 from, Vector3 to) {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

    public static Building IsPositionInBuilding(Vector2 worldPos)
    {
        var gos = GetGameObjectsOnTile(worldPos, "Building Interior");
        foreach (var go in gos)
        {
            var building = go.GetComponentInParent<Building>(true);
            if (building != null)
            {
                return building;
            }
        }

        return null;
    }
    
    public static List<WeightedObject<T>> GenerateCumulativeDistribution<T>(List<WeightedObject<T>> items)
    {
        var cumulativeDistribution = new List<WeightedObject<T>>();
        double runningTotal = 0;

        foreach (var item in items)
        {
            runningTotal += item.Weight;
            cumulativeDistribution.Add(new WeightedObject<T>(item.Object, runningTotal));
        }

        return cumulativeDistribution;
    }

    public static T GetRandomObject<T>(List<WeightedObject<T>> cumulativeDistribution)
    {
        var totalWeight = cumulativeDistribution.Last().Weight;
        double randomNumber = new System.Random().NextDouble() * totalWeight;

        foreach (var entry in cumulativeDistribution)
        {
            if (randomNumber <= entry.Weight)
                return entry.Object;
        }

        return default(T);  // This line should never be reached
    }
    
    /// <summary>
    /// Converts a colour to a hex code including the #, ex: #CDCDCD
    /// </summary>
    public static string ColorToHex(Color color, bool includeAlpha = false)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        int a = Mathf.RoundToInt(color.a * 255);

        if(includeAlpha)
        {
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }
        else
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }

    /// <summary>
    /// Returns the length of a navmesh path
    /// </summary>
    public static float GetPathLength(NavMeshPath path)
    {
        float length = 0;
        if (path.corners.Length < 2)
            return length;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            length += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return length;
    }
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }
}

public class WeightedObject<T>
{
    public T Object { get; set; }
    public double Weight { get; set; }

    public WeightedObject(T obj, double weight)
    {
        Object = obj;
        Weight = weight;
    }
}
