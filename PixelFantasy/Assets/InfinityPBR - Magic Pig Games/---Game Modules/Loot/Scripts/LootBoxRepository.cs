using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class LootBoxRepository : Repository<LootBox>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static LootBoxRepository lootBoxRepository;
        
        public List<LootItems> lootItemsList = new List<LootItems>();
        public Dictionary<string, LootItems> lootItemsByUid = new Dictionary<string, LootItems>();

        [Obsolete("This method is deprecated, please use GameModuleRepository.Instance.Get<LootItems>(uid) instead.", false)]
        public LootItems GetLootItemsByUid(string uid)
        {
            Debug.LogWarning("GetLootItemsByUid is obsolete. Please use GameModuleRepository.Instance.Get<LootItems>(uid) instead.");
            return GameModuleRepository.Instance.Get<LootItems>(uid);
            
            if (lootItemsByUid.TryGetValue(uid, out var value))
                return value;

            var item = lootItemsList.FirstOrDefault(x => x.Uid() == uid);
            if (item == null)
                return default;

            lootItemsByUid[uid] = item;
            return item;
        }
        
        private void Awake()
        {
            if (!lootBoxRepository)
                lootBoxRepository = this;
            else if (lootBoxRepository != this)
                Destroy(gameObject);
        }

        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        //[Header("Auto populated")] // Right now there is nothing for this section!

        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
            scriptableObjects.Clear();
            lootItemsList.Clear();
            lootItemsByUid.Clear();

            scriptableObjects = GameModuleObjects<LootBox>().ToList();
            
            lootItemsList = GameModuleObjects<LootItems>().ToList();

            foreach (var item in lootItemsList)
                lootItemsByUid[item.Uid()] = item;
#endif
        }
    }
}
