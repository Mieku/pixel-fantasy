using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Debugs;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    public class  QuestEventBoard : MonoBehaviour
    {
        public static QuestEventBoard questEventBoard; // Static reference to this script
        
        public Stack<QuestEvent> questEvents = new Stack<QuestEvent>();
        public List<IFollowQuests> followers = new List<IFollowQuests>();

        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.50f, 0.20f, 0.5f);
        
        private void Awake()
        {
            if (!questEventBoard)
                questEventBoard = this;
            else if (questEventBoard != this)
                Destroy(gameObject);
        }
        
        private void Update()
        {
            if (questEvents.Count == 0) return;
            Notify(questEvents.Pop());
        }
        
        public void AddEvent(QuestEvent questEvent)
        {
            WriteToConsole("Add QuestEvent " +
                           $"[Quest Game ID: {questEvent.questGameId}], " +
                           $"[GameQuest: {questEvent.gameQuest.objectName}], " +
                           $"[QuestStep: {(questEvent.questStep == null ? "" : questEvent.questStep.name)}], " +
                           $"[Status: {questEvent.status.ToString()}]"
                , "QuestEventBoard", writeToConsoleColor, writeToConsole, false, gameObject);
            questEvents.Push(questEvent);
        }

        public void AddEvent(GameQuest gameQuest, QuestStep.QuestStepStatus status)
        {
            AddEvent(QuestEvent.Create(gameQuest, status));
        }

        public void Subscribe(IFollowQuests follower)
        {
            WriteToConsole($"{follower} has followed QuestEventBoard events!"
                , "QuestEventBoard", writeToConsoleColor, writeToConsole, false, gameObject);
            followers.Add(follower);
        }

        public void Unsubscribe(IFollowQuests follower)
        {
            followers.Remove(follower);
        }

        private void Notify(QuestEvent questEvent)
        {
            WriteToConsole("Pushing QuestEvent " +
                           $"[Quest Game ID: {questEvent.questGameId}], " +
                           $"[GameQuest: {questEvent.gameQuest.objectName}], " +
                           $"[QuestStep: {(questEvent.questStep == null ? "" : questEvent.questStep.name)}], " +
                           $"[Status: {questEvent.status.ToString()}]"
                , "QuestEventBoard", writeToConsoleColor, writeToConsole, false, gameObject);
            // December 26, 2022. This is the original. However, if you Unsubscribe after getting the event, it throws
            // an error, as the list size has changed.
            // followers.ForEach(follower => follower.Receive(questEvent));
            
            // This is the version that Chat GPT gave me. It suggested first doing a normal for loop, but pointed out that
            // the error will not happen, but not all followers may be notified if the size changes. So it suggested this, 
            // which (as it pointed out) is "a bit more expensive in terms of performance" because it creates a new list.
            // Since this Notify() method will not likely be called very often, it should be fine. However, if you feel
            // that this is not the case, i.e. you've found this while trying to optimize, come to the Discord and let me
            // know!
            foreach (var follower in followers.ToList())
                follower.ReceiveQuestEvent(questEvent);
        }
    }

    [Serializable]
    public class QuestEvent
    {
        public string questGameId; // Reference to gameQuest.GameUid which is the unique id for the in-game quest. Each GameQuest will have a unique id
        public QuestStep.QuestStepStatus status;
        public object obj;
        public QuestStep questStep;
        public GameQuest gameQuest;

        public static QuestEvent Create(GameQuest gameQuest, QuestStep.QuestStepStatus status, QuestStep questStep = null) => 
            new QuestEvent {questGameId = gameQuest.GameId(), 
                status = status,
                gameQuest = gameQuest,
                questStep = questStep
            };
        
        // To be used later. Ask Jason Storey if you're confused.
        public static QuestEvent Create(GameQuest gameQuest, object obj, QuestStep.QuestStepStatus status, QuestStep questStep = null) => 
            new QuestEvent {questGameId = gameQuest.GameId(), 
                status = status, 
                obj = obj, 
                gameQuest = gameQuest,
                questStep =  questStep
            };
    }

    public interface IFollowQuests
    {
        void ReceiveQuestEvent(QuestEvent questEvent);
    }
}
