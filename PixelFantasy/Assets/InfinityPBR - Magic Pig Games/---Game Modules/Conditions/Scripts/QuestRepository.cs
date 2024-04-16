using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Debugs;
using static InfinityPBR.Modules.GameModuleUtilities;


namespace InfinityPBR.Modules
{
    public class QuestRepository : Repository<Quest>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static QuestRepository questRepository;

        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.80f, 0.20f, 0.6f);
        
        //private IHandleQuestRewards _customQuestRewardHandler;
        //public IHandleQuestRewards CustomQuestRewardHandler => _customQuestRewardHandler;
        
        private void Awake()
        {
            if (!questRepository)
                questRepository = this;
            else if (questRepository != this)
                Destroy(gameObject);
        }

        /*
        private void Start() => SaveQuestRewardHandler();

        private void SaveQuestRewardHandler()
        {
            _customQuestRewardHandler = GetComponent<IHandleQuestRewards>();
            if (_customQuestRewardHandler != null)
            {
                WriteToConsole("IHandleQuestRewards has been populated."
                    , "QuestRepository", writeToConsoleColor, writeToConsole, false, gameObject);
                return;
            }

            WriteToConsole("Custom IHandleQuestRewards class not found. This can be ignored if you do not " +
                           "have any Quest Rewards that require custom handling, such as Item Object rewards. Otherwise, " +
                           "you should create a custom class which implements IHandleQuestRewards and write code to " +
                           "handle the rewards that may be created. Add that script to the QuestRepository object."
                , "QuestRepository", writeToConsoleColor, writeToConsole, true, gameObject);
        }
        */
        
        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        //[Header("Auto populated")]
        //public List<string> questTypes = new List<string>();
       // public Dictionary<string, List<Quest>> questsByType = new Dictionary<string, List<Quest>>();
        //public List<Quest> quests = new List<Quest>();
        //public List<QuestReward> questRewards = new List<QuestReward>();
        //public List<QuestCondition> questConditions = new List<QuestCondition>();

        //public Dictionary<string, Quest> QuestsByUid = new Dictionary<string, Quest>();
        //public Dictionary<string, QuestReward> QuestsRewardsByUid = new Dictionary<string, QuestReward>();
        //public Dictionary<string, QuestCondition> QuestsConditionsByUid = new Dictionary<string, QuestCondition>();

        
        
        
        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
           // questsByType.Clear();
            scriptableObjects.Clear();
            //quests.Clear();
            //questTypes.Clear();
            //QuestsRewardsByUid.Clear();
            //QuestsConditionsByUid.Clear();
            
            scriptableObjects = GameModuleObjects<Quest>(true)
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();

            //quests = GameModuleObjects<Quest>().ToList();
           // questTypes = GameModuleObjectTypes<Quest>().ToList();

           /*
            foreach (var objectType in questTypes)
                questsByType.Add(objectType, GameModuleObjectsOfType<Quest>(objectType).ToList());
            
            questRewards = GameModuleObjects<QuestReward>().ToList();
            questConditions = GameModuleObjects<QuestCondition>().ToList();

            foreach (var reward in questRewards)
                QuestsRewardsByUid[reward.Uid()] = reward;
            
            foreach (var condition in questConditions)
                QuestsConditionsByUid[condition.Uid()] = condition;
                */
#endif
        }
    }
}
