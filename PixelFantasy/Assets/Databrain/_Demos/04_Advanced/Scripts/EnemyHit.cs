using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Examples
{
    public class EnemyHit : MonoBehaviour
    {
        public DataLibrary data;

        public EnemyUI enemyUI;

        public void OnMouseDown()
        {
            // modify health
            var _runtimeData = data.GetRuntimeDataObjectByGuid(this.gameObject.GetInstanceID().ToString(), typeof(Databrain.Examples.EnemyDataDemo)) as Databrain.Examples.EnemyDataDemo;
            _runtimeData.health--;

            enemyUI.UpdateHealthbar(_runtimeData.health);

            if (_runtimeData.health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}