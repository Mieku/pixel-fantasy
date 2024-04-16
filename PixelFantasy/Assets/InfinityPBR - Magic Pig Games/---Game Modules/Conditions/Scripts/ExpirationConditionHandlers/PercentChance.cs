using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Percent Chance Handler...", 
        menuName = "Game Modules/Expiration Condition Handler/Percent Chance", order = 1)]
    [Serializable]
    public class PercentChance : ExpirationConditionHandler
    {
        [Range(0f,1f)]
        public float chance = 0.5f;
        
        public override bool HandleExpiration(IHaveConditions owner, Condition conditionToAdd, IHaveStats source = null)
        {
            if (Random.Range(0f, 1f) >= chance)
                return false;

            owner.AddCondition(conditionToAdd.Uid(), source);
            return true;
        }
    }
}