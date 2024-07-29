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
                Payload = kinling.RuntimeData.UniqueID,
                PayloadType = LogData.ELogPayloadType.Kinling
            };
            
            SubmitLogData(logData);
            kinling.RuntimeData.SubmitPersonalLog(logData);
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

        private void SubmitLogData(LogData logData)
        {
            _logger.Log(logData);
        }
    }
}
