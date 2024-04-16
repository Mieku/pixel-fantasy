using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [Serializable]
    public class GameItemAttributeList : GameModulesList<GameItemAttribute>, IHaveStartActions
    {
        // **********************************************************************
        // General things for all Game Module Lists
        // **********************************************************************
        
        public override string GameId() => Owner.GameId();
        public override string NoteSubject() => "Item Attributes";
        public GameItemAttributeList Clone() => JsonUtility.FromJson<GameItemAttributeList>(JsonUtility.ToJson(this));

        // **********************************************************************
        // Owner 
        // **********************************************************************

        public IHaveStats Owner
        {
            get => GetOwner();
            private set => _owner = value;
        }
        public void SetOwner(IHaveStats value) => Owner = value;
        private IHaveStats GetOwner() => ParentItemObject != null ? ParentItemObject.Owner : _owner;
        private IHaveStats _owner;
        
        // Parent Item Object [Unique to Game Item Attribute List]
        public GameItemObject ParentItemObject { get; private set; }
        public void SetParentItemObject(GameItemObject value) => ParentItemObject = value;
        
        // **********************************************************************
        // Start Actions
        //
        // These are actions that must be required when the application loads. At Start() on your manager script (often
        // the script which holds this object), start this coroutine. It will wait until the blackboard is present and
        // available.
        // **********************************************************************

        public virtual IEnumerator StartActions()
        {
            while (MainBlackboard.blackboard == null
                   || GameModuleRepository.Instance == null)

                yield return null;
            
            foreach (var item in list)
                item.StartActions();
        }
        
        // **********************************************************************
        // Affected Stats & Dirty
        // **********************************************************************

        public override void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            if (gameModuleObject is GameItemAttribute gameItemAttribute)
                Owner?.SetStatsDirty(gameItemAttribute.DirectlyAffectsList());
        }

        public void SetAffectedStatsDirty(List<Stat> statList = null)
            => Owner?.SetStatsDirty(statList ?? DirectlyAffectsList());

        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevels
            .SelectMany(x => x.targets)
            .Distinct()
            .ToList();

        public void SetDirty() => SetAffectedStatsDirty();
        
        // **********************************************************************
        // Modification Levels
        // **********************************************************************

        public virtual List<ModificationLevel> ModificationLevels => GetModificationLevels();
        protected virtual List<ModificationLevel> GetModificationLevels(bool affectsActorsOnly = true) 
            => !affectsActorsOnly 
                ? list.Select(attribute => attribute.ModificationLevel).ToList() 
                : (from gameItemAttribute in list 
                    where gameItemAttribute.Parent().affectsActor 
                    select gameItemAttribute.ModificationLevel).ToList();
        
        // **********************************************************************
        // Standard methods for specific Scriptable Object types
        // **********************************************************************

        public virtual GameItemAttribute Get(ItemAttribute itemAttribute, bool addIfNull = false, bool distinct = false)
            => this.Get(itemAttribute.Uid(), addIfNull, distinct);

        public virtual GameItemAttribute Add(GameItemAttribute newItem, bool distinct = true, bool ignoreIncompatible = false) 
            => Add(newItem.Parent().Uid(), distinct);
        
        public virtual GameItemAttribute Add(ItemAttribute newItem, bool distinct = true, bool ignoreIncompatible = false) 
            => Add(newItem.Uid(), distinct);
        
        public virtual GameItemAttribute Add(string uid, bool distinct = true, bool ignoreIncompatible = false)
        {
            if (distinct && this.Contains(uid)) return this.Get(uid);
            
            // Check for incompatibility -- All existing attributes must be compatible with the new addition
            if (!ignoreIncompatible && list.Any(x => x.IncompatibleWith(uid)))
                return default;

#if UNITY_EDITOR
            var newAttribute = new GameItemAttribute(GameModuleUtilities.GameModuleObject<ItemAttribute>(uid), Owner);
#else
            var newAttribute = new GameItemAttribute(GameModuleRepository.Instance.Get<ItemAttribute>(uid), Owner);
#endif
            return FinishTransfer(newAttribute);
        }
        
        public virtual bool Contains(ItemAttribute itemAttribute) => this.Contains(itemAttribute.Uid());
        
        public virtual bool ContainsAll(IEnumerable<ItemAttribute> itemAttributes) => itemAttributes.All(Contains);
        public virtual bool ContainsAny(IEnumerable<ItemAttribute> itemAttributes) => itemAttributes.Any(Contains);
        
        public virtual void RemoveAll(ItemAttribute itemAttribute) => this.RemoveAll(itemAttribute.Uid());
        public virtual void Remove(ItemAttribute itemAttribute) => this.Remove(itemAttribute.Uid());
        
        // **********************************************************************
        // Transfering
        // **********************************************************************
        
        protected virtual GameItemAttribute FinishTransfer(GameItemAttribute newAttribute)
        {
            // Check for whether this replaces others of the same type
            if (newAttribute.Parent().replaceOthers)
                this.RemoveType(newAttribute.objectType);
            
            // Add the new attribute to the list
            list.Add(newAttribute);
            
            // Grab the new item and set the parent information, and mark stats affected as dirty
            //var newItem = Get(uid); // March 2 2023 - Removed this line as I think newAttribute works in this case
            newAttribute.SetParentItemObject(ParentItemObject);
            newAttribute.SetParentList(this);
            SetAffectedStatsDirty(newAttribute);
            
            // Add required attributes
            //AddRequiredAttributes(itemAttributeRepository.GetByUid(uid)); // March 3, 2023 -- Can grab the Parent() directly instead
            AddRequiredAttributes(newAttribute.Parent()); 
            
            return newAttribute;
        }

        public virtual GameItemAttribute ReceiveTransfer(GameItemAttribute obj, bool distinct = true, bool makeClone = true, bool ignoreIncompatible = false)
        {
            if (distinct && this.Contains(obj.Uid())) return this.Get(obj.Uid());
            
            // Check for incompatibility -- All existing attributes must be compatible with the new addition
            if (!ignoreIncompatible && list.Any(x => x.IncompatibleWith(obj.Parent())))
                return default;
            
            var newAttribute = makeClone ? obj.Clone() : obj;
            newAttribute.SetOwner(Owner);
            
            return FinishTransfer(newAttribute);
        }

        protected virtual void AddRequiredAttributes(ItemAttribute itemAttribute)
        {
            foreach (var requiredAttribute in itemAttribute.requiredAttributes)
            {
                // If we can only have one per type, remove all else of the other type first
                if (requiredAttribute.onePerType)
                    list.RemoveAll(x => x.objectType == requiredAttribute.itemAttribute.objectType);
                
                // Add the required attribute
                Add(requiredAttribute.itemAttribute);
            }
        }

        public virtual GameItemAttribute TransferTo(GameItemAttributeList transferTo, GameItemAttribute transferObject, bool distinct = false)
        {
            transferObject.SetDirty();
            var objectAtDestination = transferTo.ReceiveTransfer(transferObject, distinct);
            
            // If distinct = true, transfer may fail if an object like this is already present.
            if (objectAtDestination == null)
                return null;
            
            this.RemoveExact(transferObject);
            SetAffectedStatsDirty(objectAtDestination);
            return objectAtDestination;
        }
        
        // **********************************************************************
        // Additional Methods, Variables, and Properties for this type
        // **********************************************************************

        
        
        // June 4, 2023 -- These are not used right now. Perhaps in the future. Not sure if we will have individual objects of this type post to the blackboard
        // Handles force posting for individual objects in the list
        public bool forcePostToBlackboard; // If true, objects will post to blackboard even if they are individual set to not post
        public bool forceNotifyOnPost; // If true, when objects post, they will be forced to notify followers of the blackboard
    }
}