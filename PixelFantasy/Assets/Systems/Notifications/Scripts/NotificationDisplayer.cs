using System;
using System.Collections.Generic;
using Systems.Details.Build_Details.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Notifications.Scripts
{
    public class NotificationData
    {
        public string UniqueID;
        public ENotificationType NotificationType;
        public string NotificationTitle;
        public string NotificationBody;
        public string RequesterUID;
        public Action OnLeftClick;
        public Action OnRightClick;
    }

    [Serializable]
    public enum ENotificationType
    {
        Normal,
        Warning,
        Emergency,
    }
    
    public class NotificationDisplayer : MonoBehaviour
    {
        [SerializeField] private Transform _layout;
        [SerializeField] private NotificationDisplay _normalDisplayPrefab;
        [SerializeField] private NotificationDisplay _warningDisplayPrefab;
        [SerializeField] private NotificationDisplay _emergencyDisplayPrefab;
        [SerializeField] private PanelLayoutRebuilder _rebuilder;

        private Dictionary<string, NotificationDisplay> _displayedNotifications = new Dictionary<string, NotificationDisplay>();

        private void Awake()
        {
            _normalDisplayPrefab.gameObject.SetActive(false);
            _warningDisplayPrefab.gameObject.SetActive(false);
            _emergencyDisplayPrefab.gameObject.SetActive(false);
            _rebuilder.RefreshLayout();
        }

        public void DisplayNotification(NotificationData data, Action onLeftClick = null, Action onRightClick = null)
        {
            NotificationDisplay prefab;
            switch (data.NotificationType)
            {
                case ENotificationType.Normal:
                    prefab = _normalDisplayPrefab;
                    break;
                case ENotificationType.Warning:
                    prefab = _warningDisplayPrefab;
                    break;
                case ENotificationType.Emergency:
                    prefab = _emergencyDisplayPrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Does notification already exist?
            if (_displayedNotifications.ContainsKey(data.UniqueID))
            {
                var existing = _displayedNotifications[data.UniqueID];
                existing.UpdateData(data);
                data.OnLeftClick = onLeftClick;
                data.OnRightClick = onRightClick;
            }
            else
            {
                var notification = Instantiate(prefab, _layout);
                notification.gameObject.SetActive(true);
                notification.Init(data, this);
                data.OnLeftClick = onLeftClick;
                data.OnRightClick = onRightClick;
                _displayedNotifications.Add(data.UniqueID, notification);
            }
            
            _rebuilder.RefreshLayout();
        }

        public void ClearNotification(string uniqueID)
        {
            var notification = _displayedNotifications[uniqueID];
            Destroy(notification.gameObject);
            _displayedNotifications.Remove(uniqueID);
            
            _rebuilder.RefreshLayout();
        }
    }
}
