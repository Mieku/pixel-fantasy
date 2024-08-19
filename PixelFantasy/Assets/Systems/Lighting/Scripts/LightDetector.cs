using FunkyCode;
using FunkyCode.Utilities;
using Managers;
using Systems.Buildings.Scripts;
using UnityEngine;

public class LightDetector : MonoBehaviour
{
    public float visability = 0;
        
    public LightCollision2D? CollisionInfo = null;

    private LightCollider2D lightCollider;

    private void OnEnable()
    {
        lightCollider = GetComponent<LightCollider2D>();

        lightCollider?.AddEvent(CollisionEvent);

        GameEvents.MinuteTick += MinuteTick;
    }

    private void OnDisable()
    {
        GameEvents.MinuteTick -= MinuteTick;
        
        lightCollider?.RemoveEvent(CollisionEvent);
    }

    private void MinuteTick()
    {
        CheckVisibility();
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
            Polygon2 polygon = lightCollider.mainShape.GetPolygonsLocal()[0];

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

    private float CheckDaylightVisibility()
    {
        var room = StructureDatabase.Instance.RoomAtWorldPos(transform.position);
        bool isIndoors = room != null;

        if (isIndoors) return 0f;

        var daylightPercent = EnvironmentManager.Instance.DaylightPercent;
        return daylightPercent;
    }

    public void CheckVisibility()
    {
        var artificialLight = CheckArtificialVisibility();
        var dayLight = CheckDaylightVisibility();

        float result = Mathf.Max(artificialLight, dayLight);
        visability = result;
    }
}