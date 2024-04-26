using Systems.Social.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Popups.Kinling_Info_Popup
{
    public class RelationshipDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private TextMeshProUGUI _relationshipType;
        [SerializeField] private TextMeshProUGUI _opinion;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;

        private RelationshipState _relationshipState;

        public void Init(RelationshipState relationshipState)
        {
            _relationshipState = relationshipState;
           // _kinlingName.text = _relationshipState.Kinling.FullName;
            _relationshipType.text = _relationshipState.RelationshipTypeName;
            if (_relationshipState.Opinion >= 0)
            {
                _opinion.color = _positiveColour;
                _opinion.text = "+" + _relationshipState.Opinion;
            }
            else
            {
                _opinion.color = _negativeColour;
                _opinion.text = "" + _relationshipState.Opinion;
            }
        }
    }
}
