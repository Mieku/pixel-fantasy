using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Examples
{

    public class AdvancedDemoManager : MonoBehaviour
    {
        public DataLibrary data;

        public GameObject enemy;

        private List<GameObject> enemyList = new List<GameObject>();
        private bool loadingDone = false;
        

        void Start()
        {
            enemyList = new List<GameObject>();
            data.OnLoaded += Loaded;
        }

        void OnDestroy()
        {
            data.OnLoaded -= Loaded;
        }

        public void ClearScene()
        {
            // Destroy all enemy objects
            for (int i = 0; i < enemyList.Count; i++)
            {
                Destroy(enemyList[i]);
            }
        }

        void Loaded()
        {
            if (loadingDone)
                return;


            loadingDone = true;

            // Rebuild enemies
            enemyList = new List<GameObject>();

            // First get all loaded enemy objects
            var _enemies = data.GetAllRuntimeDataObjectsByType(typeof(Databrain.Examples.EnemyDataDemo));

            // Iterate through the list and instantiate new enemy objects
            for (int i = 0; i < _enemies.Count; i++)
            {
                var _enemy = Instantiate(enemy, (_enemies[i] as Databrain.Examples.EnemyDataDemo).position, Quaternion.identity);

                enemyList.Add(_enemy);

                _enemy.GetComponent<EnemyDataComponent>().Initialize(_enemies[i].guid);
            }

            loadingDone = false;
        }


        public void SpawnEnemy(int _count)
        {
            for (int i = 0; i < _count; i++)
            {
                var _enemy = Instantiate(enemy, new Vector3(0, 1, 0), Quaternion.identity);

                enemyList.Add(_enemy);

                _enemy.GetComponent<EnemyDataComponent>().OnSpawned();
            }
        }
    }
}