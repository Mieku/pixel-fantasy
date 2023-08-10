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

        private Color _canPlaceColour;
        private Color _cantPlaceColour;

        private void Awake()
        {
            _canPlaceColour = Librarian.Instance.GetColour("Placement Green");
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
                    footing.color = _canPlaceColour;
                }
                else
                {
                    footing.color = _cantPlaceColour;
                    result = false;
                }
            }

            return result;
        }
    }
}
