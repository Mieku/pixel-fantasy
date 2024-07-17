using System;
using System.Collections.Generic;
using System.Diagnostics;
using Characters;
using Controllers;
using Handlers;
using Items;
using Managers;
using Newtonsoft.Json;
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
            }

            public SaveHeader Header;
            public EnvironmentData EnvironmentData;
            public List<KinlingData> Kinlings;
            public List<ItemData> ItemsData;
            public List<FurnitureData> FurnitureData;
            public List<ZoneData> ZonesData;
            public List<BasicResourceData> ResourcesData;
            public TileMapData TileMapData;
            public List<RampData> RampData;
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
                Kinlings = KinlingsDatabase.Instance.GetKinlingsData(),
                ItemsData = ItemsDatabase.Instance.GetItemsData(),
                FurnitureData = FurnitureDatabase.Instance.GetFurnitureData(),
                ZonesData = ZonesDatabase.Instance.GetZonesData(),
            };

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            string json = JsonConvert.SerializeObject(saveData, settings);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    
            stopwatch.Stop();
            Debug.Log($"Save Complete in {stopwatch.ElapsedMilliseconds} ms");
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
                KinlingsDatabase.Instance.LoadKinlingsData(saveData.Kinlings);
                ZonesDatabase.Instance.LoadZonesData(saveData.ZonesData);
                FurnitureDatabase.Instance.LoadFurnitureData(saveData.FurnitureData);
                ItemsDatabase.Instance.LoadItemsData(saveData.ItemsData);
            }
            
            stopwatch.Stop();
            Debug.Log($"Load Complete in {stopwatch.ElapsedMilliseconds} ms");
        }

        public void ClearWorld() // TODO: for testing
        {
            TilemapController.Instance.ClearAllTiles();
            ResourcesDatabase.Instance.DeleteResources();
            _rampsHandler.DeleteRamps();
            KinlingsDatabase.Instance.DeleteAllKinlings();
            ZonesDatabase.Instance.ClearAllZones();
            ItemsDatabase.Instance.ClearAllItems();
            FurnitureDatabase.Instance.ClearAllFurniture();
        }
    }
}