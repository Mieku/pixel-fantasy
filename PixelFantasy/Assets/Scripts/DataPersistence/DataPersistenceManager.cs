using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AI;
using Characters;
using Controllers;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player;
using Systems.Buildings.Scripts;
using Systems.Game_Setup.Scripts;
using Systems.Zones.Scripts;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DataPersistence
{
    public class DataPersistenceManager : Singleton<DataPersistenceManager>
    {
        [SerializeField] private RampsHandler _rampsHandler;

        public static bool WorldIsClearing;

        private static string SaveFilePath;

        private static List<SaveHeader> SaveHeadersCache = new List<SaveHeader>();

        protected override void Awake()
        {
            base.Awake();
            SaveFilePath = $"{Application.persistentDataPath}/Saves";
            
            Initialize();
        }

        private static void Initialize()
        {
            SaveHeadersCache = LoadAllSaveHeaders();
        }

        private void AddSaveHeaderToCache(SaveHeader header)
        {
            SaveHeadersCache.Add(header);
            
            // Resorts them
            SaveHeadersCache.Sort((x, y) => y.SaveDate.CompareTo(x.SaveDate));
        }

        private void RemoveSaveHeaderFromCache(SaveHeader header)
        {
            SaveHeadersCache.Remove(header);
            
            // Resorts them
            SaveHeadersCache.Sort((x, y) => y.SaveDate.CompareTo(x.SaveDate));
        }

        public List<SaveHeader> GetAllSaveHeaders()
        {
            if (SaveHeadersCache == null)
            {
                SaveHeadersCache = LoadAllSaveHeaders();
            }

            return SaveHeadersCache;
        }

        private SaveHeader GenerateHeader()
        {
            string uniqueID = $"{Guid.NewGuid()}";
            string saveFileName = SanitizeFileName($"{uniqueID}");
            string screenshotPath = null;

            var screenshotter = FindObjectOfType<ScreenshotController>();
            if (screenshotter != null)
            {
                screenshotPath = screenshotter.TakeSaveScreenshot(saveFileName);
            }
            
            return new SaveHeader()
            {
                GameVersion = Application.version,
                SaveFileName = saveFileName,
                SettlementName = GameManager.Instance.GameData.SettlementName,
                SaveName = GameManager.Instance.GameData.SettlementName,
                ScreenshotPath = screenshotPath,
                SaveDate = DateTime.Now,
                GameDate = EnvironmentManager.Instance.GameTime.Readable(),
                UniqueID = uniqueID,
            };
        }
        
        public IEnumerator AutoSaveGameCoroutine(Action<float> progressCallback)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Define the total number of steps
            int totalSteps = 14;
            int currentStep = 0;

            // Increment step and report progress
            void ReportProgress()
            {
                currentStep++;
                progressCallback?.Invoke((float)currentStep / totalSteps);
            }

            // Create and write the autosave data
            yield return StartCoroutine(CreateSaveData(ReportProgress, (saveData) =>
            {
                saveData.Header.IsAutoSave = true;
                saveData.Header.SaveFileName = $"AutoSave_{saveData.Header.SaveFileName}";
                StartCoroutine(WriteSaveData(saveData, () =>
                {
                    Debug.Log("Auto Save Complete in " + stopwatch.ElapsedMilliseconds + " ms");
                }));
            }));

            stopwatch.Stop();
            ReportProgress();

            // Handle autosave deletion if exceeding the max limit
            ManageAutoSaves();

            yield return null;
        }

        private void ManageAutoSaves()
        {
            var headers = GetAllSaveHeaders();
            var autoSaveHeaders = headers.Where(h => h.IsAutoSave).ToList();

            // Sort by date or any other criteria to identify the oldest
            autoSaveHeaders.Sort((a, b) => b.SaveDate.CompareTo(a.SaveDate));

            int maxAutoSaves = PlayerSettings.MaxAutoSaves;

            for (int i = maxAutoSaves; i < autoSaveHeaders.Count; i++)
            {
                DeleteSave(autoSaveHeaders[i]);
            }
        }
        
        public IEnumerator SaveGameCoroutine(Action<float> progressCallback)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Define the total number of steps
            int totalSteps = 14;
            int currentStep = 0;

            // Increment step and report progress
            void ReportProgress()
            {
                currentStep++;
                progressCallback?.Invoke((float)currentStep / totalSteps);
            }

            yield return StartCoroutine(CreateSaveData(ReportProgress, (saveData) =>
            {
                saveData.Header.IsAutoSave = false;
                StartCoroutine(WriteSaveData(saveData, () =>
                {
                    Debug.Log("Save Complete in " + stopwatch.ElapsedMilliseconds + " ms");
                }));
            }));

            stopwatch.Stop();
            ReportProgress();
            yield return null;
        }

        private IEnumerator CreateSaveData(Action stepCompleteCallback, Action<SaveData> onSaveDataCreated)
        {
            // Increment step and report progress
            void ReportProgress()
            {
                stepCompleteCallback?.Invoke();
            }
            
            SaveData saveData = new SaveData
            {
                // Collecting data and reporting progress
                Header = GenerateHeader()
            };

            saveData.GameData = GameManager.Instance.GameData;
            ReportProgress();
            yield return null;

            saveData.CameraData = CameraManager.Instance.SaveCameraData();
            ReportProgress();
            yield return null;

            saveData.EnvironmentData = EnvironmentManager.Instance.GetEnvironmentData();
            ReportProgress();
            yield return null;

            saveData.TileMapData = TilemapController.Instance.GetTileMapData();
            ReportProgress();
            yield return null;

            saveData.ResourcesData = ResourcesDatabase.Instance.GetResourcesData();
            ReportProgress();
            yield return null;

            saveData.RampData = _rampsHandler.GetRampsData();
            ReportProgress();
            yield return null;

            saveData.Kinlings = KinlingsDatabase.Instance.SaveKinlingsData();
            ReportProgress();
            yield return null;

            saveData.ItemsData = ItemsDatabase.Instance.SaveItemsData();
            saveData.StacksData = ItemsDatabase.Instance.SaveStacksData();
            ReportProgress();
            yield return null;

            saveData.FurnitureData = FurnitureDatabase.Instance.SaveFurnitureData();
            ReportProgress();
            yield return null;

            saveData.ZonesData = ZonesDatabase.Instance.SaveZonesData();
            ReportProgress();
            yield return null;

            saveData.StructuresData = StructureDatabase.Instance.SaveStructureData();
            ReportProgress();
            yield return null;

            saveData.FloorsData = FlooringDatabase.Instance.SaveFloorData();
            ReportProgress();
            yield return null;

            saveData.TasksData = TasksDatabase.Instance.SaveTaskData();
            ReportProgress();
            yield return null;
            
            onSaveDataCreated?.Invoke(saveData);
        }

        private IEnumerator WriteSaveData(SaveData saveData, Action onComplete)
        {
            // Serialization
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                Error = HandleSerializationError
            };

            string json = string.Empty;
            try
            {
                json = JsonConvert.SerializeObject(saveData, settings);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error serializing game data: " + ex.Message);
                Debug.LogError("StackTrace: " + ex.StackTrace);
                yield break;
            }
            
            // Ensure there is a save directory
            if (!Directory.Exists(SaveFilePath))
            {
                Directory.CreateDirectory(SaveFilePath);
            }

            // Saving to file
            try
            {
                AddSaveHeaderToCache(saveData.Header);
                File.WriteAllText(Path.Combine(SaveFilePath, $"{saveData.Header.SaveFileName}.json"), json);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving game: " + ex.Message);
                Debug.LogError("StackTrace: " + ex.StackTrace);
            }
            finally
            {
                onComplete?.Invoke();
            }
        }
        
        private void HandleSerializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            var currentError = e.ErrorContext.Error.Message;
            Debug.LogError($"Serialization Error: {currentError} with {e.CurrentObject} path {e.ErrorContext.Path}");
            e.ErrorContext.Handled = true;
        }

        public IEnumerator LoadGameCoroutine(SaveData saveData, Action onLoadFinished, Action<string> onStepStarted, Action<string> onStepCompleted)
        {
            // Ensures the game is paused during load, returns to prior state when done
            var gameSpeed = TimeManager.Instance.GameSpeed;
            TimeManager.Instance.SetGameSpeed(GameSpeed.Paused);
            
            onStepStarted?.Invoke("Prepping Data");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
    
            GameEvents.Trigger_OnGameLoadStart();
            
            yield return StartCoroutine(ClearWorld());
            onStepCompleted?.Invoke("Prepping Data");
                
            onStepStarted?.Invoke("Loading Tasks");
            TasksDatabase.Instance.LoadTasksData(saveData.TasksData);
            onStepCompleted?.Invoke("Loading Tasks");
            yield return null;
        
            onStepStarted?.Invoke("Loading Environment");
            GameManager.Instance.GameData = saveData.GameData;
            CameraManager.Instance.LoadCameraData(saveData.CameraData);
            EnvironmentManager.Instance.LoadEnvironmentData(saveData.EnvironmentData);
            onStepCompleted?.Invoke("Loading Environment");
            yield return null;
        
            onStepStarted?.Invoke("Loading Tilemaps");
            TilemapController.Instance.LoadTileMapData(saveData.TileMapData);
            _rampsHandler.LoadRampsData(saveData.RampData);
            onStepCompleted?.Invoke("Loading Tilemaps");
            yield return null;
        
            onStepStarted?.Invoke("Spawning Resources");
            ResourcesDatabase.Instance.LoadResourcesData(saveData.ResourcesData);
            onStepCompleted?.Invoke("Spawning Resources");
            yield return null;
        
            onStepStarted?.Invoke("Spawning Items");
            ItemsDatabase.Instance.LoadItemsData(saveData.ItemsData);
            ItemsDatabase.Instance.LoadStacksData(saveData.StacksData);
            onStepCompleted?.Invoke("Spawning Items");
            yield return null;
        
            onStepStarted?.Invoke("Spawning Zones");
            ZonesDatabase.Instance.LoadZonesData(saveData.ZonesData);
            onStepCompleted?.Invoke("Spawning Zones");
            yield return null;
        
            onStepStarted?.Invoke("Spawning Structures");
            StructureDatabase.Instance.LoadStructureData(saveData.StructuresData);
            FlooringDatabase.Instance.LoadFloorData(saveData.FloorsData);
            onStepCompleted?.Invoke("Spawning Structures");
            yield return null;
        
            onStepStarted?.Invoke("Spawning Furniture");
            FurnitureDatabase.Instance.LoadFurnitureData(saveData.FurnitureData);
            onStepCompleted?.Invoke("Spawning Furniture");
            yield return null;
                
            NavMeshManager.Instance.UpdateNavMesh(true);
        
            onStepStarted?.Invoke("Spawning Kinlings");
            KinlingsDatabase.Instance.LoadKinlingsData(saveData.Kinlings);
            onStepCompleted?.Invoke("Spawning Kinlings");
            yield return null;
    
            stopwatch.Stop();
            Debug.Log($"Load Complete in {stopwatch.ElapsedMilliseconds} ms");
            onLoadFinished?.Invoke();
            
            GameEvents.Trigger_OnInventoryChanged();
            GameEvents.Trigger_OnGameLoadComplete();
            TimeManager.Instance.SetGameSpeed(gameSpeed);
            
            yield return null;
        }
        
        public IEnumerator ClearWorld()
        {
            WorldIsClearing = true;

            // Stop all kinlings' tasks and AI behaviors
            KinlingsDatabase.Instance.StopAllKinlingTasks();
            yield return null; // Allow frame to update

            // Optionally, you can add a short delay to ensure all tasks have fully stopped
            yield return new WaitForSeconds(0.1f);

            // Start clearing the world
            TilemapController.Instance.ClearAllTiles();
            yield return null; // Yield to allow frame update

            ResourcesDatabase.Instance.DeleteResources();
            yield return null; // Yield to allow frame update

            _rampsHandler.DeleteRamps();
            yield return null; // Yield to allow frame update

            yield return StartCoroutine(KinlingsDatabase.Instance.DeleteAllKinlings());
            yield return null; // Yield to allow frame update

            ZonesDatabase.Instance.ClearAllZones();
            yield return null; // Yield to allow frame update

            ItemsDatabase.Instance.ClearAllItems();
            yield return null; // Yield to allow frame update

            FurnitureDatabase.Instance.ClearAllFurniture();
            yield return null; // Yield to allow frame update

            StructureDatabase.Instance.ClearAllStructures();
            yield return null; // Yield to allow frame update

            FlooringDatabase.Instance.ClearAllFloors();
            yield return null; // Yield to allow frame update

            TasksDatabase.Instance.ClearAllTasks();
            yield return null; // Yield to allow frame update

            PlayerInteractableDatabase.Instance.ClearDatabase();
            yield return null; // Yield to allow frame update

            WorldIsClearing = false;
        }

        public bool AreSavesAvailable()
        {
            // Ensure there is a save directory
            if (!Directory.Exists(SaveFilePath))
            {
                Directory.CreateDirectory(SaveFilePath);
            }
    
            var saveFiles = Directory.GetFiles(SaveFilePath);
            if (saveFiles.Length > 0)
            {
                return true;
            }

            return false;
        }
        
        private static List<SaveHeader> LoadAllSaveHeaders()
        {
            List<SaveHeader> results = new List<SaveHeader>();
    
            // Ensure there is a save directory
            if (!Directory.Exists(SaveFilePath))
            {
                Directory.CreateDirectory(SaveFilePath);
            }
    
            var saveFiles = Directory.GetFiles(SaveFilePath);
            foreach (var saveFile in saveFiles)
            {
                if (File.Exists(saveFile))
                {
                    string json = File.ReadAllText(saveFile);

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    // Deserialize only the SaveHeader part of the JSON
                    var saveHeader = JsonConvert.DeserializeObject<SaveHeader>(
                        JObject.Parse(json)["Header"]!.ToString(), 
                        settings
                    );
                    results.Add(saveHeader);
                }
            }
            
            // Sort results by SaveDate in descending order
            results.Sort((x, y) => y.SaveDate.CompareTo(x.SaveDate));

            return results;
        }

        public SaveData LoadSaveFromHeader(SaveHeader header)
        {
            string filePath = Path.Combine(SaveFilePath, $"{header.SaveFileName}.json");
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                return JsonConvert.DeserializeObject<SaveData>(json, settings);
            }
            else
            {
                Debug.LogError("Save file not found: " + filePath);
                return null;
            }
        }

        public SaveData GetMostRecentSave()
        {
            var allSaves = GetAllSaveHeaders();
            if (allSaves == null || allSaves.Count == 0)
            {
                return null;
            }

            return LoadSaveFromHeader(allSaves.First());
        }
        
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_'); // Replace invalid characters with an underscore
            }
            return fileName;
        }

        public void ChangeSaveName(SaveHeader header, string newSaveName, Action<SaveHeader> onComplete)
        {
            var oldSave = LoadSaveFromHeader(header);
            RemoveSaveHeaderFromCache(header);
            oldSave.Header.SaveName = newSaveName;

            StartCoroutine(WriteSaveData(oldSave, () =>
            {
                onComplete?.Invoke(oldSave.Header);
            }));
        }

        public void OverwriteSave(SaveHeader headerToOverwrite, Action<float> progressCallback, Action<SaveHeader> onComplete)
        {
            var saveName = headerToOverwrite.SaveName;
            DeleteSave(headerToOverwrite);
            
            // Define the total number of steps
            int totalSteps = 14;
            int currentStep = 0;

            // Increment step and report progress
            void ReportProgress()
            {
                currentStep++;
                progressCallback?.Invoke((float)currentStep / totalSteps);
            }
            
            StartCoroutine(CreateSaveData(ReportProgress, (saveData) =>
            {
                saveData.Header.SaveName = saveName;
                saveData.Header.IsAutoSave = false;
                
                StartCoroutine(WriteSaveData(saveData, () =>
                {
                    ReportProgress();
                    onComplete?.Invoke(saveData.Header);
                }));
            }));
        }

        public void DeleteSave(SaveHeader header)
        {
            string filePath = Path.Combine(SaveFilePath, $"{header.SaveFileName}.json");
            string screenshotPath = header.ScreenshotPath;
            RemoveSaveHeaderFromCache(header);
            
            // Delete Save
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            // Delete Screenshot
            if (File.Exists(screenshotPath))
            {
                File.Delete(screenshotPath);
            }
        }
    }
    
    [Serializable]
    public class SaveHeader
    {
        public string GameVersion;
        public string SaveName;
        public string SaveFileName;
        public string SettlementName;
        public string ScreenshotPath;
        public DateTime SaveDate;
        public string GameDate;
        public string UniqueID;
        public bool IsAutoSave;
    }
    
    [Serializable]
    public class SaveData
    {
        public SaveHeader Header;
        public GameData GameData;
        public CameraData CameraData;
        public EnvironmentData EnvironmentData;
        public Dictionary<string, KinlingData>  Kinlings;
        public Dictionary<string, ItemData> ItemsData;
        public Dictionary<string, ItemStackData> StacksData;
        public Dictionary<string, FurnitureData> FurnitureData;
        public List<ZoneData> ZonesData;
        public Dictionary<string, ConstructionData> StructuresData;
        public Dictionary<string, FloorData> FloorsData;
        public Dictionary<string, BasicResourceData> ResourcesData;
        public TileMapData TileMapData;
        public List<RampData> RampData;
        public List<TaskQueue> TasksData;
    }
}