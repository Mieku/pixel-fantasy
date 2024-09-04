using System;
using Characters;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Systems.Notifications.Scripts
{
    public class NotificationManager : Singleton<NotificationManager>
    {
        [SerializeField] private NotificationDisplayer _notificationDisplayer;
        [SerializeField] private Toaster _toaster;

        public void Toast(string message)
        {
            _toaster.Toast(message);
        }
        
        public void CreatePersonalLog(Kinling kinling, string message, LogData.ELogType logType,
            GameTime gameTime = null)
        {
            if (gameTime == null)
            {
                gameTime = EnvironmentManager.Instance.GameTime;
            }
            
            LogData logData = new LogData
            {
                GameTime = gameTime,
                Message = message,
                LogType = logType,
                Payload = kinling.RuntimeData.UniqueID,
                PayloadType = LogData.ELogPayloadType.Kinling
            };
            
            kinling.RuntimeData.SubmitPersonalLog(logData);
        }

        public NotificationData CreateNotification(ENotificationType notificationType, string title, string message,
            PlayerInteractable requester = null)
        {
            NotificationData notificationData = new NotificationData
            {
                UniqueID = $"Notification_{Guid.NewGuid()}",
                NotificationType = notificationType,
                NotificationTitle = title,
                NotificationBody = message
            };

            if (requester != null)
            {
                notificationData.RequesterUID = requester.UniqueID;
            }
            
            _notificationDisplayer.DisplayNotification(notificationData);
            
            return notificationData;
        }

        public NotificationData UpdateNotification(string uniqueID, ENotificationType notificationType, string title, string message,
            PlayerInteractable requester = null)
        {
            NotificationData notificationData = new NotificationData
            {
                UniqueID = uniqueID,
                NotificationType = notificationType,
                NotificationTitle = title,
                NotificationBody = message
            };
            
            if (requester != null)
            {
                notificationData.RequesterUID = requester.UniqueID;
            }
            
            _notificationDisplayer.DisplayNotification(notificationData);
            return notificationData;
        }

        public void ClearNotification(string uniqueID)
        {
            _notificationDisplayer.ClearNotification(uniqueID);
        }
    }
}
