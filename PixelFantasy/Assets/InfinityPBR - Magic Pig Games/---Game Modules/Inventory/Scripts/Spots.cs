using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/*
 * INFINITY PBR - https://infinitypbr.com
 * Join the Discord for support & discussion with the community: https://discord.com/invite/cmZY2tH
 * Scripting documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 * Youtube videos, tutorials, demos, and integrations: https://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw
 * All assets on the Asset Store: https://assetstore.unity.com/publishers/4645?aid=1100lxWw&pubref=p88
 * 
 * The Spots class should be added to any object, like a player or a treasure box, which has an inventory that you
 * want to display. It should be accompanied by a List<Item> items which will hold the Item objects for the inventory.
 *
 * Spots handles the visible grid locations for each object.
 */

namespace InfinityPBR.Modules.Inventory
{
    [Serializable]
    public class Spots
    {
        public int rows = 8; // Y
        public int columns = 12; // X

        public List<InventoryRow> row = new List<InventoryRow>();

        // An alternate way of doing this is to compare the number of spots in the List<InventoryRow> with the
        // product of rows and columns, and if it's not the same, re do the setup. But that would take more resources.
        public int SpotCount => row.Sum(rowN => rowN.column.Count);
        public bool IsSetup { get; private set; }

        //public string iHaveInventoryGameId;

        //public GameItemObjectList DisplayableInventory =>
        //    ItemObjectRepository.itemObjectRepository.DisplayableInventory(iHaveInventoryGameId);

        public bool AllSpotsAreEmpty(int rowN, int columnN, int height, int width, GameObject ignoreThisObject = null)
        {
            // Compute the maximum range of the height and width
            var maxHeight = rowN + height;
            var maxWidth = columnN + width;
            
            // If the size is greater than the edges, return false
            if (maxHeight > rows || maxWidth > columns) return false;

            foreach (var spot in GetInventorySpots(rowN, columnN, height, width))
            {
                if (!spot.IsFilled) continue;
                if (!ignoreThisObject) return false;
                if (spot.inGameObject != ignoreThisObject) return false;
            }
            
            /*
            for (var rowI = rowN; rowI < maxHeight; rowI++)
            {
                for (var columnI = columnN; columnI < maxWidth; columnI++)
                {
                    var spot = row[rowI].column[columnI];
                    
                    if (!spot.IsFilled) continue;
                    if (!ignoreThisObject) return false;
                    if (spot.inGameObject != ignoreThisObject) return false;
                }
            }
            */

            return true; // None were false, so we must be good!
        }

        public (int, InventorySpot) ItemsInSpotArea(int rowN, int columnN, int height, int width)
        {
            InventorySpot firstSpotWithObject = null;
            var foundObjects = new List<GameObject>();

            // If the size is greater than the edges, limit the height and/or width
            height = Mathf.Clamp(height, 0, rows - rowN); 
            width = Mathf.Clamp(width, 0, columns - columnN);

            //var maxRow = rowN + height;
            //var maxColumn = columnN + width;

            foreach (var spot in GetInventorySpots(rowN, columnN, height, width))
            {
                // Skip if the spot is empty
                if (!spot.IsFilled)
                    continue;

                // Try to put the item in the list if it isn't already there
                if (!foundObjects.Contains(spot.inGameObject))
                    foundObjects.Add(spot.inGameObject);

                // Assign the first spot that had an item to this
                firstSpotWithObject ??= spot;
            }
            
            /*
            for (var rowI = rowN; rowI < maxRow; rowI++)
            {
                for (var columnI = columnN; columnI < maxColumn; columnI++)
                {
                    var spot = row[rowI].column[columnI];
                    
                    // Skip if the spot is empty
                    if (!spot.IsFilled)
                        continue;

                    // Try to put the item in the list if it isn't already there
                    if (!foundObjects.Contains(spot.inGameObject))
                        foundObjects.Add(spot.inGameObject);

                    // Assign the first spot that had an item to this
                    firstSpotWithObject ??= row[rowI].column[columnI];
                }
            }
            */

            return (foundObjects.Count, firstSpotWithObject);
        }

        /*
        private bool AddGameObjectToList(List<GameObject> list, GameObject gameObject)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == gameObject)
                    return false;
            }

            list.Add(gameObject);
            return true;
        }
        */

        public void RemoveItemFromSpots(GameItemObject gameItemObject)
        {
            Debug.Log($"Remove From spots: {gameItemObject.inventorySpotY} / {gameItemObject.inventorySpotX}");
            // Get the Grid info for the spot.
            var spotInRow = gameItemObject.GetSpotInRow();
            var spotInColumn = gameItemObject.GetSpotInColumn();
            var height = gameItemObject.GetInventoryHeight();
            var width = gameItemObject.GetInventoryWidth();

            // For each spot this item occupies, remove it.
            foreach (var inventorySpot in GetInventorySpots(spotInRow, spotInColumn, height, width))
                inventorySpot.Clear();
        }
        
        /// <summary>
        /// Returns a List<InventorySpot> with all of the spots in the grid specified by the start Y/X values and
        /// height and width.
        /// </summary>
        /// <param name="rowN"></param>
        /// <param name="columnN"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public List<InventorySpot> GetInventorySpots(int rowN, int columnN, int height, int width)
        {
            var spots = new List<InventorySpot>();
            var maxHeight = rowN + height;
            var maxColumn = columnN + width;

            // If the size is greater than the edges, return spots (empty at this point)
            if (maxHeight > rows) return spots;
            if (maxColumn > columns) return spots;

            for (var rowI = rowN; rowI < maxHeight; rowI++)
            {
                for (var columnI = columnN; columnI < maxColumn; columnI++)
                    spots.Add(row[rowI].column[columnI]);
            }

            return spots;
        }

        public void AddInGameObjectToSpots(GameItemObject itemObject, GameObject inGameObject)
        {
            // Get the Grid info for the spot.
            var spotY = itemObject.GetSpotInRow();
            var spotX = itemObject.GetSpotInColumn();
            var height = itemObject.GetInventoryHeight();
            var width = itemObject.GetInventoryWidth();

            // For each spot this item occupies, remove it.
            foreach (var inventorySpot in GetInventorySpots(spotY, spotX, height, width))
                inventorySpot.inGameObject = inGameObject;
        }

        /// <summary>
        /// This will add an item to the spots in an grid, given the item, start Y and X spot.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="rowN"></param>
        /// <param name="columnN"></param>
        /// <param name="inGameObject"></param>
        public void AddItemToInventoryGrid(GameItemObject gameItemObject, int rowN, int columnN, GameObject inGameObject = null)
        {
            // Cache these values
            var height = gameItemObject.GetInventoryHeight();
            var width = gameItemObject.GetInventoryWidth();
            //Debug.Log($"adding to grid height {height} and width {width}, {gameItemObject.objectName} at {rowN} and {columnN}");

            foreach (var spot in GetInventorySpots(rowN, columnN, height, width))
            {
                spot.gameId = gameItemObject.GameId();
                if (!inGameObject)
                    continue;
                spot.inGameObject = inGameObject;
            }
        }
        
        /// <summary>
        /// Will return the Y and X indexes if the item can fit. If either are -1, item can't fit.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public (int, int) CanFitItem(GameItemObject gameItemObject)
        {
            // Cache these values
            var height = gameItemObject.GetInventoryHeight();
            var width = gameItemObject.GetInventoryWidth();

            for (var rowI = 0; rowI < row.Count; rowI++)
            {
                for (var columnI = 0; columnI < row[rowI].column.Count; columnI++)
                {
                    var spot = row[rowI].column[columnI];
                    
                    if (spot.IsFilled)
                        continue;

                    // If this area, starting in the top/left rowI / columnI can fit the item (i.e. they are all
                    // empty), then return the row/column. Indicates the item can fit.
                    if (AllSpotsAreEmpty(rowI, columnI, height, width))
                        return (rowI, columnI);
                }
            }
            return (-1, -1); // No spots were all empty, so return false -- item can't fit!
        }

        /// <summary>
        /// Will return the Y and X indexes if the item can fit. If either are -1, item can't fit.
        /// </summary>
        /// <param name="itemObject"></param>
        /// <param name="fitAttempts"></param>
        /// <returns></returns>
        public (int, int) CanFitItemRandomSpot(GameItemObject itemObject, int fitAttempts = 10)
        {
            // Cache these values
            var height = itemObject.GetInventoryHeight();
            var width = itemObject.GetInventoryWidth();
            
            for (var i = 0; i < fitAttempts; i++)
            {
                var randomRow = Random.Range(0, rows - height);
                var randomColumn = Random.Range(0, columns - width);
                
                if (AllSpotsAreEmpty(randomRow, randomColumn, height, width))
                    return (randomRow, randomColumn);
            }
            
            return (-1, -1);
        }

        /// <summary>
        /// Will return true if the item can fit starting in grid spot provided
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="rowN"></param>
        /// <param name="columnN"></param>
        /// <param name="ignoreThisObject"></param>
        /// <returns></returns>
        public bool CanFitItem(GameItemObject gameItemObject, int rowN, int columnN, GameObject ignoreThisObject = null)
        {
            //Debug.Log($"iHaveInventoryGameId is {iHaveInventoryGameId}");
            // If the DisplayableInventory can't receive quest items, and this is one, then return false
            //if (gameItemObject.QuestItem && DisplayableInventory.canReceiveQuestItems)
            //    return false;
            
            // Cache these values
            var height = gameItemObject.GetInventoryHeight();
            var width = gameItemObject.GetInventoryWidth();

            return AllSpotsAreEmpty(rowN, columnN, height, width, ignoreThisObject);
        }

        /// <summary>
        /// Will return true if the item can fit starting in grid spot provided
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rowN"></param>
        /// <param name="columnN"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="ignoreThisObject"></param>
        /// <returns></returns>
        public bool CanFitItem(int rowN, int columnN, int height, int width, GameObject ignoreThisObject = null) 
            => AllSpotsAreEmpty(rowN, columnN, height, width, ignoreThisObject);

        public void SetupAndRelinkInventory(string newIHaveInventoryGameId, bool forceSetup = false)
        {
            Setup(newIHaveInventoryGameId, forceSetup);
            Relink(GameModuleRepository.Instance.DisplayableInventory(newIHaveInventoryGameId));
        }
        
        /// <summary>
        /// Called once per InventorySpots object, or called when the InventorySpots is reset, often after being
        /// resized.
        /// </summary>
        public void Setup(string newIHaveInventoryGameId, bool forceSetup = false)
        {
            //Debug.Log($"SPOTS SETUP: {iHaveInventoryGameId}");
            //iHaveInventoryGameId = newIHaveInventoryGameId;
            if (!forceSetup && IsSetup)
            {
                SetIHaveInventoryGameId(newIHaveInventoryGameId);
                return;
            }
            
            row.Clear();
            for (var rowI = 0; rowI < rows; rowI++)
            {
                row.Add(new InventoryRow());
                for (var columnI = 0; columnI < columns; columnI++)
                {
                    var newSpot = new InventorySpot
                    {
                        iHaveInventoryGameId = newIHaveInventoryGameId
                    };
                    row[rowI].column.Add(newSpot);
                }
            }

            IsSetup = true;
        }

        public void SetIHaveInventoryGameId(string iHaveInventoryGameId)
        {
            foreach (var columnN in row.SelectMany(rowN => rowN.column))
                columnN.iHaveInventoryGameId = iHaveInventoryGameId;
        }

        public void Relink(GameItemObjectList inventory)
        {
            foreach (var itemObject in inventory.list)
            {
                var spotY = itemObject.GetSpotInRow();
                var spotX = itemObject.GetSpotInColumn();
                AddItemToInventoryGrid(itemObject, spotY, spotX);
            }
        }
    }

    [Serializable]
    public class InventoryRow
    {
        public List<InventorySpot> column = new List<InventorySpot>();
    }

    [Serializable]
    public class InventorySpot
    {
        public string gameId; // This is the gameId of the item in this spot
        public bool IsFilled => !string.IsNullOrWhiteSpace(gameId); // returns true if gameId is not null, so we are filled
        public string iHaveInventoryGameId; // This is the gameId of the IHaveInventory object which has the DisplayableInventory()

        public GameItemObject Item => DisplayableInventory.GetByGameId(gameId);
        
        private GameItemObjectList DisplayableInventory 
            => GameModuleRepository.Instance.DisplayableInventory(iHaveInventoryGameId);

        [FormerlySerializedAs("InGameObject")] public GameObject inGameObject;

        // Clears the spot, i.e. makes it so it is empty again
        public void Clear()
        {
            gameId = "";
            inGameObject = null; // Jan 24, 2023 -- This may not actually be needed or wanted. Added it because it made sense
        }
    }
}