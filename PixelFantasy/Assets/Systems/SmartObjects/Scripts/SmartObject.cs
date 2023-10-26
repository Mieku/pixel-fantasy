using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.SmartObjects.Scripts
{
    public class SmartObject : MonoBehaviour
    {
        [SerializeField] protected string _displayName;
        [SerializeField] protected Transform _interactionMarker;
        public string DisplayName => _displayName;
        
        protected List<BaseInteraction> _cachedInteractions = null;
        public virtual List<BaseInteraction> Interactions
        {
            get
            {
                if (_cachedInteractions == null)
                {
                    _cachedInteractions = new List<BaseInteraction>(GetComponents<BaseInteraction>());
                }

                return _cachedInteractions;
            }
        }

        public void RefreshInteractions()
        {
            _cachedInteractions = new List<BaseInteraction>(GetComponents<BaseInteraction>());
        }

        public Vector2 InteractionPoint =>
            _interactionMarker != null ? _interactionMarker.position : transform.position;

        private void Start()
        {
            SmartObjectManager.Instance.RegisterSmartObject(this);
        }

        private void OnDestroy()
        {
            SmartObjectManager.Instance.DeregisterSmartObject(this);

            foreach (var interaction in Interactions)
            {
                interaction.InterruptInteraction();
            }
        }
    }
}
