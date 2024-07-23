using System;
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
            public EnvironmentData EnvironmentData;
            public List<KinlingData> Kinlings;
            public List<ItemData> ItemsData;
            public List<FurnitureData> FurnitureData;
            public List<ZoneData> ZonesData;
            public List<ConstructionData> StructuresData;
            public List<FloorData> FloorsData;
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
        
        public void SaveGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
    
            SaveData saveData = new SaveData
            {
                Header = GenerateHeader(),
                EnvironmentData = EnvironmentManager.Instance.GetEnvironmentData(),
                TileMapData = TilemapController.Instance.GetTileMapData(),
                ResourcesData = ResourcesDatabase.Instance.GetResourcesData(),
                RampData = _rampsHandler.GetRampsData(),
                Kinlings = KinlingsDatabase.Instance.SaveKinlingsData(),
                ItemsData = ItemsDatabase.Instance.GetItemsData(),
                FurnitureData = FurnitureDatabase.Instance.GetFurnitureData(),
                ZonesData = ZonesDatabase.Instance.GetZonesData(),
                StructuresData = StructureDatabase.Instance.GetStructureData(),
                FloorsData = FlooringDatabase.Instance.GetFloorData(),
                TasksData = TasksDatabase.Instance.GetTaskData(),
            };

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                Error = HandleSerializationError
            };

            try
            {
                string json = JsonConvert.SerializeObject(saveData, settings);
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

        public void LoadGame()
        {
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
                
                ClearWorld();
                
                EnvironmentManager.Instance.LoadEnvironmentData(saveData.EnvironmentData);
                TilemapController.Instance.LoadTileMapData(saveData.TileMapData);
                ResourcesDatabase.Instance.LoadResourcesData(saveData.ResourcesData);
                _rampsHandler.LoadRampsData(saveData.RampData);
                ZonesDatabase.Instance.LoadZonesData(saveData.ZonesData);
                StructureDatabase.Instance.LoadStructureData(saveData.StructuresData);
                FlooringDatabase.Instance.LoadFloorData(saveData.FloorsData);
                FurnitureDatabase.Instance.LoadFurnitureData(saveData.FurnitureData);
                ItemsDatabase.Instance.LoadItemsData(saveData.ItemsData);
                TasksDatabase.Instance.LoadTasksData(saveData.TasksData);
                KinlingsDatabase.Instance.LoadKinlingsData(saveData.Kinlings);
            }
            
            stopwatch.Stop();
            Debug.Log($"Load Complete in {stopwatch.ElapsedMilliseconds} ms");
        }

        public void ClearWorld()
        {
            TilemapController.Instance.ClearAllTiles();
            ResourcesDatabase.Instance.DeleteResources();
            _rampsHandler.DeleteRamps();
            KinlingsDatabase.Instance.DeleteAllKinlings();
            ZonesDatabase.Instance.ClearAllZones();
            ItemsDatabase.Instance.ClearAllItems();
            FurnitureDatabase.Instance.ClearAllFurniture();
            StructureDatabase.Instance.ClearAllStructures();
            FlooringDatabase.Instance.ClearAllFloors();
            TasksDatabase.Instance.ClearAllTasks();
            PlayerInteractableDatabase.Instance.ClearDatabase();
        }
    }
}