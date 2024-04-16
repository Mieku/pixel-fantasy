using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class StatsRepository : Repository<Stat>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static StatsRepository statsRepository;
        
        private void Awake()
        {
            if (!statsRepository)
                statsRepository = this;
            else if (statsRepository != this)
                Destroy(gameObject);
        }

        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        //[Header("Auto populated")]
        //public List<string> statTypes = new List<string>();
        //public Dictionary<string, List<Stat>> statsByType = new Dictionary<string, List<Stat>>();
        public List<Stat> statModifiable => GameModuleRepository.Instance.statModifiable;
        public List<Stat> statTrainable => GameModuleRepository.Instance.statTrainable;
        //public List<LookupTable> lookupTables = new List<LookupTable>();

        // The StatsRepository also holds the LookupTable data, so we will include that here.
        //public LookupTable GetLookupTableByUid(string uid) =>
        //    lookupTables.FirstOrDefault(x => x.Uid() == uid);

        //public LookupTable GetLookupTableByName(string objectName) =>
        //    lookupTables.FirstOrDefault(x => x.objectName == objectName);

        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
            //statsByType.Clear();
            scriptableObjects.Clear();
            statModifiable.Clear();
            statTrainable.Clear();
            //statTypes.Clear();
            //lookupTables.Clear();

            
            scriptableObjects = GetStatArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();
            /*
            statModifiable = scriptableObjects
                .Where(x => x.canBeModified)
                .ToList();
            
            statTrainable = scriptableObjects
                .Where(x => x.canBeTrained)
                .ToList();

            statTypes = scriptableObjects
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            foreach (string objectType in statTypes)
            {
                var objectsByObjectType = scriptableObjects
                    .Where(x => x.objectType == objectType)
                    .ToList();

                statsByType.Add(objectType, objectsByObjectType);
            }

            lookupTables = LookupTableArray().ToList();
            */
#endif
        }
        
        //public Queue<GameStat> recomputeStatQueue = new Queue<GameStat>();
        //public Queue<GameStat> recomputeStatQueue => ModulesHelper.instance.recomputeStatQueue;

        //public Coroutine recomputeStatCoroutine => ModulesHelper.instance.recomputeStatCoroutine;
        /*public IEnumerator RecomputeQueue()
        {
            yield return 0; // Wait one frame
            
            while(ModulesHelper.instance.recomputeStatQueue.Count > 0)
            {
                ModulesHelper.instance.recomputeStatQueue.Dequeue().Recompute();
            }

            ModulesHelper.instance.recomputeStatCoroutine = null;
        }

        public void StartRecomputeQueue()
        {
            if (ModulesHelper.instance.recomputeStatCoroutine != null) return;
            
            ModulesHelper.instance.recomputeStatCoroutine = StartCoroutine(RecomputeQueue());
        }*/
    }
}
