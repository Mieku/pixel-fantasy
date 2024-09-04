using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Managers;
using ScriptableObjects;
using Systems.Appearance.Scripts;
using Systems.Notifications.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Social.Scripts
{
    public class SocialAI : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        [SerializeField] private GameObject _speechBubbleHandle;
        [SerializeField] private SpriteRenderer _speechTopicIcon;
        [SerializeField] private SocialTopicSettings _socialTopics;
        [SerializeField] private SocialTopicSettings _romanticTopics;
        [SerializeField] private SocialTopicSettings _positiveResponses;
        [SerializeField] private SocialTopicSettings _negativeResponses;

        public bool AvailableToChat => _state == ESocialState.Available && !_kinling.RuntimeData.IsAsleep;
        
        //private const float CHAT_COOLDOWN = 5.0f;
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
        private float _socialFrequency => _kinling.RuntimeData.Stats.GetSocialFrequency() * 60f;
        
        public List<RelationshipData> Relationships => _kinling.RuntimeData.Relationships;
        
        public enum ESocialState
        {
            Available,
            Chatting
        }
        
        private void Update()
        {
            if(!_kinling.HasInitialized) return;
            if (_kinling.RuntimeData.IsAsleep) return;
            
            if (_state == ESocialState.Available)
            {
                _chatTimer += TimeManager.Instance.DeltaTime;
                if (_chatTimer >= _socialFrequency)
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
                        _chatTimer = _socialFrequency * .1f;
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

        private void BeginChat(KinlingData targetKinling)
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

        private bool CheckShouldFlirt(KinlingData targetKinling)
        {
            var relationship = GetRelationshipState(targetKinling);
            // Make sure they are not in a romantic relationship with someone else
            if (_kinling.RuntimeData.Partner != null) return false;
            if (targetKinling.Partner != null) return false;
            // Make sure they are an appropriate age
            if (_kinling.RuntimeData.MaturityStage < EMaturityStage.Adult) return false;
            if (_kinling.RuntimeData.MaturityStage != targetKinling.MaturityStage) return false;
            // Make sure they align with their sexual preference
            if (!_kinling.RuntimeData.IsKinlingAttractedTo(targetKinling)) return false;
            if (!targetKinling.IsKinlingAttractedTo(_kinling.RuntimeData)) return false;
            // Make sure their opinion is high enough
            if (relationship.Opinion < MIN_OPINION_TO_FLIRT) return false;

            float attemptFlirtingChance = (BASE_ATTEMPT_FLIRTING_CHANCE * _kinling.RuntimeData.Stats.GetAttractiveness()) + relationship.OverallCohesion / 100f;
            float roll = Random.Range(0f, 1f);
            return roll <= attemptFlirtingChance;
        }

        private void InitiateFlirt(KinlingData targetKinling)
        {
            var topic = _romanticTopics.GetRandomTopic();
            DisplayChatBubble(topic);
            targetKinling.GetKinling().SocialAI.ReceiveFlirt(_kinling.RuntimeData, GetFlirtResponse);
        }
        
        public void ReceiveFlirt(KinlingData otherKinling, Action<bool, KinlingData> onResponse)
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
        
        private bool DetermineFlirtResponse(KinlingData otherKinling)
        {
            // This is based on their cohesion, relationship and mood
            int weight = (int)(ROMANTIC_COHESION_BASE  * _kinling.RuntimeData.Stats.GetAttractiveness());
            var otherKinlingRelationship = GetRelationshipState(otherKinling);
            var curOverallMood = _kinling.MoodData.OverallMood / 100f;
            int moodCohesion = (int)(MOOD_COHESION_BASE * curOverallMood) - (MOOD_COHESION_BASE / 2);
            
            weight += otherKinlingRelationship.OverallCohesion;
            weight += moodCohesion;

            weight = Mathf.Clamp(weight, 2, 98);

            int random100 = Random.Range(1, 101);
            return random100 <= weight;
        }
        
        private void GetFlirtResponse(bool isPositive, KinlingData responder)
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
                _kinling.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("Rejected")); // Mood De-buff
            }
            
            _kinling.RuntimeData.Needs.IncreaseNeedValue(NeedType.Fun, CHAT_SOCIAL_NEED_BENEFIT);
            GameEvents.Trigger_OnKinlingChanged(_kinling.RuntimeData);
        }

        private void InitiateChitChat(KinlingData targetKinling)
        {
            var topic = _socialTopics.GetRandomTopic();
            DisplayChatBubble(topic);
            targetKinling.GetKinling().SocialAI.RecieveChat(_kinling.RuntimeData, GetChatResponse);
        }
        
        public void RecieveChat(KinlingData otherKinling, Action<bool, KinlingData> onResponse)
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

        IEnumerator ResponseSequence(KinlingData otherKinling, SocialTopic topic, bool isPositiveResponse, Action<bool, KinlingData> onResponse)
        {
            yield return new WaitForSeconds(1);
            
            DisplayChatBubble(topic);

            onResponse.Invoke(isPositiveResponse, _kinling.RuntimeData);

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

        private bool DetermineResponse(KinlingData otherKinling)
        {
            // This is based on their cohesion, relationship and mood
            int weight = (int)(COHESION_BASE * _kinling.RuntimeData.Stats.GetAttractiveness());
            var otherKinlingRelationship = GetRelationshipState(otherKinling);
            var curOverallMood = _kinling.MoodData.OverallMood / 100f;
            int moodCohesion = (int)(MOOD_COHESION_BASE * curOverallMood) - (MOOD_COHESION_BASE / 2);
            
            weight += otherKinlingRelationship.OverallCohesion;
            weight += moodCohesion;

            weight = Mathf.Clamp(weight, 2, 98);

            int random100 = Random.Range(1, 101);
            return random100 <= weight;
        }

        private void GetChatResponse(bool isPositive, KinlingData responder)
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
            
            _kinling.RuntimeData.Needs.IncreaseNeedValue(NeedType.Fun, CHAT_SOCIAL_NEED_BENEFIT);
            GameEvents.Trigger_OnKinlingChanged(_kinling.RuntimeData);
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

        private KinlingData ChooseKinlingToChatWith(List<KinlingData> options)
        {
            List<WeightedObject<KinlingData>> weightedRelationships =
                new List<WeightedObject<KinlingData>>();
            foreach (var option in options)
            {
                var relationshipState = GetRelationshipState(option);
                var weight = relationshipState.InteractionWeight;
                WeightedObject<KinlingData> weightedState =
                    new WeightedObject<KinlingData>(option, weight);
                weightedRelationships.Add(weightedState);
            }

            var cumulativeDistribution = Helper.GenerateCumulativeDistribution(weightedRelationships);
            var randomKinling = Helper.GetRandomObject(cumulativeDistribution);

            return randomKinling;
        }

        private List<KinlingData> NearbyKinlings()
        {
            var allUnits = KinlingsDatabase.Instance.GetAllUnitsInRadius(transform.position, SOCIAL_RADIUS);
            List<KinlingData> results = new List<KinlingData>();
            foreach (var unit in allUnits)
            {
                if (unit != _kinling.RuntimeData)
                {
                    if (unit.GetKinling().SocialAI.AvailableToChat)
                    {
                        results.Add(unit);
                    }
                }
            }

            return results;
        }

        private RelationshipData GetRelationshipState(KinlingData otherKinling)
        {
            RelationshipData result = Relationships.Find(state => state.OthersUID == otherKinling.UniqueID);
            if (result == null)
            {
                NotificationManager.Instance.CreatePersonalLog(_kinling, $"{_kinling.FullName} has met {otherKinling.Fullname}", LogData.ELogType.Message);
                result = new RelationshipData(_kinling.RuntimeData, otherKinling);
                _kinling.RuntimeData.Relationships.Add(result);
                
                GameEvents.Trigger_OnKinlingChanged(_kinling.RuntimeData);
            }

            return result;
        }

        public void FormRomanticRelationship(KinlingData otherKinling)
        {
            var relationship = GetRelationshipState(otherKinling);
            relationship.IsPartner = true;
            _kinling.RuntimeData.PartnerUID = otherKinling.UniqueID;
            
            _kinling.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("Started Relationship")); // Mood Buff
            //NotificationManager.Instance.CreateKinlingLog(_kinling, $"{_kinling.FullName} is now in a relationship with {otherKinling.Fullname}!", LogData.ELogType.Positive);
            GameEvents.Trigger_OnKinlingChanged(_kinling.RuntimeData);
        }

        public void ReceiveMateRequest()
        {
            // Task mateTask = new Task("Receive Mate", ETaskType.Personal, null, EToolType.None);
            //
            // _kinling.TaskAI.QueueTask(mateTask);
        }

        public bool ReadyToGoMate
        {
            get;
            set;
        }

        public void CancelMateRequest()
        {
            // _kinling.TaskAI.CancelTask("Receive Mate");
            // _kinling.TaskAI.CancelTask("Mate");
        }

        public void MatingComplete(bool wasSuccessful)
        {
            if (wasSuccessful)
            {
                _kinling.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("Got some Lovin'"));
                CheckPregnancy();
            }
            else
            {
                _kinling.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("Lovin' was Disturbed"));
            }
        }

        private void CheckPregnancy()
        {
            if (_kinling.RuntimeData.Gender == EGender.Female && _kinling.RuntimeData.Partner.Gender == EGender.Male)
            {
                if (_kinling.RuntimeData.MaturityStage == EMaturityStage.Adult && _kinling.RuntimeData.Partner.MaturityStage == EMaturityStage.Adult)
                {
                    bool isPregnant = Helper.RollDice(GameSettings.Instance.BasePregnancyChance);
                    if (isPregnant)
                    {
                        KinlingsDatabase.Instance.SpawnChild(_kinling.RuntimeData, _kinling.RuntimeData.Partner);
                        GameEvents.Trigger_OnKinlingChanged(_kinling.RuntimeData);
                    }
                }
            }
        }
    }
}
