using System;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Notifications.Scripts
{
    public class BuildingNotification : MonoBehaviour
    {
        [SerializeField] private Image _notificationIcon;

        [SerializeField] private Sprite _upgradeIcon;
        [SerializeField] private Sprite _issueIcon;

        private ENotificationType _notificationType;

        private void Awake()
        {
            SetNotification(ENotificationType.None);
        }

        public void SetNotification(ENotificationType notificationType)
        {
            _notificationType = notificationType;
            switch (_notificationType)
            {
                case ENotificationType.None:
                    _notificationIcon.gameObject.SetActive(false);
                    break;
                case ENotificationType.Issue:
                    _notificationIcon.gameObject.SetActive(true);
                    _notificationIcon.sprite = _issueIcon;
                    break;
                case ENotificationType.Upgrade:
                    _notificationIcon.gameObject.SetActive(true);
                    _notificationIcon.sprite = _upgradeIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum ENotificationType
        {
            None,
            Issue,
            Upgrade,
        }
    }
}
