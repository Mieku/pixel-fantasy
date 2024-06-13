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

        public void Init(RelationshipData relationshipData, bool showBG)
        {
            _bg.SetActive(showBG);

            _kinlingName.text = relationshipData.KinlingData.Nickname;
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

            _otherValue.text = relationshipData.TheirOpinionText;
        }
    }
}
