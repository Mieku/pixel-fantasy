using System.Collections;
using System.Collections.Generic;
using InfinityPBR.Modules.Inventory;
using UnityEngine;
using static InfinityPBR.Debugs;

namespace InfinityPBR.Modules
{
    public class ModulesHelper : MonoBehaviour
    {
        public static ModulesHelper Instance;
        
        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(1f, 0.70f, 0.3f);

        private List<GameQuestList> _questActors = new List<GameQuestList>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this.gameObject);
            else
                Instance = this;
        }
        
        private void Start() => SaveQuestRewardHandler();
        
        private void Update()
        {
            for (var index = _questActors.Count - 1; index >= 0; index--)
            {
                var actor = _questActors[index];
                actor.Tick();
                // If there are no quests in progress, unregister this actor from the ModulesHelper. The actor will
                // register again when AddQuest is called.
                actor.UnregisterQuestActorWithModulesHelper();
            }
        }

        // A method to add an actor to the gameActors list
        public void RegisterQuestList(GameQuestList actor)
        {
            if (_questActors.Contains(actor))
                return;
            
            
            _questActors.Add(actor);
        }

        // A method to remove an actor from the gameActors list
        public void UnregisterQuestList(GameQuestList actor)
        {
            
            _questActors.Remove(actor);
        }
        
        // ***********************************************************************************************************
        // STATS
        // ***********************************************************************************************************
        
        public Queue<GameStat> recomputeStatQueue = new Queue<GameStat>();
        
        public Coroutine recomputeStatCoroutine;
        public IEnumerator RecomputeQueue()
        {
            yield return 0; // Wait one frame
            
            while(recomputeStatQueue.Count > 0)
                recomputeStatQueue.Dequeue().Recompute();

            recomputeStatCoroutine = null;
        }

        public void StartRecomputeQueue()
        {
            if (recomputeStatCoroutine != null) return;
            
            recomputeStatCoroutine = StartCoroutine(RecomputeQueue());
        }
        
        // ***********************************************************************************************************
        // CONDITIONS
        // ***********************************************************************************************************
        
        // We can't start a Coroutine from a non-monobehaviour, so the GameCondition will call upon the Repository
        // to have it's coroutines started
        public void SetupGameCondition(GameCondition gameCondition)
        {
            if (!gameCondition.Infinite)
                StartCoroutine(gameCondition.CheckEndTime());
            if (gameCondition.Periodic)
                StartCoroutine(gameCondition.CheckPeriodicTime());
        }

        // ***********************************************************************************************************
        // QUESTS
        // ***********************************************************************************************************

        public IHandleQuestRewards CustomQuestRewardHandler { get; private set; }

        private void SaveQuestRewardHandler()
        {
            CustomQuestRewardHandler = GetComponent<IHandleQuestRewards>();
            if (CustomQuestRewardHandler != null)
            {
                WriteToConsole("IHandleQuestRewards has been populated."
                    , "ModulesHelper/Quests", writeToConsoleColor, writeToConsole, false, gameObject);
                return;
            }

            WriteToConsole("Custom IHandleQuestRewards class not found. This can be ignored if you do not " +
                           "have any Quest Rewards that require custom handling, such as Item Object rewards. Otherwise, " +
                           "you should create a custom class which implements IHandleQuestRewards and write code to " +
                           "handle the rewards that may be created. Add that script to the QuestRepository object."
                , "ModulesHelper/Quests", writeToConsoleColor, writeToConsole, true, gameObject);
        }
        
        public void HandleItemObjectReward(GameItemObjectList gameItemObjectList)
        {
            Debug.Log($"HandleItemObjectReward() has been called for {gameItemObjectList.Count()} Items. CustomQuestRewardHandler is null: {CustomQuestRewardHandler == null}");
            if (CustomQuestRewardHandler == null)
            {
                WriteToConsole("HandleItemObjectReward() has been called, but there is no IHandleQuestReward " +
                               "handler attached to the QuestRepository object!"
                    , "ModulesHelper/Quests", writeToConsoleColor, writeToConsole, true, gameObject);
                return;
            }
            
            CustomQuestRewardHandler.HandleItemObjectReward(gameItemObjectList);
        }
    }
}
