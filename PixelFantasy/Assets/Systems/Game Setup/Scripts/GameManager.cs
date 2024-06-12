using System.Collections;
using System.Collections.Generic;
using Characters;
using Controllers;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Items;
using Managers;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
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
        [SerializeField] private int _numStarterKinlings;

        public DataLibrary DataLibrary;
        
        [FormerlySerializedAs("_starterStockpileData")]
        [DataObjectDropdown("DataLibrary")]
        [BoxGroup("Starting Items")] [SerializeField] private StorageSettings _starterStockpileSettings;
        [BoxGroup("Starting Items")] [SerializeField] private List<ItemAmount> _startingItems = new List<ItemAmount>();
        
        [DataObjectDropdown("DataLibrary")]
        [SerializeField] private KinlingData _genericKinlingData;
        [SerializeField] private RaceSettings _race;
        
        private Storage _starterStockpile;

        private void Start()
        {
            //SetUpGame();
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
            
            //LoadStarterStockpile(_worldBuilder.StartPos ,_startingItems);
            
            // Allow frame to render and update UI/loading screen here
            yield return null;
            
            // Wait for a frame after all world-building tasks are complete before updating the NavMesh
            yield return new WaitForEndOfFrame();

            NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
            
            // Again, yield to keep the UI responsive
            yield return null;
            
            LoadStarterKinlings(_worldBuilder.StartPos, _numStarterKinlings);
            yield return null;
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            Vector2 lookPos = new Vector2
            {
                x = _worldBuilder.StartPos.x,
                y = _worldBuilder.StartPos.y
            };
            CameraManager.Instance.LookAtPosition(lookPos);
            yield return null;
        }
        
        private void LoadStarterKinlings(Vector3Int startCell, int amount)
        {
            Vector2 startPos = new Vector2(startCell.x, startCell.y);
            for (int i = 0; i < amount; i++)
            {
                DataLibrary.RegisterInitializationCallback(() =>
                {
                    var kinlingData = (KinlingData)DataLibrary.CloneDataObjectToRuntime(_genericKinlingData);
                    kinlingData.Randomize(_race);
                    kinlingData.title = kinlingData.Fullname;
                    kinlingData.name = $"{kinlingData.Fullname}_{kinlingData.guid}";
                    var pos = Helper.RandomLocationInRange(startPos);
                    kinlingData.Mood.JumpMoodToTarget();
                    KinlingsManager.Instance.SpawnKinling(kinlingData, pos);
                });
            }
        }

        public List<KinlingData> GenerateNewKinlings(int amount)
        {
            List<KinlingData> results = new List<KinlingData>();
            for (int i = 0; i < amount; i++)
            {
                DataLibrary.RegisterInitializationCallback(() =>
                {
                    var kinlingData = (KinlingData)DataLibrary.CloneDataObjectToRuntime(_genericKinlingData);
                    kinlingData.Randomize(_race);
                    kinlingData.title = kinlingData.Fullname;
                    kinlingData.name = $"{kinlingData.Fullname}_{kinlingData.guid}";
                    kinlingData.Mood.JumpMoodToTarget();
                    AppearanceBuilder.Instance.UpdateAppearance(kinlingData);
                    results.Add(kinlingData);
                });
            }

            return results;
        }
    }
}
