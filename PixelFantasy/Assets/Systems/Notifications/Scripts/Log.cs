using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Notifications.Scripts
{
    public class Log : MonoBehaviour
    {
        [SerializeField] private Image _logBG;
        [SerializeField] private TextMeshProUGUI _logMessage;
        [SerializeField] private Color _defaultTextColour;
        [SerializeField] private Color _positiveTextColour;
        [SerializeField] private Color _warningTextColour;
        [SerializeField] private Color _alertTextColour;
        [SerializeField] private Color _defaultLogBGColour;
        [SerializeField] private Color _notificationLogBGColour;
        [SerializeField] private Color _dangerLogBGColour;

        private const string DATE_COLOUR = "#C2C2D1";
        private NotificationLogger _logger;
        private LogData _logData;

        public LogData LogData => _logData;

        private string TimeStampLog
        {
            get
            {
                if (_logData.GameTime == null) return "";

                return $"<color={DATE_COLOUR}>{_logData.TimeStamp}</color>";
            }
        }

        public void Init(LogData logData, NotificationLogger logger)
        {
            _logData = logData;
            _logger = logger;
            
            DisplayLog();
        }

        private void DisplayLog()
        {
            switch (_logData.LogType)
            {
                case LogData.ELogType.Message:
                    _logBG.color = _defaultLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Left;
                    _logMessage.color = _defaultTextColour;
                    _logMessage.text = $"{TimeStampLog}{_logData.Message}";
                    break;
                case LogData.ELogType.Positive:
                    _logBG.color = _defaultLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Left;
                    _logMessage.color = _positiveTextColour;
                    _logMessage.text = $"{TimeStampLog}{_logData.Message}";
                    break;
                case LogData.ELogType.Warning:
                    _logBG.color = _defaultLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Left;
                    _logMessage.color = _warningTextColour;
                    _logMessage.text = $"{TimeStampLog}{_logData.Message}";
                    break;
                case LogData.ELogType.Negative:
                    _logBG.color = _defaultLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Left;
                    _logMessage.color = _alertTextColour;
                    _logMessage.text = $"{TimeStampLog}{_logData.Message}";
                    break;
                case LogData.ELogType.Danger:
                    _logBG.color = _dangerLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Center;
                    _logMessage.color = _defaultTextColour;
                    _logMessage.text = $"{_logData.Message}";
                    break;
                case LogData.ELogType.Notification:
                    _logBG.color = _notificationLogBGColour;
                    _logMessage.alignment = TextAlignmentOptions.Center;
                    _logMessage.color = _defaultTextColour;
                    _logMessage.text = $"{_logData.Message}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnClick()
        {
            switch (_logData.PayloadType)
            {
                case LogData.ELogPayloadType.None:
                    break;
                case LogData.ELogPayloadType.Kinling:
                    KinlingsManager.Instance.SelectKinling(_logData.Payload);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnRightClick()
        {
            switch (_logData.LogType)
            {
                case LogData.ELogType.Message:
                case LogData.ELogType.Positive:
                case LogData.ELogType.Warning:
                case LogData.ELogType.Negative:
                    break;
                case LogData.ELogType.Danger:
                case LogData.ELogType.Notification:
                    _logger.ClearLog(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class LogData
    {
        public string Message;
        public ELogType LogType;
        public string Payload;
        public ELogPayloadType PayloadType;
        public GameTime GameTime;

        public string TimeStamp => $"Day {GameTime.Day}  ";
        
        public enum ELogType
        {
            Message,
            Positive,
            Warning,
            Negative,
            Danger,
            Notification,
        }
        
        public enum ELogPayloadType
        {
            None,
            Kinling,
        }
    }
}
