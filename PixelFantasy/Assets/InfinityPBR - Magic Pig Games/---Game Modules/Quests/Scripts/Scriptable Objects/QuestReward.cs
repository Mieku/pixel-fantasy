using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    [Serializable]
    public abstract class QuestReward : ModulesScriptableObject
    {
        [Header("Quest Reward Common")]
        public string description; // Can be internal for your notes, or you can expose it to players via the UI
        
        [HideInInspector] public int toolbarIndex;
        [HideInInspector] public int menubarIndex;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public bool drawnByGameModulesManager;

        // Used in the Inspector
        [HideInInspector] public bool show;
        

        public abstract void GiveReward(IUseGameModules owner);
        
        public virtual QuestReward Clone() => JsonUtility.FromJson<QuestReward>(JsonUtility.ToJson(this));
        
    }
}