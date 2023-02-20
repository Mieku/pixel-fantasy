using System.Collections;
using System.Collections.Generic;
using SGoap;
using UnityEngine;

public class AssignedTargetSensor : Sensor
{
    private GameObject _target;
    
    public bool HasTarget => _target != null;

    public void AssignTarget(GameObject target)
    {
        _target = target;
        if (target != null)
        {
            Agent.States.AddState("hasTarget", 1);
        }
        else
        {
            Agent.States.RemoveState("hasTarget");
        }
        
    }

    public GameObject GetTarget()
    {
        return _target;
    }
    
    public override void OnAwake()
    {
    }
}
