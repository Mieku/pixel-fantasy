using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Managers
{
    public class RoofManager : Singleton<RoofManager>
    {
        //[SerializeField] private Tilemap _roofTM;

        private bool _roofsShown;
        public bool RoofsShown => _roofsShown;

        private void Start()
        {
            ShowRoofs(false);
        }

        private void Update()
        {
            DetectKeyboardInput();
        }

        private void DetectKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleRoofs();
            }
        }

        public void ShowRoofs(bool isShown)
        {
            _roofsShown = isShown;
            //_roofTM.gameObject.SetActive(isShown);
            GameEvents.Trigger_OnRoofGuideToggled(isShown);
        }

        public void ToggleRoofs()
        {
            ShowRoofs(!_roofsShown);
        }
    }
}
