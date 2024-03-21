using System;
using System.Collections.Generic;
using Controllers;
using Data.Resource;
using DataPersistence;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Handlers
{
    public class MountainsHandler : MonoBehaviour
    {
        [SerializeField] private Mountain _mountainPrefab;

        private List<Mountain> _mountains = new List<Mountain>();

        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);

        [Button("Debug Mountain Stats")]
        public void Debug_MountainStats()
        {
            List<MountainStats> stats = new List<MountainStats>();
            foreach (var mountain in _mountains)
            {
                var stat = stats.Find(mount => mount.MountainSettings == mountain.RuntimeMountainData.MountainSettings);
                if (stat == null)
                {
                    stat = new MountainStats();
                    stat.MountainSettings = mountain.RuntimeMountainData.MountainSettings;
                    stat.Count = 1;
                    stats.Add(stat);
                }
                else
                {
                    stat.Count++;
                }
            }
            
            float total = _mountains.Count;
            string statLog = "<b>Mountain Distribution Stats</b>\n";
            foreach (var stat in stats)
            {
                var percent = (stat.Count / total) * 100f;
                statLog += $"{stat.MountainSettings.title}: {stat.Count} {percent:0.00}%\n";
            }
            Debug.Log(statLog);
        }

        public void DeleteMountains()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                // Destroy immediate to ensure the object is removed instantly
                children.Add(child.gameObject);
            }

            foreach (var child in children)
            {
                DestroyImmediate(child);
            }
            
            _mountains.Clear();
            
            _mountainTM.ClearAllTiles();
        }

        public void SpawnMountain(MountainSettings mountainSettings, float x, float y)
        {
            var spawnPosition = new Vector3(x, y, -1);
            var mountain = Instantiate(_mountainPrefab, spawnPosition, Quaternion.identity, transform);
            mountain.InitializeResource(mountainSettings);
            mountain.gameObject.name = mountainSettings.title;
            _mountains.Add(mountain);
        }
    }
    
    public class MountainStats
    {
        public MountainSettings MountainSettings;
        public int Count;
    }
}
