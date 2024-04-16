using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Condition Reward...", menuName = "Game Modules/Quest Reward/Condition Reward", order = 1)]
    [Serializable]
    public class ConditionReward : QuestReward
    {
        public List<Condition> conditions = new List<Condition>();
        
        public override void GiveReward(IUseGameModules owner)
        { 
            var conditionOwner = (IHaveConditions)owner;

            foreach (var condition in conditions)
                conditionOwner.AddCondition(condition.Uid());
        }
    }
}