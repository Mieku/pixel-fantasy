using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AI;
using Characters;
using Controllers;
using Handlers;
using Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        [SerializeField] private GameObject _UIHandle;

        public static bool WorldIsClearing;
        
        [Serializable]
        public class SaveData
        {
            public SaveHeader Header;
            public CameraData CameraData;
            public EnvironmentData EnvironmentData;
            public Dictionary<string, KinlingData>  Kinlings;
            public Dictionary<string, ItemData> ItemsData;
            public Dictionary<string, FurnitureData> FurnitureData;
            public List<ZoneData> ZonesData;
            public Dictionary<string, ConstructionData> StructuresData;
            public Dictionary<string, FloorData> FloorsData;
            public Dictionary<string, BasicResourceData> ResourcesData;
            public TileMapData TileMapData;
            public List<RampData> RampData;
            public List<TaskQueue> TasksData;
        }

        private SaveHeader GenerateHeader()
        {
            string uniqueID = $"{Guid.NewGuid()}";
            string saveFileName = SanitizeFileName($"{uniqueID}");
            
            return new SaveHeader()
            {
                GameVersion = Application.version,
                SaveFileName = saveFileName,
                SettlementName = GameManager.Instance.GameData.SettlementName,
                SaveName = GameManager.Instance.GameData.SettlementName,
                ScreenshotPath = TakeScreenshot(saveFileName),
                SaveDate = DateTime.Now,
                GameDate = EnvironmentManager.Instance.GameTime.Readable(),
                UniqueID = uniqueID,
            };
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
            if (!System.IO.Directory.Exists($"{Application.persistentDataPath}/Saves"))
            {
                System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Saves");
            }

            // Saving to file
            try
            {
                System.IO.File.WriteAllText($"{Application.persistentDataPath}/Saves/{saveData.Header.SaveFileName}.json", json);
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

        private string TakeScreenshot(string fileName)
        {
            string screenshotFolderPath = $"{Application.persistentDataPath}/Saves/SaveScreenshots";
    
            if (!System.IO.Directory.Exists(screenshotFolderPath))
            {
                System.IO.Directory.CreateDirectory(screenshotFolderPath);
            }
    
            string filePath = System.IO.Path.Combine(screenshotFolderPath, fileName + ".png");
    
            _UIHandle.gameObject.SetActive(false);

            // Capture the screenshot and save it
            ScreenCapture.CaptureScreenshot(filePath);
    
            _UIHandle.gameObject.SetActive(true);

            return filePath;
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

            GameManager.Instance.GameData.SettlementName = saveData.Header.SettlementName;
    
            stopwatch.Stop();
            Debug.Log($"Load Complete in {stopwatch.ElapsedMilliseconds} ms");
            onLoadFinished?.Invoke();
            
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
            if (!System.IO.Directory.Exists($"{Application.persistentDataPath}/Saves"))
            {
                System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Saves");
            }
    
            var saveFiles = System.IO.Directory.GetFiles($"{Application.persistentDataPath}/Saves");
            if (saveFiles.Length > 0)
            {
                return true;
            }

            return false;
        }
        
        public List<SaveHeader> GetAllSaveHeaders()
        {
            List<SaveHeader> results = new List<SaveHeader>();
    
            // Ensure there is a save directory
            if (!System.IO.Directory.Exists($"{Application.persistentDataPath}/Saves"))
            {
                System.IO.Directory.CreateDirectory($"{Application.persistentDataPath}/Saves");
            }
    
            var saveFiles = System.IO.Directory.GetFiles($"{Application.persistentDataPath}/Saves");
            foreach (var saveFile in saveFiles)
            {
                if (System.IO.File.Exists(saveFile))
                {
                    string json = System.IO.File.ReadAllText(saveFile);

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    // Deserialize only the SaveHeader part of the JSON
                    var saveHeader = JsonConvert.DeserializeObject<SaveHeader>(
                        JObject.Parse(json)["Header"].ToString(), 
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
            string filePath = $"{Application.persistentDataPath}/Saves/{header.SaveFileName}.json";
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);

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
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_'); // Replace invalid characters with an underscore
            }
            return fileName;
        }

        public void ChangeSaveName(SaveHeader header, string newSaveName, Action<SaveHeader> onComplete)
        {
            var oldSave = LoadSaveFromHeader(header);
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
                
                StartCoroutine(WriteSaveData(saveData, () =>
                {
                    ReportProgress();
                    onComplete?.Invoke(saveData.Header);
                }));
            }));
        }

        public void DeleteSave(SaveHeader header)
        {
            string filePath = $"{Application.persistentDataPath}/Saves/{header.SaveFileName}.json";
            string screenshotPath = header.ScreenshotPath;
            
            // Delete Save
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            
            // Delete Screenshot
            if (System.IO.File.Exists(screenshotPath))
            {
                System.IO.File.Delete(screenshotPath);
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
    }
}