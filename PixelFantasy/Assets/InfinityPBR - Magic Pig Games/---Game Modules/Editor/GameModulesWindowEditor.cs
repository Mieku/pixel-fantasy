using System;
using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(GameModulesWindow))]
    [CanEditMultipleObjects]
    [Serializable]
    public class GameModulesWindowEditor : ModulesScriptableObjectEditor
    {
        private GameModulesWindow ThisObject;
        private string showing = "Introduction";
        private readonly string[] internalButtons = {"Introduction", "Scripting Docs", "YouTube Tutorials", "Discord & More"};
        private readonly string[] coreButtons = {"Stats", "Items", "Loot", "Dictionary", "Voices", "Conditions"};
        private readonly string[] systemsButtons = {"Gametime", "1st Person Movement", "Inventory", "Character Creation"};

        private Vector2 scrollPosition;
        private Vector2 scrollPosition2;
        
        protected override void Setup()
        {
            ThisObject = (GameModulesWindow) target;
        }

        protected override void Draw()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            Buttons(internalButtons);
            Buttons(coreButtons);
            Buttons(systemsButtons);
            EditorGUILayout.EndScrollView();
            
            Space();
            
            scrollPosition2 = EditorGUILayout.BeginScrollView(scrollPosition2);
            Introduction();
            ScriptingDocs();
            YouTubeTutorials();
            DiscordAndMore();
            StatsInfo();
            ItemsInfo();
            LootInfo();
            DictionaryInfo();
            VoicesInfo();
            ConditionsInfo();
            GametimeInfo();
            FirstPersonMovementInfo();
            InventoryInfo();
            CharacterCreationInfo();
            EditorGUILayout.EndScrollView();
        }

        private void Buttons(string[] buttons)
        {
            StartRow();
            int x = 0;
            foreach(string button in buttons)
            {
                BackgroundColor(showing == button ? Color.green : Color.black);
                ContentColor(showing == button ? Color.green : Color.grey);
                ShowButton(button);
                x++;
                if (x > 5)
                {
                    EndRow();
                    StartRow();
                    x = 0;
                }
            }
            EndRow();
            
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }

        private void ShowButton(string label)
        {
            if (Button(label, 140))
                showing = label;
        }

        private void ScriptingDocs()
        {
            if (showing != "Scripting Docs") return;

            Label("The scripting docs site has a ton of information about the Game Modules code, including what methods do what, " +
                  "sample code, and examples of how the code can be used.\n\nhttps://infinitypbr.gitbook.io/infinity-pbr/", false, true);
        }
        
        private void YouTubeTutorials()
        {
            if (showing != "YouTube Tutorials") return;

            Label("Our YouTube has a ton of content. Tutorials, demos, tips and tricks, integrations with other assets, " +
                  "and way more. Plus livestreams, interviews...and More!\n\nhttps://www.youtube.com/channel/UCzATh2-NC_xZSGnhZF-cFaw", false, true);
        }
        
        private void DiscordAndMore()
        {
            if (showing != "Discord & More") return;

            Label("Come join the Discord!", true, true);
            Space();
            
            Label("Get support, converse with other developers, and just have a good time. Plus, all the best " +
                  "updates get posted there first, including notifications of live streams, sales, and more.\n\n" +
                  "https://discord.com/invite/cmZY2tH", false, true);
        }
        
        private void ItemsInfo()
        {
            if (showing != "Items") return;

            Label("Similar to the Stats module, the Items module is much more than it may seem on the surface. Of course " +
                  "it can be used for swords and other actual items. But it can also be used for other things.\n\nAt it's core, it " +
                  "contains \"Item Objects\" and \"Item Attributes\". Objects are things that a thing can acquire, and Attributes " +
                  "are things that modify the Objects.\n\n" +
                  "However, Attributes can also modify other things. For instance, you may choose to add your Player Race and " +
                  "Character Class as attributes. For Item Objects, you can use these as \"Modifiers\" which affect the player " +
                  "who was \"acquired\" the modification.\n\nItem Objects and Attributes can affect Stats, via " +
                  "built in tools.", false, true);
        }
        
        private void LootInfo()
        {
            if (showing != "Loot") return;

            Label("The Loot module is a method of creating a list of Item Objects, with or without Item Attributes, " +
                  "in various ways. The obvious way is for treasure boxes, but you can add a \"Lootbox\" to enemies as well, " +
                  "or to drawers in the game, or other places.\n\n" +
                  "There are built in features which allow you to pre-set groups of potential items and attributes, which " +
                  "can then be added to Loot Boxes. The items you select can be set to specific Objects and Attributes, or " +
                  "set to be randomized between any number that you have selected.\n\nFurther, you can set individual items " +
                  "to be forced, as in it will always spawn, or use curves to give the player a chance to get more loot out " +
                  "of the box. Similarly, whether or not Item Attributes are included on the item can be forced, or randomized " +
                  "via curves.\n\n" +
                  "You are also able to modify the algorithm, so you can add in your own custom logic to modify the chances " +
                  "of Item Objects being spawned, or Attributes being added.\n\n" +
                  "The demo scene for Loot & Inventory is combined, so you can see how we use an override to connect the Loot " +
                  "module with the Inventory module. If you're not using the Inventory module, you may wish to run your own " +
                  "override to connect the Loot to your system.", false, true);
        }
        
        private void StatsInfo()
        {
            if (showing != "Stats") return;

            Label("The \"Stats\" module is designed to contain Stats data that may change throughout the game.\n\n" +
                  "However, it goes further than this, as it is designed to allow the Stats to modify each other, automatically. " +
                  "This includes letting the Items module, via ItemObjects and ItemAttributes, modify the Stats.\n\nIt is " +
                  "important to note that the intention goes beyond basic stats like health, strength, or skills like sword " +
                  "and shield. Any value that you want to track in the game can be made into a Stat, which can then be " +
                  "affected by other stats, by the player (if you choose) and other objects.", false, true);
        }
        
        private void DictionaryInfo()
        {
            if (showing != "Dictionary") return;

            Label(
                "We created the \"Dictionary\" module to make all other modules more flexible. You will be looking to " +
                "add things to your game that we could never plan for, nor would we want to, since others would not " +
                "want features that you want.\n\nThe \"Dictionary\" is essentially a serializable list of things of " +
                "many common types, including types like \"ItemObject\" that are specific to the Game Modules.\n\nYou can " +
                "add Dictionary objects to any object that needs to store data. Most of the other modules have the " +
                "dictionary built into them, so you can use them there as well, to add any sort of data to your obejcts " +
                "that you'll need for your project.", false, true);
        }
        
        private void CharacterCreationInfo()
        {
            if (showing != "Character Creation") return;

            Label("Infinity PBR Character Creation Scene\n\nThis module is delievered in a .unitypackage as many users " +
                  "may not need it, and it will throw errors if specific packages are not included in your project.\n\n" +
                  "Please install \"Post Processing\" from the package manager (Unity registry) to avoid errors, and then " +
                  "extract the .unitypackage file.\n\n" +
                  "Note, to get the demo scene to work without errors, the following Infinity PBR packages are required:" +
                  "Dungeon 2 (environment): https://assetstore.unity.com/packages/3d/environments/dungeons/dungeon-2-beta-1-2-183294?aid=1100lxWw&pubref=p95-1\n" +
                  "Human: https://assetstore.unity.com/packages/3d/characters/humanoids/humans/human-character-60016?aid=1100lxWw&pubref=p95-2\n" +
                  "Elf: https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/elf-character-141802?aid=1100lxWw&pubref=p95-3\n" +
                  "Half-Orc: https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/character-pack-half-orc-human-elf-male-female-156256?aid=1100lxWw&pubref=p95-4\n" +
                  "Armor Pack 1: https://assetstore.unity.com/packages/3d/props/clothing/armor/armor-pack-1-135427?aid=1100lxWw&pubref=p95-5\n" +
                  "Armor Pack 2: https://assetstore.unity.com/packages/3d/props/clothing/armor/armor-pack-2-176688?aid=1100lxWw&pubref=p95-6\n\n" +
                  "The characters and wardrobe are all part of the RPG Character Complete bundle, which you can find here:\n" +
                  "https://assetstore.unity.com/packages/3d/characters/creatures/rpg-character-complete-bundle-10-pack-bundle-175505?aid=1100lxWw&pubref=p95-7\n\n" +
                  "If you have any questions, please check out our Discord where you can ask for support and engage with the community: https://discord.com/invite/cmZY2tH", false, true);
        }
        
        private void InventoryInfo()
        {
            if (showing != "Inventory") return;

            Label("The Inventory module is one that is less \"flexible\" than other modules because it is meant to " +
                  "be a drag-and-drop inventory system similar to games like \"Might and Magic VI\". If you don't need " +
                  "this type of Inventory, then you can skip using this. However, if you want to use this kind of system, " +
                  "you should find it quite helpful.\n\n" +
                  "The demo scene for the Loot & Inventory are the same, so you can see how the Inventory system works with " +
                  "the Loot system, and how items (using the Items module) can be passed back and forth between inventories.", false, true);
        }
        
        private void FirstPersonMovementInfo()
        {
            if (showing != "1st Person Movement") return;
            
            Label("1st Person Movement is very very simple. It's fine if you don't need advanced movement, or you just need " +
                  "a quick drag-and-drop solution for prototyping. It works for my game, Legend of the Stones, so I'm " +
                  "including it here. Though, I don't expect many users will find it very valuable :)", false, true);
        }
        
        private void GametimeInfo()
        {
            if (showing != "Gametime") return;

            Label("Gametime is a fairly simple module, which adds a timer to your project. It has some built in calendar " +
                  "systems, as well as a pause level system.", false, true);
        }
        
        private void VoicesInfo()
        {
            if (showing != "Voices") return;

            Label("The VOICES module adds a system designed to make it easier to populate common lines of AudioClips " +
                  "that may come from multiple voices, as in, NPCS, your characters, etc. Creatively could be used for " +
                  "non-voices too!\n\nNOTE:\n" +
                  "Demo audio clips were created using Replica Studios, an AI voice generator, and used with permission.\n\n" +
                  "While the Voices module will work with any audio clips (including non-vocal clips, if you'd like), " +
                  "if you need various voices for your prototype, I highly recommend Replica Studios.\n\nThey have been " +
                  "kind enough to provide a referral code. If you sign up using this code you'll get 60 minutes of free " +
                  "credit (instead of 30), and I'll get 30 minutes of credit added to my account.\n\nCheck it out " +
                  "here: http://www.replicastudios.com/r/infinity60\n\nCode is \"infinity60\"", false, true);
        }
        
        private void ConditionsInfo()
        {
            if (showing != "Conditions") return;

            Label("The CONDITIONS module is intended to add auto-removed Stat-affecting modifications to objects. Think things " +
                  "like \"Poison\" or buffs like \"Heroism\" etc.\n\n" +
                  "Once set up, the system is almost completely automatic.", false, true);
        }

        private void Introduction()
        {
            if (showing != "Introduction") return;
            
            Label("Thank you for getting the Game Modules from Infinity PBR. This package is the same backend systems I'm using " +
                  "while building my epic RPG \"Legend of the Stones\", and I'm super happy for you to use the systems too.\n\n" +
                  "Whenever I create a new system that seems flexible, I create a new \"Module\" and add it to this bundle. In " +
                  "many cases, the modules can be used individually, but they are much more powerful when used together.", false, true);
            Space();
            Label("Important", true);
            Label("This system is meant to be very flexible. The goal is to let you do whatever you want for YOUR game without " +
                  "forcing you to change how your game works. This means that much of the systems may seem somewhat vague.\n\n" +
                  "For example, the \"Stats\" module can work with any sort of stat or skill, but can easily be used to track " +
                  "other data that you'd like to keep track of, and have affected by other things in the game.\n\n" +
                  "The \"Items\" module can be used for way more than just physical items. It can be used for modifications and " +
                  "conditions, enemies and probably much more that I've not yet thought of.\n\n" +
                  "Game Modules is meant to be flexible so that you can do almost anything you want with it. With that, it does " +
                  "require a basic to intermediate level of C# / coding knowledge, as there will be times when you need to write " +
                  "custom code to fully integrate the Game Modules into your systems.", false, true);
        }

        protected override void Header()
        {
            LabelBig("Game Modules", 36, true);
            LabelBig("Module descriptions");
            Space();
        }
    }
}