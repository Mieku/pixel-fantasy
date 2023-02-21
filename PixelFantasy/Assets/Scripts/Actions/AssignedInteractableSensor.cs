using System.Collections;
using System.Collections.Generic;
using SGoap;
using UnityEngine;

public class AssignedInteractableSensor : Sensor
{
    private Interactable _interactable;
    
    public bool HasInteractable => _interactable != null;

    public void AssignInteractable(Interactable interactable)
    {
        _interactable = interactable;
        if (interactable != null)
        {
            AgentData.KinlingAgent.States.AddState("hasInteractable", 1);
        }
        else
        {
            AgentData.KinlingAgent.States.RemoveState("hasInteractable");
        }

        if (Destination != null)
        {
            AgentData.KinlingAgent.States.AddState("hasDestination", 1);
        }
        else
        {
            AgentData.KinlingAgent.States.RemoveState("hasDestination");
        }
        
    }

    public Interactable GetInteractable()
    {
        return _interactable;
    }

    public UnitActionDirection GetActionDirection()
    {
        return DetermineUnitActionDirection((Vector3)_interactable.transform.position, transform.position);
    }

    public Vector2? Destination => GetAdjacentPosition(_interactable.transform.position);

    private Vector2? GetAdjacentPosition(Vector2 workPosition, float distanceAway = 1f)
    {
        Vector2 unitPos = transform.position;

        var angle = Helper.CalculateAngle(workPosition, unitPos);
        var angle2 = ClampAngleTo360(angle - 90);
        var angle3 = ClampAngleTo360(angle + 90);
        var angle4 = ClampAngleTo360(angle + 180);

        Vector2 suggestedPos = ConvertAngleToPosition(angle, workPosition, distanceAway);
        if (AgentData.KinlingAgent.IsDestinationPossible(suggestedPos))
        {
            return suggestedPos;
        }
        
        suggestedPos = ConvertAngleToPosition(angle2, workPosition, distanceAway);
        if (AgentData.KinlingAgent.IsDestinationPossible(suggestedPos))
        {
            return suggestedPos;
        }
        
        suggestedPos = ConvertAngleToPosition(angle3, workPosition, distanceAway);
        if (AgentData.KinlingAgent.IsDestinationPossible(suggestedPos))
        {
            return suggestedPos;
        }
        
        suggestedPos = ConvertAngleToPosition(angle4, workPosition, distanceAway);
        if (AgentData.KinlingAgent.IsDestinationPossible(suggestedPos))
        {
            return suggestedPos;
        }

        return null;
    }

    private UnitActionDirection DetermineUnitActionDirection(Vector3 workPos, Vector3 standPos)
    {
        const float threshold = .25f;

        if (standPos.y >= workPos.y + threshold)
        {
            return UnitActionDirection.Down;
        } else if (standPos.y <= workPos.y - threshold)
        {
            return UnitActionDirection.Up;
        }
        else
        {
            return UnitActionDirection.Side;
        }
    }

    private float ClampAngleTo360(float angle)
    {
        if (angle < 0)
        {
            angle += 360;
        }
        else if (angle >= 360)
        {
            angle -= 360;
        }

        return angle;
    }
    
    public Vector2 ConvertAngleToPosition(float angle, Vector2 startPos, float distance)
    {
        Vector2 result = new Vector2();
            
        // Left
        if (angle is >= 45 and < 135)
        {
            result = new Vector2(startPos.x - distance, startPos.y);
        } 
        else if (angle is >= 135 and < 225) // Down
        {
            result = new Vector2(startPos.x, startPos.y - distance);
        }
        else if (angle is >= 225 and < 315) // Right
        {
            result = new Vector2(startPos.x + distance, startPos.y);
        }
        else // Up
        {
            result = new Vector2(startPos.x, startPos.y + distance);
        }

        return result;
    }
    
    public override void OnAwake()
    {
        
    }
}
