using System;
using System.Collections.Generic;
using FunkyCode;
using FunkyCode.Utilities;
using Managers;
using Systems.Buildings.Scripts;
using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    public float Visibility = 0;
    public bool IsIndoors;
    private List<LightCollision2D> collisionInfos = new List<LightCollision2D>();
    public Action<float> OnVisibilityUpdated;
    public Action<bool> OnIsIndoorsUpdated;

    private LightCollider2D _lightCollider;

    private void OnEnable()
    {
        _lightCollider = GetComponent<LightCollider2D>();
        _lightCollider?.AddEvent(CollisionEvent);
        GameEvents.MinuteTick += MinuteTick;
    }

    private void OnDisable()
    {
        GameEvents.MinuteTick -= MinuteTick;
        _lightCollider?.RemoveEvent(CollisionEvent);
    }

    private void MinuteTick()
    {
        CheckEnvironment();
    }

    private void CollisionEvent(LightCollision2D collision)
    {
        if (collision.points != null)
        {
            collisionInfos.Add(collision);
        }
    }

    private float CheckArtificialVisibility()
    {
        float result = 0;

        if (collisionInfos.Count == 0)
        {
            return 0;
        }

        foreach (var collision in collisionInfos)
        {
            if (collision.points != null)
            {
                Polygon2 polygon = _lightCollider.mainShape.GetPolygonsLocal()[0];

                int pointsCount = polygon.points.Length;
                int pointsInView = collision.points.Count;

                // Initial visibility based on points in view vs total points
                float collisionResult = ((float)pointsInView / pointsCount);

                if (pointsInView > 0)
                {
                    float totalMultiplier = 0;
                    float maxMultiplier = 0;

                    for (int i = 0; i < pointsInView; i++)
                    {
                        Vector2 point = collision.points[i];

                        // Calculate distance from light source to the point
                        float distance = Vector2.Distance(Vector2.zero, point);
                        float pointMultiplier = (1 - (distance / collision.light.size)) * 2;

                        // Clamp the multiplier between 0 and 1
                        pointMultiplier = Mathf.Clamp(pointMultiplier, 0, 1);

                        // Sum multipliers and track the maximum multiplier
                        totalMultiplier += pointMultiplier;
                        maxMultiplier = Mathf.Max(maxMultiplier, pointMultiplier);
                    }

                    // Calculate the average multiplier
                    float averageMultiplier = totalMultiplier / pointsInView;

                    // Use the higher value between the average and the maximum multiplier for this collision
                    collisionResult *= Mathf.Max(averageMultiplier, maxMultiplier);
                }

                result = Mathf.Max(result, collisionResult);
            }
        }

        collisionInfos.Clear(); // Clear the list after processing
        return result;
    }

    private float CheckDaylightVisibility(bool isIndoors)
    {
        if (isIndoors) return 0f;

        var daylightPercent = EnvironmentManager.Instance.DaylightPercent;
        return daylightPercent;
    }

    public bool CheckIsIndoors()
    {
        var room = StructureDatabase.Instance.RoomAtWorldPos(transform.position);
        return room != null;
    }

    public void CheckEnvironment()
    {
        IsIndoors = CheckIsIndoors();

        var artificialLight = CheckArtificialVisibility();
        var dayLight = CheckDaylightVisibility(IsIndoors);

        float visibility = Mathf.Max(artificialLight, dayLight);
        Visibility = visibility;

        OnIsIndoorsUpdated?.Invoke(IsIndoors);
        OnVisibilityUpdated?.Invoke(Visibility);
    }
}
