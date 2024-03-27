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
using UnityEngine.Serialization;

namespace Systems.Game_Setup.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private WorldBuilder _worldBuilder;
        [SerializeField] private bool _generateWorldOnStart;

        public DataLibrary DataLibrary;
        
        [FormerlySerializedAs("_starterStockpileData")]
        [DataObjectDropdown("DataLibrary")]
        [BoxGroup("Starting Items")] [SerializeField] private StorageSettings _starterStockpileSettings;
        [BoxGroup("Starting Items")] [SerializeField] private List<ItemAmount> _startingItems = new List<ItemAmount>();
        
        [DataObjectDropdown("DataLibrary")]
        [BoxGroup("Starting Kinlings")] [SerializeField] private List<KinlingData> _starterKinlings;
        
        private Storage _starterStockpile;

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
            
            LoadStarterStockpile(_worldBuilder.StartPos ,_startingItems);
            
            // Allow frame to render and update UI/loading screen here
            yield return null;
            
            // Wait for a frame after all world-building tasks are complete before updating the NavMesh
            yield return new WaitForEndOfFrame();

            NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
            
            // Again, yield to keep the UI responsive
            yield return null;
            
            LoadStarterKinlings(_worldBuilder.StartPos, _starterKinlings);
            yield return null;
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            CameraManager.Instance.LookAtPosition(_starterStockpile.transform.position);
            yield return null;
        }

        private void LoadStarterStockpile(Vector3Int startCell, List<ItemAmount> preloadedItems)
        {
            Vector2 startPos = new Vector2(startCell.x, startCell.y);

            _starterStockpile = (Storage)Spawner.Instance.SpawnFurniture(_starterStockpileSettings.FurniturePrefab, startPos);
            
            // Create the item datas
            List<ItemData> datas = new List<ItemData>();
            foreach (var preloadedItemAmount in preloadedItems)
            {
                for (int i = 0; i < preloadedItemAmount.Quantity; i++)
                {
                    var data = preloadedItemAmount.Item.CreateInitialDataObject();
                    var item = (ItemData) DataLibrary.CloneDataObjectToRuntime(data);
                    item.InitData(preloadedItemAmount.Item);
                    datas.Add(item);
                }
            }
            
            _starterStockpile.ForceLoadItems(datas, _starterStockpileSettings);
        }
        
        private void LoadStarterKinlings(Vector3Int startCell, List<KinlingData> starterKinlings)
        {
            //List<Kinling> spawnedKinlings = new List<Kinling>();
            Vector2 startPos = new Vector2(startCell.x, startCell.y);
            
            // Spawn First
            foreach (var kinling in starterKinlings)
            {
                var pos = Helper.RandomLocationInRange(startPos);
                KinlingsManager.Instance.SpawnKinling(kinling, pos);
                // var kinlingData = (KinlingData)DataLibrary.CloneDataObjectToRuntime(GenericKinlingData, gameObject);
                // var pos = Helper.RandomLocationInRange(startPos);
                // var spawnedKinling = Spawner.Instance.SpawnKinling(kinling, pos, false);
                // spawnedKinling.SetKinlingData(kinling);
                // spawnedKinlings.Add(spawnedKinling);
            }
        }
    }
}
