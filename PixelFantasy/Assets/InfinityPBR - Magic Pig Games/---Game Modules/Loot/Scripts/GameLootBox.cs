using System;
using System.Collections;
using InfinityPBR.Modules.Inventory;
using UnityEngine;
using static InfinityPBR.Debugs;

/*
 * GAME LOOT BOX
 *
 * Include a GameLootBox object in your management class for anything that holds loot.
 * 
 * This is the in-game, runtime object to use for Loot Box. It will automatically cache the scriptable object from
 * the Loot Box repository.
 *
 * Best to not modify this script, but rather override specific methods to integrate the Loot Box module into your
 * game system. This way, any updates we make to this script will not affect your game in the future.
 */

/*
 * NOTE: Opposed to other "Game[Thing]" objects, GameLootBox is a Monobehaviour. This is because it is not
 * an object that is meant to attach to a player or character, but is a stand-alone component of a treasure
 * box or other thing that holds items.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot")]
    [Serializable]
    public class GameLootBox : MonoBehaviour, IHaveInventory, IHaveDictionaries, IHaveGameId, ISaveable
    {
        [Header("MUST BE UNIQUE!")]
        [Tooltip("Each LootBox must have a unique name. You can name this via code, or manually here, and perhaps it can be " +
                 "human-readable and descriptive.")]
        [SerializeField] private string _gameId;

        [Header("Required")] 
        [Tooltip("This is the uid from the LootBox Scriptable Object which will be used when this box is populated.")]
        [SerializeField] private string lootBoxUid;

        [Header("Options")] 
        [Tooltip("If true, this will generate loot on Awake(). You may wish to generate manually when a player first " +
                 "\"opens\" the box, instead.")]
        public bool generateOnAwake = true;

        // Public Variables / Properties
        public LootBox LootBox => GetLootBox();

        [HideInInspector] public bool generated;
        [Header("Auto-Generated")] public GameItemObjectList lootBoxInventory = new GameItemObjectList();
        public Dictionaries dictionaries = new Dictionaries("Loot Box");
        private IHandleLootBoxes _customLootBoxHandler;
        
        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.4f, 0.8f, 0.8f);

        // Privates
        [SerializeField] [HideInInspector] private LootBox _lootBox;

        protected virtual void OnValidate() => GameId();
        protected virtual void Awake()
        {
            StartCoroutine(RegisterToRepository());
            _customLootBoxHandler = GetComponent<IHandleLootBoxes>();
        }
        protected virtual void Start() => StartCoroutine(AutoGenerateLoot());

        protected virtual IEnumerator RegisterToRepository()
        {
            while (GameModuleRepository.Instance == null)
                yield return null;
            
            RegisterDisplayableInventoryWithRepository();
        }
        
        protected virtual IEnumerator AutoGenerateLoot()
        {
            if (!generateOnAwake || generated) 
                yield break;

            GenerateLoot();
        }

        // ------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------------------------------
        
        /// <summary>
        /// This will generate a new list of item objects, into itemObjectList. Will overwrite unless false is passed.
        /// </summary>
        /// <param name="overwrite"></param>
        public virtual void GenerateLoot(bool overwrite = true)
        {
            if (!overwrite && generated) return; // If overwrite is false and loot has been generated, then return
            lootBoxInventory = LootBox.GenerateLoot(_customLootBoxHandler).Clone(); // Generate the loot and clone it.
            generated = true;
            
            WriteToConsole($"This lootbox generated {lootBoxInventory.Count()} items.", "GameLootBox"
                , writeToConsoleColor, writeToConsole, false, gameObject);
            HandleLoot();
        }

        protected virtual void HandleLoot()
        {
            /*
             * CUSTOM CODE ZONE
             *
             * Now, you have a list of Items. What are you going to do with it??
             *
             * This portion is entirely up to you and your project. In the demo, I'm using the Inventory module, to
             * put these items into the actual "Box" using that module. In some cases, not all of items we just
             * generated will even fit!
             *
             * But, your system may not be using the Inventory module, and if that is the case, then you'll want to
             * remove the code below, and write your own system to handle the List<Item> that was just populated.
             *
             * -- THIS IS A METHOD YOU SHOULD OVERRIDE FROM YOUR OWN SCRIPT WHICH INHERITS FROM GameLootBox --
             */
            
            // Check out "GameLootBoxDemo.cs" to see how we do it in the demo scene.
        }

        // ------------------------------------------------------------------------------------------
        // PRIVATE METHODS
        // ------------------------------------------------------------------------------------------
        
        protected virtual LootBox GetLootBox()
        {
            if (_lootBox != null) return _lootBox;
            if (String.IsNullOrWhiteSpace(lootBoxUid)) return default;

            LinkLootBox();
            return _lootBox;
        }

        protected virtual void LinkLootBox()
        {
            _lootBox = GameModuleRepository.Instance.Get<LootBox>(lootBoxUid); // Assign the Scriptable Object
            dictionaries = _lootBox.dictionaries.Clone(); // Clone the dictionary over (don't copy, or you may overwrite the Scriptable Object data!)
        }
        
        // ------------------------------------------------------------------------------------------
        // DICTIONARY (IHaveDictionary)
        // ------------------------------------------------------------------------------------------

        public void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);
        public KeyValue GetKeyValue(string key) => dictionaries.Key(key);
        public bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
        public void CheckForMissingObjectReferences()
        {
            // Not used in this context
        }


        /*
         * These methods are required by implementations but are not currently used in this context.
         */
        public string ObjectId()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void LateTick()
        {
            throw new NotImplementedException();
        }

        public string GetOwnerName()
        {
            throw new NotImplementedException();
        }

        public void SetOwner(object newOwner)
        {
            throw new NotImplementedException();
        }

        public GameItemObjectList DisplayableInventory() => lootBoxInventory;
        public Spots DisplayableSpots()
        {
            throw new NotImplementedException();
        }

        public void RegisterDisplayableInventoryWithRepository() 
            => GameModuleRepository.Instance.RegisterDisplayableInventory(this);

        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject);
        

        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject, int spotRow, int spotColumn) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject, spotRow, spotColumn);

        public virtual string GameId(bool forceNew = false) =>
            string.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;

        public virtual object SaveState()
        {
            var data = new GameLootBoxSaveData
            {
                lootBoxUid = lootBoxUid,
                lootBoxInventory = lootBoxInventory,
                dictionaries = dictionaries,
                generated = generated
            };

            return data;
        }

        public virtual void LoadState(string jsonEncodedState)
        {
            var data = JsonUtility.FromJson<GameLootBoxSaveData>(jsonEncodedState);
            
            lootBoxUid = data.lootBoxUid;
            lootBoxInventory = data.lootBoxInventory;
            dictionaries = data.dictionaries;
            generated = data.generated;
        }

        public struct GameLootBoxSaveData
        {
            public string lootBoxUid;
            public GameItemObjectList lootBoxInventory;
            public Dictionaries dictionaries;
            public bool generated;
        }

        public virtual string SaveableObjectId() => GameId();

        public void PreSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PostSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PreLoadActions()
        {
            throw new NotImplementedException();
        }

        public void PostLoadActions()
        {
            throw new NotImplementedException();
        }
    }
}
