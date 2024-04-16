using System;
using System.Collections;
using InfinityPBR.Modules.Inventory;

/*
 * This script inherits from the fully set up GameModulesActor, and adds the methods required to operate the Inventory
 * System, which is an opinionated, visual drag-and-drop inventory system. If you do not want that kind of inventory
 * system in your game, then you are likely better off coding your own system! The Inventory Module was created
 * specifically to drive that kind of inventory system.
 *
 * For questions & support, visit the Discord, linked from www.InfinityPBR.com
 * For documentation and tutorial videos, visit the online documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 */

/*
 * IMPORTANT: Please check the notes on GameModulesActor for other important information, as this inherits from that
 * class!
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameModulesInventoryActor : GameModulesActor, IHaveInventory
    {
        //=====================================================================================
        // INVENTORY MANAGEMENT
        //=====================================================================================
        
        // This will use the same inventory / equipment lists from GameModulesActor, but adds
        // methods and variables to work with the visual drag and drop Inventory system that comes
        // with Game Modules.
        
        public Spots spots = new Spots();
        
        /// <summary>
        /// StartActions will ensure all of the displayable inventory in the inventory list is properly linked.
        /// </summary>
        public override IEnumerator StartActions()
        {
            // Note, due to this not being a monobehavior, and that StartActions on base is a coroutine, we have to 
            // call "Base" like this.
            IEnumerator baseActions = base.StartActions();
            while (baseActions.MoveNext()) 
            {
                yield return baseActions.Current;
            }
            
            RegisterDisplayableInventoryWithRepository(); // Register this inventory to the repository
            spots.SetupAndRelinkInventory(GameId(), true); // Ensure connections are correct for visual inventory
            
            yield break;
        }
        
        // Displayable Inventory is the inventory list that can be displayed using the visual drag-and-drop inventory
        // system that comes w/ Game Modules. If you are not using this system, you should inherit from
        // GameModulesActor rather than this.
        public GameItemObjectList DisplayableInventory() => inventoryItems;
        public Spots DisplayableSpots() => spots; 
        
        /// <summary>
        /// Registers this inventory with the ItemObjectRepository, so that it can be used by the Inventory System.
        /// </summary>
        public virtual void RegisterDisplayableInventoryWithRepository()
        {
            GameModuleRepository.Instance.RegisterDisplayableInventory(this);
        }

        /// <summary>
        /// Takes a GameItemObject and places it into the inventory grid, if there is room. If you are using the visual
        /// inventory system, this should be used instead of the Transfer() method, so that the item is added to the
        /// inventory only if it can be placed in the inventory grid, and so that it receives the correct data.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject);

        /// <summary>
        /// /// <summary>
        /// Takes a GameItemObject and places it into the inventory grid at a specific spot row/column, if it can fit.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="spotRow"></param>
        /// <param name="spotColumn"></param>
        /// <returns></returns>
        public virtual bool TakeIntoInventoryGrid(GameItemObject gameItemObject, int spotRow, int spotColumn) =>
            DisplayableInventory().TakeIntoInventoryGrid(gameItemObject, spotRow, spotColumn);
    }
}
