using System;
using System.Collections.Generic;
using Characters;
using Systems.Notifications.Scripts;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class StatsSection : MonoBehaviour
    {
        [SerializeField] private StatDisplay _statDisplayPrefab;
        [SerializeField] private Transform _statsParent;
        [SerializeField] private Log _logPrefab;
        [SerializeField] private Transform _logParent;
        [SerializeField] private List<StatDisplay> _statDisplays;
        
        private KinlingData _kinlingData;
        private Action _refreshLayoutCallback;
        
        private List<Log> _displayedLogs = new List<Log>();
        
        public void ShowSection(KinlingData kinlingData, Action refreshLayoutCallback)
        {
            _kinlingData = kinlingData;
            _refreshLayoutCallback = refreshLayoutCallback;
            gameObject.SetActive(true);
            _statDisplayPrefab.gameObject.SetActive(false);
            _logPrefab.gameObject.SetActive(false);
            
            RefreshContent();
        }

        public void RefreshContent()
        {
            RefreshPersonalLog();
            RefreshStats();
        }

        private void RefreshStats()
        {
            foreach (var statDisplay in _statDisplays)
            {
                statDisplay.Refresh(_kinlingData);
            }
        }

        private void RefreshPersonalLog()
        {
            foreach (var displayedLog in _displayedLogs)
            {
                Destroy(displayedLog.gameObject);
            }
            _displayedLogs.Clear();

            var logs = _kinlingData.GetPersonalLog();
            foreach (var log in logs)
            {
                var logDisplay = Instantiate(_logPrefab, _logParent);
                logDisplay.gameObject.SetActive(true);
                logDisplay.Init(log, null);
                _displayedLogs.Add(logDisplay);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _kinlingData = null;
        }
    }
}
