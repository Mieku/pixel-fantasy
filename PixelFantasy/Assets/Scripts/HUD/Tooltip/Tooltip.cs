using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD.Tooltip
{
    [ExecuteInEditMode()]
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _header;
        [SerializeField] private TextMeshProUGUI _content;
        [SerializeField] private LayoutElement _layoutElement;
        [SerializeField] private int _charWrapLimit;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void SetText(string content, string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                _header.gameObject.SetActive(false);
            }
            else
            {
                _header.gameObject.SetActive(true);
                _header.text = header;
            }

            _content.text = content;
            
            RefreshLayout();
        }
        
        private void RefreshLayout()
        {
            int headerLength = _header.text.Length;
            int contentLength = _content.text.Length;

            _layoutElement.enabled = (headerLength > _charWrapLimit || contentLength > _charWrapLimit) ? true : false;
        }
        
        private void Update()
        {
            if (Application.isEditor)
            {
                RefreshLayout();
            }

            Vector2 pos = Input.mousePosition;

            float pivotX = pos.x / Screen.width;
            float pivotY = pos.y / Screen.height;

            _rectTransform.pivot = new Vector2(pivotX, pivotY);
            transform.position = pos;
        }
    }
}
