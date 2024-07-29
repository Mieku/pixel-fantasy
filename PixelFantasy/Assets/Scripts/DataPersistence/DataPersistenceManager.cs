using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AI;
using Characters;
using Controllers;
using Handlers;
using Managers;
using Newtonsoft.Json;
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
            [Serializable]
            public class SaveHeader
            {
                public string GameVersion;
                public string SaveName;
                public string SettlementName;
                public string ScreenshotPath;
                public DateTime SaveDate;
            }

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

        private SaveData.SaveHeader GenerateHeader()
        {
            var screenshotPath = TakeScreenshot();
            
            return new SaveData.SaveHeader()
            {
                GameVersion = Application.version,
                SaveName = "Tester Save",
                SettlementName = GameManager.Instance.PlayerData.SettlementName,
                ScreenshotPath = screenshotPath,
                SaveDate = DateTime.Now,
            };
        }
        
        public IEnumerator SaveGameCoroutine()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            SaveData saveData = new SaveData
            {
                Header = GenerateHeader(),
                CameraData = CameraManager.Instance.SaveCameraData(),
                EnvironmentData = EnvironmentManager.Instance.GetEnvironmentData(),
                TileMapData = TilemapController.Instance.GetTileMapData(),
                ResourcesData = ResourcesDatabase.Instance.GetResourcesData(),
                RampData = _rampsHandler.GetRampsData(),
                Kinlings = KinlingsDatabase.Instance.SaveKinlingsData(),
                ItemsData = ItemsDatabase.Instance.SaveItemsData(),
                FurnitureData = FurnitureDatabase.Instance.SaveFurnitureData(),
                ZonesData = ZonesDatabase.Instance.SaveZonesData(),
                StructuresData = StructureDatabase.Instance.SaveStructureData(),
                FloorsData = FlooringDatabase.Instance.SaveFloorData(),
                TasksData = TasksDatabase.Instance.SaveTaskData(),
            };

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

            yield return null; // Yield to allow frame update

            try
            {
                System.IO.File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
                Debug.Log("Save Complete in " + stopwatch.ElapsedMilliseconds + " ms");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error saving game: " + ex.Message);
                Debug.LogError("StackTrace: " + ex.StackTrace);
            }
            finally
            {
                stopwatch.Stop();
            }

            yield return null; // Yield to allow frame update
        }
        
        private void HandleSerializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            var currentError = e.ErrorContext.Error.Message;
            Debug.LogError($"Serialization Error: {currentError} with {e.CurrentObject} path {e.ErrorContext.Path}");
            e.ErrorContext.Handled = true;
        }

        private string TakeScreenshot()
        {
            string fileName = "savefile_screenshot.png";
            string screenshotFolderPath = Application.persistentDataPath + "/SaveScreenshots";
            
            if (!System.IO.Directory.Exists(screenshotFolderPath))
            {
                System.IO.Directory.CreateDirectory(screenshotFolderPath);
            }
            
            string filePath = System.IO.Path.Combine(screenshotFolderPath, fileName);
            
            //_UIHandle.gameObject.SetActive(false);

            // Capture the screenshot and save it
            ScreenCapture.CaptureScreenshot(filePath);
            
            //_UIHandle.gameObject.SetActive(true);

            return filePath;
        }
        
        public IEnumerator LoadGameCoroutine(Action onLoadFinished, Action<string> onStepStarted, Action<string> onStepCompleted)
        {
            // Ensures the game is paused during load, returns to prior state when done
            var gameSpeed = TimeManager.Instance.GameSpeed;
            TimeManager.Instance.SetGameSpeed(GameSpeed.Paused);
            
            onStepStarted?.Invoke("Prepping Data");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
    
            GameEvents.Trigger_OnGameLoadStart();
    
            string path = Application.persistentDataPath + "/savefile.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
        
                var saveData = JsonConvert.DeserializeObject<SaveData>(json, settings);
                
                yield return StartCoroutine(ClearWorld());
                onStepCompleted?.Invoke("Prepping Data");
        
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
        
                onStepStarted?.Invoke("Loading Tasks");
                TasksDatabase.Instance.LoadTasksData(saveData.TasksData);
                onStepCompleted?.Invoke("Loading Tasks");
                yield return null;
                
                NavMeshManager.Instance.UpdateNavMesh(true);
        
                onStepStarted?.Invoke("Spawning Kinlings");
                KinlingsDatabase.Instance.LoadKinlingsData(saveData.Kinlings);
                onStepCompleted?.Invoke("Spawning Kinlings");
                yield return null;
            }
    
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
    }
}