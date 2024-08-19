using System;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    public abstract class InteriorAware : MonoBehaviour
    {
        private bool _isInterior;

        private void Start()
        {
            _isInterior = IsInRoom();
            OnInteriorChanged(_isInterior);
        }

        protected void CheckIfInterior()
        {
            var isInRoom = IsInRoom();
            if (_isInterior != isInRoom)
            {
                _isInterior = isInRoom;
                OnInteriorChanged(_isInterior);
            }
        }
        
        private bool IsInRoom()
        {
            var room = GetRoom();
            if (room == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected Room GetRoom()
        {
            return StructureDatabase.Instance.RoomAtWorldPos(transform.position);
        }

        protected abstract void OnInteriorChanged(bool isInterior);
    }
}
