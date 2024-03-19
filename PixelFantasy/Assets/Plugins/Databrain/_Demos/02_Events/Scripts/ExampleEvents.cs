using Databrain.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Databrain.Events
{
    public class ExampleEvents : DatabrainGenericEvent<ExampleEventsData>{}

    public class ExampleEventsData
    {
        public int parameter1;
        public float parameter2;

        public ExampleEventsData(int parameter1, float parameter2)
        {
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
        }
    }
}