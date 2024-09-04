using System;
using Controllers;
using HUD.Tooltip;
using TMPro;
using UnityEngine;

namespace Systems.Notifications.Scripts
{
    public class NotificationDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TooltipTrigger _tooltip;

        private NotificationData _data;
        private NotificationDisplayer _parent;

        public void Init(NotificationData data, NotificationDisplayer parent)
        {
            _parent = parent;
            _data = data;
            _titleText.text = _data.NotificationTitle;
            
            _tooltip.Header = _data.NotificationTitle;
            _tooltip.Content = _data.NotificationBody;
        }

        public void UpdateData(NotificationData data)
        {
            _data = data;
            _titleText.text = _data.NotificationTitle;
            
            _tooltip.Header = _data.NotificationTitle;
            _tooltip.Content = _data.NotificationBody;
        }

        public void OnLeftClick()
        {
            if (!string.IsNullOrEmpty(_data.RequesterUID))
            {
                var requester = PlayerInteractableDatabase.Instance.Query(_data.RequesterUID);
                if (requester != null)
                {
                    SelectionManager.Instance.Select(requester);
                    CameraManager.Instance.LookAtPosition(requester.transform.position);
                }
            }
            
            _data.OnLeftClick?.Invoke();
        }

        public void OnRightClick()
        {
            _data.OnRightClick?.Invoke();
        }
    }
}
