using System;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavLinkWarper : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Vector3 _destination;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_agent.isOnOffMeshLink)
            {
                Teleport();
            }
        }

        private void Teleport()
        {
            var destination = _agent.destination;
            OffMeshLinkData data = _agent.currentOffMeshLinkData;
            _agent.Warp(data.endPos);
            _agent.SetDestination(destination);
        }
    }
}
