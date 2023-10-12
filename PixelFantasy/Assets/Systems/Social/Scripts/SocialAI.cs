using Characters;
using UnityEngine;

namespace Systems.Social.Scripts
{
    public class SocialAI : MonoBehaviour
    {
        [SerializeField] private Unit _unit;
        [SerializeField] private GameObject _speechBubbleHandleLeft;
        [SerializeField] private SpriteRenderer _speechTopicIconLeft;
        [SerializeField] private GameObject _speechBubbleHandleRight;
        [SerializeField] private SpriteRenderer _speechTopicIconRight;
        [SerializeField] private SocialTopicOptionsData _socialTopics;
        [SerializeField] private SocialTopicOptionsData _romanticTopics;
        [SerializeField] private SocialTopicOptionsData _positiveResponses;
        [SerializeField] private SocialTopicOptionsData _negativeResponses;

        private const float CHAT_COOLDOWN = 5.0f;
        private ESocialState _state;


        public enum ESocialState
        {
            Available,
            Chatting
        }
    }
}
