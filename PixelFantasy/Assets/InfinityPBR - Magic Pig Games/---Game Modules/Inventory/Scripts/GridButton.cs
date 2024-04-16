using System;
using UnityEngine;
using UnityEngine.UI;
using static InfinityPBR.Modules.Inventory.OnScreenItem;

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
    public class GridButton : MonoBehaviour
    {
        public Panel panel;
        
        // This is the X and Y position in the grid that this button lives.
        [HideInInspector] public int columnN;
        [HideInInspector] public int rowN;
        [HideInInspector] public Vector3 screenPoint;

        // These handle the color tint based on whether the player is hovering, can place an item, etc.
        [HideInInspector] public bool isHovering;
        [HideInInspector] public bool inHoverGroup;
        [HideInInspector] public bool canPlaceHoverGroup;

        // Private variables
        private RectTransform rectTransform; // This rectTransform
        private Image image; // This image
        private Button button; // This button
        private bool screenPointSet; // Will be true once the screen point has been set
        
        // Button is not filled if the spots haven't been set up (they're null) or  if the spot is not IsFilled
        public bool ButtonIsFilled => panel.Spots != null && panel.Spots.row[rowN].column[columnN].IsFilled;

        void Awake() => rectTransform = GetComponent<RectTransform>();

        /// <summary>
        /// This will return the GameObject that is up/down/left/right of this button object.
        /// </summary>
        /// <param name="xN"></param>
        /// <param name="yN"></param>
        /// <returns></returns>
        private GameObject GetNeighbor(int xN, int yN)
        {
            // If either X or Y are now < 0 or >= the size of the grid, return null -- we are at the edge
            if (xN - xN < 0 || xN + xN >= panel.GridColumnsCount)
                return null;
            if (yN - yN < 0 || yN + yN >= panel.GridRowsCount)
                return null;

            return panel.gridRows[rowN + yN].gridX[columnN + xN];
        }

        // Called via UI Event "Pointer Enter"
        public void StartHover()
        {
            if (!panel.showColorOnHover)
                return;

            isHovering = true;
        }

        // Called via UI Event "Pointer Exit"
        public void StopHover() => isHovering = false;

        // Called via UI Event
        // Tell the panel we got clicked
        public virtual void GotClicked() => panel.GridClicked(rowN, columnN);

        public void Start()
        {
            // If these haven't been assigned, assign them
            if (!image) image = GetComponent<Image>();
            if (!button) button = GetComponent<Button>();
        }
        
        public void Update()
        {
            // Reset these values. They will be acted upon in LateUpdate()
            inHoverGroup = false;
            canPlaceHoverGroup = false;

            SetScreenPoint();
        }

        // This is where we will change the tint of the buttons
        public void LateUpdate()
        {
            SetHoverGroup();
            SetButtonColor();
        }

        // Set the screen position of the button
        private void SetScreenPoint()
        {
            if (screenPointSet)
                return;
            
            screenPoint = RectTransformUtility.WorldToScreenPoint(panel.uiCamera, rectTransform.position);
            screenPointSet = true;
        }

        /// <summary>
        /// Returns the screen point of the Button
        /// </summary>
        /// <returns></returns>
        public Vector2 GetScreenPoint()
        {
            screenPoint = RectTransformUtility.WorldToScreenPoint(panel.uiCamera, rectTransform.position);
            return new Vector2(screenPoint.x, screenPoint.y);
        }

        // We will set the tint of the button here
        private void SetButtonColor()
        {
            if (!image || !button) return;

            if (!onScreenItem.ItemIsHeld || !inHoverGroup)
            {
                image.color = ButtonIsFilled ? panel.filledColor : panel.emptyColor;
                return;
            }

            if (panel.computeCanPlaceOnHoverGroup)
            {
                image.color = !canPlaceHoverGroup ? panel.unavailableColor : panel.availableColor;
                return;
            }
            
            image.color = ButtonIsFilled ? panel.unavailableColor : panel.availableColor;
        }

        // If the user is hovering, compute the group of buttons that may be affected by any held item, and set those
        // appropriately.
        private void SetHoverGroup()
        {
            if (!isHovering) return;
            if (!onScreenItem.ItemIsHeld) return;
            if (!panel.showColorOnHover) return;

            var height = onScreenItem.ItemHeight;
            var width = onScreenItem.ItemWidth;

            if (height <= 0 || width <= 0)
                return;

            var gridButtons = panel.GetButtonsInArea(rowN, columnN, height, width);
            foreach(var gridButton in gridButtons)
                gridButton.inHoverGroup = true;

            if (!panel.computeCanPlaceOnHoverGroup)
                return;
            
            var numberOfObjects = 0;
            InventorySpot firstSpot = null;
            (numberOfObjects, firstSpot) = panel.Spots.ItemsInSpotArea(rowN, columnN, height, width);

            var canPlaceObject = numberOfObjects <= 1 
                                 && panel.Spots.AllSpotsAreEmpty(
                                     rowN, columnN, height, width, firstSpot?.inGameObject);
            
            foreach(var gridButton in gridButtons)
                gridButton.canPlaceHoverGroup = canPlaceObject;
        }
    }
}
