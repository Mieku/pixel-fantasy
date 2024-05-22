using Databrain;
using Databrain.Attributes;
using Databrain.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Examples
{

    public class EventManager : MonoBehaviour
    {

        public DataLibrary dataLibrary;

        [DataObjectDropdown("dataLibrary")]
        public ExampleEvents event1;

        [DataObjectDropdown("dataLibrary")]
        public ExampleEvents event2;


        public void OnGUI()
        {
            if (GUILayout.Button("CALL EVENT"))
            {
                event1.Raise(new ExampleEventsData(0, 0.1f));
            }

            if (GUILayout.Button("CALL EVENT 2"))
            {
                event2.Raise(new ExampleEventsData(10, 0.1f));
            }
        }


        private void Start()
        {

            event1.RegisterListener(OnEvent);
            event2.RegisterListener(OnEvent2);
        }

        private void OnDisable()
        {
            event1.UnregisterListener(OnEvent);
            event2.UnregisterListener(OnEvent2);
        }

        void OnGlobalEvent()
        {
            Debug.Log("global event has been called");
        }

        void OnEvent(ExampleEventsData _data)
        {
            Debug.Log("event 1 action has been raised");
        }

        void OnEvent2(ExampleEventsData _data)
        {
            Debug.Log("event 2 action has been raised");
        }
    }
}