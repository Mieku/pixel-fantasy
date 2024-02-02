using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoSocialContent : MonoBehaviour
    {
        [SerializeField] private Transform _familyLayout;
        [SerializeField] private Transform _kinLayout;
        [SerializeField] private Transform _outsiderLayout;
        [SerializeField] private RelationshipDisplay _relationshipDisplayPrefab;
        
        private Kinling _kinling;
        private List<RelationshipDisplay> _displayedRelationships = new List<RelationshipDisplay>();
        
        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            
            gameObject.SetActive(true);
            Refresh();
        }
        
        public void Close()
        {
            ClearDisplayedRelationships();
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            ClearDisplayedRelationships();

            var allRelationships = _kinling.SocialAI.Relationships;
            foreach (var relationship in allRelationships)
            {
                if (relationship.IsPartner)
                {
                    var relationshipDisplay = Instantiate(_relationshipDisplayPrefab, _familyLayout);
                    relationshipDisplay.Init(relationship);
                    _displayedRelationships.Add(relationshipDisplay);
                }
                else
                {
                    var relationshipDisplay = Instantiate(_relationshipDisplayPrefab, _kinLayout);
                    relationshipDisplay.Init(relationship);
                    _displayedRelationships.Add(relationshipDisplay);
                }
            }
        }

        private void ClearDisplayedRelationships()
        {
            foreach (var displayedRelationship in _displayedRelationships)
            {
                Destroy(displayedRelationship.gameObject);
            }
            _displayedRelationships.Clear();
        }
    }
}
