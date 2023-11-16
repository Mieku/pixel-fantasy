using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Systems.Notifications.Scripts
{
    public class NotificationLogger : MonoBehaviour
    {
        [SerializeField] private GameObject _panelHandle;
        [SerializeField] private Log _logPrefab;
        [SerializeField] private Transform _logParent;
        [SerializeField] private int _maxLogAgeDays;

        private List<Log> _topperLogs = new List<Log>();
        private List<Log> _displayedLogs = new List<Log>();

        private void Awake()
        {
            GameEvents.HourTick += GameEvent_HourTick;
        }

        private void OnDestroy()
        {
            GameEvents.HourTick -= GameEvent_HourTick;
        }

        private void GameEvent_HourTick(int hour)
        {
            if (hour == 1)
            {
                CleanOldLogs();
            }
        }

        public void Show()
        {
            _panelHandle.SetActive(true);
        }

        public void Hide()
        {
            _panelHandle.SetActive(false);
        }

        public void Log(LogData logData)
        {
            switch (logData.LogType)
            {
                case LogData.ELogType.Message:
                case LogData.ELogType.Positive:
                case LogData.ELogType.Warning:
                case LogData.ELogType.Alert:
                    CreateNormalLog(logData);
                    break;
                case LogData.ELogType.Danger:
                case LogData.ELogType.Notification:
                    CreateTopperLog(logData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateNormalLog(LogData logData)
        {
            var log = Instantiate(_logPrefab, _logParent);
            log.Init(logData, this);
            log.transform.SetSiblingIndex(GetNormalLogStartIndex());
            _displayedLogs.Add(log);
        }

        private void CreateTopperLog(LogData logData)
        {
            var topperLog = Instantiate(_logPrefab, _logParent);
            topperLog.Init(logData, this);
            _topperLogs.Add(topperLog);
        }

        private int GetNormalLogStartIndex()
        {
            int result = 0;

            if (_displayedLogs.Count == 0)
            {
                return 0;
            }
            
            foreach (var log in _displayedLogs)
            {
                var logIndex = log.transform.GetSiblingIndex();
                if (logIndex > result)
                {
                    result = logIndex;
                }
            }
            
            return result + 1;
        }
        
        public void ClearLog(Log logToClear)
        {
            if (_topperLogs.Contains(logToClear))
            {
                _topperLogs.Remove(logToClear);
            } 
            else if (_displayedLogs.Contains(logToClear))
            {
                _displayedLogs.Remove(logToClear);
            }
            
            Destroy(logToClear.gameObject);
        }

        private void CleanOldLogs()
        {
            int currentDay = EnvironmentManager.Instance.GameTime.Day;
            int minDay = Mathf.Max(1, currentDay - _maxLogAgeDays);

            List<Log> _expiredLogs = new List<Log>();
            
            foreach (var displayedLog in _displayedLogs)
            {
                if (displayedLog.LogData.GameTime != null)
                {
                    var logDay = displayedLog.LogData.GameTime.Day;
                    if (logDay <= minDay)
                    {
                        _expiredLogs.Add(displayedLog);
                    }
                }
            }

            foreach (var expiredLog in _expiredLogs)
            {
                ClearLog(expiredLog);
            }
        }
    }
}
