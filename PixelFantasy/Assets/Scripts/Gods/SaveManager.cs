using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DataPersistence;
using DataPersistence.States;
using UnityEngine;

namespace Gods
{
    public class SaveManager : God<SaveManager>
    {
        private GameState gameState;
        private List<IDataPersistence> dataPersistenceObjects;
        private FileDataHandler dataHandler;

        private void Start()
        {
            this.dataHandler = new FileDataHandler();
            this.dataPersistenceObjects = FindAllDataPersistenceObjects();
            NewGame();
        }

        public void NewGame()
        {
            gameState = new GameState();
        }
        
        public void SaveGame()
        {
            // Pass data to other scripts so they can update it
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                dataPersistenceObject.SaveState(ref gameState);
            }
            
            // Save that data to a file
            dataHandler.Save(gameState);
        }

        public void LoadGame()
        {
            // Load saved data from a file
            this.gameState = dataHandler.Load();
            
            if (this.gameState == null)
            {
                NewGame();
            }
            
            // Push the loaded data to all other scripts that need it
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                dataPersistenceObject.LoadData(gameState);
            }
        }

        private List<IDataPersistence> FindAllDataPersistenceObjects()
        {
            IEnumerable<IDataPersistence> dataPersistenceObjects =
                FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

            return new List<IDataPersistence>(dataPersistenceObjects);
        }
    }
}
