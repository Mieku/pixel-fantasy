using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Traits.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Social.Scripts
{
    public class SocialAI : MonoBehaviour
    {
        [SerializeField] private Unit _unit;
        [SerializeField] private GameObject _speechBubbleHandle;
        [SerializeField] private SpriteRenderer _speechTopicIcon;
        [SerializeField] private SocialTopicOptionsData _socialTopics;
        [SerializeField] private SocialTopicOptionsData _romanticTopics;
        [SerializeField] private SocialTopicOptionsData _positiveResponses;
        [SerializeField] private SocialTopicOptionsData _negativeResponses;

        public bool AvailableToChat => _state == ESocialState.Available;
        public string UniqueId => _unit.UniqueId;

        private const float CHAT_COOLDOWN = 5.0f;
        private const float SOCIAL_RADIUS = 6f;
        private const float BUBBLE_DURATION = 4.0f;
        private const float CHAT_SOCIAL_NEED_BENEFIT = 0.1f;
        private const int POSITIVE_INTERACTION_SCORE = 1;
        private const int NEGATIVE_INTERACTION_SCORE = -1;
        
        private ESocialState _state;
        private float _chatTimer;
        private float _bubbleTimer;

        private List<RelationshipState> _relationships = new List<RelationshipState>();


        public enum ESocialState
        {
            Available,
            Chatting
        }
        
        private void Update()
        {
            if (_state == ESocialState.Available)
            {
                _chatTimer += TimeManager.Instance.DeltaTime;
                if (_chatTimer >= CHAT_COOLDOWN)
                {
                    // Try to chat
                    var nearbyKinlings = NearbyKinlings();
                    if (nearbyKinlings.Count > 0)
                    {
                        var kinlingToChatWith = ChooseKinlingToChatWith(nearbyKinlings);
                        if (kinlingToChatWith != null)
                        {
                            BeginChat(kinlingToChatWith);
                        }
                        else
                        {
                            // Just in case...
                            _chatTimer = 0;
                        }
                    }
                    else // No one is available
                    {
                        _chatTimer = 0;
                    }
                }
            }

            if (_state == ESocialState.Chatting)
            {
                _bubbleTimer += TimeManager.Instance.DeltaTime;
                if (_bubbleTimer >= BUBBLE_DURATION)
                {
                    EndChat();
                }
            }
        }

        private void BeginChat(SocialAI targetKinling)
        {
            _state = ESocialState.Chatting;

            var topic = _socialTopics.GetRandomTopic();
            DisplayChatBubble(topic);
            targetKinling.RecieveChat(this, GetChatResponse);
        }

        public void RecieveChat(SocialAI otherKinling, Action<bool, SocialAI> onResponse)
        {
            _state = ESocialState.Chatting;
            bool isPositiveResponse = DetermineResponse(otherKinling);
            SocialTopic responseTopic;
            if (isPositiveResponse)
            {
                responseTopic = _positiveResponses.GetRandomTopic();
            }
            else
            {
                responseTopic = _negativeResponses.GetRandomTopic();
            }

            StartCoroutine(ResponseSequence(otherKinling, responseTopic, isPositiveResponse, onResponse));
        }

        IEnumerator ResponseSequence(SocialAI otherKinling, SocialTopic topic, bool isPositiveResponse, Action<bool, SocialAI> onResponse)
        {
            yield return new WaitForSeconds(1);
            
            DisplayChatBubble(topic);

            onResponse.Invoke(isPositiveResponse, this);

            var otherKinlingRelationshipState = GetRelationshipState(otherKinling);

            if (isPositiveResponse)
            {
                otherKinlingRelationshipState.AddToScore(POSITIVE_INTERACTION_SCORE);
            }
            else
            {
                otherKinlingRelationshipState.AddToScore(NEGATIVE_INTERACTION_SCORE);
            }
        }

        private bool DetermineResponse(SocialAI otherKinling)
        {
            // TODO: Make this based on their cohesion, and mood
            
            // For now 50/50
            int random = Random.Range(0, 2);
            if (random == 0) return true;

            return false;
        }

        private void GetChatResponse(bool isPositive, SocialAI responder)
        {
            var responderRelationshipState = GetRelationshipState(responder);

            if (isPositive)
            {
                responderRelationshipState.AddToScore(POSITIVE_INTERACTION_SCORE);
            }
            else
            {
                responderRelationshipState.AddToScore(NEGATIVE_INTERACTION_SCORE);
            }
            
            _unit.NeedsAI.UpdateIndividualStat(Librarian.Instance.GetStat("Social"), CHAT_SOCIAL_NEED_BENEFIT, StatTrait.ETargetType.Impact);
        }

        private void DisplayChatBubble(SocialTopic topic)
        {
            _speechBubbleHandle.SetActive(true);
            _speechTopicIcon.sprite = topic.TopicIcon;
        }

        private void EndChat()
        {
            _speechBubbleHandle.SetActive(false);

            _bubbleTimer = 0;
            _chatTimer = 0;
            _state = ESocialState.Available;
        }

        private SocialAI ChooseKinlingToChatWith(List<SocialAI> options)
        {
            List<WeightedObject<SocialAI>> weightedRelationships =
                new List<WeightedObject<SocialAI>>();
            foreach (var option in options)
            {
                var relationshipState = GetRelationshipState(option);
                var weight = relationshipState.InteractionWeight;
                WeightedObject<SocialAI> weightedState =
                    new WeightedObject<SocialAI>(option, weight);
                weightedRelationships.Add(weightedState);
            }

            var cumulativeDistribution = Helper.GenerateCumulativeDistribution(weightedRelationships);
            var randomKinling = Helper.GetRandomObject(cumulativeDistribution);

            return randomKinling;
        }

        private List<SocialAI> NearbyKinlings()
        {
            bool isIndoors = _unit.IsIndoors();
            var allUnits = UnitsManager.Instance.GetAllUnitsInRadius(transform.position, SOCIAL_RADIUS);
            List<SocialAI> results = new List<SocialAI>();
            foreach (var unit in allUnits)
            {
                if (unit != _unit)
                {
                    if (isIndoors == unit.IsIndoors() && unit.SocialAI.AvailableToChat)
                    {
                        results.Add(unit.SocialAI);
                    }
                }
            }

            return results;
        }

        private RelationshipState GetRelationshipState(SocialAI otherKinling)
        {
            string otherUID = otherKinling._unit.UniqueId;
            RelationshipState result = _relationships.Find(state => state.KinlingUniqueID == otherUID);
            if (result == null)
            {
                result = new RelationshipState(otherKinling);
                _relationships.Add(result);
            }

            return result;
        }
    }
    
    
    

}
