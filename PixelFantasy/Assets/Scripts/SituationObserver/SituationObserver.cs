using System.Collections.Generic;
using SituationObserver;
using UnityEngine;

namespace Managers
{
    public class SituationObserver : Singleton<SituationObserver>
    {
        private List<ISituationWatcher> _situationWatchers = new List<ISituationWatcher>();

        private void Start()
        {
            GameEvents.MinuteTick += OnTick;
            RegisterWatchers();
        }

        private void OnDestroy()
        {
            GameEvents.MinuteTick -= OnTick;
        }

        private void RegisterWatchers()
        {
            // Add all watchers here
            _situationWatchers.Add(new BedWatcher());
            // Add other watchers as needed, e.g., _situationWatchers.Add(new HungerWatcher());
        }

        private void OnTick()
        {
            foreach (var watcher in _situationWatchers)
            {
                watcher.CheckSituation();
            }
        }
    }
}