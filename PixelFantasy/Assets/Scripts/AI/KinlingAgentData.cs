using System.Collections;
using System.Collections.Generic;
using Characters.Interfaces;
using SGoap;
using UnityEngine;
using UnityEngine.AI;

public class KinlingAgentData
{
    public Transform Target { get; set; }
    public UnitAnimController Animator { get; set; }
    public KinlingAgent KinlingAgent { get; set; }
    public NavMeshAgent NavMeshAgent { get; set; }
    public CoolDown Cooldown { get; set; }
    public float DistanceToTarget => Vector3.Distance(Target.position, KinlingAgent.transform.position);

    public Vector3 Position
    {
        get => KinlingAgent.transform.position;
        set => KinlingAgent.transform.position = value;
    }
}
