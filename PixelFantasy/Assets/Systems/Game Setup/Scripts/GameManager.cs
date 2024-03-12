using System.Collections;
using System.Collections.Generic;
using Characters;
using Controllers;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Buildings.Scripts;
using Systems.World_Building.Scripts;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private WorldBuilder _worldBuilder;
        [SerializeField] private bool _generateWorldOnStart;

        public DataLibrary DataLibrary;
        
        [DataObjectDropdown("DataLibrary")]
        [BoxGroup("Starting Items")] [SerializeField] private StorageData _starterStockpileData;
        
        [BoxGroup("Starting Items")] [SerializeField] private Storage _starterStockpile;
        [BoxGroup("Starting Items")] [SerializeField] private List<ItemAmount> _startingItems = new List<ItemAmount>();
        
        [BoxGroup("Starting Kinlings")] [SerializeField] private List<KinlingData> _starterKinlings;

        private void Start()
        {
            SetUpGame();
        }

        [Button("Set Up Game")]
        public void SetUpGame()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Game must be playing to set up game");
                return;
            }

            StartCoroutine(SetUpGameCoroutine());
            
            
            StructureManager.Instance.Init(_worldBuilder.WorldSize);
        }
        
        public IEnumerator SetUpGameCoroutine()
        {
            if (_generateWorldOnStart)
            {
                yield return StartCoroutine(_worldBuilder.GeneratePlaneCoroutine());
            }
            
            LoadStarterStockpile(_startingItems);
            
            // Allow frame to render and update UI/loading screen here
            yield return null;
            
            // Wait for a frame after all world-building tasks are complete before updating the NavMesh
            yield return new WaitForEndOfFrame();

            NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
            
            // Again, yield to keep the UI responsive
            yield return null;
            
            LoadStarterKinlings(_starterKinlings);
            yield return null;
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            CameraManager.Instance.LookAtPosition(_starterStockpile.transform.position);
            yield return null;
        }

        private void LoadStarterStockpile(List<ItemAmount> preloadedItems)
        {
            //_starterStockpile.Init(_starterStockpileData);
            // _starterStockpile.SetState(EFurnitureState.Built);
            //
            // foreach (var preloadedItemAmount in preloadedItems)
            // {
            //     for (int i = 0; i < preloadedItemAmount.Quantity; i++)
            //     {
            //         var spawnedItem = Spawner.Instance.SpawnItem(preloadedItemAmount.Item,
            //             _starterStockpile.transform.position, false);
            //         _starterStockpile.ForceDepositItem(spawnedItem.Data);
            //     }
            // }
        }
        
        private void LoadStarterKinlings(List<KinlingData> starterKinlings)
        {
            List<Kinling> spawnedKinlings = new List<Kinling>();
            
            // Spawn First
            foreach (var kinling in starterKinlings)
            {
                var pos = Helper.RandomLocationInRange(_starterStockpile.transform.position);
                var spawnedKinling = Spawner.Instance.SpawnKinling(kinling, pos, false);
                spawnedKinlings.Add(spawnedKinling);
            }

            // Load Data
            foreach (var kinling in spawnedKinlings)
            {
                var data = starterKinlings.Find(kinlingData => kinlingData.UID == kinling.UniqueId);
                kinling.SetKinlingData(data);
            }
        }
    }
}
