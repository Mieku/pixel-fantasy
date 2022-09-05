using System;
using UnityEngine;

namespace HUD.Tooltip
{
    public class TooltipSystem : MonoBehaviour
    {
        private static TooltipSystem _current;

        public Tooltip Tooltip;

        public void Awake()
        {
            _current = this;
        }

        private void Start()
        {
            Hide();
        }

        public static void Show(string content, string header = "")
        {
            _current.Tooltip.SetText(content, header);
            _current.Tooltip.gameObject.SetActive(true);
        }
        
        public static void Hide()
        {
            _current.Tooltip.gameObject.SetActive(false);
        }
    }
}
