using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class BuildController : MonoBehaviour
    {
        [SerializeField] private List<CategoryBtn> _catergoryBtns = new List<CategoryBtn>();

        private CategoryBtn _selectedBtn;

        private void Awake()
        {
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        private void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (_selectedBtn != null)
            {
                _selectedBtn.Cancel();
            }
        }

        public void HideOtherOptions(CategoryBtn buttonToKeep)
        {
            foreach (var option in _catergoryBtns.Where(option => option != buttonToKeep))
            {
                option.gameObject.SetActive(false);
            }
        }

        public void ShowAllOptions()
        {
            foreach (var option in _catergoryBtns)
            {
                option.gameObject.SetActive(true);
            }
        }

        public void AssignSelected(CategoryBtn selectedBtn)
        {
            if (_selectedBtn != null)
            {
                _selectedBtn.UnselectButton();
            }

            _selectedBtn = selectedBtn;
        }

        public void AssignUnselected(CategoryBtn btnToUnselect)
        {
            if (_selectedBtn != btnToUnselect)
            {
                Debug.LogError("Attempted to unselect a different button!");
                return;
            }

            _selectedBtn = null;
        }
    }
}
