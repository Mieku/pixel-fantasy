using Databrain.Attributes;
using UnityEngine;

namespace Databrain.Examples
{
    public class EnemyDataComponent : MonoBehaviour
    {
        public EnemyUI enemyUI;

        public DataLibrary data;

        [DataObjectDropdown("data")]
        public EnemyDataDemo enemyData;

        private EnemyDataDemo runtimeData;
        private bool dataReady = false;

       

        // Called after loading by the AdvancedDemoManager script
        public void Initialize(string _id)
        {
            // Get runtime data object by provided id
            runtimeData = data.GetRuntimeDataObjectByGuid(_id, typeof(Databrain.Examples.EnemyDataDemo)) as EnemyDataDemo;

            // We can now update the runtime clone id by the new gameobject instance id.
            runtimeData.SetNewGuid(this.gameObject.GetInstanceID().ToString());

            enemyUI.UpdateHealthbar(runtimeData.health);

            dataReady = true;
        }

        // Called once on object creation
        public void OnSpawned()
        {
            // Clone and create new runtime object
            runtimeData = data.CloneDataObjectToRuntime(enemyData, this.gameObject) as EnemyDataDemo;

            dataReady = true;
        }

        public void OnDestroy()
        {
            // Make sure to remove the runtime data object
            data.RemoveDataObjectFromRuntime(runtimeData);
        }


        void LateUpdate()
        {
            // wait till data is ready
            if (!dataReady)
                return;

            runtimeData.position = transform.position;
        }
    }
}