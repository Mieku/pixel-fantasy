using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Social.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class RelationshipDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private TextMeshProUGUI _relationshipType;
        [SerializeField] private TextMeshProUGUI _value;
        [SerializeField] private TextMeshProUGUI _otherValue;
        [SerializeField] private GameObject _bg;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        [SerializeField] private Color _neutralColour;

        public void Init(RelationshipData relationshipData, bool showBG, List<KinlingOptionDisplay> displayedKinlings)
        {
            _bg.SetActive(showBG);

            KinlingData kinlingData =
                displayedKinlings.Find(dK => dK.KinlingData.UniqueID == relationshipData.OthersUID).KinlingData;
            
            KinlingData requestingKinlingData = displayedKinlings.Find(dK => dK.KinlingData.UniqueID == relationshipData.OwnerUID).KinlingData;
            
            _kinlingName.text = kinlingData.Nickname;
            _relationshipType.text = relationshipData.RelationshipTypeName;

            _value.text = relationshipData.OpinionText;
            if (relationshipData.Opinion > 0)
            {
                _value.color = _positiveColour;
            } else if (relationshipData.Opinion < 0)
            {
                _value.color = _negativeColour;
            }
            else
            {
                _value.color = _neutralColour;
            }

            var opinion = GetTheirOpinionText(requestingKinlingData, kinlingData);
            _otherValue.text = opinion;
        }
        
        public string GetTheirOpinionText(KinlingData requesting, KinlingData theirData)
        {
            var theirRelationship = theirData.Relationships.Find(r => r.OthersUID == requesting.UniqueID);
            int theirOpinion = 0;
            if (theirRelationship != null)
            {
                theirOpinion = theirRelationship.Opinion;
            }
                
            if (theirOpinion > 0)
            {
                return $"+{theirOpinion}";
            }
            else
            {
                return $"{theirOpinion}";
            }
        }
    }
}
