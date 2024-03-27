using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Data.Item;
using Managers;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using Systems.Skills.Scripts;
using Systems.Traits.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Systems.Social.Scripts
{
    public class SocialAI : MonoBehaviour
    {
        [FormerlySerializedAs("_unit")] [SerializeField] private Kinling _kinling;
        [SerializeField] private GameObject _speechBubbleHandle;
        [SerializeField] private SpriteRenderer _speechTopicIcon;
        [SerializeField] private SocialTopicSettings _socialTopics;
        [SerializeField] private SocialTopicSettings _romanticTopics;
        [SerializeField] private SocialTopicSettings _positiveResponses;
        [SerializeField] private SocialTopicSettings _negativeResponses;

        public bool AvailableToChat => _state == ESocialState.Available && !_kinling.RuntimeData.IsAsleep;
        public string UniqueId => _kinling.UniqueId;

        private const float CHAT_COOLDOWN = 5.0f;
        private const float SOCIAL_RADIUS = 6f;
        private const float BUBBLE_DURATION = 4.0f;
        private const float CHAT_SOCIAL_NEED_BENEFIT = 0.1f;
        private const int POSITIVE_INTERACTION_SCORE = 1;
        private const int NEGATIVE_INTERACTION_SCORE = -1;
        private const int COHESION_BASE = 50;
        private const int ROMANTIC_COHESION_BASE = 20;
        private const int MOOD_COHESION_BASE = 20;
        private const int MIN_OPINION_TO_FLIRT = 15;
        private const float BASE_ATTEMPT_FLIRTING_CHANCE = 0.10f;
        
        private ESocialState _state;
        private float _chatTimer;
        private float _bubbleTimer;

        private List<RelationshipState> _relationships = new List<RelationshipState>();
        public List<RelationshipState> Relationships => _relationships;


        public enum ESocialState
        {
            Available,
            Chatting
        }
        
        private void Update()
        {
            if (_kinling.RuntimeData.IsAsleep) return;
            
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

        public void ForceFlirtChatBubble()
        {
            var topic = _romanticTopics.GetRandomTopic();
            DisplayChatBubble(topic);

            _chatTimer = 0;
            _bubbleTimer = 0; 
            _state = ESocialState.Chatting;
        }

        private void BeginChat(SocialAI targetKinling)
        {
            _state = ESocialState.Chatting;

            bool tryFlirting = CheckShouldFlirt(targetKinling);
            if (tryFlirting)
            {
                InitiateFlirt(targetKinling);
            }
            else
            {
                // Chit-chatting
                InitiateChitChat(targetKinling);
            }
        }

        private bool CheckShouldFlirt(SocialAI targetKinling)
        {
            var relationship = GetRelationshipState(targetKinling);
            // Make sure they are not in a romantic relationship with someone else
            if (_kinling.RuntimeData.Partner != null) return false;
            if (targetKinling._kinling.RuntimeData.Partner != null) return false;
            // Make sure they are an appropriate age
            if (_kinling.RuntimeData.MaturityStage < EMaturityStage.Adult) return false;
            if (_kinling.RuntimeData.MaturityStage != targetKinling._kinling.RuntimeData.MaturityStage) return false;
            // Make sure they align with their sexual preference
            if (!_kinling.IsKinlingAttractedTo(targetKinling._kinling)) return false;
            if (!targetKinling._kinling.IsKinlingAttractedTo(_kinling)) return false;
            // Make sure their opinion is high enough
            if (relationship.Opinion < MIN_OPINION_TO_FLIRT) return false;

            float attemptFlirtingChance = BASE_ATTEMPT_FLIRTING_CHANCE + (relationship.OverallCohesion / 100f);
            float roll = Random.Range(0f, 1f);
            return roll <= attemptFlirtingChance;
        }

        private void InitiateFlirt(SocialAI targetKinling)
        {
            var topic = _romanticTopics.GetRandomTopic();
            DisplayChatBubble(topic);
            targetKinling.RecieveFlirt(this, GetFlirtResponse);
        }
        
        public void RecieveFlirt(SocialAI otherKinling, Action<bool, SocialAI> onResponse)
        {
            _state = ESocialState.Chatting;
            bool isPositiveResponse = DetermineFlirtResponse(otherKinling);
            SocialTopic responseTopic;
            if (isPositiveResponse)
            {
                responseTopic = _positiveResponses.GetRandomTopic();
                FormRomanticRelationship(otherKinling);
            }
            else
            {
                responseTopic = _negativeResponses.GetRandomTopic();
            }

            StartCoroutine(ResponseSequence(otherKinling, responseTopic, isPositiveResponse, onResponse));
        }
        
        private bool DetermineFlirtResponse(SocialAI otherKinling)
        {
            // This is based on their cohesion, relationship and mood
            int weight = ROMANTIC_COHESION_BASE;
            var otherKinlingRelationship = GetRelationshipState(otherKinling);
            var curOverallMood = _kinling.KinlingMood.OverallMood / 100f;
            int moodCohesion = (int)(MOOD_COHESION_BASE * curOverallMood) - (MOOD_COHESION_BASE / 2);
            
            weight += otherKinlingRelationship.OverallCohesion;
            weight += moodCohesion;

            weight = Mathf.Clamp(weight, 2, 98);

            int random100 = Random.Range(1, 101);
            return random100 <= weight;
        }
        
        private void GetFlirtResponse(bool isPositive, SocialAI responder)
        {
            var responderRelationshipState = GetRelationshipState(responder);

            if (isPositive)
            {
                responderRelationshipState.AddToScore(POSITIVE_INTERACTION_SCORE);
                FormRomanticRelationship(responder);
            }
            else
            {
                responderRelationshipState.AddToScore(NEGATIVE_INTERACTION_SCORE);
                _kinling.KinlingMood.ApplyEmotion(Librarian.Instance.GetEmotion("Rejected")); // Mood De-buff
            }
            
            //_unit.NeedsAI.UpdateIndividualStat(Librarian.Instance.GetStat("Social"), CHAT_SOCIAL_NEED_BENEFIT, StatTrait.ETargetType.Impact);
        }

        private void InitiateChitChat(SocialAI targetKinling)
        {
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
            // This is based on their cohesion, relationship and mood
            int weight = COHESION_BASE;
            var otherKinlingRelationship = GetRelationshipState(otherKinling);
            var curOverallMood = _kinling.KinlingMood.OverallMood / 100f;
            int moodCohesion = (int)(MOOD_COHESION_BASE * curOverallMood) - (MOOD_COHESION_BASE / 2);
            
            weight += otherKinlingRelationship.OverallCohesion;
            weight += moodCohesion;

            weight = Mathf.Clamp(weight, 2, 98);

            int random100 = Random.Range(1, 101);
            return random100 <= weight;
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
            
            //_unit.NeedsAI.UpdateIndividualStat(Librarian.Instance.GetStat("Social"), CHAT_SOCIAL_NEED_BENEFIT, StatTrait.ETargetType.Impact);
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
            var allUnits = KinlingsManager.Instance.GetAllUnitsInRadius(transform.position, SOCIAL_RADIUS);
            List<SocialAI> results = new List<SocialAI>();
            foreach (var unit in allUnits)
            {
                if (unit != _kinling)
                {
                    if (unit.SocialAI.AvailableToChat)
                    {
                        results.Add(unit.SocialAI);
                    }
                }
            }

            return results;
        }

        private RelationshipState GetRelationshipState(SocialAI otherKinling)
        {
            string otherUID = otherKinling._kinling.UniqueId;
            RelationshipState result = _relationships.Find(state => state.KinlingUniqueID == otherUID);
            if (result == null)
            {
                NotificationManager.Instance.CreateKinlingLog(_kinling, $"{_kinling.FullName} has met {otherKinling._kinling.FullName}", LogData.ELogType.Message);
                result = new RelationshipState(otherKinling);
                _relationships.Add(result);
            }

            return result;
        }

        private void FormRomanticRelationship(SocialAI otherKinling)
        {
            var relationship = GetRelationshipState(otherKinling);
            relationship.IsPartner = true;
            _kinling.RuntimeData.Partner = otherKinling._kinling.RuntimeData;
            
            Debug.Log($"Relationship started!");
            _kinling.KinlingMood.ApplyEmotion(Librarian.Instance.GetEmotion("Started Relationship")); // Mood Buff
            NotificationManager.Instance.CreateKinlingLog(_kinling, $"{_kinling.FullName} is now in a relationship with {otherKinling._kinling.FullName}!", LogData.ELogType.Positive);
        }

        public void ReceiveMateRequest()
        {
            Task mateTask = new Task("Receive Mate", ETaskType.Personal, null, EToolType.None);
            
            _kinling.TaskAI.QueueTask(mateTask);
        }

        public bool ReadyToGoMate
        {
            get;
            set;
        }

        public void CancelMateRequest()
        {
            _kinling.TaskAI.CancelTask("Receive Mate");
            _kinling.TaskAI.CancelTask("Mate");
        }

        public void MatingComplete(bool wasSuccessful)
        {
            if (wasSuccessful)
            {
                _kinling.KinlingMood.ApplyEmotion(Librarian.Instance.GetEmotion("Got some Lovin'"));
                CheckPregnancy();
            }
            else
            {
                _kinling.KinlingMood.ApplyEmotion(Librarian.Instance.GetEmotion("Lovin' was Disturbed"));
            }
        }

        private void CheckPregnancy()
        {
            if (_kinling.RuntimeData.Gender == Gender.Female && _kinling.RuntimeData.Partner.Gender == Gender.Male)
            {
                if (_kinling.RuntimeData.MaturityStage == EMaturityStage.Adult && _kinling.RuntimeData.Partner.MaturityStage == EMaturityStage.Adult)
                {
                    bool isPregnant = Helper.RollDice(GameSettings.Instance.BasePregnancyChance);
                    if (isPregnant)
                    {
                        KinlingsManager.Instance.SpawnChild(_kinling.RuntimeData, _kinling.RuntimeData.Partner);
                    }
                }
            }
        }
    }
}
