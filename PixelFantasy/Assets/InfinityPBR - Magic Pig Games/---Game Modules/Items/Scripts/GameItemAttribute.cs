using System;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules;
using UnityEngine;

/*
 * GAME ITEM OBJECT
 *
 * This is the in-game, runtime object to use for Item Objects. It will automatically cache the scriptable object from
 * the Items repository.
 */

namespace InfinityPBR
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [Serializable]
    public class GameItemAttribute : IAmGameModuleObject, IAffectStats
    {
        // ************************************************************************************************
        // Connection to the parent object
        // ************************************************************************************************
        
        private ItemAttribute _parent;
        public ItemAttribute Parent() 
            => _parent 
                ? _parent 
                : _parent = GameModuleRepository.Instance.Get<ItemAttribute>(Uid());
        
        [SerializeField] private string _uid;
        public void SetUid(string value) => _uid = value;
        public string Uid() => _uid;
        
        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this
        public string objectType; // The type (parent directory from hierarchy name)
        public Dictionaries dictionaries = new Dictionaries("Unnamed");
        
        public GameItemAttributeList ParentList { get; private set; }
        public void SetParentList(GameItemAttributeList value) => ParentList = value;
        
        public GameItemObject ParentItemObject { get; private set; }
        public void SetParentItemObject(GameItemObject value) => ParentItemObject = value;

        private IHaveStats _owner;
        public void SetOwner(IHaveStats value) => _owner = value;
        public IHaveStats Owner => GetOwner();
        public IHaveStats GetOwner()
        {
            if (ParentItemObject != null) return ParentItemObject.Owner;
            if (ParentList != null) return ParentList.Owner;
            return _owner;
        }

        // **********************************************************************
        // GameId
        // **********************************************************************
        
        [SerializeField] private string _gameId;

        // Will create a new _gameId if one does not exist
        // NOTE: call forceNew true in Constructor. Also, be careful when cloning, as
        // in some cases you may wish to make a new GameId, but in other cases, cloning
        // will not want to make a new GameId.
        public virtual string GameId(bool forceNew = false) =>
            String.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;
        
        // ************************************************************************************************
        // Members unique for this type
        // ************************************************************************************************
        
        public int NameOrder => Parent().nameOrder;
        public string HumanName => Parent().humanName;

        public ModificationLevel ModificationLevel => GetModificationLevel(); // returns the current Modification Level from Stats
        private ModificationLevel _modificationLevel;
        
        
        public void SetDirty(bool dirtyValue = true)
        {
            if (!dirtyValue) return;
            SetAffectedStatsDirty(null);
        }

        public void StartActions()
        {
            CheckDictionaries();
        }
        

        // This is to ensure that if anything changes in the setup on Dictionaries for
        // ItemAttribute objects of this type, it is reflected here in the GameItemAttribute
        private void CheckDictionaries() 
            => Parent().dictionaries.keyValues
                .ForEach(keyValue => dictionaries.AddNewKeyValue(keyValue));
        
        public void SetAffectedStatsDirty(object obj) 
            => Owner?.SetStatsDirty(DirectlyAffectsList());

        // ------------------------------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------------------------------

        public GameItemAttribute(ItemAttribute parent, IHaveStats newOwner = null)
        {
            _parent = parent; // v3.6
            SetUid(parent.Uid()); // v3.6 -- set the uid with this method now
            objectName = parent.objectName;
            objectType = parent.objectType;
            dictionaries = parent.dictionaries.Clone(); // Clone the dictionaries object, so we don't overwrite our Scriptable Object data!

            GameId();
            SetOwner(newOwner); // Will set null if value is default. Owner is first taken from Parents before reverting to _owner
        }
        
        
        public ModificationLevel GetModificationLevel() => Parent().modificationLevel;

        public List<Stat> DirectlyAffectedByList(Stat stat = null)
        {
            // Not used in this context
            return default;
        }

        public List<ModificationLevel> GetModificationLevels() => new List<ModificationLevel>{GetModificationLevel()};

        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevel.targets;
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public GameItemAttribute Clone()
        {
            // Note: The clone will clone the Dictionaries object after, or it will continue to be linked directly to
            // the original object.
            var clonedItemAttribute = JsonUtility.FromJson<GameItemAttribute>(JsonUtility.ToJson(this));
            clonedItemAttribute.dictionaries = clonedItemAttribute.dictionaries.Clone();
            return clonedItemAttribute;
        }
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);

        public bool CompatibleWith(string itemAttributeUid) => !IncompatibleWith(itemAttributeUid);
        
        public bool CompatibleWith(ItemAttribute itemAttribute) => !IncompatibleWith(itemAttribute);
        public bool CompatibleWith(GameItemAttribute gameItemAttribute) => !IncompatibleWith(gameItemAttribute);
        
        public bool IncompatibleWith(string itemAttributeUid) 
            => Parent().incompatibleAttributes.Any(x => x.Uid() == itemAttributeUid);
        public bool IncompatibleWith(ItemAttribute itemAttribute) 
            => Parent().incompatibleAttributes.Any(x => x == itemAttribute);
        public bool IncompatibleWith(GameItemAttribute gameItemAttribute) 
            => Parent().incompatibleAttributes.Any(x => x == gameItemAttribute.Parent());
    }
}