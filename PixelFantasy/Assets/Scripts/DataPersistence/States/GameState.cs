using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataPersistence.States
{
    [Serializable]
    public class GameState
    {
        public Dictionary<string, object> States = new Dictionary<string, object>();
    }
}
