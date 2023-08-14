using System;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Needs
{
    public class NeedsAI : MonoBehaviour
    {
        [SerializeField] private List<Need> _needs;

        private void Awake()
        {
            GameEvents.MinuteTick += MinuteTick;
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= MinuteTick;
        }

        private void MinuteTick()
        {
            foreach (var need in _needs)
            {
                need.DecayMinute();
            }
        }
    }
}
