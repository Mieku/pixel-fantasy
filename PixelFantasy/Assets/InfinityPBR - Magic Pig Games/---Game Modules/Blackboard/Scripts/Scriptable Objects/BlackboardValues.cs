using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/blackboard")]
    [CreateAssetMenu(fileName = "BlackboardValues", menuName = "Game Modules/Create/Blackboard Values", order = 1)]
    [Serializable]
    public class BlackboardValues : ModulesScriptableObject, IHaveUid
    {
        public List<BlackboardNote> startingNotes = new List<BlackboardNote>();

        public void PassValuesTo(Blackboard blackboard) 
            => startingNotes.ForEach(x => blackboard.AddNote(x.Clone()));
    }
}
