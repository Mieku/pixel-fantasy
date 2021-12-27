using System;
using Character.Interfaces;
using UnityEngine;

namespace Character
{
    public class UnitSelector : MonoBehaviour
    {
        private GameObject selectedIcon;
        private IMovePosition movePosition;
        
        private void Awake()
        {
            selectedIcon = transform.Find("SelectionMarker").gameObject;
            SetSelectionVisible(false);
            movePosition = GetComponent<IMovePosition>();
        }

        public void SetSelectionVisible(bool visible)
        {
            selectedIcon.SetActive(visible);
        }

        public void MoveTo(Vector3 targetPos)
        {
            movePosition.SetMovePosition(targetPos);
        }
    }
}
