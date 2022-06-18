using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using DataPersistence.States;
using UnityEngine;

namespace Gods
{
    public class SaveManager : God<SaveManager>
    {
        private GameState gameState;
        private List<Saveable> dataPersistenceObjects;
        private FileDataHandler dataHandler;
        [SerializeField] private AstarPath _pathFinder;

        public bool IsLoading;
        
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
            Debug.Log("Saving Game...");
            GameEvents.Trigger_OnSavingGameBeginning();
            
            // Pass data to other scripts so they can update it
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                dataPersistenceObject.SaveState(ref gameState);
            }
            
            // Save that data to a file
            dataHandler.Save(gameState);
            
            GameEvents.Trigger_OnSavingGameEnd();
        }

        public void LoadGame()
        {
            IsLoading = true;
            GameEvents.Trigger_OnLoadingGameBeginning();
            StartCoroutine(LoadingSequence());
        }

        private IEnumerator LoadingSequence()
        {
            Debug.Log("Loading Game...");
            
            // Load saved data from a file
            this.gameState = dataHandler.Load();
            
            if (this.gameState == null)
            {
                NewGame();
            }
            
            SortedDictionary<int, List<Saveable>> _orderedDataPersistenceObjects = new SortedDictionary<int, List<Saveable>>();
            foreach (var dataPersistenceObject in dataPersistenceObjects)
            {
                if (!_orderedDataPersistenceObjects.ContainsKey(dataPersistenceObject.LoadOrder))
                {
                    _orderedDataPersistenceObjects[dataPersistenceObject.LoadOrder] = new List<Saveable>();
                }
                
                _orderedDataPersistenceObjects[dataPersistenceObject.LoadOrder].Add(dataPersistenceObject);
                
            }
            
            foreach (var dataPersistenceObjects in _orderedDataPersistenceObjects)
            {
                foreach (var dataPersistenceObject in dataPersistenceObjects.Value)
                {
                    dataPersistenceObject.LoadData(gameState);
                }
                yield return new WaitForSeconds(0); // Gives time to make sure loaded
                // TODO: See if I can set up a safer way to do this using callbacks
            }
            
            _pathFinder.Scan();
            
            GameEvents.Trigger_OnLoadingGameEnd();
            IsLoading = false;
        }

        private List<Saveable> FindAllDataPersistenceObjects()
        {
            IEnumerable<Saveable> dataPersistenceObjects =
                FindObjectsOfType<MonoBehaviour>().OfType<Saveable>();

            return new List<Saveable>(dataPersistenceObjects);
        }
    }
}
