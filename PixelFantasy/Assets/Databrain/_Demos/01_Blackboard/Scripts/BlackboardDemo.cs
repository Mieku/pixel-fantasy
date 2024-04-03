using Databrain.Attributes;
using Databrain.Blackboard;

using UnityEngine;

namespace Databrain.Examples
{
    public class BlackboardDemo : MonoBehaviour
    {
        public DataLibrary data;

        [DataObjectDropdown("data", true)]
        public FloatVariable floatVariable;
        
        private bool dataIsReady;

        public void OnGUI()
        {
            GUI.enabled = dataIsReady;

            if (GUILayout.Button("Modify float value"))
            {
                (floatVariable.GetRuntimeDataObject() as FloatVariable).Value++;
            }
        }

        public void Start()
        {
            data.RegisterInitializationCallback(Ready);
        }

        void Ready()
        {
            dataIsReady = true;
            floatVariable.onValueChanged.UnregisterListener(ValueChanged);
            floatVariable.onValueChanged.RegisterListener(ValueChanged);
        }

        private void ValueChanged(BlackboardVariable obj)
        {
            Debug.Log("value has been changed " + (obj as FloatVariable).Value);
        }
    }
}