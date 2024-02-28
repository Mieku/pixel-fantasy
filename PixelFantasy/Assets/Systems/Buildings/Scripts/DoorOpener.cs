using System.Collections.Generic;
using Characters;
using Managers;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    public class DoorOpener : MonoBehaviour
    {
        private SpriteRenderer _doorRenderer;
        private List<Kinling> _kinlingsInDoorway = new List<Kinling>();
        private bool _lockedClosed;
        private bool _isOpen;
        private bool _isFlippedRight;

        [SerializeField] private bool _isVertical;

        private void Awake()
        {
            _doorRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (_isVertical)
            {
                CheckIfDoorShouldOpenVerticalLeft();
            }
            else
            {
                CheckIfDoorShouldOpen();
            }
        }

        public void ColourDoorSprite(Color color)
        {
            if (_doorRenderer != null)
            {
                _doorRenderer.color = color;
            }
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
        
        private void CheckIfDoorShouldOpenVerticalLeft()
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
            _isOpen = true;

            if (_isVertical)
            {
                _doorRenderer.enabled = true;
            }
            else
            {
                _doorRenderer.enabled = false;
            }
        }

        private void CloseDoor()
        {
            _isOpen = false;
            if (_isVertical)
            {
                _doorRenderer.enabled = false;
            }
            else
            {
                _doorRenderer.enabled = true;
            }
        }

        public void KinlingInDoorway(Kinling kinling)
        {
            if (!_kinlingsInDoorway.Contains(kinling))
            {
                _kinlingsInDoorway.Add(kinling);
            }
            
            CheckIfDoorShouldOpen();
        }

        public void KinlingOutOfDoorway(Kinling kinling)
        {
            if (_kinlingsInDoorway.Contains(kinling))
            {
                _kinlingsInDoorway.Remove(kinling);
            }
            
            CheckIfDoorShouldOpen();
        }
        
        public void KinlingInDoorwayVerticalLeft(Kinling kinling)
        {
            if (!_isOpen && !_doorRenderer.flipX)
            {
                _doorRenderer.flipX = true;
            } 
            
            if (!_kinlingsInDoorway.Contains(kinling))
            {
                _kinlingsInDoorway.Add(kinling);
            }

            CheckIfDoorShouldOpen();
        }
        
        public void KinlingInDoorwayVerticalRight(Kinling kinling)
        {
            if (!_isOpen && _doorRenderer.flipX)
            {
                _doorRenderer.flipX = false;
            } 
            
            if (!_kinlingsInDoorway.Contains(kinling))
            {
                _kinlingsInDoorway.Add(kinling);
            }
            
            CheckIfDoorShouldOpen();
        }
    }
}
