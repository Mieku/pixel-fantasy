using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [Serializable]
    public class GameItemObjectList : GameModulesList<GameItemObject>, IHaveStartActions
    {
        // **********************************************************************
        // General things for all Game Module Lists
        // **********************************************************************
        
        public override string GameId() => Owner.GameId();
        public override string NoteSubject() => "Item Objects";
        public virtual GameItemObjectList Clone() => JsonUtility.FromJson<GameItemObjectList>(JsonUtility.ToJson(this));
        
        // **********************************************************************
        // Owner 
        // **********************************************************************

        public IHaveStats Owner { get; private set; }
        public void SetOwner(IHaveStats value) => Owner = value;
        
        // **********************************************************************
        // Start Actions
        //
        // These are actions that must be required when the application loads. At Start() on your manager script (often
        // the script which holds this object), start this coroutine. It will wait until the blackboard is present and
        // available.
        // **********************************************************************

        public virtual IEnumerator StartActions()
        {
            // Wait for blackboard but also to ensure the Owner has been populated.
            while (MainBlackboard.blackboard == null 
                   || Owner == null)
                yield return null;

            this.AddThisToBlackboard();
            RelinkPrefabs();
            
            foreach (var gameItemObject in list)
                gameItemObject.StartActions();
        }
        
        // **********************************************************************
        // Affected Stats & Dirty
        // **********************************************************************

        public virtual void SetAffectedStatsDirty(GameItemObject gameItemObject) 
            => Owner?.SetStatsDirty(gameItemObject.DirectlyAffectsList());
        
        public override void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            if (gameModuleObject is GameItemObject gameItemObject)
                Owner?.SetStatsDirty(gameItemObject.DirectlyAffectsList());
        }
        
        public virtual void SetAffectedStatsDirty(List<Stat> statList = null) 
            => Owner?.SetStatsDirty(statList ?? DirectlyAffectsList());

        public virtual List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevels
            .SelectMany(x => x.targets)
            .Distinct()
            .ToList();

        public virtual void SetDirty() => SetAffectedStatsDirty();
        
        // **********************************************************************
        // Modification Levels
        // **********************************************************************

        public virtual List<ModificationLevel> ModificationLevels => GetModificationLevels();
        protected virtual List<ModificationLevel> GetModificationLevels()
        {
            var modificationLevels = new List<ModificationLevel>();
            list.ForEach(x => modificationLevels.AddRange(x.ModificationLevels));
            return modificationLevels;
        }
        
        // **********************************************************************
        // Standard methods for specific Scriptable Object types
        // **********************************************************************
        
        public virtual GameItemObject Add(ItemObject newItem, bool distinct = true) 
            => Add(newItem.Uid(), distinct);
        
        public virtual GameItemObject Add(GameItemObject newItem, bool distinct = true) 
            => Add(newItem.Uid(), distinct);

        public override GameItemObject Add(string uid, bool distinct = true)
        {
            if (distinct && Contains(uid)) return this.Get(uid);
            list.Add(new GameItemObject(GameModuleRepository.Instance.Get<ItemObject>(uid), Owner));

            var newItem = list[^1];

            newItem.SetParentList(this);
            SetAffectedStatsDirty(newItem);
            this.AddThisToBlackboard();
            return newItem;
        }

        public virtual GameItemObject Get(ItemObject itemObject, bool addIfNull = false, bool distinct = false)
            => this.Get(itemObject.Uid(), addIfNull, distinct);

        public virtual void Remove(ItemObject itemObject) => this.Remove(itemObject.Uid());
        public virtual void RemoveAll(ItemObject itemObject) => this.RemoveAll(itemObject.Uid());
        
        public virtual void RemoveDuplicates(ItemObject itemObject, bool includeItemAttributes = false) => RemoveDuplicates(itemObject.Uid(), includeItemAttributes);
        public virtual void RemoveDuplicates(GameItemObject gameItemObject, bool includeItemAttributes = false) => RemoveDuplicates(gameItemObject.Uid(), includeItemAttributes);
        public virtual void RemoveDuplicates(string itemObjectUid = null, bool includeItemAttributes = false)
        {
            var duplicates = GetDuplicates(itemObjectUid, includeItemAttributes);
            foreach (var duplicate in duplicates)
                list.Remove(duplicate);
        }

        public virtual void RemoveDuplicates(IEnumerable<ItemObject> itemObjects, bool includeItemAttributes = false) => RemoveDuplicates(itemObjects.Select(i => i.Uid()), includeItemAttributes);
        public virtual void RemoveDuplicates(IEnumerable<GameItemObject> gameItemObjects, bool includeItemAttributes = false) => RemoveDuplicates(gameItemObjects.Select(g => g.Uid()), includeItemAttributes);
        public virtual void RemoveDuplicates(IEnumerable<string> itemObjectUids, bool includeItemAttributes = false)
        {
            var duplicates = GetDuplicates(itemObjectUids, includeItemAttributes);
            foreach (var duplicate in duplicates)
                list.Remove(duplicate);
        }


        public virtual IEnumerable<GameItemObject> GetDuplicates(ItemObject itemObject, bool includeItemAttributes = false) => GetDuplicates(itemObject.Uid(), includeItemAttributes);
        public virtual IEnumerable<GameItemObject> GetDuplicates(GameItemObject gameItemObject, bool includeItemAttributes = false) => GetDuplicates(gameItemObject.Uid(), includeItemAttributes);
        public virtual IEnumerable<GameItemObject> GetDuplicates(string itemObjectUid = null, bool includeItemAttributes = false)
        {
            var duplicates = list.Where(x => itemObjectUid == null || x.Uid() == itemObjectUid);
    
            var groupsToReturn = duplicates.GroupBy(x => x.Uid()).Where(group =>
                group.Count() > 1 && (!includeItemAttributes || group.Skip(1).All(y => AttributesAreEqual(group.First().attributeList.list, y.attributeList.list))));
    
            return groupsToReturn.SelectMany(group => group.Skip(1)); // skip the first item
        }

        public virtual IEnumerable<GameItemObject> GetDuplicates(IEnumerable<ItemObject> itemObjects, bool includeItemAttributes = false) => GetDuplicates(itemObjects.Select(i => i.Uid()), includeItemAttributes);
        public virtual IEnumerable<GameItemObject> GetDuplicates(IEnumerable<GameItemObject> gameItemObjects, bool includeItemAttributes = false) => GetDuplicates(gameItemObjects.Select(g => g.Uid()), includeItemAttributes);
        public virtual IEnumerable<GameItemObject> GetDuplicates(IEnumerable<string> itemObjectUids, bool includeItemAttributes = false)
        {
            var duplicates = list.Where(x => itemObjectUids.Contains(x.Uid()));

            var groupsToReturn = duplicates.GroupBy(x => x.Uid()).Where(group =>
                group.Count() > 1 && (!includeItemAttributes || group.Skip(1).All(y => AttributesAreEqual(group.First().attributeList.list, y.attributeList.list))));

            return groupsToReturn.SelectMany(group => group.Skip(1)); // skip the first item
        }


        // Get a single GameItemObject with a speicic ItemAttribute, optionally specify itemObjectUid
        public virtual GameItemObject GetWithAttribute(ItemAttribute itemAttribute, string itemObjectUid = null) 
            => GetWithAttribute(itemAttribute.Uid(), itemObjectUid);
        public virtual GameItemObject GetWithAttribute(GameItemAttribute gameItemAttribute, string itemObjectUid = null) 
            => GetWithAttribute(gameItemAttribute.Uid(), itemObjectUid);
        public virtual GameItemObject GetWithAttribute(string itemAttributeUid, string itemObjectUid = null)
            => list.FirstOrDefault(x => (itemObjectUid == null || x.Uid() == itemObjectUid) && x.HasAttribute(itemAttributeUid));
        
        public virtual bool TryGetWithAttribute(ItemAttribute itemAttribute, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAttribute(itemAttribute.Uid(), out found, itemObjectUid);
        public virtual bool TryGetWithAttribute(GameItemAttribute gameItemAttribute, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAttribute(gameItemAttribute.Uid(), out found, itemObjectUid);
        public virtual bool TryGetWithAttribute(string itemAttributeUid, out GameItemObject found, string itemObjectUid = null)
        {
            found = GetWithAttribute(itemAttributeUid, itemObjectUid);
            return found != null;
        }
        
        // Get a single GameItemObject with all itemAttributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithAttributes(IEnumerable<ItemAttribute> itemAttributes, string uid = null)
            => GetWithAttributes(itemAttributes.Select(x => x.Uid()), uid);
        public virtual GameItemObject GetWithAttributes(IEnumerable<GameItemAttribute> gameItemAttributes, string uid = null)
            => GetWithAttributes(gameItemAttributes.Select(x => x.Uid()), uid);
        public virtual GameItemObject GetWithAttributes(IEnumerable<string> itemAttributeUids, string uid = null)
            => list.FirstOrDefault(x => (uid == null || x.Uid() == uid) && itemAttributeUids.All(x.HasAttribute));
        
        public virtual bool TryGetWithAttributes(IEnumerable<ItemAttribute> itemAttributes, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAttributes(itemAttributes.Select(attr => attr.Uid()), out found, itemObjectUid);
        public virtual bool TryGetWithAttributes(IEnumerable<GameItemAttribute> gameItemAttributes, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAttributes(gameItemAttributes.Select(attr => attr.Uid()), out found, itemObjectUid);
        public virtual bool TryGetWithAttributes(IEnumerable<string> itemAttributeUids, out GameItemObject found, string itemObjectUid = null)
        {
            found = GetWithAttributes(itemAttributeUids, itemObjectUid);
            return found != null;
        }
        
        // Get a single GameItemObject with ANY of the itemAttributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithAnyAttribute(IEnumerable<ItemAttribute> itemAttributes, string uid = null)
            => GetWithAnyAttribute(itemAttributes.Select(x => x.Uid()), uid);
        public virtual GameItemObject GetWithAnyAttribute(IEnumerable<GameItemAttribute> gameItemAttributes, string uid = null)
            => GetWithAnyAttribute(gameItemAttributes.Select(x => x.Uid()), uid);
        public virtual GameItemObject GetWithAnyAttribute(IEnumerable<string> itemAttributeUids, string uid = null)
            => list.FirstOrDefault(x => (uid == null || x.Uid() == uid) && itemAttributeUids.Any(x.HasAttribute));

        public virtual bool TryGetWithAnyAttribute(IEnumerable<ItemAttribute> itemAttributes, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAnyAttribute(itemAttributes.Select(attr => attr.Uid()), out found, itemObjectUid);
        public virtual bool TryGetWithAnyAttribute(IEnumerable<GameItemAttribute> gameItemAttributes, out GameItemObject found, string itemObjectUid = null) 
            => TryGetWithAnyAttribute(gameItemAttributes.Select(attr => attr.Uid()), out found, itemObjectUid);
        public virtual bool TryGetWithAnyAttribute(IEnumerable<string> itemAttributeUids, out GameItemObject found, string itemObjectUid = null)
        {
            found = GetWithAnyAttribute(itemAttributeUids, itemObjectUid);
            return found != null;
        }

        // Get all GameItemObjects of a specific ItemObject, GameItemObject, or uid
        public virtual IEnumerable<GameItemObject> GetAll(ItemObject itemObject) => GetAll(itemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAll(GameItemObject gameItemObject) => GetAll(gameItemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAll(string uid) => list.Where(x => x.Uid() == uid);
        
        public virtual bool TryGetAll(ItemObject itemObject, out IEnumerable<GameItemObject> results) 
        {
            results = GetAll(itemObject.Uid());
            return results.Any();
        }

        public virtual bool TryGetAll(GameItemObject gameItemObject, out IEnumerable<GameItemObject> results) 
        {
            results = GetAll(gameItemObject.Uid());
            return results.Any();
        }

        public virtual bool TryGetAll(string uid, out IEnumerable<GameItemObject> found) 
        {
            found = GetAll(uid);
            return found.Any();
        }

        // Get all GameItemObjects with a specific ItemAttribute, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithAttribute(GameItemAttribute gameItemAttribute, string itemObjectUid = null) => GetAllWithAttribute(gameItemAttribute.Uid(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAttribute(ItemAttribute itemAttribute, string itemObjectUid = null) => GetAllWithAttribute(itemAttribute.Uid(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAttribute(string itemAttributeUid, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => x.HasAttribute(itemAttributeUid))
                : list.Where(x => x.Uid() == itemObjectUid && x.HasAttribute(itemAttributeUid));
        
        public virtual bool TryGetAllWithAttribute(GameItemAttribute gameItemAttribute, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithAttribute(gameItemAttribute, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAttribute(ItemAttribute itemAttribute, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithAttribute(itemAttribute, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAttribute(string itemAttributeUid, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithAttribute(itemAttributeUid, itemObjectUid);
            return found.Any();
        }
        
        // Get all GameItemObjects with ALL of the itemAttributes, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithAttributes(IEnumerable<ItemAttribute> itemAttributes, string itemObjectUid = null) => GetAllWithAttributes(itemAttributes.Select(attr => attr.Uid()).ToArray(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAttributes(IEnumerable<GameItemAttribute> gameItemAttributes, string itemObjectUid = null) => GetAllWithAttributes(gameItemAttributes.Select(attr => attr.Uid()).ToArray(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAttributes(IEnumerable<string> itemAttributeUids, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => itemAttributeUids.All(x.HasAttribute))
                : list.Where(x => x.Uid() == itemObjectUid && itemAttributeUids.All(x.HasAttribute));

        public virtual bool TryGetAllWithAttributes(IEnumerable<ItemAttribute> itemAttributes, out IEnumerable<GameItemObject> found, string itemObjectUid = null)
        {
            found = GetAllWithAttributes(itemAttributes, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAttributes(IEnumerable<GameItemAttribute> gameItemAttributes, out IEnumerable<GameItemObject> found, string itemObjectUid = null)
        {
            found = GetAllWithAttributes(gameItemAttributes, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAttributes(IEnumerable<string> itemAttributeUids, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithAttributes(itemAttributeUids, itemObjectUid);
            return found.Any();
        }

        // Get all GameItemObjects with ANY of the itemAttributes, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithAnyAttribute(IEnumerable<ItemAttribute> itemAttributes, string itemObjectUid = null) => GetAllWithAnyAttribute(itemAttributes.Select(attr => attr.Uid()).ToArray(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAnyAttribute(IEnumerable<GameItemAttribute> gameItemAttributes, string itemObjectUid = null) => GetAllWithAnyAttribute(gameItemAttributes.Select(attr => attr.Uid()).ToArray(), itemObjectUid);
        public virtual IEnumerable<GameItemObject> GetAllWithAnyAttribute(IEnumerable<string> itemAttributeUids, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => itemAttributeUids.Any(x.HasAttribute))
                : list.Where(x => x.Uid() == itemObjectUid && itemAttributeUids.Any(x.HasAttribute));
        
        public virtual bool TryGetAllWithAnyAttribute(IEnumerable<ItemAttribute> itemAttributes, out IEnumerable<GameItemObject> found, string itemObjectUid = null)
        {
            found = GetAllWithAnyAttribute(itemAttributes, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAnyAttribute(IEnumerable<GameItemAttribute> gameItemAttributes, out IEnumerable<GameItemObject> found, string itemObjectUid = null)
        {
            found = GetAllWithAnyAttribute(gameItemAttributes, itemObjectUid);
            return found.Any();
        }

        public virtual bool TryGetAllWithAnyAttribute(IEnumerable<string> itemAttributeUids, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithAnyAttribute(itemAttributeUids, itemObjectUid);
            return found.Any();
        }

        // Get GameItemObjects with NO attributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithNoAttributes(ItemObject itemObject) => GetWithNoAttributes(itemObject.Uid());
        public virtual GameItemObject GetWithNoAttributes(GameItemObject gameItemObject) => GetWithNoAttributes(gameItemObject.Uid());
        public virtual GameItemObject GetWithNoAttributes(string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.FirstOrDefault(x => x.attributeList.Count() > 0)
                : list.FirstOrDefault(x => x.Uid() == itemObjectUid && x.attributeList.Count() > 0);

        public virtual bool TryGetWithNoAttributes(out GameItemObject found, ItemObject itemObject) => TryGetWithNoAttributes(out found, itemObject.Uid());
        public virtual bool TryGetWithNoAttributes(out GameItemObject found, GameItemObject gameItemObject) => TryGetWithNoAttributes(out found, gameItemObject.Uid());
        public virtual bool TryGetWithNoAttributes(out GameItemObject found, string itemObjectUid = null) 
        {
            found = GetWithNoAttributes(itemObjectUid);
            return found != null;
        }
        
        // Get GameItemObjects with more than X amount of attributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithMoreThanXAttributes(int amount, ItemObject itemObject) => GetWithMoreThanXAttributes(amount, itemObject.Uid());
        public virtual GameItemObject GetWithMoreThanXAttributes(int amount, GameItemObject gameItemObject) => GetWithMoreThanXAttributes(amount, gameItemObject.Uid());
        public virtual GameItemObject GetWithMoreThanXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.FirstOrDefault(x => x.attributeList.Count() > amount)
                : list.FirstOrDefault(x => x.Uid() == itemObjectUid && x.attributeList.Count() > amount);

        public virtual bool TryGetWithMoreThanXAttributes(int x, out GameItemObject found, ItemObject itemObject) => TryGetWithMoreThanXAttributes(x, out found, itemObject.Uid());
        public virtual bool TryGetWithMoreThanXAttributes(int x, out GameItemObject found, GameItemObject gameItemObject) => TryGetWithMoreThanXAttributes(x, out found, gameItemObject.Uid());
        public virtual bool TryGetWithMoreThanXAttributes(int x, out GameItemObject found, string itemObjectUid = null) 
        {
            found = GetWithMoreThanXAttributes(x, itemObjectUid);
            return found != null;
        }
        
        // Get GameItemObjects with less than X amount of attributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithLessThanXAttributes(int amount, ItemObject itemObject) => GetWithLessThanXAttributes(amount, itemObject.Uid());
        public virtual GameItemObject GetWithLessThanXAttributes(int amount, GameItemObject gameItemObject) => GetWithLessThanXAttributes(amount, gameItemObject.Uid());
        public virtual GameItemObject GetWithLessThanXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.FirstOrDefault(x => x.attributeList.Count() < amount)
                : list.FirstOrDefault(x => x.Uid() == itemObjectUid && x.attributeList.Count() < amount);

        public virtual bool TryGetWithLessThanXAttributes(int x, out GameItemObject found, ItemObject itemObject) => TryGetWithLessThanXAttributes(x, out found, itemObject.Uid());
        public virtual bool TryGetWithLessThanXAttributes(int x, out GameItemObject found, GameItemObject gameItemObject) => TryGetWithLessThanXAttributes(x, out found, gameItemObject.Uid());
        public virtual bool TryGetWithLessThanXAttributes(int x, out GameItemObject found, string itemObjectUid = null) 
        {
            found = GetWithLessThanXAttributes(x, itemObjectUid);
            return found != null;
        }
        
        // Get GameItemObjects with exactly X amount of attributes, optionally specify itemObjectUid
        public virtual GameItemObject GetWithExactlyXAttributes(int amount, ItemObject itemObject) => GetWithExactlyXAttributes(amount, itemObject.Uid());
        public virtual GameItemObject GetWithExactlyXAttributes(int amount, GameItemObject gameItemObject) => GetWithExactlyXAttributes(amount, gameItemObject.Uid());
        public virtual GameItemObject GetWithExactlyXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.FirstOrDefault(x => x.attributeList.Count() == amount)
                : list.FirstOrDefault(x => x.Uid() == itemObjectUid && x.attributeList.Count() == amount);

        public virtual bool TryGetWithExactlyXAttributes(int amount, out GameItemObject found, ItemObject itemObject) => TryGetWithExactlyXAttributes(amount, out found, itemObject.Uid());
        public virtual bool TryGetWithExactlyXAttributes(int amount, out GameItemObject found, GameItemObject gameItemObject) => TryGetWithExactlyXAttributes(amount, out found, gameItemObject.Uid());
        public virtual bool TryGetWithExactlyXAttributes(int amount, out GameItemObject found, string itemObjectUid = null) 
        {
            found = GetWithExactlyXAttributes(amount, itemObjectUid);
            return found != null;
        }

        // Get all GameItemObjects without any attributes at all, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithNoAttributes(ItemObject itemObject) => GetAllWithNoAttributes(itemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithNoAttributes(GameItemObject gameItemObject) => GetAllWithNoAttributes(gameItemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithNoAttributes(string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => x.attributeList.Count() == 0)
                : list.Where(x => x.Uid() == itemObjectUid && x.attributeList.Count() == 0);

        public virtual bool TryGetAllWithNoAttributes(out IEnumerable<GameItemObject> found, ItemObject itemObject) => TryGetAllWithNoAttributes(out found, itemObject.Uid());
        public virtual bool TryGetAllWithNoAttributes(out IEnumerable<GameItemObject> found, GameItemObject gameItemObject) => TryGetAllWithNoAttributes(out found, gameItemObject.Uid());
        public virtual bool TryGetAllWithNoAttributes(out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithNoAttributes(itemObjectUid);
            return found.Any();
        }
        
        // Get all GameItemObjects with more than X amount of attributes, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithMoreThanXAttributes(int amount, ItemObject itemObject) => GetAllWithMoreThanXAttributes(amount, itemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithMoreThanXAttributes(int amount, GameItemObject gameItemObject) => GetAllWithMoreThanXAttributes(amount, gameItemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithMoreThanXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => x.attributeList.Count() > amount)
                : list.Where(x => x.Uid() == itemObjectUid && x.attributeList.Count() > amount);

        public virtual bool TryGetAllWithMoreThanXAttributes(int x, out IEnumerable<GameItemObject> found, ItemObject itemObject) => TryGetAllWithMoreThanXAttributes(x, out found, itemObject.Uid());
        public virtual bool TryGetAllWithMoreThanXAttributes(int x, out IEnumerable<GameItemObject> found, GameItemObject gameItemObject) => TryGetAllWithMoreThanXAttributes(x, out found, gameItemObject.Uid());
        public virtual bool TryGetAllWithMoreThanXAttributes(int x, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithMoreThanXAttributes(x, itemObjectUid);
            return found.Any();
        }

        // Get all GameItemObjects with less than X amount of attributes, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithLessThanXAttributes(int amount, ItemObject itemObject) => GetAllWithLessThanXAttributes(amount, itemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithLessThanXAttributes(int amount, GameItemObject gameItemObject) => GetAllWithLessThanXAttributes(amount, gameItemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithLessThanXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => x.attributeList.Count() < amount)
                : list.Where(x => x.Uid() == itemObjectUid && x.attributeList.Count() < amount);

        public virtual bool TryGetAllWithLessThanXAttributes(int x, out IEnumerable<GameItemObject> found, ItemObject itemObject) => TryGetAllWithLessThanXAttributes(x, out found, itemObject.Uid());
        public virtual bool TryGetAllWithLessThanXAttributes(int x, out IEnumerable<GameItemObject> found, GameItemObject gameItemObject) => TryGetAllWithLessThanXAttributes(x, out found, gameItemObject.Uid());
        public virtual bool TryGetAllWithLessThanXAttributes(int x, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithLessThanXAttributes(x, itemObjectUid);
            return found.Any();
        }
        
        // Get all GameItemObjects with exactly X amount of attributes, optionally specify itemObjectUid
        public virtual IEnumerable<GameItemObject> GetAllWithExactlyXAttributes(int amount, ItemObject itemObject) => GetAllWithExactlyXAttributes(amount, itemObject.Uid());
        public virtual IEnumerable<GameItemObject> GetAllWithExactlyXAttributes(int amount, GameItemObject gameItemObject) => GetAllWithExactlyXAttributes(amount, gameItemObject.Uid());        
        public virtual IEnumerable<GameItemObject> GetAllWithExactlyXAttributes(int amount, string itemObjectUid = null) 
            => itemObjectUid == null
                ? list.Where(x => x.attributeList.Count() == amount)
                : list.Where(x => x.Uid() == itemObjectUid && x.attributeList.Count() == amount);
        
        public virtual bool TryGetAllWithExactlyXAttributes(int amount, out IEnumerable<GameItemObject> found, ItemObject itemObject) => TryGetAllWithExactlyXAttributes(amount, out found, itemObject.Uid());
        public virtual bool TryGetAllWithExactlyXAttributes(int amount, out IEnumerable<GameItemObject> found, GameItemObject gameItemObject) => TryGetAllWithExactlyXAttributes(amount, out found, gameItemObject.Uid());
        public virtual bool TryGetAllWithExactlyXAttributes(int amount, out IEnumerable<GameItemObject> found, string itemObjectUid = null) 
        {
            found = GetAllWithExactlyXAttributes(amount, itemObjectUid);
            return found.Any();
        }

        // Remove all GameItemObjects with exactly X amount of attributes, optionally specify itemObjectUid
        public virtual void RemoveAllWithExactlyXAttributes(int amount, ItemObject itemObject) => RemoveAllWithExactlyXAttributes(amount, itemObject.Uid());
        public virtual void RemoveAllWithExactlyXAttributes(int amount, GameItemObject gameItemObject) => RemoveAllWithExactlyXAttributes(amount, gameItemObject.Uid());        
        public virtual void RemoveAllWithExactlyXAttributes(int amount, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => x.attributeList.Count() == amount);
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && x.attributeList.Count() == amount);
        }

        // Remove all GameItemObjects with less than X amount of attributes, optionally specify itemObjectUid
        public virtual void RemoveAllWithLessThanXAttributes(int amount, ItemObject itemObject) => RemoveAllWithLessThanXAttributes(amount, itemObject.Uid());
        public virtual void RemoveAllWithLessThanXAttributes(int amount, GameItemObject gameItemObject) => RemoveAllWithLessThanXAttributes(amount, gameItemObject.Uid());
        public virtual void RemoveAllWithLessThanXAttributes(int amount, string itemObjectUid = null) 
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => x.attributeList.Count() < amount);
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && x.attributeList.Count() < amount);
        }

        // Remove all GameItemObjects with more than X amount of attributes, optionally specify itemObjectUid
        public virtual void RemoveAllWithMoreThanXAttributes(int amount, ItemObject itemObject) => RemoveAllWithMoreThanXAttributes(amount, itemObject.Uid());
        public virtual void RemoveAllWithMoreThanXAttributes(int amount, GameItemObject gameItemObject) => RemoveAllWithMoreThanXAttributes(amount, gameItemObject.Uid());
        public virtual void RemoveAllWithMoreThanXAttributes(int amount, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => x.attributeList.Count() > amount);
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && x.attributeList.Count() > amount);
        }

        // Remove all GameItemObjects without any attributes at all, optionally specify itemObjectUid
        public virtual void RemoveAllWithNoAttributes(ItemObject itemObject) => RemoveAllWithNoAttributes(itemObject.Uid());
        public virtual void RemoveAllWithNoAttributes(GameItemObject gameItemObject) => RemoveAllWithNoAttributes(gameItemObject.Uid());
        public virtual void RemoveAllWithNoAttributes(string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => x.attributeList.Count() == 0);
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && x.attributeList.Count() == 0);
        }

        // Remove all GameItemObjects that contain a specific ItemAttribute, optionally specify itemObjectUid
        public virtual void RemoveAllWithAttribute(ItemAttribute attribute, string itemObjectUid = null) => RemoveAllWithAttribute(attribute.Uid(), itemObjectUid);
        public virtual void RemoveAllWithAttribute(GameItemAttribute gameAttribute, string itemObjectUid = null) => RemoveAllWithAttribute(gameAttribute.Uid(), itemObjectUid);
        public virtual void RemoveAllWithAttribute(string attributeUid, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => x.attributeList.GetAll(attributeUid).Any());
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && x.attributeList.GetAll(attributeUid).Any());
        }

        // Remove all GameItemObjects that contain all of the provided ItemAttributes, optionally specify itemObjectUid
        public virtual void RemoveAllWithAllAttributes(IEnumerable<ItemAttribute> attributes, string itemObjectUid = null) => RemoveAllWithAllAttributes(attributes.Select(a => a.Uid()), itemObjectUid);
        public virtual void RemoveAllWithAllAttributes(IEnumerable<GameItemAttribute> gameAttributes, string itemObjectUid = null) => RemoveAllWithAllAttributes(gameAttributes.Select(a => a.Uid()), itemObjectUid);
        public virtual void RemoveAllWithAllAttributes(IEnumerable<string> attributeUids, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => attributeUids.All(uid => x.attributeList.GetAll(uid).Any()));
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && attributeUids.All(uid => x.attributeList.GetAll(uid).Any()));
        }

        // Remove all GameItemObjects that contain any of the provided ItemAttributes, optionally specify itemObjectUid
        public virtual void RemoveAllWithAnyAttributes(IEnumerable<ItemAttribute> attributes, string itemObjectUid = null) => RemoveAllWithAnyAttributes(attributes.Select(a => a.Uid()), itemObjectUid);
        public virtual void RemoveAllWithAnyAttributes(IEnumerable<GameItemAttribute> gameAttributes, string itemObjectUid = null) => RemoveAllWithAnyAttributes(gameAttributes.Select(a => a.Uid()), itemObjectUid);
        public virtual void RemoveAllWithAnyAttributes(IEnumerable<string> attributeUids, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                list.RemoveAll(x => attributeUids.Any(uid => x.attributeList.GetAll(uid).Any()));
            else
                list.RemoveAll(x => x.Uid() == itemObjectUid && attributeUids.Any(uid => x.attributeList.GetAll(uid).Any()));
        }


        public virtual bool ContainsAttribute(string uid) => list.Any(x => x.HasAttribute(uid));
        public virtual bool ContainsAttribute(ItemAttribute itemAttribute) => list.Any(x => x.HasAttribute(itemAttribute.Uid()));
        public virtual bool ContainsAttribute(GameItemAttribute gameItemAttribute) => list.Any(x => x.HasAttribute(gameItemAttribute.Uid()));
        
        public virtual bool ContainsQuestItem() => list.Any(x => x.QuestItem);

        public virtual bool Contains(ItemObject itemObject) => Contains(itemObject.Uid());
        public virtual bool Contains(string uid, string attributeUid = null) 
            => attributeUid == null 
                ? list.Any(x => x.Uid() == uid) 
                : list.Any(x => x.Uid() == uid && x.HasAttribute(attributeUid));
        
        // Check if any GameItemObject contains a specific ItemAttribute, optionally specify itemObjectUid
        public virtual bool ContainsWithAttribute(ItemAttribute attribute, string itemObjectUid = null) => ContainsWithAttribute(attribute.Uid(), itemObjectUid);
        public virtual bool ContainsWithAttribute(GameItemAttribute gameAttribute, string itemObjectUid = null) => ContainsWithAttribute(gameAttribute.Uid(), itemObjectUid);
        public virtual bool ContainsWithAttribute(string attributeUid, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                return list.Any(x => x.attributeList.GetAll(attributeUid).Any());
            return list.Any(x => x.Uid() == itemObjectUid && x.attributeList.GetAll(attributeUid).Any());
        }

        // Check if any GameItemObject contains all of the provided ItemAttributes, optionally specify itemObjectUid
        public virtual bool ContainsWithAllAttributes(IEnumerable<ItemAttribute> attributes, string itemObjectUid = null) => ContainsWithAllAttributes(attributes.Select(a => a.Uid()), itemObjectUid);
        public virtual bool ContainsWithAllAttributes(IEnumerable<GameItemAttribute> gameAttributes, string itemObjectUid = null) => ContainsWithAllAttributes(gameAttributes.Select(a => a.Uid()), itemObjectUid);
        public virtual bool ContainsWithAllAttributes(IEnumerable<string> attributeUids, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                return list.Any(x => attributeUids.All(uid => x.attributeList.GetAll(uid).Any()));
            return list.Any(x => x.Uid() == itemObjectUid && attributeUids.All(uid => x.attributeList.GetAll(uid).Any()));
        }

        // Check if any GameItemObject contains any of the provided ItemAttributes, optionally specify itemObjectUid
        public virtual bool ContainsWithAnyAttributes(IEnumerable<ItemAttribute> attributes, string itemObjectUid = null) => ContainsWithAnyAttributes(attributes.Select(a => a.Uid()), itemObjectUid);
        public virtual bool ContainsWithAnyAttributes(IEnumerable<GameItemAttribute> gameAttributes, string itemObjectUid = null) => ContainsWithAnyAttributes(gameAttributes.Select(a => a.Uid()), itemObjectUid);
        public virtual bool ContainsWithAnyAttributes(IEnumerable<string> attributeUids, string itemObjectUid = null)
        {
            if (itemObjectUid == null)
                return list.Any(x => attributeUids.Any(uid => x.attributeList.GetAll(uid).Any()));
            return list.Any(x => x.Uid() == itemObjectUid && attributeUids.Any(uid => x.attributeList.GetAll(uid).Any()));
        }
        
        // Check if any GameItemObject contains exactly X amount of attributes, optionally specify itemObjectUid
        public virtual bool ContainsWithExactlyXAttributes(int amount, ItemObject itemObject) => ContainsWithExactlyXAttributes(amount, itemObject.Uid());
        public virtual bool ContainsWithExactlyXAttributes(int amount, GameItemObject gameItemObject) => ContainsWithExactlyXAttributes(amount, gameItemObject.Uid());
        public virtual bool ContainsWithExactlyXAttributes(int amount, string itemObjectUid = null)
        {
            return itemObjectUid == null
                ? list.Any(x => x.attributeList.Count() == amount)
                : list.Any(x => x.Uid() == itemObjectUid && x.attributeList.Count() == amount);
        }

        // Check if any GameItemObject contains less than X amount of attributes, optionally specify itemObjectUid
        public virtual bool ContainsWithLessThanXAttributes(int amount, ItemObject itemObject) => ContainsWithLessThanXAttributes(amount, itemObject.Uid());
        public virtual bool ContainsWithLessThanXAttributes(int amount, GameItemObject gameItemObject) => ContainsWithLessThanXAttributes(amount, gameItemObject.Uid());
        public virtual bool ContainsWithLessThanXAttributes(int amount, string itemObjectUid = null)
        {
            return itemObjectUid == null
                ? list.Any(x => x.attributeList.Count() < amount)
                : list.Any(x => x.Uid() == itemObjectUid && x.attributeList.Count() < amount);
        }

        // Check if any GameItemObject contains more than X amount of attributes, optionally specify itemObjectUid
        public virtual bool ContainsWithMoreThanXAttributes(int amount, ItemObject itemObject) => ContainsWithMoreThanXAttributes(amount, itemObject.Uid());
        public virtual bool ContainsWithMoreThanXAttributes(int amount, GameItemObject gameItemObject) => ContainsWithMoreThanXAttributes(amount, gameItemObject.Uid());
        public virtual bool ContainsWithMoreThanXAttributes(int amount, string itemObjectUid = null)
        {
            return itemObjectUid == null
                ? list.Any(x => x.attributeList.Count() > amount)
                : list.Any(x => x.Uid() == itemObjectUid && x.attributeList.Count() > amount);
        }

        // Check if any GameItemObject does not contain any attributes at all, optionally specify itemObjectUid
        public virtual bool ContainsWithNoAttributes(ItemObject itemObject) => ContainsWithNoAttributes(itemObject.Uid());
        public virtual bool ContainsWithNoAttributes(GameItemObject gameItemObject) => ContainsWithNoAttributes(gameItemObject.Uid());
        public virtual bool ContainsWithNoAttributes(string itemObjectUid = null)
        {
            return itemObjectUid == null
                ? list.Any(x => x.attributeList.Count() == 0)
                : list.Any(x => x.Uid() == itemObjectUid && x.attributeList.Count() == 0);
        }

        // Check if there is exactly X amount of a specific ItemObject in the list
        public virtual bool ContainsExactlyXItemObjects(int amount, ItemObject itemObject) => ContainsExactlyXItemObjects(amount, itemObject.Uid());
        public virtual bool ContainsExactlyXItemObjects(int amount, GameItemObject gameItemObject) => ContainsExactlyXItemObjects(amount, gameItemObject.Uid());
        public virtual bool ContainsExactlyXItemObjects(int amount, string itemObjectUid = null)
        {
            return list.Count(x => itemObjectUid == null || x.Uid() == itemObjectUid) == amount;
        }

        // Check if there is less than X amount of a specific ItemObject in the list
        public virtual bool ContainsLessThanXItemObjects(int amount, ItemObject itemObject) => ContainsLessThanXItemObjects(amount, itemObject.Uid());
        public virtual bool ContainsLessThanXItemObjects(int amount, GameItemObject gameItemObject) => ContainsLessThanXItemObjects(amount, gameItemObject.Uid());
        public virtual bool ContainsLessThanXItemObjects(int amount, string itemObjectUid = null)
        {
            return list.Count(x => itemObjectUid == null || x.Uid() == itemObjectUid) < amount;
        }

        // Check if there is more than X amount of a specific ItemObject in the list
        public virtual bool ContainsMoreThanXItemObjects(int amount, ItemObject itemObject) => ContainsMoreThanXItemObjects(amount, itemObject.Uid());
        public virtual bool ContainsMoreThanXItemObjects(int amount, GameItemObject gameItemObject) => ContainsMoreThanXItemObjects(amount, gameItemObject.Uid());
        public virtual bool ContainsMoreThanXItemObjects(int amount, string itemObjectUid = null)
        {
            return list.Count(x => itemObjectUid == null || x.Uid() == itemObjectUid) > amount;
        }

        // Check if there are no ItemObjects in the list
        public virtual bool IsEmpty() => !list.Any();
        
        // Contains more than X amount of distinct ItemObjects
        public virtual bool ContainsMoreThanXItemObjects(int amount, IEnumerable<ItemObject> itemObjects, bool distinct = false) => ContainsMoreThanXItemObjects(amount, itemObjects.Select(i => i.Uid()), distinct);
        public virtual bool ContainsMoreThanXItemObjects(int amount, IEnumerable<GameItemObject> gameItemObjects, bool distinct = false) => ContainsMoreThanXItemObjects(amount, gameItemObjects.Select(g => g.Uid()), distinct);
        public virtual bool ContainsMoreThanXItemObjects(int amount, IEnumerable<string> itemObjectUids, bool distinct = false) 
            => (distinct 
                ? list.Select(x => x.Uid()).Distinct() 
                : list.Select(x => x.Uid()))
                .Count(itemObjectUids.Contains) > amount;

        // Contains less than X amount of distinct ItemObjects
        public virtual bool ContainsLessThanXItemObjects(int amount, IEnumerable<ItemObject> itemObjects, bool distinct = false) => ContainsLessThanXItemObjects(amount, itemObjects.Select(i => i.Uid()), distinct);
        public virtual bool ContainsLessThanXItemObjects(int amount, IEnumerable<GameItemObject> gameItemObjects, bool distinct = false) => ContainsLessThanXItemObjects(amount, gameItemObjects.Select(g => g.Uid()), distinct);
        public virtual bool ContainsLessThanXItemObjects(int amount, IEnumerable<string> itemObjectUids, bool distinct = false) 
            => (distinct 
                ? list.Select(x => x.Uid()).Distinct() 
                : list.Select(x => x.Uid()))
                .Count(itemObjectUids.Contains) < amount;

        // Contains exactly X amount of distinct ItemObjects
        public virtual bool ContainsExactlyXItemObjects(int amount, IEnumerable<ItemObject> itemObjects, bool distinct = false) => ContainsExactlyXItemObjects(amount, itemObjects.Select(i => i.Uid()), distinct);
        public virtual bool ContainsExactlyXItemObjects(int amount, IEnumerable<GameItemObject> gameItemObjects, bool distinct = false) => ContainsExactlyXItemObjects(amount, gameItemObjects.Select(g => g.Uid()), distinct);
        public virtual bool ContainsExactlyXItemObjects(int amount, IEnumerable<string> itemObjectUids, bool distinct = false) 
            => (distinct 
                ? list.Select(x => x.Uid()).Distinct() 
                : list.Select(x => x.Uid()))
                .Count(itemObjectUids.Contains) == amount;

        // Contains any of the distinct ItemObjects
        public virtual bool ContainsAnyItemObjects(IEnumerable<ItemObject> itemObjects) => ContainsAnyItemObjects(itemObjects.Select(i => i.Uid()));
        public virtual bool ContainsAnyItemObjects(IEnumerable<GameItemObject> gameItemObjects) => ContainsAnyItemObjects(gameItemObjects.Select(g => g.Uid()));
        public virtual bool ContainsAnyItemObjects(IEnumerable<string> itemObjectUids) 
            => list.Select(x => x.Uid()).Any(itemObjectUids.Contains);

        // Contains all of the distinct ItemObjects
        public virtual bool ContainsAllItemObjects(IEnumerable<ItemObject> itemObjects, bool distinct = false) => ContainsAllItemObjects(itemObjects.Select(i => i.Uid()), distinct);
        public virtual bool ContainsAllItemObjects(IEnumerable<GameItemObject> gameItemObjects, bool distinct = false) => ContainsAllItemObjects(gameItemObjects.Select(g => g.Uid()), distinct);
        public virtual bool ContainsAllItemObjects(IEnumerable<string> itemObjectUids, bool distinct = false) 
            => itemObjectUids.All(uid => (distinct 
                ? list.Select(x => x.Uid()).Distinct() 
                : list.Select(x => x.Uid()))
                .Count(x => x == uid) > 0);

        // Does not contain any of the distinct ItemObjects
        public virtual bool ContainsNoItemObjects(IEnumerable<ItemObject> itemObjects) => ContainsNoItemObjects(itemObjects.Select(i => i.Uid()));
        public virtual bool ContainsNoItemObjects(IEnumerable<GameItemObject> gameItemObjects) => ContainsNoItemObjects(gameItemObjects.Select(g => g.Uid()));
        public virtual bool ContainsNoItemObjects(IEnumerable<string> itemObjectUids, bool distinct = false) 
            => !list.Select(x => x.Uid()).Any(itemObjectUids.Contains);
        
        // Helper method to compare attributes
        protected virtual bool AttributesAreEqual(IEnumerable<GameItemAttribute> list1, IEnumerable<GameItemAttribute> list2)
        {
            if (list1.Count() != list2.Count()) return false;
            
            var attrList1 = list1.Select(a => a.Uid()).OrderBy(uid => uid).ToList();
            var attrList2 = list2.Select(a => a.Uid()).OrderBy(uid => uid).ToList();

            return attrList1.SequenceEqual(attrList2);
        }

        // Check if the list contains duplicate GameItemObjects based on their Uid() and optionally their attributeList
        public virtual bool ContainsDuplicates(ItemObject itemObject, bool includeItemAttributes = false) => ContainsDuplicates(itemObject.Uid(), includeItemAttributes);
        public virtual bool ContainsDuplicates(GameItemObject gameItemObject, bool includeItemAttributes = false) => ContainsDuplicates(gameItemObject.Uid(), includeItemAttributes);
        public virtual bool ContainsDuplicates(string itemObjectUid = null, bool includeItemAttributes = false)
        {
            var duplicates = list.Where(x => itemObjectUid == null || x.Uid() == itemObjectUid);

            return duplicates.GroupBy(x => x.Uid()).Any(group =>
                group.Count() > 1 && (!includeItemAttributes || group.Skip(1).All(y => AttributesAreEqual(group.First().attributeList.list, y.attributeList.list))));
        }

        // Check if the list contains duplicate GameItemObjects from the provided IEnumerable, based on their Uid() and optionally their attributeList
        public virtual bool ContainsDuplicates(IEnumerable<ItemObject> itemObjects, bool includeItemAttributes = false) => ContainsDuplicates(itemObjects.Select(i => i.Uid()), includeItemAttributes);
        public virtual bool ContainsDuplicates(IEnumerable<GameItemObject> gameItemObjects, bool includeItemAttributes = false) => ContainsDuplicates(gameItemObjects.Select(g => g.Uid()), includeItemAttributes);
        public virtual bool ContainsDuplicates(IEnumerable<string> itemObjectUids, bool includeItemAttributes = false)
        {
            var duplicates = list.Where(x => itemObjectUids.Contains(x.Uid()));

            return duplicates.GroupBy(x => x.Uid()).Any(group =>
                group.Count() > 1 && (!includeItemAttributes || group.Skip(1).All(y => AttributesAreEqual(group.First().attributeList.list, y.attributeList.list))));
        }

        

        public virtual int CountDuplicates(ItemObject itemObject, bool includeItemAttributes = false, bool includeFirstItem = false) => CountDuplicates(itemObject.Uid(), includeItemAttributes, includeFirstItem);
        public virtual int CountDuplicates(GameItemObject gameItemObject, bool includeItemAttributes = false, bool includeFirstItem = false) => CountDuplicates(gameItemObject.Uid(), includeItemAttributes, includeFirstItem);
        public virtual int CountDuplicates(string itemObjectUid = null, bool includeItemAttributes = false, bool includeFirstItem = false)
        {
            var duplicates = GetDuplicates(itemObjectUid, includeItemAttributes);
            return duplicates.GroupBy(d => d.Uid()).Sum(g => includeFirstItem ? g.Count() : g.Count() - 1);
        }

        public virtual int CountDuplicates(IEnumerable<ItemObject> itemObjects, bool includeItemAttributes = false, bool includeFirstItem = false) => CountDuplicates(itemObjects.Select(i => i.Uid()), includeItemAttributes, includeFirstItem);
        public virtual int CountDuplicates(IEnumerable<GameItemObject> gameItemObjects, bool includeItemAttributes = false, bool includeFirstItem = false) => CountDuplicates(gameItemObjects.Select(g => g.Uid()), includeItemAttributes, includeFirstItem);
        public virtual int CountDuplicates(IEnumerable<string> itemObjectUids, bool includeItemAttributes = false, bool includeFirstItem = false)
        {
            var duplicates = GetDuplicates(itemObjectUids, includeItemAttributes);
            return duplicates.GroupBy(d => d.Uid()).Sum(g => includeFirstItem ? g.Count() : g.Count() - 1);
        }



        public virtual int CountQuestItems(string uid = null) => uid == null 
            ? list.Count(x => x.QuestItem)
            : list.Count(x => x.Uid() == uid && x.QuestItem);
        
        public virtual int CountItemsWithAttribute(string uid) => list.Count(x => x.HasAttribute(uid));
        public virtual int CountItemsWithAttribute(ItemAttribute itemAttribute) => list.Count(x 
            => x.HasAttribute(itemAttribute.Uid()));
        public virtual int CountItemsWithAttribute(GameItemAttribute gameItemAttribute) => list.Count(x 
            => x.HasAttribute(gameItemAttribute.Uid()));

        // **********************************************************************
        // Transferring
        // **********************************************************************
        
        // NOTE: For ItemObject, we will default to NOT distinct.
        public virtual GameItemObject ReceiveTransfer(GameItemObject gameItemObject, bool distinct = false, bool makeClone = true, bool ignoreQuestItem = false)
        {
            if (gameItemObject == null) return default;

            // Can't transfer quest items
            if (!ignoreQuestItem // If we aren't ignoring quest items
                && gameItemObject.QuestItem  // And this is a quest item
                && !canReceiveQuestItems) // and we can't receive quest item....
                return default;
            
            if (distinct && Contains(gameItemObject.Uid())) return this.Get(gameItemObject.Uid());
            var clone = makeClone ? gameItemObject.Clone() : gameItemObject;
            list.Add(clone);
            clone.SetParentList(this);
            SetAffectedStatsDirty(clone);
            this.AddThisToBlackboard();
           
            return clone;
        }
        
        public virtual GameItemObject TransferTo(GameItemObjectList transferTo, GameItemObject transferObject, bool distinct = false, bool makeClone = true, bool ignoreQuestItem = false)
        {
            var objectAtDestination = transferTo.ReceiveTransfer(transferObject, distinct, makeClone, ignoreQuestItem);
            
            // If distinct = true, transfer may fail if an object like this is already present.
            if (objectAtDestination == null)
                return null;
            
            this.RemoveExact(transferObject);
            this.AddThisToBlackboard();
            return objectAtDestination;
            
        }
        
        // **********************************************************************
        // Additional Methods, Variables, and Properties for this type
        // **********************************************************************

        [Header("Quest Item objects")]
        public bool canReceiveQuestItems;
        [FormerlySerializedAs("transferRule")] public QuestItemTransferRule questItemTransferRule = QuestItemTransferRule.No;
        
        public enum QuestItemTransferRule
        {
            No, Yes
        }

        // June 4, 2023 -- These are not used right now. Perhaps in the future. Not sure if we will have individual objects of this type post to the blackboard
        // Handles force posting for individual objects in the list
        public bool forcePostToBlackboard; // If true, objects will post to blackboard even if they are individual set to not post
        public bool forceNotifyOnPost; // If true, when objects post, they will be forced to notify followers of the blackboard

        // **********************************************************************
        // Inventory Module -- Handler for spots and passing objects into this list
        // **********************************************************************

        //private Spots Spots => itemObjectRepository.DisplayableSpots(Owner.GameId());
        protected virtual Spots Spots => SpotsTest();
        protected virtual Spots SpotsTest() {
            //Debug.Log($"repositor is null? {itemObjectRepository == null}");
            //Debug.Log($"Owner is null? {Owner == null}");
                return GameModuleRepository.Instance.DisplayableSpots(Owner.GameId());
        }
        
        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject)
        {
            if (Spots == null)
            {
                Debug.LogError($"Attempting to take {gameItemObject.FullName()} into this inventory, but " +
                               "spots is null. Did you forget to register spots with the ItemObjectRepository?");
                return false;
            }
            
            int spotRow;
            int spotColumn;
            (spotRow, spotColumn) = Spots.CanFitItem(gameItemObject);
            //Debug.Log($"Should put in row {spotRow} column {spotColumn}");

            if (spotRow < 0 || spotColumn < 0)
                return false;
            
            return TakeIntoInventoryGrid(gameItemObject, spotRow, spotColumn);
        }
        
        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject, int spotRow, int spotColumn)
        {
            if (Spots == null)
            {
                Debug.LogError($"Attempting to take {gameItemObject.FullName()} into this inventory, but " +
                               "spots is null. Did you forget to register spots with the ItemObjectRepository?");
                return false;
            }
            
            ReceiveTransfer(gameItemObject); // Transferring will clone the GameItemObject
            list[^1].SetSpots(spotRow, spotColumn);
            Spots.AddItemToInventoryGrid(list[^1], spotRow, spotColumn);
            
            return true;
        }
        
        /// <summary>
        /// Relinks the prefabInventory and prefabWorld from the ItemObject scriptable object. Required after loading data
        /// if using the Inventory module.
        /// </summary>
        public virtual void RelinkPrefabs()
        {
            foreach (var gameItemObject in list)
                gameItemObject.RelinkPrefabs();
        }
        
        
    }
}