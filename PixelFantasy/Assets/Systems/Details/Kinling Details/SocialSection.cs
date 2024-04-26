using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using NUnit.Framework;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class SocialSection : MonoBehaviour
    {
        [SerializeField] private RelationshipDisplay _relationshipDisplayPrefab;
        [SerializeField] private Transform _displayParent;

        private KinlingData _kinlingData;
        private Action _refreshLayoutCallback;
        private List<RelationshipDisplay> _displayedRelationships = new List<RelationshipDisplay>();

        public void ShowSection(KinlingData kinlingData, Action refreshLayoutCallback)
        {
            _kinlingData = kinlingData;
            _refreshLayoutCallback = refreshLayoutCallback;
            gameObject.SetActive(true);
            _relationshipDisplayPrefab.gameObject.SetActive(false);
            
            RefreshContent();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _kinlingData = null;

            foreach (var displayedRelationship in _displayedRelationships)
            {
                Destroy(displayedRelationship.gameObject);
            }
            _displayedRelationships.Clear();
        }

        public void RefreshContent()
        {
            // Synchronize the _displayedRelationships list with the current relationships data
            for (int i = _displayedRelationships.Count - 1; i >= 0; i--)
            {
                var displayedRelationship = _displayedRelationships[i];
                if (!_kinlingData.Relationships.Contains(displayedRelationship.Relationship))
                {
                    // Remove the relationship display if it's no longer in the data source
                    _displayedRelationships.RemoveAt(i);
                    Destroy(displayedRelationship.gameObject);
                }
                else
                {
                    // Refresh the existing displays that are still valid
                    displayedRelationship.Refresh(i);
                }
            }

            // Add new relationship displays for any new relationships found in the data source
            foreach (var relationship in _kinlingData.Relationships)
            {
                if (!_displayedRelationships.Any(display => display.Relationship == relationship))
                {
                    var newDisplay = Instantiate(_relationshipDisplayPrefab, _displayParent);
                    newDisplay.Init(relationship, _kinlingData, _displayedRelationships.Count);
                    newDisplay.gameObject.SetActive(true);
                    _displayedRelationships.Add(newDisplay);
                }
            }
            
            _refreshLayoutCallback.Invoke();
        }

    }
}
