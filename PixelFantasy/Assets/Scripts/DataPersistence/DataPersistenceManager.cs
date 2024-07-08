using System;
using System.Collections.Generic;
using Controllers;
using Items;
using Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace DataPersistence
{
    public class DataPersistenceManager : Singleton<DataPersistenceManager>
    {
        [Serializable]
        public class SaveData
        {
            public TileMapData TileMapData;
            public List<BasicResourceData> ResourcesData;
        }
        
        public void SaveGame()
        {
            SaveData saveData = new SaveData
            {
                TileMapData = TilemapController.Instance.GetTileMapData(),
                ResourcesData = ResourcesDatabase.Instance.GetResourcesData()
            };

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(saveData, settings);
            System.IO.File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        }

        public void LoadGame()
        {
            string path = Application.persistentDataPath + "/savefile.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                
                var saveData = JsonConvert.DeserializeObject<SaveData>(json, settings);
                
                TilemapController.Instance.LoadTileMapData(saveData.TileMapData);
                ResourcesDatabase.Instance.LoadResourcesData(saveData.ResourcesData);
            }
        }
    }
}