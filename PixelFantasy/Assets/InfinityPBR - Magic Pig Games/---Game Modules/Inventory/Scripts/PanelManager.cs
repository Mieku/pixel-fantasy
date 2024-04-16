using System;
using UnityEngine;
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
    public class PanelManager : MonoBehaviour
    {
        public static PanelManager panelManager;
        
        [Tooltip("Optional: This is the player inventory panel, which we expect to always be available to the user.")]
        public Panel playerInventoryPanel;
        [HideInInspector] public Panel otherPanel;
        public Panel ActivePanel => GetActivePanel();
        public Camera UICamera;
        public Transform inventoryPanelParent;
        public Transform otherPanelParent;
        public int gridButtonSize = 100; // Size of the square inventory grid buttons.

        [Header("Pause Level Options")] 
        public int pauseLevelOnOpen = 2;
        public bool resetPauseLevelOnClose = true;
        
        [Header("Debug Options")] 
        public bool writeToConsole = true; // When true, the console will print out helpful information
        public Color writeToConsoleColor = new Color(0.15f, 0.4f, 0.1f);

        public bool PlayerInventoryIsOpen => playerInventoryPanel != null && (playerInventoryPanel.gameObject != null && playerInventoryPanel.gameObject.activeSelf);
        public bool OtherInventoryIsOpen => otherPanel != null && (otherPanel.gameObject != null && otherPanel.gameObject.activeSelf);
        public bool InventoryIsOpen => PlayerInventoryIsOpen || OtherInventoryIsOpen;
        
        public GameObject OtherPanelObject { get; private set; }
        
        public Panel GetActivePanel()
        {
            if (playerInventoryPanel.gameObject.activeSelf) return playerInventoryPanel;
           
            if (!otherPanel) return null;
            if (!otherPanel.gameObject) return null;
            
            if (otherPanel.gameObject.activeSelf) return otherPanel;
            
            return null;
        }

        void Awake()
        {
            if (panelManager == null)
                panelManager = this;
            else if (panelManager != this)
                Destroy(gameObject);

            CheckRequired();
        }

        void LinkPanelManager()
        {
            if (panelManager)
                return;
            
            panelManager = this;
        }

        private void CheckRequired()
        {
            if (playerInventoryPanel == null)  WriteToConsole("<color=#ffff00>Required:</color> playerInventoryPanel is required for PanelManager", "PanelManager", writeToConsoleColor, writeToConsole, true, gameObject);
            if (UICamera == null)  WriteToConsole("<color=#ffff00>Required:</color> UICamera is required for PanelManager", "PanelManager", writeToConsoleColor, writeToConsole, true, gameObject);
            if (inventoryPanelParent == null)  WriteToConsole("<color=#ffff00>Required:</color> inventoryPanelParent is required for PanelManager", "PanelManager", writeToConsoleColor, writeToConsole, true, gameObject);
            if (otherPanelParent == null)  WriteToConsole("<color=#ffff00>Required:</color> otherPanelParent is required for PanelManager", "PanelManager", writeToConsoleColor, writeToConsole, true, gameObject);
        }

        private void Update()
        {
            LinkPanelManager();
            
            if (!otherPanel)
                return;

            otherPanel.gameObject.SetActive(!playerInventoryPanel.isActiveAndEnabled);
        }

        /// <summary>
        /// This will populate the otherPanel variable with the newPanel passed into it, and optionally set the
        /// new panel to be active.
        /// </summary>
        /// <param name="newPanel"></param>
        /// <param name="obj"></param>
        /// <param name="setActive"></param>
        public Panel SetOtherPanel(Panel newPanel, GameObject obj, bool setActive = true)
        {
            ToggleOtherInventoryPanel(false);
            if (!newPanel)
                return default;

            otherPanel = newPanel;
            OtherPanelObject = obj;
            otherPanel.SetUICamera(UICamera);
            if (!setActive)
                return otherPanel;
            
            ToggleOtherInventoryPanel();
            return otherPanel;
        }

        private void ResetPauseLevel()
        {
            if (!resetPauseLevelOnClose) return;
            Debug.LogWarning("Reset Pause Level");
            Timeboard.timeboard.gametime.ResetToLastPauseLevel();
        }

        private void SetPauseLevel()
        {
            if (pauseLevelOnOpen < 0) return;

            Timeboard.timeboard.gametime.SetPauseLevelMax(pauseLevelOnOpen);
        }

        /// <summary>
        /// Turn on or off the otherPanel. If true, will turn off the player panel if it is active.
        /// </summary>
        /// <param name="turnOn"></param>
        public void ToggleOtherInventoryPanel(bool turnOn = true)
        {
            if (!otherPanel) return;
            
            otherPanel.gameObject.SetActive(turnOn);
            if (!turnOn)
            {
                OtherPanelObject = null;
                ResetPauseLevel();
                return;
            }

            // Deactivate the player inventory & reset pause level
            TogglePlayerInventoryPanel(false);
            
            SetPauseLevel();
        }

        /// <summary>
        /// Turn on or off the player inventory panel. If true, will turn off any otherPanel that may be active.
        /// </summary>
        /// <param name="turnOn"></param>
        /// <param name="iHaveInventoryGameId"></param>
        public void TogglePlayerInventoryPanel(bool turnOn = true, string iHaveInventoryGameId = "")
        {
            if (!playerInventoryPanel) return;

            // Don't do anything if we are already turned on/off and that's what we want to do
            if (playerInventoryPanel.gameObject.activeSelf == turnOn)
                return;

            if (!string.IsNullOrWhiteSpace(iHaveInventoryGameId))
                playerInventoryPanel.SetIHaveInventoryGameId(iHaveInventoryGameId);
            
            playerInventoryPanel.gameObject.SetActive(turnOn);
            
            if (!turnOn)
            {
                if (!otherPanel)
                    ResetPauseLevel();
                return;
            }

            // Deactivate the other inventory, and reset the pause level
            ToggleOtherInventoryPanel(false);
            SetPauseLevel();
        }

        public Panel CreatePanel(GameObject inventoryPrefab, Transform parent, IHaveInventory iHaveInventory, Spots spots)
        {
            var newObject = Instantiate(inventoryPrefab, parent);

            if (newObject.TryGetComponent<Panel>(out var panel))
            {
                panel.SetIHaveInventoryGameId(iHaveInventory.GameId());
                panel.SetupActions(iHaveInventory.GameId());
                
                return panel;
            }

            WriteToConsole($"Warning there was no panel on the instantiated object {newObject.name}"
                , "PanelManager", writeToConsoleColor, writeToConsole);
            return null;
        }
    }
}

