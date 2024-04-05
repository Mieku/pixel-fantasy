using System;
using System.Collections.Generic;
using Items;
using Managers;
using UnityEngine;

namespace Buildings
{
    public class Footings : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> _footings;
        [SerializeField] private SpriteRenderer _doorIcon;
        [SerializeField] private GameObject _footingsHandle;
        [SerializeField] private SpriteRenderer _doorSpaceFooting;
        
        private Color _cantPlaceColour;
        private GameObject _parent;

        private void Awake()
        {
            _cantPlaceColour = Librarian.Instance.GetColour("Placement Red");
            _parent = transform.parent.gameObject;
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
                if (Helper.IsGridPosValidToBuild(footing.transform.position, invalidPlacementTags, new List<string>() , _parent))
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

        public List<BasicResource> GetClearbleResourcesInFootingsArea()
        {
            List<BasicResource> results = new List<BasicResource>();
            List<Vector2> locations = new List<Vector2>();
            foreach (var footing in _footings)
            {
                locations.Add(footing.transform.position);
            }

            var potentialResources = Helper.GetAllGenericOnGridPositions<BasicResource>(locations);
            foreach (var potentialResource in potentialResources)
            {
                foreach (var location in locations)
                {
                    bool inArea = potentialResource.IsGridInObstacleArea(location);
                    if (inArea)
                    {
                        results.Add(potentialResource);
                        break;
                    }
                }
            }
            
            return results;
        }

        public List<Item> GetItemsInFootingArea()
        {
            List<Item> results;
            List<Vector2> locations = new List<Vector2>();
            foreach (var footing in _footings)
            {
                if (footing != _doorSpaceFooting)
                {
                    locations.Add(footing.transform.position);
                }
            }

            results = Helper.GetAllGenericOnGridPositions<Item>(locations);
            return results;
        }
    }
}
