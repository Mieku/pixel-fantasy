using System;
using Buildings;
using Characters;
using Managers;
using UnityEngine;

namespace Systems.Notifications.Scripts
{
    public class NotificationManager : Singleton<NotificationManager>
    {
        [SerializeField] private NotificationLogger _logger;
        [SerializeField] private Toaster _toaster;

        public void Toast(string message)
        {
            _toaster.Toast(message);
        }
        
        public void CreateGeneralLog(string message, LogData.ELogType logType, GameTime gameTime = null)
        {
            if (gameTime == null)
            {
                gameTime = EnvironmentManager.Instance.GameTime;
            }
            
            LogData logData = new LogData
            {
                GameTime = gameTime,
                Message = message,
                LogType = logType
            };
            
            SubmitLogData(logData);
        }

        public void CreateKinlingLog(Kinling kinling, string message, LogData.ELogType logType, GameTime gameTime = null)
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
                Payload = kinling.UniqueId,
                PayloadType = LogData.ELogPayloadType.Kinling
            };
            
            SubmitLogData(logData);
        }

        // public void CreateBuildingLog(Building building, string message, LogData.ELogType logType, GameTime gameTime = null)
        // {
        //     if (gameTime == null)
        //     {
        //         gameTime = EnvironmentManager.Instance.GameTime;
        //     }
        //     
        //     LogData logData = new LogData
        //     {
        //         GameTime = gameTime,
        //         Message = message,
        //         LogType = logType,
        //         Payload = building.UniqueId,
        //         PayloadType = LogData.ELogPayloadType.Building
        //     };
        //     
        //     SubmitLogData(logData);
        // }

        private void SubmitLogData(LogData logData)
        {
            _logger.Log(logData);
        }
    }
}
