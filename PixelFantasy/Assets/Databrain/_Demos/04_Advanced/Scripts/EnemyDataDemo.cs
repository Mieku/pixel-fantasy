using Databrain.Attributes;
using UnityEngine;


namespace Databrain.Examples
{
    public class EnemyDataDemo : AdvancedDemoDataBase
    {
        [ExposeToInspector]
        [DatabrainSerialize]
        public int health;
        [ExposeToInspector]
        [DatabrainSerialize]
        public Vector3 position;
    }
}