using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Quest Quest Reward...", menuName = "Game Modules/Quest Reward/Quest Quest Reward", order = 1)]
    [Serializable]
    public class QuestQuestReward : QuestReward
    {
        public List<Quest> quests = new List<Quest>();
        
        public override void GiveReward(IUseGameModules owner)
        { 
            var questOwner = (IHaveQuests)owner;

            foreach (var quest in quests)
                questOwner.AddQuest(quest.Uid());
        }
    }
    
}