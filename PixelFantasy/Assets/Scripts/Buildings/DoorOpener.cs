using System;
using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class DoorOpener : MonoBehaviour
    {
        private SpriteRenderer _doorRenderer;
        private BoxCollider2D _boxCollider;
        private List<Unit> _kinlingsInDoorway = new List<Unit>();
        private bool _lockedClosed;

        private void Awake()
        {
            _doorRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            CheckIfDoorShouldOpen();
        }

        public void LockClosed(bool isLocked)
        {
            if (isLocked)
            {
                _lockedClosed = true;
                _doorRenderer.enabled = true;
            }
            else
            {
                _lockedClosed = false;
                CheckIfDoorShouldOpen();
            }
        }

        private void CheckIfDoorShouldOpen()
        {
            if(_lockedClosed) return;
            
            if (_kinlingsInDoorway.Count == 0)
            {
                _doorRenderer.enabled = true;
            }
            else
            {
                _doorRenderer.enabled = false;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null)
            {
                if (!_kinlingsInDoorway.Contains(unit))
                {
                    _kinlingsInDoorway.Add(unit);
                }
            }
            CheckIfDoorShouldOpen();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Unit unit = other.GetComponent<Unit>();
            if (unit != null)
            {
                if (_kinlingsInDoorway.Contains(unit))
                {
                    _kinlingsInDoorway.Remove(unit);
                }
            }
            CheckIfDoorShouldOpen();
        }
    }
}
