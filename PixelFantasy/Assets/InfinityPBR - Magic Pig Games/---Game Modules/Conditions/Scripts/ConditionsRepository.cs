using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class ConditionsRepository : Repository<Condition>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static ConditionsRepository conditionsRepository;
        
        private void Awake()
        {
            if (!conditionsRepository)
                conditionsRepository = this;
            else if (conditionsRepository != this)
                Destroy(gameObject);
        }

        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        [Header("Auto populated")]
        public List<string> conditionTypes = new List<string>();
        public Dictionary<string, List<Condition>> conditionsByType = new Dictionary<string, List<Condition>>();
        public List<LookupTable> lookupTables = new List<LookupTable>();
        
        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
            scriptableObjects.Clear();
            conditionsByType.Clear();
            conditionTypes.Clear();
            lookupTables.Clear();
            
            scriptableObjects = GetConditionArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();

            conditionTypes = scriptableObjects
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            foreach (string objectType in conditionTypes)
            {
                var objectsByObjectType = scriptableObjects
                    .Where(x => x.objectType == objectType)
                    .ToList();

                conditionsByType.Add(objectType, objectsByObjectType);
            }

            lookupTables = LookupTableArray().ToList();
#endif
        }

        [Obsolete("Use GameModulesRepository.Instance.SetupGameCondition(GameCondition gameCondition) instead.", false)]
        public void SetupGameCondition(GameCondition gameCondition)
        {
            Debug.LogWarning("Obsolete: Use GameModulesRepository.Instance.SetupGameCondition(GameCondition gameCondition) instead.");
            if (!gameCondition.Infinite)
                StartCoroutine(gameCondition.CheckEndTime());
            if (gameCondition.Periodic)
                StartCoroutine(gameCondition.CheckPeriodicTime());
        }
    }
}
