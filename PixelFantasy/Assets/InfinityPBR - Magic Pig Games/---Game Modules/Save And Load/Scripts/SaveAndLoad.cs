using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static InfinityPBR.Modules.MainBlackboard;
using static InfinityPBR.Modules.Timeboard;
using static InfinityPBR.Debugs;

/*
 * This is the Save and Load module, which should make it very easy to save and load data for your project :)
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class SaveAndLoad : MonoBehaviour
    {
        [Header("Options")] 
        public List<string> doNotLoadOnScenes = new List<string>(); // List of scene names that we should not "Load()" on.
        
        [Header("Debug Options")]
        public bool prettyPrint = true; // Makes the json file easier to read for humans
        public bool writeToConsole = true; // When true, the console will print out helpful information
        public Color writeToConsoleColor = new Color(0.5f, 0.25f, 0.6f);
        
        public static SaveAndLoad saveAndLoad; // Static reference to this class

        // IsLoaded can be checked to see if the data is loaded -- this is helpful to ensure that objects do not
        // start their operation until the data is loaded. Note, the value will be set false when Load() starts,
        // but you may wish to set it earlier if you're loading from another scene. The scene fader in the demo
        // will do this automatically.
        public bool IsLoaded { get; private set; }
        public void SetIsLoaded(bool value) => IsLoaded = value;
        
        // gameList holds the key details for all saved games, such as the SaveGameId, GameName, Gametime Now,
        // and system time.
        public GameList gameList = new GameList();
        
        // GameName can be set by your players when they save the game, or however you'd like. It can be used
        // by players to differentiate all of the saved games.
        public string GameName => gameName;
        [SerializeField] private string gameName = "New Game";
        
        // This method can be used to allow players to change the "name" of their game, which may be displayed in
        // the save/load menus of your project.
        public void SetGameName(string value) => gameName = value;
        
        // SaveGameId is a unique ID for a single save game file. If there is none set, the system will attempt
        // to load the last saved game. This is useful for building your project, as each scene will load data
        // automatically. Can also be used in a "continue" method, as simply loading any scene will load the last
        // saved game data.
        [SerializeField] private string saveGameId;
        public string SaveGameId => GetSaveGameId(); // The id of the saved game in the game list.
        private bool _loadedLastGameId = false; // Set true once we have loaded the last game ID (or attempted to).

        //private string GetSaveGameId() => string.IsNullOrWhiteSpace(_saveGameId) ? _lastSaveGameId : _saveGameId;
        private string GetSaveGameId()
        {
            if (string.IsNullOrWhiteSpace(saveGameId))
            {
                Debug.Log("SaveGameId is null or whitespace, attempting to load last saved game.");
                return _lastSaveGameId;
            }

            return saveGameId;
        }

        private string _lastSaveGameId; // This is set with LoadGameList() on Awake

        // This will create a new saveGameId and set it. Useful for "start" scenes like character creation,
        // to ensure the new game has a unique saveGameId.
        public void CreateNewSaveGameId() => saveGameId = Guid.NewGuid().ToString();

        // Sets the value, useful for loading a previously saved data file.
        //public void SetSaveGameId(string value) => _saveGameId = value;
        public void SetSaveGameId(string value)
        {
            Debug.Log($"setting _saveGameId to {value}");
            saveGameId = value;
        }

        // There can be only one saveAndLoad object.
        private void Awake()
        {
            if (saveAndLoad == null)
                saveAndLoad = this;
            else if (saveAndLoad != this)
                Destroy(gameObject);

            StartCoroutine(SetLastSaveGameIdOnAwake());
            //_lastSaveGameId = LoadGameList(); // July 9 2023 -- this seems to cause a problem if timeboard is not yet registered
        }

        private IEnumerator SetLastSaveGameIdOnAwake()
        {
            // wait for static reference to MainBlackboard to not be null
            while (MainBlackboard.blackboard == null)
                yield return null;
            while (Timeboard.timeboard == null)
                yield return null;
            _lastSaveGameId = LoadGameList();
            _loadedLastGameId = true;
            Debug.Log($"Loaded last save game id: {_lastSaveGameId}");
        }
        
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneWasLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneWasLoaded;

        private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
        {
            // If the scene name is one that we "do not load" on, then skip this.
            if (doNotLoadOnScenes.Contains(scene.name))
            {
                SetIsLoaded(true); // We still need to call this to simply end the loop
                return;
            }

            // July 17, 2023 -- we will do this in a coroutine to ensure Timeboard is active.
            StartCoroutine(LoadWhenBlackboardIsAvailable());
            
        }

        private IEnumerator LoadWhenBlackboardIsAvailable()
        {
            while (MainBlackboard.blackboard == null)
                yield return null;
            while (Timeboard.timeboard == null)
                yield return null;
            while (!_loadedLastGameId)
                yield return null;
            
            if (string.IsNullOrWhiteSpace(SaveGameId))
            {
                WriteToConsole("Save and Load: No Save Game Id. This implies that there is no last saved game as well, so we " +
                               "have nothing to load. Will abort the Load() process."
                    , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
                SetIsLoaded(true);
                yield return null;
            }
            else
            {
                Load(SaveGameId);
            }
        }

        // This is where we get the filepath, and have methods to encode and decode the data to json
        private string Filepath(string newSaveGameId) => Application.dataPath + "/" + newSaveGameId + ".json";
        private string DataAsJson(Dictionary<string, string> data) => JsonUtility.ToJson(data, prettyPrint);
        private object DataFromJson(string json) => JsonUtility.FromJson<object>(json);

        [ContextMenu("Save")]
        public virtual void Save(string sceneName = "")
        {
            var savedSceneName = string.IsNullOrWhiteSpace(sceneName) ? SceneManager.GetActiveScene().name : sceneName;
            WriteToConsole("Save and Load: Save Started!"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            blackboard.AddEvent("Save and Load", "Game Data", "Save Start", SaveGameId);
            
            if (string.IsNullOrWhiteSpace(SaveGameId))
            {
                WriteToConsole("Save and Load: SaveGameId was null or whitespace, so we will create a new one."
                    , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
                CreateNewSaveGameId();
            }
            
            // Load the data first, so we don't overwrite existing data that we don't want to overwrite, or lose data
            var savedState = LoadJsonData(SaveGameId);
            GetAllStates(savedState); // Gather all of the states from all of the objects into a Dictionary.
            SaveJsonData(savedState, savedSceneName); // Save the data as Json at the FilePath
            
            blackboard.AddEvent("Save and Load", "Game Data", "Save End", SaveGameId);
            WriteToConsole("Save and Load: Save Complete!"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
        }

        [ContextMenu("Load")]
        public virtual void Load(string gameId)
        {
            SetIsLoaded(false);
            WriteToConsole($"Save and Load: Load Started! (saveGameId is {gameId}"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            blackboard.AddEvent("Save and Load", "Game Data", "Load Start", gameId);

            var savedState = LoadJsonData(gameId);
            LoadAllStates(savedState);
            LoadGameFile(gameId);

            blackboard.AddEvent("Save and Load", "Game Data", "Load End", gameId);
            WriteToConsole("Save and Load: Load Complete!"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            SetIsLoaded(true);
        }

        // In this method, savedState is ALL of the game data, not just from one object.
        private bool SaveJsonData(SaveableData saveableData, string savedSceneName)
        {
            WriteToConsole($"Saving Json Data. Object is null? {saveableData == null}"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            // Write the file, and confirm it was saved properly.
            var contents = JsonUtility.ToJson(saveableData, prettyPrint);
            File.WriteAllText(Filepath(SaveGameId), contents);
            if (File.Exists(Filepath(SaveGameId)))
            {
                gameList.AddOrUpdate(SaveGameId, GameName, savedSceneName);
                SaveGameList();
                return true;
            }
            
            Debug.LogError("Newly created Save File not found!");
            return false;
        }

        private SaveableData LoadJsonData(string gameId)
        {
            WriteToConsole($"Loading Json Data. saveGameId is {gameId}"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            // If the file doesn't exist, an empty dictionary will be returned
            if (!File.Exists(Filepath(gameId)))
            {
                WriteToConsole($"File {gameId} did not exist, loading an empty SaveableData() instead."
                    , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
                return new SaveableData();
            }

            // Load the json data from the file, then convert it from json
            var jsonData = File.ReadAllText(Filepath(gameId));
            //var loadedData = DataFromJson(jsonData);
            //JsonUtility.FromJson<SaveableData>(jsonData);
            return JsonUtility.FromJson<SaveableData>(jsonData);
        }

        private Saveable[] Saveables => FindObjectsOfType<Saveable>();

        // This will go through all Saveable objects, and put the results of SaveState() on each into the
        // Dictionary<string, object>
        private void GetAllStates(SaveableData saveableData)
        {
            var saveables = Saveables;
            WriteToConsole($"Getting all states to save. There are {saveables.Length} Saveable objects."
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            
            foreach (var saveable in saveables)
                GetSaveable(saveable, saveableData);
        }

        private void GetSaveable(Saveable saveable, SaveableData saveableData) 
            => saveableData.AddKeyValue(saveable.SaveId, saveable.SaveState());

        // This will take the saveable data, find all the saveable objects, and pass the proper data values
        // to those objects, so they can handle it as they choose.
        private void LoadAllStates(SaveableData saveableData)
        {
            var saveables = Saveables;
            WriteToConsole($"Loading all states. There are {saveables.Length} states and " +
                           $"{saveableData.keyValues.Count} keyValues."
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            
            foreach (var saveable in saveables)
            {
                if (saveableData.TryGetValue(saveable.SaveId, out var jsonEncodedValue))
                {
                    WriteToConsole($"Handling Saveable object {saveable.gameObject.name} with " +
                                   $"saveId {saveable.SaveId}"
                        , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
                    saveable.LoadState(jsonEncodedValue);
                }
                else
                {
                    WriteToConsole("[Not an error] Could not get JSON Encoded value on saveable " +
                                   $"{saveable.gameObject.name} with saveId {saveable.SaveId}"
                        , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
                }
            }
        }

        private void LoadGameFile(string gameId)
        {
            WriteToConsole($"Loading Game File. SaveGameId is {gameId}."
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            
            var gameFile = gameList.gameFiles.FirstOrDefault(x => x.saveGameId == gameId);
            if (gameFile == null)
            {
                Debug.LogWarning($"Failed to load game file for saveGameId {gameId}.");
                return;
            }

            gameName = gameFile.gameName;
            this.saveGameId = gameFile.saveGameId;
        }
        
        // This saves the list of game files. Use this list as the reference to the games saved, in order to allow
        // players to choose which game to load.
        public virtual void SaveGameList()
        {
            WriteToConsole($"Saving GameList with {gameList.gameFiles.Count} GameFiles"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            
            // Order the list
            gameList.gameFiles 
                = gameList.gameFiles
                    .OrderByDescending(x => x.systemTimecode)
                    .ToList();
            
            var dataAsJson = JsonUtility.ToJson(gameList,prettyPrint);
            var filePath = Filepath("gameList");
            File.WriteAllText(filePath, dataAsJson);
            if (!File.Exists(filePath))
                Debug.LogError("Error saving Game List!");
            else
                blackboard.AddEvent("Save and Load", "Game List", "Saved", null);
        }

        // Will load the list of game files on the system.
        public virtual string LoadGameList()
        {
            WriteToConsole("Loading GameList"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            var filePath = Filepath("gameList");
            if (File.Exists(filePath))
            {
                var dataAsJson = File.ReadAllText(filePath);
                gameList = JsonUtility.FromJson<GameList>(dataAsJson);
                // Order the list
                gameList.gameFiles 
                    = gameList.gameFiles
                        .OrderBy(x => x.systemTimecode)
                        .ToList();
            }
            else
                SaveGameList();

            // Note: The first time this is loaded, LoadGameList() will occur before the blackboard is available,
            // and this event will not fire.
            if (blackboard != null) 
                blackboard.AddEvent("Save and Load", "Game List", "Loaded", null);

            // If there are no games, return an empty string.
            return gameList.gameFiles.Count > 0 ? gameList.gameFiles[^1].saveGameId : default;
        }

        public virtual void DeleteGame(string idToDelete)
        {
            WriteToConsole($"Deleting saveGameId {idToDelete}"
                , "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
            Debug.Log($"File {Filepath(idToDelete)} exists? {File.Exists(Filepath(idToDelete))}");
            if (!File.Exists(Filepath(idToDelete)))
            {
                DeleteGameFromList(idToDelete); // Run this just in case it exists here, but not otherwise
                return;
            }

            File.Delete(Filepath(idToDelete));
            DeleteGameFromList(idToDelete);
        }

        private void DeleteGameFromList(string idToDelete)
        {
            gameList.gameFiles.RemoveAll(x => x.saveGameId == idToDelete);
            SaveGameList();
        }
        
        public GameFile GetGameFile(string gameId) 
            => gameList.gameFiles.FirstOrDefault(x => x.saveGameId == gameId);

        public bool TryGetGameFile(string gameId, out GameFile gameFile)
        {
            gameFile = GetGameFile(gameId);
            return gameFile != null;
        }

        public void LoadLastGame() => Load(LastGameFile().saveGameId);

        public GameFile LastGameFile() => gameList.gameFiles.Count == 0 ? default : gameList.gameFiles[^1];
        public void Write(string message) => WriteToConsole(message, "SaveAndLoad", writeToConsoleColor, writeToConsole, false, gameObject);
    }
}