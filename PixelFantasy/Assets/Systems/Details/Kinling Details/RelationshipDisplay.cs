using Characters;
using Controllers;
using Managers;
using Systems.Social.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Kinling_Details
{
    public class RelationshipDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _relationshipType;
        [SerializeField] private TextMeshProUGUI _opinion;
        [SerializeField] private GameObject _bg;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;

        public RelationshipData Relationship;
        private KinlingData _thisKinling;

        public void Init(RelationshipData relationship, KinlingData thisKinling, int index)
        {
            Relationship = relationship;
            _thisKinling = thisKinling;

            Refresh(index);
        }

        public void Refresh(int index)
        {
            var kinling = KinlingsDatabase.Instance.Query(Relationship.OthersUID);
            _name.text = kinling.Fullname;
            _relationshipType.text = Relationship.RelationshipTypeName;

            string opinionText;
            if (Relationship.Opinion > 0)
            {
                opinionText = $"<color={Helper.ColorToHex(_positiveColour)}>{Relationship.OpinionText}</color>";
            }
            else if (Relationship.Opinion < 0)
            {
                opinionText = $"<color={Helper.ColorToHex(_negativeColour)}>{Relationship.OpinionText}</color>";
            }
            else
            {
                opinionText = $"0";
            }

            string othersOpinionText;
            var theirRelationship = kinling.Relationships.Find(state => state.OthersUID == _thisKinling.UniqueID);
            if (theirRelationship == null)
            {
                othersOpinionText = $"0";
            }
            else
            {
                if (theirRelationship.Opinion > 0)
                {
                    othersOpinionText = $"<color={Helper.ColorToHex(_positiveColour)}>{theirRelationship.OpinionText}</color>";
                }
                else if (theirRelationship.Opinion < 0)
                {
                    othersOpinionText = $"<color={Helper.ColorToHex(_negativeColour)}>{theirRelationship.OpinionText}</color>";
                }
                else
                {
                    othersOpinionText = $"0";
                }
            }

            _opinion.text = $"{opinionText}  ({othersOpinionText})";

            int modulus = index % 2;
            _bg.SetActive(modulus == 0);
        }

        public void OnPressed()
        {
            var kinlingData = KinlingsDatabase.Instance.Query(Relationship.OwnerUID);
            PlayerInputController.Instance.SelectUnit(kinlingData.GetKinling());
            CameraManager.Instance.LookAtPosition(kinlingData.Position);
        }
    }
}
