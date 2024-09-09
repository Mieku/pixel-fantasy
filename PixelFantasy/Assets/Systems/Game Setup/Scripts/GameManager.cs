using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Controllers;
using DataPersistence;
using Managers;
using Player;
using ScriptableObjects;
using Systems.Appearance.Scripts;
using Systems.Buildings.Scripts;
using Systems.Notifications.Scripts;
using Systems.Social.Scripts;
using Systems.World_Building.Scripts;
using TWC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Systems.Game_Setup.Scripts
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public GameData GameData;
        public bool GameIsQuitting;
        public bool GameIsLoaded;
        
        [SerializeField] private InputActionReference _takeScreenShotInputAction;

        public int RandomSeedSalt => Time.frameCount;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            if (_takeScreenShotInputAction != null)
            {
                _takeScreenShotInputAction.action.performed += TakeScreenShot;
            }
        }

        private void OnDestroy()
        {
            if(GameIsQuitting) return;

            if (_takeScreenShotInputAction != null)
            {
                _takeScreenShotInputAction.action.performed -= TakeScreenShot;
            }
        }

        private void Start()
        {
            GameSettings.Instance.RefreshCaches();
            PlayerSettings.LoadSavedKeyBinds();
        }

        private void Update()
        {

        }
        
        private void TakeScreenShot(InputAction.CallbackContext ctx)
        {
            var screenshotter = FindObjectOfType<ScreenshotController>();
            if (screenshotter != null)
            {
                screenshotter.TakeScreenshot();
                
                NotificationManager.Instance?.Toast("Screenshot taken");
            }
        }

        public void StartNewGame(string settlementName, List<KinlingData> starterKinlings, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            GameIsLoaded = false;
            GameData = new GameData();
            GameData.SettlementName = settlementName;
            
            StartCoroutine(LoadSceneAndSetUpNewGame(starterKinlings, blueprintLayers));
        }

        public void StartLoadedGame(SaveData saveData, bool loadScene)
        {
            GameIsLoaded = false;
            StartCoroutine(LoadSceneAndContinueLoad(saveData, loadScene));
        }
        
        public IEnumerator LoadSceneAndContinueLoad(SaveData saveData, bool loadScene)
        {
            LoadingScreen.Instance.Show("Generating World", "Initializing...", 13);
            yield return new WaitForEndOfFrame();
            
            // Start loading the scene
            if (loadScene)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            
                // Wait until the scene is fully loaded
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
                LoadingScreen.Instance.StepCompleted();
            
                yield return new WaitForEndOfFrame();
            }
            
            Vector2Int worldSize = new Vector2Int(36, 36);
            StructureDatabase.Instance.Init(worldSize);
            bool gameIsLoaded = false;
            
            yield return StartCoroutine(DataPersistenceManager.Instance.LoadGameCoroutine(saveData, () =>
            {
                // Load completed
                gameIsLoaded = true;
                LoadingScreen.Instance.StepCompleted();
            }, (step) =>
            {
                // Step started
                LoadingScreen.Instance.SetLoadingInfoText(step);
            }, (step) =>
            {
                // Step completed
                LoadingScreen.Instance.StepCompleted();
            }));

            while (!gameIsLoaded)
            {
                yield return null;
            }
            
            // Wait for a frame after all world-building tasks are complete before updating the NavMesh
            yield return new WaitForEndOfFrame();

            NavMeshManager.Instance.UpdateNavMesh(forceRebuild: true);
            LoadingScreen.Instance.StepCompleted();
            GameIsLoaded = true;
            
            // Again, yield to keep the UI responsive
            yield return null;
            
            LoadingScreen.Instance.Hide();
        }

        private IEnumerator LoadSceneAndSetUpNewGame(List<KinlingData> starterKinlings, List<TileWorldCreatorAsset.BlueprintLayerData> blueprintLayers)
        {
            LoadingScreen.Instance.Show("Generating World", "Creating New Game...", 13);
            yield return new WaitForEndOfFrame();
            
            // Start loading the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Now the scene is loaded, find the WorldBuilder
            var worldBuilder = FindObjectOfType<WorldBuilder>();
            
            if (worldBuilder == null)
            {
                Debug.LogError("WorldBuilder not found in the loaded scene.");
                yield break;
            }

            StartCoroutine(worldBuilder.SetUpNewGameCoroutine(starterKinlings, blueprintLayers));

            // Initialize the StructureManager with the WorldBuilder's WorldSize
            Vector2Int worldSize = new Vector2Int(36, 36);
            StructureDatabase.Instance.Init(worldSize);
            
            GameEvents.Trigger_OnGameLoadComplete();
        }

        public void GoToMainMenu()
        {
            StartCoroutine(DataPersistenceManager.Instance.ClearWorld());
            GameData = new GameData();
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Application.Quit();

            // If you are running the game in the editor, this line will stop playing the scene
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public List<KinlingData> GenerateNewKinlings(int amount, RaceSettings race)
        {
            Random.InitState(RandomSeedSalt);
            List<KinlingData> results = new List<KinlingData>();
            for (int i = 0; i < amount; i++)
            {
                var kinlingData = new KinlingData();
                kinlingData.Randomize(race);
                kinlingData.Mood.JumpMoodToTarget();
                AppearanceBuilder.Instance.UpdateAppearance(kinlingData);
                results.Add(kinlingData);
            }
            
            GenerateNewRelationships(results);
            
            return results;
        }

        public void GenerateNewRelationships(List<KinlingData> kinlings)
        {
            // Clear any current relationships
            foreach (var kinling in kinlings)
            {
                kinling.PartnerUID = null;
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
                    if (randomKinling.IsKinlingAttractedTo(potentialPartner) && potentialPartner.IsKinlingAttractedTo(randomKinling) && string.IsNullOrEmpty(potentialPartner.PartnerUID))
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
                    RelationshipData relationship = randomKinling.Relationships.Find(r => r.OthersUID == match.UniqueID);
                    if (relationship == null)
                    {
                        relationship = new RelationshipData(randomKinling, match);
                    }
                     
                    RelationshipData theirRelationship = match.Relationships.Find(r => r.OthersUID == randomKinling.UniqueID);
                    if (theirRelationship == null)
                    {
                        theirRelationship = new RelationshipData(match, randomKinling);
                    }

                    relationship.IsPartner = true;
                    relationship.Opinion = Random.Range(10, 50);
                    randomKinling.PartnerUID = match.UniqueID;
                    randomKinling.Relationships.Add(relationship);
                    
                    theirRelationship.IsPartner = true;
                    theirRelationship.Opinion = Random.Range(10, 50);
                    match.PartnerUID = randomKinling.UniqueID;
                    match.Relationships.Add(theirRelationship);
                }
            }

            foreach (var kinling in kinlings)
            {
                foreach (var otherKinling in kinlings)
                {
                    if (kinling != otherKinling)
                    {
                        RelationshipData relationship = kinling.Relationships.Find(r => r.OthersUID == otherKinling.UniqueID);
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

        private void OnApplicationQuit()
        {
            GameIsQuitting = true;
        }
    }
}
