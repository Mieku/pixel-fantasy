using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.AI;
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
    public static bool IsGridPosValidToBuild(Vector2 gridPos, List<string> listOfInvalidTags, List<string> listOfRequiredTags = null, GameObject parent = null)
    {
        var detectedTags = GetTagsAtGridPos(gridPos, parent);

        bool passesInvalidTags = listOfInvalidTags.All(invalidTag => !detectedTags.Contains(invalidTag));
        bool passesRequiredTags = true;

        if (listOfRequiredTags != null)
        {
            passesRequiredTags = listOfRequiredTags.All(requiredTag => detectedTags.Contains(requiredTag));
        }
        
        return passesInvalidTags && passesRequiredTags;
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
    public static List<string> GetTagsAtGridPos(Vector2 gridPos, GameObject parent = null)
    {
        var leftStart = new Vector2(gridPos.x - 0.25f, gridPos.y);
        var bottomStart = new Vector2(gridPos.x, gridPos.y - 0.25f);
        
        var allHitHor = Physics2D.RaycastAll(leftStart, Vector2.right, 0.5f);
        var allHitVert = Physics2D.RaycastAll(bottomStart, Vector2.up, 0.5f);

        List<string> detectedTags = new List<string>();
        foreach (var hitHor in allHitHor)
        {
            if (parent != null)
            {
                if (!hitHor.transform.IsChildOf(parent.transform))
                {
                    detectedTags.Add(hitHor.collider.transform.tag);
                }
            }
            else
            {
                detectedTags.Add(hitHor.collider.transform.tag);
            }
        }
        foreach (var hitVert in allHitVert)
        {
            if (parent != null)
            {
                if (!hitVert.transform.IsChildOf(parent.transform))
                {
                    detectedTags.Add(hitVert.collider.transform.tag);
                }
            }
            else
            {
                detectedTags.Add(hitVert.collider.transform.tag);
            }
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

    // public static Building IsPositionInBuilding(Vector2 worldPos)
    // {
    //     var gos = GetGameObjectsOnTile(worldPos, "Building Interior");
    //     foreach (var go in gos)
    //     {
    //         var building = go.GetComponentInParent<Building>(true);
    //         if (building != null)
    //         {
    //             return building;
    //         }
    //     }
    //
    //     return null;
    // }
    
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

    public struct PathResult
    {
        public bool pathExists;
        public NavMeshPath navMeshPath;
    }

    public static PathResult DoesPathExist(Vector2 startPoint2D, Vector2 endPoint2D)
    {
        float defaultZLevel = 0f;
        
        // Bitmask for all areas
        int allAreasMask = 0xFFFFFF;
        // Clear the bit for the 'Not Walkable' area (assuming it's at index 1)
        int walkableAreasMask = allAreasMask & ~(1 << 1);
        
        Vector3 startPoint = new Vector3(startPoint2D.x, startPoint2D.y, defaultZLevel);
        Vector3 endPoint = new Vector3(endPoint2D.x, endPoint2D.y, defaultZLevel);
        
        NavMeshHit startHit, endHit;
        bool startValid = NavMesh.SamplePosition(startPoint, out startHit, 0.1f, walkableAreasMask);
        bool endValid = NavMesh.SamplePosition(endPoint, out endHit, 0.1f, walkableAreasMask);
        
        PathResult result;
        result.navMeshPath = new NavMeshPath();
        
        if (startValid && endValid)
        {
            NavMesh.CalculatePath(startHit.position, endHit.position, walkableAreasMask, result.navMeshPath);
            result.pathExists = result.navMeshPath.status == NavMeshPathStatus.PathComplete;
        }
        else
        {
            result.pathExists = false;
        }
        
        return result;
    }

    public static void DebugPath(NavMeshPath path, Color colour, float duration)
    {
        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i], path.corners[i + 1], colour, duration);
    }
    
    public static Vector2 RandomLocationInRange(Vector2 centerPoint, float range = 5f)
    {
        Vector2 randomLocation = centerPoint;
        randomLocation += Random.Range(-range, range) * Vector2.up;
        randomLocation += Random.Range(-range, range) * Vector2.right;

        return randomLocation;
    }
    
    public static List<int> GenerateDistinctRandoms(int quantity, int min, int max)
    {
        if (max - min + 1 < quantity)
        {
            throw new ArgumentException("The range is not sufficient for the requested quantity of distinct numbers.");
        }

        var random = new System.Random();
        var numbers = new HashSet<int>();

        while (numbers.Count < quantity)
        {
            int number = random.Next(min, max + 1);
            numbers.Add(number);
        }

        // Convert the HashSet to a List to maintain order and return.
        List<int> distinctNumbersList = new List<int>(numbers);
        return distinctNumbersList;
    }
    
    public static bool IsTooCloseToOthers(Vector2 position, List<Vector2> others, float minDistance)
    {
        foreach (var other in others)
        {
            if (Vector2.Distance(position, other) < minDistance)
            {
                return true; // Found a position too close to the current one
            }
        }
        return false; // No positions too close were found
    }
    
    /// <summary>
    /// Calculates the boundary box for a list of affected cells.
    /// </summary>
    public static (Vector2Int min, Vector2Int max) CalculateBoundaryBox(List<Vector2Int> affectedCells)
    {
        if (affectedCells == null || !affectedCells.Any())
            return (new Vector2Int(int.MaxValue, int.MaxValue), new Vector2Int(int.MinValue, int.MinValue));

        int minX = affectedCells.Min(cell => cell.x);
        int maxX = affectedCells.Max(cell => cell.x);
        int minY = affectedCells.Min(cell => cell.y);
        int maxY = affectedCells.Max(cell => cell.y);

        return (new Vector2Int(minX, minY), new Vector2Int(maxX, maxY));
    }
    
    /// <summary>
    /// Checks if a given cell is within the specified boundary box.
    /// </summary>
    public static bool IsWithinBounds(Vector2Int cell, Vector2Int min, Vector2Int max)
    {
        return cell.x >= min.x && cell.x <= max.x && cell.y >= min.y && cell.y <= max.y;
    }
    
    public static Color AdjustColorLuminance(Color light, Color dark, Color target)
    {
        float lightLuminance = GetLuminance(light);
        float darkLuminance = GetLuminance(dark);
        float targetLuminance = GetLuminance(target);

        float brightnessDifference = lightLuminance - darkLuminance;
        float adjustedLuminance = targetLuminance - brightnessDifference;

        return ChangeLuminance(target, adjustedLuminance);
    }
    
    static float GetLuminance(Color color)
    {
        return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
    }
    
    static Color ChangeLuminance(Color original, float newLuminance)
    {
        float currentLuminance = GetLuminance(original);
        float luminanceRatio = newLuminance / currentLuminance;

        // Clamp the color values to ensure they are valid
        return new Color(Mathf.Clamp(original.r * luminanceRatio, 0, 1),
            Mathf.Clamp(original.g * luminanceRatio, 0, 1),
            Mathf.Clamp(original.b * luminanceRatio, 0, 1));
    }
    
    public static Color32[] Repaint4C(Color32[] pixels, Color32 baseColor, List<Color32> palette, List<Color32> exemptColours, int colorCount = 4)
    {
        int baseIndex = FindClosestColorIndex(baseColor, palette);
        var colorGradient = GenerateIndexedColorGradient(baseIndex, palette, colorCount);

        float maxBrightness = pixels.Max(p => GetBrightness(p));
        float minBrightness = pixels.Min(p => GetBrightness(p));
        float brightnessRange = maxBrightness - minBrightness;

        Color32[] resultPixels = new Color32[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0 && !exemptColours.Contains(pixels[i]))
            {
                float normalizedBrightness = (GetBrightness(pixels[i]) - minBrightness) / brightnessRange;
                int gradientIndex = (int)(normalizedBrightness * (colorGradient.Count - 1));
                resultPixels[i] = colorGradient[gradientIndex];
            }
            else
            {
                resultPixels[i] = pixels[i]; // Preserve transparency and outline color
            }
        }

        return resultPixels;
    }
    
    private static List<Color32> GenerateIndexedColorGradient(int baseIndex, List<Color32> palette, int colorCount = 4)
    {
        List<Color32> gradient = new List<Color32>();
        switch (colorCount)
        {
            case 1:
                gradient.Add(palette[baseIndex]); 
                break;
            case 2:
                // Only use base and one darker shade if total colors needed is 2
                gradient.Add(palette[Mathf.Clamp(baseIndex - 1, 0, palette.Count - 1)]); // Darker
                gradient.Add(palette[baseIndex]); // Base
                break;
            case 4:
            default:
                // Use one lighter, base, and two darker shades if total colors needed is 4 or unspecified
                gradient.Add(palette[Mathf.Clamp(baseIndex - 2, 0, palette.Count - 1)]); // Darkest
                gradient.Add(palette[Mathf.Clamp(baseIndex - 1, 0, palette.Count - 1)]); // Darker
                gradient.Add(palette[baseIndex]); // Base
                gradient.Add(palette[Mathf.Clamp(baseIndex + 1, 0, palette.Count - 1)]); // Lighter
                return SortColorsByBrightness(gradient);
        }
        return gradient;
    }
    
    public static List<Color32> SortColorsByBrightness(List<Color32> colors)
    {
        var sortedColors = colors.OrderBy(color => GetLuminance(color)).ToList();
        return sortedColors;
    }

    // Helper method to calculate the luminance of a Color32
    private static float GetLuminance(Color32 color)
    {
        // Convert Color32 to Color to work with floating point precision
        Color c = color;
        // Standard formula for luminance
        return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
    }
        
    private static float GetBrightness(Color32 color)
    {
        return 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;  // Standard luminosity formula
    }

    private static int FindClosestColorIndex(Color32 color, List<Color32> palette)
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;
        for (int i = 0; i < palette.Count; i++)
        {
            float distance = ColorDistance(color, palette[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    private static float ColorDistance(Color32 c1, Color32 c2)
    {
        return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
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
