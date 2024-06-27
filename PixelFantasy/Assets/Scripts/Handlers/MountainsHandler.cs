using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Controllers;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

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
                statLog += $"{stat.MountainSettings.ResourceName}: {stat.Count} {percent:0.00}%\n";
            }
            Debug.Log(statLog);
        }

        public void DeleteMountains()
        {
            foreach (var mountain in _mountains)
            {
                Destroy(mountain.gameObject);
            }
            
            _mountains.Clear();
            
            _mountainTM.ClearAllTiles();
        }

        public void SpawnMountain(MountainSettings mountainSettings, float x, float y)
        {
            var spawnPosition = new Vector3(x, y, -1);
            var mountain = Instantiate(_mountainPrefab, spawnPosition, Quaternion.identity, transform);
            mountain.InitializeMountain(mountainSettings, spawnPosition);
            mountain.gameObject.name = mountainSettings.ResourceName;
            _mountains.Add(mountain);
        }

        public IEnumerator BatchSpawnMountainsAsync(List<Vector3Int> positions, List<MountainSettings> settings, int batchSize = 100)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < positions.Count; i += batchSize)
            {
                for (int j = 0; j < batchSize && (i + j) < positions.Count; j++)
                {
                    var mountain = Instantiate(_mountainPrefab, transform);
                    mountain.InitializeMountain(settings[i + j], new Vector3(positions[i + j].x + 0.5f, positions[i + j].y + 0.5f, -1));
                    mountain.gameObject.name = settings[i + j].ResourceName;
                    _mountains.Add(mountain);
                }

                yield return null; // Wait for the next frame
            }

            stopwatch.Stop();
            Debug.Log($"Batch spawning {positions.Count} mountains took {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    public class MountainStats
    {
        public MountainSettings MountainSettings;
        public int Count;
    }
}
