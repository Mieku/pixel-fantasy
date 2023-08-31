using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Buildings
{
    public class Footings : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> _footings;
        [SerializeField] private SpriteRenderer _doorIcon;
        [SerializeField] private GameObject _footingsHandle;
        
        private Color _cantPlaceColour;

        private void Awake()
        {
            _cantPlaceColour = Librarian.Instance.GetColour("Placement Red");
        }

        public void DisplayFootings(bool display)
        {
            _footingsHandle.SetActive(display);
        }

        public bool FootingsValid(List<string> invalidPlacementTags)
        {
            bool result = true;

            foreach (var footing in _footings)
            {
                if (Helper.IsGridPosValidToBuild(footing.transform.position, invalidPlacementTags))
                {
                    footing.gameObject.SetActive(false);
                }
                else
                {
                    footing.gameObject.SetActive(true);
                    footing.color = _cantPlaceColour;
                    result = false;
                }
            }

            return result;
        }
    }
}
