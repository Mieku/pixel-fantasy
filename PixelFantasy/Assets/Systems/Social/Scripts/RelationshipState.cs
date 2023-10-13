using System;
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
        
        public string KinlingUniqueID;
        public int Opinion;
        public bool IsPartner;
        public int NaturalCohesion { get; protected set; }

        private const int MIN_NATURAL_COHESION = -10;
        private const int MAX_NATURAL_COHESION = 10;
        private const int ACQUAINTANCE_COHESION = 0;
        private const int FRIEND_COHESION = 15;
        private const int RIVAL_COHESION = -15;

        public RelationshipState(SocialAI socialAI, int opinion = 0)
        {
            KinlingUniqueID = socialAI.UniqueId;
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

        /// <summary>
        /// The chance that the kinling will prefer to speak with them
        /// </summary>
        public float InteractionWeight
        {
            get
            {
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
    }
}
