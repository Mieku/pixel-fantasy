using System.Collections.Generic;
using Items;
using Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace DataPersistence
{
    public class DataPersistenceManager : Singleton<DataPersistenceManager>
    {
        public void SaveGame() 
        {
            var resourcesData = ResourcesDatabase.Instance.GetResourcesData();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(resourcesData, settings);
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

                var resourcesData = JsonConvert.DeserializeObject<List<BasicResourceData>>(json, settings);
                ResourcesDatabase.Instance.LoadResourcesData(resourcesData);
            }
        }
    }
}