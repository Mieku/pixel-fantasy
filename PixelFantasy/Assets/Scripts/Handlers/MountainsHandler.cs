using System.Collections.Generic;
using Controllers;
using DataPersistence;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Handlers
{
    public class MountainsHandler : Saveable
    {
        [SerializeField] private Mountain _mountainPrefab;
        
        protected override string StateName => "Mountains";
        public override int LoadOrder => 1;

        private List<Mountain> _mountains = new List<Mountain>();
        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);

        [Button("Debug Mountain Stats")]
        public void Debug_MountainStats()
        {
            List<MountainStats> stats = new List<MountainStats>();
            foreach (var mountain in _mountains)
            {
                var stat = stats.Find(mount => mount.Data == mountain.ResourceData as MountainData);
                if (stat == null)
                {
                    stat = new MountainStats();
                    stat.Data = mountain.ResourceData as MountainData;
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
                statLog += $"{stat.Data.ResourceName}: {stat.Count} {percent:0.00}%\n";
            }
            Debug.Log(statLog);
        }

        public void DeleteChildren()
        {
            foreach (var mountain in _mountains)
            {
                DestroyImmediate(mountain.gameObject);
            }
            _mountains.Clear();
            
            _mountainTM.ClearAllTiles();
        }

        public void SpawnMountain(MountainData mountainData, float x, float y)
        {
            var spawnPosition = new Vector3(x, y, -1);
            var mountain = Instantiate(_mountainPrefab, spawnPosition, Quaternion.identity, transform);
            mountain.Init(mountainData);
            mountain.gameObject.name = mountainData.ResourceName;
            _mountains.Add(mountain);
        }

        protected override void ClearChildStates(List<object> childrenStates)
        {
            // Delete current persistent children
            var currentChildren = GetPersistentChildren();
            foreach (var child in currentChildren)
            {
                child.GetComponent<UID>().RemoveUID();
            }
            
            foreach (var child in currentChildren)
            {
                DestroyImmediate(child);
            }
            currentChildren.Clear();
        }
        
        protected override void SetChildStates(List<object> childrenStates)
        {
            // Instantiate all the children in data, Trigger RestoreState with their state data
            foreach (var childState in childrenStates)
            {
                var resourceData = (Mountain.State)childState;
                var childObj = Instantiate(_mountainPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(resourceData);
            }
        }
    }
    
    public class MountainStats
    {
        public MountainData Data;
        public int Count;
    }
}
