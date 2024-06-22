using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using Systems.Social.Scripts;
using Systems.World_Building.Scripts;
using TWC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Systems.Game_Setup.Scripts
{
    public class GameManager : PersistentSingleton<GameManager>
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
        
        private void Start()
        {
            //SetUpGame();
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public void StartNewGame(List<KinlingData> starterKinlings, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            StartCoroutine(LoadSceneAndSetUpNewGame(starterKinlings, blueprintLayers));
        }

        private IEnumerator LoadSceneAndSetUpNewGame(List<KinlingData> starterKinlings, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            // Start loading the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

            Debug.Log("Async is started");

            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Debug.Log("Async is done");

            // Now the scene is loaded, find the WorldBuilder
            _worldBuilder = FindObjectOfType<WorldBuilder>();

            if (_worldBuilder == null)
            {
                Debug.LogError("WorldBuilder not found in the loaded scene.");
                yield break;
            }

            // Start the coroutine to set up the new game
            StartCoroutine(SetUpNewGameCoroutine(starterKinlings, blueprintLayers));

            // Initialize the StructureManager with the WorldBuilder's WorldSize
            Vector2Int worldSize = new Vector2Int(36, 36);
            StructureManager.Instance.Init(worldSize);
        }
        
        public IEnumerator SetUpNewGameCoroutine(List<KinlingData> starterKinling, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            if (_generateWorldOnStart)
            {
                yield return StartCoroutine(_worldBuilder.GeneratePlaneCoroutine(blueprintLayers));
            }
            
            // Allow frame to render and update UI/loading screen here
            yield return null;
            
            // Wait for a frame after all world-building tasks are complete before updating the NavMesh
            yield return new WaitForEndOfFrame();

            NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
            
            // Again, yield to keep the UI responsive
            yield return null;
            
            ApplyStarterKinlings(_worldBuilder.StartPos, starterKinling);
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
        
        // public IEnumerator SetUpGameCoroutine()
        // {
        //     if (_generateWorldOnStart)
        //     {
        //         yield return StartCoroutine(_worldBuilder.GeneratePlaneCoroutine());
        //     }
        //     
        //     // Allow frame to render and update UI/loading screen here
        //     yield return null;
        //     
        //     // Wait for a frame after all world-building tasks are complete before updating the NavMesh
        //     yield return new WaitForEndOfFrame();
        //
        //     NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
        //     
        //     // Again, yield to keep the UI responsive
        //     yield return null;
        //     
        //     LoadStarterKinlings(_worldBuilder.StartPos, _numStarterKinlings);
        //     yield return null;
        //     
        //     GameEvents.Trigger_RefreshInventoryDisplay();
        //     
        //     Vector2 lookPos = new Vector2
        //     {
        //         x = _worldBuilder.StartPos.x,
        //         y = _worldBuilder.StartPos.y
        //     };
        //     CameraManager.Instance.LookAtPosition(lookPos);
        //     yield return null;
        // }

        private void ApplyStarterKinlings(Vector3Int startCell, List<KinlingData> starterKinlings)
        {
            Vector2 startPos = new Vector2(startCell.x, startCell.y);
            foreach (var kinling in starterKinlings)
            {
                var pos = Helper.RandomLocationInRange(startPos);
                KinlingsManager.Instance.SpawnKinling(kinling, pos);
            }
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
            
            GenerateNewRelationships(results);
            
            return results;
        }

        public void GenerateNewRelationships(List<KinlingData> kinlings)
        {
            // Clear any current relationships
            foreach (var kinling in kinlings)
            {
                kinling.Partner = null;
                kinling.Relationships.Clear();
            }
            
            // Generate Relationships
            int numPartners = Random.Range(0, kinlings.Count / 2 + 1);
            
            // Partners
            List<KinlingData> potentialPartners = kinlings.ToList();
            int infiniteWatch = 0;
            for (int i = 0; i < numPartners; i++)
            {
                infiniteWatch++;
                if (infiniteWatch > 100) break;
                
                var randomKinling = potentialPartners[Random.Range(0, potentialPartners.Count)];
                potentialPartners.Remove(randomKinling);

                List<KinlingData> potentialMatches = new List<KinlingData>();
                foreach (var potentialPartner in potentialPartners)
                {
                    if (randomKinling.IsKinlingAttractedTo(potentialPartner) && potentialPartner.IsKinlingAttractedTo(randomKinling) && potentialPartner.Partner == null)
                    {
                        potentialMatches.Add(potentialPartner);
                    }
                }

                if (potentialMatches.Count == 0)
                {
                    potentialPartners.Add(randomKinling);
                    i--;
                    continue;
                }
                
                var match = potentialMatches[Random.Range(0, potentialMatches.Count)];
                if (match != null)
                {
                    potentialPartners.Remove(match);
                    RelationshipData relationship = randomKinling.Relationships.Find(r => r.KinlingData == match);
                    if (relationship == null)
                    {
                        relationship = new RelationshipData(randomKinling, match);
                    }
                     
                    RelationshipData theirRelationship = match.Relationships.Find(r => r.KinlingData == randomKinling);
                    if (theirRelationship == null)
                    {
                        theirRelationship = new RelationshipData(match, randomKinling);
                    }

                    relationship.IsPartner = true;
                    relationship.Opinion = Random.Range(10, 50);
                    randomKinling.Partner = match;
                    randomKinling.Relationships.Add(relationship);
                    
                    theirRelationship.IsPartner = true;
                    theirRelationship.Opinion = Random.Range(10, 50);
                    match.Partner = randomKinling;
                    match.Relationships.Add(theirRelationship);
                }
            }

            foreach (var kinling in kinlings)
            {
                foreach (var otherKinling in kinlings)
                {
                    if (kinling != otherKinling)
                    {
                        RelationshipData relationship = kinling.Relationships.Find(r => r.KinlingData == otherKinling);
                        if (relationship == null)
                        {
                            relationship = new RelationshipData(kinling, otherKinling)
                            {
                                Opinion = Random.Range(-10, 30)
                            };
                            kinling.Relationships.Add(relationship);
                        }
                    }
                }
            }
        }
    }
}
