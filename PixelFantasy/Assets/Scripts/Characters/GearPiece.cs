using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    public class GearPiece : MonoBehaviour
    {
        [SerializeField] private GameObject _sideView;
        [SerializeField] private GameObject _upView;
        [SerializeField] private GameObject _downView;

        private UnitActionDirection _curDirection;
        
        public void AssignDirection(UnitActionDirection direction)
        {
            _curDirection = direction;

            switch (direction)
            {
                case UnitActionDirection.Side:
                    _sideView.SetActive(true);
                    _upView.SetActive(false);
                    _downView.SetActive(false);
                    break;
                case UnitActionDirection.Up:
                    _sideView.SetActive(false);
                    _upView.SetActive(true);
                    _downView.SetActive(false);
                    break;
                case UnitActionDirection.Down:
                    _sideView.SetActive(false);
                    _upView.SetActive(false);
                    _downView.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
