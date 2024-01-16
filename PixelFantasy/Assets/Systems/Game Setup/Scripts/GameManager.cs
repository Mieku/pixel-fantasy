using System;
using System.Collections.Generic;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Game_Setup.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
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
            
            LoadStarterStockpile(_startingItems);
            LoadStarterKinlings(_starterKinlings);
        }

        private void LoadStarterStockpile(List<ItemAmount> preloadedItems)
        {
            foreach (var preloadedItemAmount in preloadedItems)
            {
                for (int i = 0; i < preloadedItemAmount.Quantity; i++)
                {
                    var spawnedItem = Spawner.Instance.SpawnItem(preloadedItemAmount.Item,
                        _starterStockpile.transform.position, false);
                    _starterStockpile.ForceDepositItem(spawnedItem);
                }
            }
        }

        private void LoadStarterKinlings(List<KinlingData> starterKinlings)
        {
            foreach (var kinling in starterKinlings)
            {
                var pos = Helper.RandomLocationInRange(_starterStockpile.transform.position);
                Spawner.Instance.SpawnKinling(kinling, pos);
            }
        }
    }
}
