using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InfinityPBR.Modules.Inventory.OnScreenItem;
using static InfinityPBR.Modules.Inventory.PanelManager;
using static InfinityPBR.Debugs;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 */

namespace InfinityPBR.Modules.Inventory
{ 
    [Serializable]
    public class Panel : MonoBehaviour
    {
        [HideInInspector] public Camera uiCamera; // Generally this will be set by PanelManager

        [Header("Inventory UI Plumbing")] 
        public GameObject inventoryArea;
        public GameObject buttonPrefab; // This is the UI Prefab for buttons on the grid
        public RectTransform inventoryObjectsParentRectTransform; // This is the RectTransform of the inventoryObjectsParent
        public float zPosition = -80; // This is the z-position (distance from camera essentially) the object will be at

        [Header("Button Colors")]
        public Color emptyColor = new Color(0, 0, 0, 0);
        public Color filledColor = new Color(0, 0, 0, 0.3f);
        public Color availableColor = Color.green;
        public Color unavailableColor = Color.red;

        [Header("Options")]
        public bool showColorOnHover = true;
        public bool computeCanPlaceOnHoverGroup = true;
        public int gridButtonPixelSize = 100;
        public bool resetToLastPauseLevelOnClose = true;

        private List<GameObject> inventoryObjects = new List<GameObject>();
        private RectTransform rectTransform;

        public Vector3 screenPoint;
        public bool loadItems;

        // percent of 100 pixels. i.e. if value is 90, then this is .9f and objects will be scaled accordingly
        public float GridSpotPixelPercent => gridButtonPixelSize / 100f; 
        
        // This will store all the grid buttons, Y, then X. Y = Rows, X = Columns.
        // gridY[3].gridX[2] for example -- goes from top left, left->right, top->bottom
        public List<GridRow> gridRows = new List<GridRow>();

        private bool hasSetUpButtons;

        // The inventoryArea should be sized to match some dimension of the gridButtonSize, i.e. an area that is
        // 1200 by 800 will hold 12 grid buttons horizontally, and 8 vertically, for a total of 96 buttons.
        // This way you can create various sized inventory areas for various sized holding objects, like chests, or
        // bags, or drawers.
        private RectTransform areaRectTransform;
        private GridLayoutGroup areaGridLayoutGroup;

        public int GridColumnsCount => !areaRectTransform ? 0 : Mathf.FloorToInt(areaRectTransform.sizeDelta.x / gridButtonPixelSize);
        public int GridRowsCount => !areaRectTransform ? 0 : Mathf.FloorToInt(areaRectTransform.sizeDelta.y / gridButtonPixelSize);

        // IMPORTANT! 
        // This should be passed in from the other object -- a player, or perhaps the controller of a chest of items etc.
        // This is how the Panel knows what is being held, but this data is stored elsewhere -- the panel only references
        // the data that is owned and stored OUTSIDE of the Inventory module.
        //
        // This is a requirement of any integration with this module!
        //public Spots spots;
        public string iHaveInventoryGameId;
        public GameItemObjectList InventoryItems 
            => GameModuleRepository.Instance.DisplayableInventory(iHaveInventoryGameId);
        
        public Spots Spots 
            => GameModuleRepository.Instance.DisplayableSpots(iHaveInventoryGameId);

        public void SetIHaveInventoryGameId(string value)
        {
            WriteToConsole($"Panel and Spots iHaveInventoryGameId will be set to {value}"
                , "Panel", writeToConsoleColor, writeToConsole);
            iHaveInventoryGameId = value;
        }
        
        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.5f, 0.6f, 0.4f);

        // Used when the panel is instantiated.
        public void SetupActions(string newIHaveInventoryGameId)
        {
            SetIHaveInventoryGameId(newIHaveInventoryGameId);
            
            WriteToConsole("Panel AwakeActions()"
                , "Panel", writeToConsoleColor, writeToConsole);
            
            CheckRequired();
            
            hasSetUpButtons = false;
            SetupAreaAndButtons();
            
            PopulateInventory();
            loadItems = true; // Make sure we load the objects
        }

        private void SetupAreaAndButtons()
        {
            SetupArea();
            rectTransform = GetComponent<RectTransform>();
            screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, rectTransform.position);
            SetInventoryAreaSize();
            SetupButtons();
        }

        // This is used mostly for panels that are in the game, and not instantiated, such as "Player Inventory", esp.
        // when there are multiple players in the game which all use the same inventory panel, or when the contents
        // of the inventory changes while the panel is not visible.
        private void OnEnable()
        {
            // StartActions() will populate this. If we don't have it, that means we are loading the panel for the
            // first time. StartActions() will handle everything here. 
            if (string.IsNullOrWhiteSpace(iHaveInventoryGameId))
                return;
            
            WriteToConsole("Panel OnEnable()"
                , "Panel", writeToConsoleColor, writeToConsole);
            SetupAreaAndButtons();
            loadItems = true;
        }

        private void OnDisable()
        {
            WriteToConsole("Panel OnDisable()"
                , "Panel", writeToConsoleColor, writeToConsole);
            SetAllHoverFalse();
            RemoveObjects();
        }

        public virtual void SetInventoryAreaSize()
        {
            var sizeDelta = areaRectTransform.sizeDelta;
            sizeDelta.y = Spots.rows * gridButtonPixelSize;
            sizeDelta.x = Spots.columns * gridButtonPixelSize;
            areaRectTransform.sizeDelta = sizeDelta;

            areaGridLayoutGroup.cellSize = new Vector2(gridButtonPixelSize, gridButtonPixelSize);
        }

        private void Update()
        {
            if (!loadItems)
                return;
            
            DisplayObjects();
            loadItems = false;
        }

        public void DisplayObjects()
        {
            WriteToConsole($"Displaying {InventoryItems.list.Count} Objects in the inventoryItems list. iHaveInventoryGameId is {iHaveInventoryGameId}"
                , "Panel", writeToConsoleColor, writeToConsole);
            
            foreach (var itemObject in InventoryItems.list)
            {
                if (!itemObject.Parent().prefabInventory)
                {
                    WriteToConsole($"NO PREFAB INVENTORY: {itemObject.GetInventoryWidth()} / {itemObject.GetInventoryHeight()} / {itemObject.GetWorldPrefab().name} / {itemObject.GetInventoryPrefab().name}"
                        , "Panel", writeToConsoleColor, writeToConsole);
                    continue;
                }

                var objectPrefab = itemObject.Parent().prefabInventory;
                if (!objectPrefab)
                    continue;
                
                var newObject = DisplayObject(objectPrefab, itemObject);
                
                // Add the object to the spots inGameObject
                Spots.AddInGameObjectToSpots(itemObject, newObject);
                
                // Add the object to the list of active objects.
                inventoryObjects.Add(newObject);
            }
        }

        /// <summary>
        /// This will display an object on the grid.
        /// </summary>
        /// <param name="objectPrefab"></param>
        /// <param name="item"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private GameObject DisplayObject(GameObject objectPrefab, GameItemObject item, float z = -1)
        {
            // Instantiate the object. If you're going to try pooling, this is probably a place to make modifications.
            var newObject = Instantiate(objectPrefab, inventoryObjectsParentRectTransform);
 
            // Get the InventoryDemoGridButton for the spot.
            var spotY = item.GetSpotInRow();
            var spotX = item.GetSpotInColumn();
            
            var gridButton = gridRows[spotY].gridX[spotX].GetComponent<GridButton>();

            // Get the screenpoint for the grid spot, then compute the position for the object to be...
            // ScreenPointToLocalPointInRectangle -- my understanding is that we will pass in the RectTransform
            // for the parent of the inventory objects, plus the screen point of the grid (which is the X/Y of the
            // position on the screen), specify the UICamera as the camera to reference, and set those values to the
            // Vector2 localPoint variable we create.
            var gridScreenPoint = gridButton.GetScreenPoint();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryObjectsParentRectTransform, gridScreenPoint, uiCamera, out var localPoint);

            var objRectTransform = newObject.GetComponent<RectTransform>();
            // Set the position of the new objects RectTransform. Note the z position matters when dealing with
            // stacking of objects.
            objRectTransform.anchoredPosition3D = new Vector3(localPoint.x, localPoint.y, Math.Abs(z - (-1)) < 0.001 ? zPosition : z);
            
            // Set the scale of the new object based on the panel grid pixel size
            var originalScale = objRectTransform.localScale;
            objRectTransform.localScale = originalScale * GridSpotPixelPercent;

            return newObject;
        }

        /// <summary>
        /// Remove all of the objects currently on the grid, destroying them.
        /// </summary>
        private void RemoveObjects()
        {
            foreach (GameObject obj in inventoryObjects)
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// This will turn off all current items, then set and load from the provided List<Item> and Spots
        /// </summary>
        /// <param name="newList"></param>
        /// <param name="newSpots"></param>
        public void ToggleObjects(GameItemObjectList newList, Spots newSpots)
        {
            RemoveObjects();
            loadItems = true;
        }

        /// <summary>
        /// This will remove all current objects and reload them.
        /// </summary>
        public void ToggleObjects()
        {
            RemoveObjects();
            loadItems = true;
        }

        public void ToggleObjects(string newGameId)
        {
            RemoveObjects();
            SetIHaveInventoryGameId(newGameId);
            loadItems = true;
        }

        private void CheckRequired()
        {
            inventoryArea.RequiredBy(this);
            buttonPrefab.RequiredBy(this);
            inventoryObjectsParentRectTransform.RequiredBy(this);
        }

        /// <summary>
        /// This will set up the inventory area, populate buttons etc.
        /// </summary>
        private void SetupArea()
        {
            if (!inventoryArea) return;

            areaRectTransform = inventoryArea.GetComponent<RectTransform>();
            areaGridLayoutGroup = inventoryArea.GetComponent<GridLayoutGroup>();
        }
        
        /// <summary>
        /// This will set up the buttons -- removing any existing ones, then adding new ones.
        /// </summary>
        private void SetupButtons()
        {
            if (hasSetUpButtons) return;

            DestroyAndClearButtons();
            
            // Add buttons and objects
            for (var iY = 0; iY < GridRowsCount; iY++)
            {
                gridRows.Add(new GridRow());
                for (int iX = 0; iX < GridColumnsCount; iX++)
                {
                    var newButton = Instantiate(buttonPrefab, inventoryArea.transform);
                    newButton.name = "Grid Button Y-X " + iY + "-" + iX;
                    gridRows[iY].gridX.Add(newButton);
                    var gridButton = newButton.GetComponent<GridButton>();
                    gridButton.columnN = iX;
                    gridButton.rowN = iY;
                    gridButton.panel = this;
                }
            }

            hasSetUpButtons = true; // Mark setup as true for this inventory. Will only happen once per play in theory.
        }
        
        /// <summary>
        /// Returns a List<InventorySpot> with all of the spots in the grid specified by the start Y/X values and
        /// height and width.
        /// </summary>
        /// <param name="yN"></param>
        /// <param name="xN"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public List<GridButton> GetButtonsInArea(int yN, int xN, int height, int width)
        {
            List<GridButton> gridButtons = new List<GridButton>();

            // If the size is greater than the edges, limit the height and/or width
            height = Mathf.Clamp(height, 0, GridRowsCount - yN); 
            width = Mathf.Clamp(width, 0, GridColumnsCount - xN);

            if (height == 0 || width == 0)
                return gridButtons;

            for (int yC = yN; yC < yN + height; yC++)
            {
                for (int xC = xN; xC < xN + width; xC++)
                    gridButtons.Add(gridRows[yC].gridX[xC].GetComponent<GridButton>());
            }

            return gridButtons;
        }

        private void DestroyAndClearButtons()
        {
            // Destroy all buttons if any exist
            foreach (var gridRow in gridRows)
            {
                foreach (var gridRowButton in gridRow.gridX)
                    Destroy(gridRowButton);
            }

            gridRows.Clear(); // Clear the Lists
        }

        /// <summary>
        /// Will populate the inventory area for the active player
        /// </summary>
        private void PopulateInventory()
        {
            if (!inventoryArea) return;

            ClearInventoryObjects();
        }

        private void ClearInventoryObjects()
        {
            foreach (GameObject obj in inventoryObjects)
                Destroy(obj);

            inventoryObjects.Clear();
        }

        // Will return false, if the panel grid does not extend beyond the bounds given
        // rowN and columnN are the top-left of the item, and teh height and width are the
        // size. 
        public bool GridOnPanel(int rowN, int columnN, int height, int width)
        {
            if (rowN + height > GridRowsCount) return false;
            if (columnN + width > GridColumnsCount) return false;

            return true;
        }

        private bool IsQuestItemCantGiveToList =>
            onScreenItem.itemHeld.QuestItem && !InventoryItems.canReceiveQuestItems;

        private bool PutHeldItem(int rowN, int columnN)
        {
            if (!onScreenItem.ItemIsHeld) return false;

            if (IsQuestItemCantGiveToList)
                return false;

            var height = onScreenItem.itemHeld.GetInventoryHeight();
            var width = onScreenItem.itemHeld.GetInventoryWidth();
            
            // Check to make sure the item can actually fit on this spot without going outside of the panel
            if (!GridOnPanel(rowN, columnN, height, width))
                return false;

            // Populate the item in the spot if there is one.
            var existingItems = 0;
            InventorySpot existingInventorySpot = null;

            (existingItems, existingInventorySpot) = Spots.ItemsInSpotArea(rowN, columnN, height, width);

            // If there are more than 1 item in the area, we can't put this down.
            if (existingItems > 1)
                return false;

            // Check to see if we can fit the item in the spot, if not, return.
            if (!Spots.CanFitItem(onScreenItem.itemHeld, rowN, columnN, existingInventorySpot?.inGameObject))
                return false;

            // If there is already an item, we need to save it temporarily (as a clone), and then remove it first.
            var tempItem = new GameItemObject(null);
            if (existingInventorySpot != null)
            {
                tempItem = existingInventorySpot.Item.Clone();
                RemoveItemFromInventoryAndSpots(existingInventorySpot.Item);
            }

            // Give the held object to the current spots / inventoryItems at these coordinates
            onScreenItem.GiveHeldObject(Spots, InventoryItems, rowN, columnN);

            // Pick up the temporary item if there is one
            if (!string.IsNullOrEmpty(tempItem.objectName))
                onScreenItem.Pickup(tempItem);

            return true;
        }

        public void GridClicked(int rowN, int columnN)
        {
            Debug.Log($"Got click on row {rowN}, column {columnN}");
            if (onScreenItem.ItemIsHeld && IsQuestItemCantGiveToList)
                return;
            
            // First, if we have a held item, we will try to put it down, picking up the existing item if there is one.
            if (PutHeldItem(rowN, columnN)) return;
            
            // We either couldn't put the held item down, or we don't have a held item. If we have a held item, and we
            // couldn't put it down, there is nothing left to try.
            if (onScreenItem.ItemIsHeld) return;
            
            // If the clicked spot is not filled, then we have nothing left to do.
            if (!Spots.row[rowN].column[columnN].IsFilled) return;
            
            // Pick up the item at this spot
            var gameItemObject = Spots.row[rowN].column[columnN].Item;
            var inGameObject = Spots.row[rowN].column[columnN].inGameObject;
            
            if (!onScreenItem.Pickup(gameItemObject)) return;
            
            RemoveItemFromInventoryAndSpots(gameItemObject);
            if (inGameObject != null)
                Destroy(inGameObject);
        }

        /// <summary>
        /// This will remove the item from the referenced inventory, as well as all the spots.
        /// </summary>
        /// <param name="gameItemObject"></param>
        private void RemoveItemFromInventoryAndSpots(GameItemObject gameItemObject)
        {
            // Remove from spots
            Spots.RemoveItemFromSpots(gameItemObject);

            // Remove from the inventory list
            InventoryItems.RemoveExact(gameItemObject);
        }

        // This is used for non-player inventory, to completely close the box, remove all items from the game etc.
        // This is different than hiding the box using the BoxManager
        public void Close()
        {
            SetAllHoverFalse();
            RemoveObjects();
            panelManager.ToggleOtherInventoryPanel(false); // This will also reset pause level if that option is selected
            Destroy(gameObject);
        }

        /// <summary>
        /// This is called to reset all the UI buttons "isHovering" status.
        /// </summary>
        public void SetAllHoverFalse()
        {
            foreach (GridRow row in gridRows)
            {
                foreach (GameObject button in row.gridX)
                    button.GetComponent<GridButton>().isHovering = false;
            }
        }

        public void SetUICamera(Camera value) => uiCamera = value;
    }
}

