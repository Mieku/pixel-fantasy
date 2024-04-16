using System;
using UnityEngine;
using static InfinityPBR.Modules.Timeboard;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/blackboard")]
    [Serializable]
    public class BlackboardEvent
    {
        /*
         * Remember, you don't have to use the topic, gameId, and status strings for those specific purposes. They can
         * be used creatively to pass more data around your project.
         */
        
        public string topic; // May often be used to describe the gameUid -- i.e. "ItemObject" or "Stat" or something else you've created
        public string gameId; // Unique in-game identifier of the obj
        public string status; // Describes the event that has occurred to the topic
        public object obj; // The object itself
        public float gameTimeNow; // Game Time Module
        
        /// <summary>
        /// Creates a new BlackboardEvent
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="gameId"></param>
        /// <param name="status"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static BlackboardEvent Create(string topic, string gameId, string status, object obj) => 
            new BlackboardEvent {topic = topic,
                gameId = gameId, 
                status = status, 
                obj = obj,
                gameTimeNow = timeboard.gametime.Now()
            };
        
        public BlackboardEvent Clone() => JsonUtility.FromJson<BlackboardEvent>(JsonUtility.ToJson(this));
    }
}