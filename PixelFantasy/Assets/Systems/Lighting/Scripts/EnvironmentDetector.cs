using System;
using FunkyCode;
using FunkyCode.Utilities;
using Managers;
using Systems.Buildings.Scripts;
using UnityEngine;

public class EnvironmentDetector : MonoBehaviour
{
    public float Visibility = 0;
    public bool IsIndoors;
    public LightCollision2D? CollisionInfo = null;
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
            if (CollisionInfo == null)
            {
                CollisionInfo = collision;
            }
            else
            {
                if (CollisionInfo.Value.points != null)
                {
                    if (collision.points.Count >= CollisionInfo.Value.points.Count)
                    {
                        CollisionInfo = collision;
                    }
                    else if (CollisionInfo.Value.light == collision.light)
                    {
                        CollisionInfo = collision;
                    }
                }
            }

        }
        else
        {
            CollisionInfo = null;
        }
    }

    private float CheckArtificialVisibility()
    {
        float result = 0;

        if (CollisionInfo == null)
        {
            return 0;
        }

        if (CollisionInfo.Value.points != null)
        {
            Polygon2 polygon = _lightCollider.mainShape.GetPolygonsLocal()[0];

            int pointsCount = polygon.points.Length;
            int pointsInView = CollisionInfo.Value.points.Count;

            result = (((float)pointsInView / pointsCount));

            if (CollisionInfo.Value.points.Count > 0)
            {
                float multiplier = 0;

                for(int i = 0; i < CollisionInfo.Value.points.Count; i++)
                {
                    Vector2 point = CollisionInfo.Value.points[i];

                    float distance = Vector2.Distance(Vector2.zero, point);
                    float pointMultipler = ( 1 - (distance / CollisionInfo.Value.light.size) ) * 2;

                    pointMultipler = pointMultipler > 1 ? 1 : pointMultipler;
                    pointMultipler = pointMultipler < 0 ? 0 : pointMultipler;
    
                    multiplier += pointMultipler;
                }

                result *= ( multiplier / CollisionInfo.Value.points.Count );
            }
        }
        
        CollisionInfo = null;

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