using System;
using System.Collections.Generic;
using Characters;
using FunkyCode;
using Managers;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class DoorOpener : MonoBehaviour
    {
        private SpriteRenderer _doorRenderer;
        private BoxCollider2D _boxCollider;
        private List<Kinling> _kinlingsInDoorway = new List<Kinling>();
        private bool _lockedClosed;
        private Light2D _doorLight;
        private bool _showingLight;
        private LightSprite2D _doorLightMask;

        private void Awake()
        {
            _doorRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _doorLight = GetComponentInChildren<Light2D>();
            _doorLightMask = GetComponent<LightSprite2D>();
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
                CloseDoor();
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
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }

        private void OpenDoor()
        {
            _doorRenderer.enabled = false;
            _doorLight.enabled = true;
            _showingLight = true;
            _doorLightMask.enabled = false;
        }

        private void CloseDoor()
        {
            _doorRenderer.enabled = true;
            _doorLight.enabled = false;
            _showingLight = false;
            _doorLightMask.enabled = true;
        }

        private void Update()
        {
            CalculateLight();
        }

        private void CalculateLight()
        {
            if (_showingLight)
            {
                var lightAlpha = EnvironmentManager.Instance.DoorLightInteriorAlpha;
                _doorLight.color.a = lightAlpha;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Kinling kinling = other.GetComponent<Kinling>();
            if (kinling != null)
            {
                if (!_kinlingsInDoorway.Contains(kinling))
                {
                    _kinlingsInDoorway.Add(kinling);
                }
            }
            CheckIfDoorShouldOpen();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Kinling kinling = other.GetComponent<Kinling>();
            if (kinling != null)
            {
                if (_kinlingsInDoorway.Contains(kinling))
                {
                    _kinlingsInDoorway.Remove(kinling);
                }
            }
            CheckIfDoorShouldOpen();
        }
    }
}
