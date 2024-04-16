using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InfinityPBR.Modules.Inventory.PanelManager;
using static InfinityPBR.Debugs;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 */

/*
 * In the demo, we are using a button to represent the box. In your game, likely the player will open this "box" in
 * some other method. But feel free to take from this script and add to your own in the ways that make sense.
 *
 * Keep in mind a "box" doesn't need to be a literal box, but could be any object or thing that has an inventory system
 * attached to it, like the top of a bed, or table, or a traditional box like a treasure, a wardrobe drawer or a barrel.
 */

namespace InfinityPBR.Modules.Inventory
{
    [Serializable]
    public class BoxManager : MonoBehaviour, ISaveable, IHaveInventory, IHaveStats
    {
        [Header("Required")]
        public GameObject inventoryPanel;
        [SerializeField] private string _gameId;
        public Spots spots = new Spots();

        [Header("Options")] 
        public bool canRemoveQuestItems = true; // Default true, as that seems to be the most likely use case
        public bool canReceiveQuestItems; // Default false to ensure players don't accidentally put quest items in a box
        
        [Header("Handled Automatically")]
        // This is the "things in the inventory". Populate it automatically with an additional script, or let it
        // start empty.
        [SerializeField] private GameItemObjectList inventoryItems = new GameItemObjectList();

        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.4f, 0.5f, 0.3f);

        private void Awake()
        {
            spots.SetIHaveInventoryGameId(GameId()); // Ensure GameId is populated + spots have it
            StartCoroutine(RegisterToRepository()); // Adds this to the Repository so spots can refer back to it
            StartCoroutine(inventoryItems.StartActions()); // StartActions on any items already populated in the inventory
            CheckSpots();
            SetOwner(this);

            // Make sure the ItemObjectList matches the bool value on the BoxManager
            inventoryItems.canReceiveQuestItems = canReceiveQuestItems;

            if (inventoryPanel != null) 
                return;

            WriteToConsole("Box Manager is missing the Box Inventory Prefab."
                    , "BoxManager", writeToConsoleColor, writeToConsole, true, gameObject);
            Destroy(this);
        }
        
        protected virtual void OnValidate(){
            GameId();
#if UNITY_EDITOR
            CheckSpots();
#endif
        }

        // This Editor-only method will update the Spots list if the rows or columns change, keeping them set up
        // properly.
        protected virtual void CheckSpots()
        {
            if (spots.rows * spots.columns == spots.SpotCount) return;
            WriteToConsole($"There should be {spots.rows * spots.columns} spots but we found {spots.SpotCount}. Will " +
                           "reload the spots object."
                , "BoxManager", writeToConsoleColor, writeToConsole, false, gameObject);
            spots.Setup(GameId(), true);
        }

        /// <summary>
        /// Adds a GameItemObject to the inventoryItems, and attempts to fit it into the "Spots" inventory. Returns
        /// true only if it was successfully added to the inventory and can fit in the inventory.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public bool AddGameItemObjectToList(GameItemObject gameItemObject)
        {
            if (!panelManager)
            {
                Debug.LogError("It looks like you may be trying to generate loot on awake, using the " +
                               "Inventory module. You'll need to wait at least one frame for panelManager to be " +
                               "set as a reference, before generating the loot.");
                return false;
            }
            
            // Figure out where this item fits inside the visual inventory area. Adjust fitAttempts if you'd like to
            // try for longer -- some larger pieces may require more attempts to fit. Depending on the combination of
            // item size and area size, you may see larger items be unable to fit more often than others.
            int spotY;
            int spotX;
            (spotY, spotX) = spots.CanFitItemRandomSpot(gameItemObject);

            if (spotY < 0 || spotX < 0)
            {
                WriteToConsole($"Item Object {gameItemObject.FullName()} can't fit in this inventory."
                    , "BoxManager", writeToConsoleColor, writeToConsole, false, gameObject);
                return false;
            }

            WriteToConsole($"Item Object {gameItemObject.FullName()} can fit in this inventory at row " + spotY + " column " + spotX + " (Top Left is 0,0)"
                , "BoxManager", writeToConsoleColor, writeToConsole, false, gameObject);
            inventoryItems.ReceiveTransfer(gameItemObject.Clone(), false, true, true); // NOTE: We are ignoring Quest items in this context!!

            var lastItemAdded = inventoryItems.list[inventoryItems.Count() - 1];

            lastItemAdded.SetSpots(spotY, spotX);
            AddItemToSpotsInventoryGrid(spots, lastItemAdded, spotY, spotX);

            return true;
        }

        private void AddItemToSpotsInventoryGrid(Spots thisSpots, GameItemObject itemObject, int spotY, int spotX)
        {
            thisSpots.AddItemToInventoryGrid(itemObject, spotY, spotX);
        }

        public virtual void OpenInventory()
        {
            if (panelManager.OtherPanelObject == gameObject)
            {
                WriteToConsole("This inventory is already open."
                    , "BoxManager", writeToConsoleColor, writeToConsole, false, gameObject);
                return;
            }
            
            WriteToConsole("Open Inventory"
                , "BoxManager", writeToConsoleColor, writeToConsole, false, gameObject);
            
            // Set the "Other Panel" on PanelManager to this newly instantiated panel, and then toggle it on.
            var newPanel = panelManager.SetOtherPanel(panelManager.CreatePanel(inventoryPanel, panelManager.otherPanelParent, this, spots), gameObject);
            newPanel.SetIHaveInventoryGameId(GameId()); // Set the panel to search for this GameId()
        }

        public virtual string GameId(bool forceNew = false) =>
            string.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;

        public virtual object SaveState()
        {
            var data = new BoxManagerSaveData
            {
               spots = spots,
               inventoryItems = inventoryItems
            };

            return data;
        }

        public virtual void LoadState(string jsonEncodedState)
        {
            var data = JsonUtility.FromJson<BoxManagerSaveData>(jsonEncodedState);

            spots = data.spots;
            inventoryItems = data.inventoryItems;

            SetOwner(this);
        }

        public struct BoxManagerSaveData
        {
            public Spots spots;
            public GameItemObjectList inventoryItems;
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

        public string GetOwnerName()
        {
            throw new NotImplementedException();
        }

        public void SetOwner(object newOwner)
        {
            inventoryItems.SetOwner((IHaveStats)newOwner);
        }

        public GameItemObjectList DisplayableInventory() => inventoryItems;
        public Spots DisplayableSpots() => spots;
        public void RegisterDisplayableInventoryWithRepository() 
            => GameModuleRepository.Instance.RegisterDisplayableInventory(this);

        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject);
        

        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject, int spotRow, int spotColumn) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject, spotRow, spotColumn);

        public IEnumerator RegisterToRepository()
        {
            while (GameModuleRepository.Instance == null)
                yield return null;

            RegisterDisplayableInventoryWithRepository();
        }

        public bool TryGetGameStat(string uid, out GameStat gameStat)
        {
            throw new NotImplementedException();
        }

        public List<ModificationLevel> GetOtherLevels(bool cache = false)
        {
            throw new NotImplementedException();
        }

        public void SetStatsDirty(List<Stat> statList)
        {
            // We aren't actually using stats, just needed it to set the owner for the inventory list.
        }

        public GameStat GetStat(string uid, bool addIfNull = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerator SetStats()
        {
            throw new NotImplementedException();
        }
    }
}
