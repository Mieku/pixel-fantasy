using System;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    public class WorkDisplay : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _progressHandle;
        [SerializeField] private GameObject _canvasHandle;

        private void Awake()
        {
            _canvasHandle.SetActive(false);
        }

        public void DisplayCommand(Command command)
        {
            if (command != null)
            {
                _canvasHandle.SetActive(true);
                _icon.sprite = command.Icon;
            }
            else
            {
                _canvasHandle.SetActive(false);
            }
        }

        public void SetProgress(float percent)
        {
            _progressFill.fillAmount = percent;
        }
    }
}
