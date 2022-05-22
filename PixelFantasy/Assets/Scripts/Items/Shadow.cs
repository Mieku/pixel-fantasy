using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Shadow : MonoBehaviour
    {
        private SpriteRenderer _parentSprite;
        private SpriteRenderer _shadowRenderer;
        
        private void Awake()
        {
            _parentSprite = transform.parent.GetComponent<SpriteRenderer>();
            _shadowRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            RefreshShadow();
        }

        public void RefreshShadow()
        {
            _shadowRenderer.sprite = _parentSprite.sprite;
            _shadowRenderer.sortingOrder = _parentSprite.sortingOrder - 1;
            transform.localScale = Vector3.one;
            transform.localPosition = new Vector3(0f, -0.25f, 0f);
        }

        [Button("RefreshShadow")]
        private void RefreshShadowBtn()
        {
            _parentSprite = transform.parent.GetComponent<SpriteRenderer>();
            _shadowRenderer = GetComponent<SpriteRenderer>();
            
            RefreshShadow();
        }
    }
}
