using System;
using Characters;
using Databrain.Attributes;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Systems.Social.Scripts
{
    [Serializable]
    public class RelationshipState
    {
        public enum ERelationshipType
        {
            Acquaintance,
            Friend,
            Rival,
        }
        
        public KinlingData KinlingData;
        public int Opinion;
        public bool IsPartner;
        
        [field: SerializeField] public int NaturalCohesion { get; protected set; }

        private const int MIN_NATURAL_COHESION = -10;
        private const int MAX_NATURAL_COHESION = 10;
        private const int ACQUAINTANCE_COHESION = 0;
        private const int FRIEND_COHESION = 15;
        private const int RIVAL_COHESION = -15;

        public RelationshipState(KinlingData kinlingData, int opinion = 0)
        {
            KinlingData = kinlingData;
            Opinion = opinion;
            NaturalCohesion = Random.Range(MIN_NATURAL_COHESION, MAX_NATURAL_COHESION + 1);
            IsPartner = false;
        }

        public int OverallCohesion
        {
            get
            {
                int result = NaturalCohesion;

                switch (RelationshipType)
                {
                    case ERelationshipType.Acquaintance:
                        result += ACQUAINTANCE_COHESION;
                        break;
                    case ERelationshipType.Friend:
                        result += FRIEND_COHESION;
                        break;
                    case ERelationshipType.Rival:
                        result += RIVAL_COHESION;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return result;
            }
        }
        
        public ERelationshipType RelationshipType
        {
            get
            {
                if (Opinion <= -20)
                {
                    return ERelationshipType.Rival;
                }

                if (Opinion <= 20)
                {
                    return ERelationshipType.Acquaintance;
                }

                return ERelationshipType.Friend;
            }
        }

        public string RelationshipTypeName
        {
            get
            {
                if (IsPartner) return "Partner";
                switch (RelationshipType)
                {
                    case ERelationshipType.Acquaintance:
                        return "Acquaintance";
                    case ERelationshipType.Friend:
                        return "Friend";
                    case ERelationshipType.Rival:
                        return "Rival";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// The chance that the kinling will prefer to speak with them
        /// </summary>
        public float InteractionWeight
        {
            get
            {
                if (IsPartner) return 0.6f;
                
                switch (RelationshipType)
                {
                    case ERelationshipType.Acquaintance:
                        return 0.25f;
                    case ERelationshipType.Friend:
                        return 0.5f;
                    case ERelationshipType.Rival:
                        return 0.1f;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void AddToScore(int amount)
        {
            Opinion = Mathf.Clamp(Opinion + amount, -100, 100);
        }

        public string OpinionText
        {
            get
            {
                if (Opinion > 0)
                {
                    return $"+{Opinion}";
                }
                else
                {
                    return $"{Opinion}";
                }
            }
        }
    }
}
