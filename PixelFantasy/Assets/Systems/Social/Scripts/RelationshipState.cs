using System;
using UnityEngine;

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
        public int RelationshipScore;

        public RelationshipState(SocialAI socialAI, int relationshipScore = 0)
        {
            KinlingUniqueID = socialAI.UniqueId;
            RelationshipScore = relationshipScore;
        }

        public ERelationshipType RelationshipType
        {
            get
            {
                if (RelationshipScore <= -20)
                {
                    return ERelationshipType.Rival;
                }

                if (RelationshipScore <= 20)
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
            RelationshipScore = Mathf.Clamp(RelationshipScore + amount, -100, 100);
        }
    }
}
