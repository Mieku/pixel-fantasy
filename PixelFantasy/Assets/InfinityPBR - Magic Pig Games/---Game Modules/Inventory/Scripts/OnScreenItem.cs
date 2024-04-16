using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static InfinityPBR.Modules.Inventory.PanelManager;
using static InfinityPBR.Debugs;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 *
 * ON SCREEN ITEM
 * Attach this script to your player or object that is in all your scenes.
 */

namespace InfinityPBR.Modules.Inventory
{
    [Serializable]
    public class OnScreenItem : MonoBehaviour
    {
        public static OnScreenItem onScreenItem;

        public Panel ActivePanel => GetActivePanel();

        public static Panel GetActivePanel()
        {
            if (!panelManager)
                return null;

            return panelManager.ActivePanel;
        }
        
        public GameItemObject itemHeld;
        public GameObject itemHeldObject;
        public bool itemHeldIsQuestItem;
        public bool ItemIsHeld => IsItemHeld();
        public int ItemHeight => GetItemHeight();
        public int ItemWidth => GetItemWidth();
        
        [Header("Required")] 
        public Camera UICamera; // Camera used to display the UI
        public Transform playerTransform; // Could be a first person object, or the player in a 3rd person
        public RectTransform heldObjectParentRectTransform; // This is the RectTransform of the heldObjectParent
        public float zPosition = -400f; // This is the zposition (distance from camera essentially) the object will be at
        public float dropForce = 50f; // Force to apply when dropping an item.
        public Vector3 dropPositionModification = Vector3.zero;

        [Header("Quest Item Options")] 
        public bool canDropQuestItem; // If true, a gameItemObject that is QuestItem can be dropped

        [Header("Debug Options")] 
        public bool writeToConsole = true; // When true, the console will print out helpful information
        public Color writeToConsoleColor = new Color(0.25f, 0.5f, 0.2f);
        
        
        /// <summary>
        /// Returns true if an item is currently being held. This is determined by itemHeld.objName being not null or empty.
        /// </summary>
        /// <returns></returns>
        public bool IsItemHeld() => itemHeld != null && !string.IsNullOrEmpty(itemHeld.objectName);

        /// <summary>
        /// Will return the grid height of the item held. Returns -1 if no item is held.
        /// </summary>
        /// <returns></returns>
        public int GetItemHeight() => ItemIsHeld ? onScreenItem.itemHeld.GetInventoryHeight() : -1;

        /// <summary>
        /// Will return the grid width of the item held. Returns -1 if no item is held.
        /// </summary>
        /// <returns></returns>
        public int GetItemWidth() => ItemIsHeld ? onScreenItem.itemHeld.GetInventoryWidth() : -1;

        //=====================================================================================
        // AWAKE METHODS
        //=====================================================================================
        
        private void Awake()
        {
            if (onScreenItem == null)
                onScreenItem = this;
            else if (onScreenItem != this)
                Destroy(gameObject);

            CheckRequiredReferences();
        }

        private void CheckRequiredReferences()
        {
            if (UICamera == null)  WriteToConsole("<color=#ffff00>Required:</color> UICamera is required for OnScreenItem", "OnScreenItem", writeToConsoleColor, writeToConsole, true, gameObject);
            if (playerTransform == null) WriteToConsole("<color=#ffff00>Required:</color> playerTransform is required for OnScreenItem", "OnScreenItem", writeToConsoleColor, writeToConsole, true, gameObject);
            if (heldObjectParentRectTransform == null) WriteToConsole("<color=#ffff00>Required:</color> heldObjectParentRectTransform is required for OnScreenItem", "OnScreenItem", writeToConsoleColor, writeToConsole, true, gameObject);
        }

        //=====================================================================================
        // UPDATE METHODS
        //=====================================================================================
        
        private void LateUpdate() =>  PositionHeldItemAtMousePoint();

        //=====================================================================================
        // GENERAL METHODS
        //=====================================================================================

        private void DisplayObject(GameObject objectPrefab, bool isQuestItem = false, float z = -40) 
        {
            itemHeldObject = Instantiate(objectPrefab, heldObjectParentRectTransform);
            itemHeldIsQuestItem = isQuestItem;
        }

        /// <summary>
        /// This method will put the held item at the position the mouse is at, on the screen.
        /// </summar>
        private void PositionHeldItemAtMousePoint()
        {
            if (!itemHeldObject)
                return;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(heldObjectParentRectTransform, Input.mousePosition, UICamera, out localPoint);
            itemHeldObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(localPoint.x, localPoint.y, zPosition);
        }
        
        /// <summary>
        /// Returns true if we are able to pick up the item passed in.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public bool Pickup(GameItemObject gameItemObject)
        {
            if (gameItemObject == null) return false;

            if (ItemIsHeld)
            {
                // If there is no active inventory, then we can't try to place it there, so we can't pick up the 
                // new object!
                if (!ActivePanel) return false;
                if (!ActivePanel.inventoryArea) return false;
                if (!ActivePanel.inventoryArea.activeSelf) return false;

                // If we weren't able to fit the object in the active inventory, we can't do anything, can't pick up
                // the new object. If returned true, then it already put the item into the inventory, destroyed the
                // currently visible object etc.
                if (!GiveHeldObject(ActivePanel.Spots, ActivePanel.InventoryItems)) return false;
            }

            var objectPrefab = gameItemObject.Parent().prefabInventory; // Grab the prefab we will display
            if (!objectPrefab) return false;

            itemHeld = gameItemObject.Clone();
            DisplayObject(objectPrefab, gameItemObject.QuestItem);
            
            WriteToConsole($"PickUp {gameItemObject.FullName()} complete. itemHeld is now {itemHeld.FullName()}"
                , "OnScreenItem", writeToConsoleColor, writeToConsole);
            
            return true;
        }

        /// <summary>
        /// This will destroy the item currently held and reset the Item()
        /// </summary>
        public void RemoveItem()
        {
            if (!itemHeldObject)
                return;
            
            Destroy(itemHeldObject);
            itemHeld = new GameItemObject(null);
        }

        /// <summary>
        /// This will give the held object to the provided Spots and List<Item>, and then will remove the item from
        /// the held position.
        /// </summary>
        /// <param name="spots"></param>
        /// <param name="gameItemObjectList"></param>
        /// <returns></returns>
        public bool GiveHeldObject(Spots spots, GameItemObjectList gameItemObjectList)
        {
            // If there is no held object, then return false
            if (!ItemIsHeld) 
                return false;

            if (gameItemObjectList.TakeIntoInventoryGrid(itemHeld))
            {
                WriteToConsole($"Item {itemHeld.FullName()} fits in this inventory."
                    , "OnScreenItem", writeToConsoleColor, writeToConsole);
                RemoveItem();
                ResetPanel(spots);
                return true;
            }

            WriteToConsole($"Item {itemHeld.FullName()} can't fit in this inventory."
                , "OnScreenItem", writeToConsoleColor, writeToConsole);
            return false;
        }
        
        /// <summary>
        /// Gives the held object to a specific gameItemObjectList with the spots data provided.
        /// </summary>
        /// <param name="spots"></param>
        /// <param name="gameItemObjectList"></param>
        /// <param name="spotRow"></param>
        /// <param name="spotColumn"></param>
        /// <returns></returns>
        public bool GiveHeldObject(Spots spots, GameItemObjectList gameItemObjectList, int spotRow, int spotColumn)
        {
            // If there is no held object, then return false
            if (!ItemIsHeld) 
                return false;
            
            WriteToConsole("Give Held Object to spot (Row/Column): " + spotRow + "/" + spotColumn + ""
                , "OnScreenItem", writeToConsoleColor, writeToConsole);
            if (gameItemObjectList.TakeIntoInventoryGrid(itemHeld, spotRow, spotColumn))
            {
                // If the inventory is active, reload it
                ResetPanel(spots);
                RemoveItem();
                return true;
            }

            WriteToConsole($"Item {itemHeld.FullName()} can't fit in this inventory at row {spotRow} " +
                           $"and column {spotColumn}."
                , "OnScreenItem", writeToConsoleColor, writeToConsole);
            return false;
        }

        private void ResetPanel(Spots spots)
        {
            if (!ActivePanel) return; // Possible that panel isn't yet registered.

            if (ActivePanel.Spots != spots) return; // Only reload if the inventoryPanel is showing the same Spots object
                
            ActivePanel.ToggleObjects();
        }

        /// <summary>
        /// This will drop the held object into the world, giving it a bit of force in front of the player.
        /// </summary>
        public bool DropObject(bool ignoreButtons = false, bool ignoreQuestItems = false
            , bool spawnAtCustomPosition = false
            , Vector3 customSpawnPosition = default
            , bool lookAtNewPosition = false
            , Vector3 dropLookAtPosition = default
            , bool customForce = false
            , float customForceAmount = 10f)
        {
            if (!itemHeldObject)
                return false;

            // Don't drop it if we can't drop quest items and it is one
            if (!canDropQuestItem && itemHeldIsQuestItem && !ignoreQuestItems)
                return false;

            WriteToConsole($"DropObject {itemHeld.FullName()}", "OnScreenItem", writeToConsoleColor, writeToConsole);
            
            // Do not drop if we're over a button and not ignoring buttons. We may be giving the item to another.
            if (EventSystem.current.IsPointerOverGameObject() && !ignoreButtons)
                return false;

            // Get the item to spawn from the held item Object. if none, return.
            var itemToSpawn = itemHeldObject.GetComponent<ItemInventory>().inGamePrefab;
            if (!itemToSpawn)
                return false;

            var spawnPosition = spawnAtCustomPosition
                ? customSpawnPosition
                : playerTransform.position
                   + dropPositionModification
                   + playerTransform.forward;
            
            // Will put the object right in front of the player
            var worldItem = Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
            
            // Look at the dropLookAtPosition
            if (lookAtNewPosition)
                worldItem.transform.LookAt(dropLookAtPosition);
            
            worldItem.GetComponent<ItemWorld>().SetGameItemObject(itemHeld.Clone()); // Clone this item data to the new object
            worldItem.name = worldItem.name.Replace("(Clone)", ""); // Optional really, but removes this

            // Throw the item forward/up with a small amount of force
            var itemRigidbody = worldItem.GetComponent<Rigidbody>();
            itemRigidbody.AddForce((worldItem.transform.position - playerTransform.position) 
                                   * (customForce ? customForceAmount : dropForce));

            RemoveItem();
            //Debug.LogWarning($"The item {worldItem.name} uid is {worldItem.GetComponent<IAmItemWorld>().GameItemObject().Uid()}");
            
            WriteToConsole("DropObject Complete", "OnScreenItem", writeToConsoleColor, writeToConsole);

            return true;
        }
    }
}

