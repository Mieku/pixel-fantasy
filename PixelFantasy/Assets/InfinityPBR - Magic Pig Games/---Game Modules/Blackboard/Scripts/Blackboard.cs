using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Debugs;
using static InfinityPBR.Modules.Timeboard;

/*
 * I suggest you create a new Blackboard class which inherits from this, so you can add your own custom
 * values. See BlackboardQuestsDemo.cs for an example from the Quests demo.
 *
 * NOTE: The "MainBlackboard" Prefab should be used if you'd like a static reference to a singular blackboard for
 * general use purposes, which can be called with Blackboard.blackboard. Only one MainBlackboard can exist.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/blackboard")]
    [Serializable]
    public class Blackboard : MonoBehaviour, ISaveable
    {

        // Setup
        public List<BlackboardValues> startingValues = new List<BlackboardValues>();

        // Things we track with this board
        public Dictionaries dictionaries; // The Game Modules dictionaries, useful for run-time (non serialized) values
        public List<BlackboardNote> notes = new List<BlackboardNote>(); // Useful for general data, serializable
        
        // These hold a record of events and notes posted.
        public List<BlackboardNote> postedNotes = new List<BlackboardNote>();
        public List<BlackboardEvent> postedEvents = new List<BlackboardEvent>();
        
        [Header("Debug Options")]
        public bool writeToConsole;
        public Color writeToConsoleColor = new Color(0.20f, 0.20f, 0.6f);
        
        // Plumbing & Setup
        
        public Stack<BlackboardEvent> blackboardEvents = new Stack<BlackboardEvent>();
        public List<IFollowBlackboard> followers = new List<IFollowBlackboard>();

        public float Now => timeboard.gametime.Now();

        protected virtual void Awake() => SetStartingValues(this);

        private void Update()
        {
            // If we have any events, we will "Pop" the next one with a notification to followers
            if (blackboardEvents.Count == 0) return;
            NotifyEvent(blackboardEvents.Pop());
        }
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            RemoveMissingStartingValues();
#endif
        }

        private void RemoveMissingStartingValues()
        {
#if UNITY_EDITOR
            startingValues.RemoveAll(x => x == null);
#endif
        }

        protected void SetStartingValues(Blackboard blackboardToUse) 
            => startingValues.ForEach(x => x.PassValuesTo(blackboardToUse));

        // ----------------------------------------------------------------------------------------
        // GENERAL
        // ----------------------------------------------------------------------------------------
        
        public void Subscribe(IFollowBlackboard follower) => followers.Add(follower);

        public void Unsubscribe(IFollowBlackboard follower) => followers.Remove(follower);
        
        // ----------------------------------------------------------------------------------------
        // EVENTS
        // ----------------------------------------------------------------------------------------
        
        /// <summary>
        /// Adds a BlackboardEvent to the stack
        /// </summary>
        /// <param name="blackboardEvent"></param>
        public void AddEvent(BlackboardEvent blackboardEvent)
        {
            WriteToConsole("Pushing BlackboardEvent " +
                           $"[Topic: {blackboardEvent.topic}], " +
                           $"[Game Id: {blackboardEvent.gameId}], " +
                           $"[Status: {blackboardEvent.status}], " +
                           $"[Object: {(blackboardEvent.obj == null ? "null" : blackboardEvent.obj.ToString())}]"
                           , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
            blackboardEvents.Push(blackboardEvent);

            //postedEvents.Add(blackboardEvent.Clone());
        }

        /// <summary>
        /// Adds a BlackboardEvent to the stack
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="gameId"></param>
        /// <param name="status"></param>
        /// <param name="obj"></param>
        public void AddEvent(string topic, string gameId, string status, object obj) 
            => AddEvent(BlackboardEvent.Create(topic, gameId, status, obj));

        /// <summary>
        /// Sends out a NotifyEvent() event to all followers
        /// </summary>
        /// <param name="blackboardEvent"></param>
        private void NotifyEvent(BlackboardEvent blackboardEvent)
        {
            WriteToConsole($"Notifying {followers.Count} followers of BlackboardEvent " +
                           $"[Topic: {blackboardEvent.topic}], " +
                           $"[Game Id: {blackboardEvent.gameId}], " +
                           $"[Status: {blackboardEvent.status}], " +
                           $"[Object: {(blackboardEvent.obj == null ? "null" : "not null")}]"
                , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
            
            // We will copy the followers list, in case this action adds any followers to it!
            var followersCopy = followers.Clone();
            followersCopy.ForEach(follower => follower.ReceiveEvent(blackboardEvent));
        }
        
        // ----------------------------------------------------------------------------------------
        // NOTES
        // ----------------------------------------------------------------------------------------

        /// <summary>
        /// Sends out a NotifyNote() event to all followers
        /// </summary>
        /// <param name="blackboardNote"></param>
        private void NotifyNote(BlackboardNote blackboardNote)
        {
            WriteToConsole($"Notifying {followers.Count} followers of BlackboardNote " +
                           $"[Topic: {blackboardNote.topic}], " +
                           $"[Subject: {blackboardNote.subject}]"
                , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
            followers.ForEach(follower => follower.ReceiveChange(blackboardNote));
        }

        /// <summary>
        /// Removes any notes with the given topic and subject
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subject"></param>
        public void RemoveNotes(string topic, string subject)
        {
            for (var i = notes.Count - 1; i >= 0; i--)
            {
                if (notes[i].topic != topic || notes[i].subject != subject)
                    continue;
                
                notes.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Will return the BlackboardNote with the subject/topic keys, if available
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="topic"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public bool TryGet(string topic, string subject, out BlackboardNote found)
        {
            found = notes.FirstOrDefault(x => x.topic == topic && x.subject == subject);
            return found != null;
        }

        /// <summary>
        /// Adds a new note to the list. If a note exists with the same subject/topic, that note will be returned instead.
        /// </summary>
        /// <param name="newNote"></param>
        /// <param name="notifyNote"></param>
        /// <returns></returns>
        public BlackboardNote AddNote(BlackboardNote newNote, bool notifyNote = false)
        {
            // If the note we are trying to add exists, return it. There can't be duplicate topic/subject notes
            if (TryGet(newNote.topic, newNote.subject, out var note))
            {
                WriteToConsole("Did you mean to call UpdateNote()? A BlackboardNote with the same Topic and Subject already exists. Will return the " +
                               "existing note. " +
                               $"[Topic: {newNote.topic}], " +
                               $"[Subject: {newNote.subject}]"
                    , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
                return note;
            }
            
            WriteToConsole("Adding BlackboardNote " +
                           $"[Topic: {newNote.topic}], " +
                           $"[Subject: {newNote.subject}]"
                , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);

            notes.Add(newNote); // Add the note to the list
            
            var addedNote = notes.Last(); // Cache the value
            
            if (notifyNote) NotifyNote(addedNote); // Send an update if notifyNote is true
            
            return addedNote; // Return the last added note to the list (this one!)
        }
        
        /// <summary>
        /// Update an existing BlackboardNote with a newValue. If the note doesn't exist, it may optionally be added.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subject"></param>
        /// <param name="newValue"></param>
        /// <param name="addIfNotFound"></param>
        /// <param name="sendNotification"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BlackboardNote UpdateNote<T>(string topic, string subject, T newValue, bool addIfNotFound = true, bool sendNotification = true)
        {
            // If we already have a note with this topic/subject, then update that.
            if (TryGet(topic, subject, out BlackboardNote note))
                return UpdateNote(note, newValue, sendNotification);
            
            if (!addIfNotFound) return default; // Return if we aren't adding notes that aren't found
            
            var newNote = new BlackboardNote(topic, subject); // Create a new note
            AddNote(newNote); // Add the note without notifying
            
            WriteToConsole("Updating BlackboardNote " +
                           $"[Topic: {newNote.topic}], " +
                           $"[Subject: {newNote.subject}] addIfNotFound {addIfNotFound} sendNotification {sendNotification}"
                , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
            
            return UpdateNote(newNote, newValue, sendNotification); // Update the note with the value, and do the notification
        }

        /// <summary>
        /// Update an existing BlackboardNote with a newValue.
        /// </summary>
        /// <param name="blackboardNote"></param>
        /// <param name="newValue"></param>
        /// <param name="sendNotification"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BlackboardNote UpdateNote<T>(BlackboardNote blackboardNote, T newValue, bool sendNotification = true)
        {
            
            WriteToConsole("Updating BlackboardNote from existing Note " +
                           $"[Topic: {blackboardNote.topic}], " +
                           $"[Subject: {blackboardNote.subject}] sendNotification {sendNotification}"
                , "Blackboard", writeToConsoleColor, writeToConsole, false, gameObject);
            
            //blackboardNote.value = newValue;
            //blackboardNote.value = (T) newValue;
            blackboardNote.updateTime = Now;
            
            // Update the values
            if (newValue is bool valueBool) blackboardNote.valueBool = valueBool; 
            if (newValue is int valueInt) blackboardNote.valueInt = valueInt; 
            if (newValue is float valueFloat) blackboardNote.valueFloat = valueFloat; 
            if (newValue is string valueString) blackboardNote.valueString = valueString;
            if (newValue is GameObject valueGameObject) blackboardNote.valueGameObject = valueGameObject;
            
            // Less general types
            if (newValue is Animation valueAnimation) blackboardNote.valueAnimation = valueAnimation;
            if (newValue is Sprite valueSprite) blackboardNote.valueSprite = valueSprite;
            if (newValue is Texture2D valueTexture2D) blackboardNote.valueTexture2D = valueTexture2D;
            if (newValue is AudioClip valueAudioClip) blackboardNote.valueAudioClip = valueAudioClip;
            if (newValue is Vector4 valueVector4) blackboardNote.valueVector4 = valueVector4;
            if (newValue is Vector3 valueVector3) blackboardNote.valueVector3 = valueVector3;
            if (newValue is Vector2 valueVector2) blackboardNote.valueVector2 = valueVector2;
            if (newValue is Color valueColor) blackboardNote.valueColor = valueColor;

            // Game Module Values
            if (newValue is GameStat valueGameStat) blackboardNote.valueGameStat = valueGameStat;
            if (newValue is GameItemObject valueGameItemObject) blackboardNote.valueGameItemObject = valueGameItemObject;
            if (newValue is GameItemAttribute valueGameItemAttribute) blackboardNote.valueGameItemAttribute = valueGameItemAttribute;
            if (newValue is GameCondition valueGameCondition) blackboardNote.valueGameCondition = valueGameCondition;
            if (newValue is GameQuest valueGameQuest) blackboardNote.valueGameQuest = valueGameQuest;
            if (newValue is GameLootBox valueGameLootBox) blackboardNote.valueGameLootBox = valueGameLootBox;


            if (sendNotification) NotifyNote(blackboardNote); // Trigger the event
            return blackboardNote;
        }

        /*
         * The methods below will return the value of the type specified from the BlackboardNote with the provided
         * topic and subject
         */
        public int ValueInt(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueInt(note) : default;
        public float ValueFloat(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueFloat(note) : default;
        public bool ValueBool(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueBool(note) : default;
        public string ValueString(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueString(note) : default;
        public object ValueObject(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueObject(note) : default;
        
        public object ValueAnimation(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueAnimation(note) : default;
        public object ValueSprite(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueSprite(note) : default;
        public object ValueTexture2D(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueTexture2D(note) : default;
        public object ValueAudioClip(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueAudioClip(note) : default;
        public object ValueVector4(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueVector4(note) : default;
        public object ValueVector3(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueVector3(note) : default;
        public object ValueVector2(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueVector2(note) : default;
        public object ValueColor(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueColor(note) : default;

        public GameObject ValueGameObject(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameObject(note) : default;
        public GameStat ValueGameStat(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameStat(note) : default;
        public GameItemObject ValueGameItemObject(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameItemObject(note) : default;
        public GameItemAttribute ValueGameItemAttribute(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameItemAttribute(note) : default;
        public GameCondition ValueGameCondition(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameCondition(note) : default;
        public GameLootBox ValueGameLootBox(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameLootBox(note) : default;
        public GameStatList ValueGameStatList(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameStatList(note) : default;
        public GameItemObjectList ValueGameItemObjectList(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameItemObjectList(note) : default;
        public GameConditionList ValueGameConditionList(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameConditionList(note) : default;
        public GameQuestList ValueGameQuestList(string topic, string subject) => TryGet(topic, subject, out var note) ? ValueGameQuestList(note) : default;

        /*
         * These methods will return the value of the type specified from the BlackboardNote provided
         */
        public int ValueInt(BlackboardNote note) => note.valueInt;
        public float ValueFloat(BlackboardNote note) => note.valueFloat;
        public bool ValueBool(BlackboardNote note) => note.valueBool;
        public string ValueString(BlackboardNote note) => note.valueString;
        public object ValueObject(BlackboardNote note) => note.value;
        
        public object ValueAnimation(BlackboardNote note) => note.value;
        public object ValueSprite(BlackboardNote note) => note.value;
        public object ValueTexture2D(BlackboardNote note) => note.value;
        public object ValueAudioClip(BlackboardNote note) => note.value;
        public object ValueVector4(BlackboardNote note) => note.value;
        public object ValueVector3(BlackboardNote note) => note.value;
        public object ValueVector2(BlackboardNote note) => note.value;
        public object ValueColor(BlackboardNote note) => note.value;

        public GameObject ValueGameObject(BlackboardNote note) => note.valueGameObject;
        public GameStat ValueGameStat(BlackboardNote note) => note.valueGameStat;
        public GameItemObject ValueGameItemObject(BlackboardNote note) => note.valueGameItemObject;
        public GameItemAttribute ValueGameItemAttribute(BlackboardNote note) => note.valueGameItemAttribute;
        public GameCondition ValueGameCondition(BlackboardNote note) => note.valueGameCondition;
        public GameLootBox ValueGameLootBox(BlackboardNote note) => note.valueGameLootBox;
        public GameQuest ValueGameQuest(BlackboardNote note) => note.valueGameQuest;
        public GameStatList ValueGameStatList(BlackboardNote note) => note.valueGameStatList;
        public GameItemObjectList ValueGameItemObjectList(BlackboardNote note) => note.valueGameItemObjectList;
        public GameConditionList ValueGameConditionList(BlackboardNote note) => note.valueGameConditionList;
        public GameQuestList ValueGameQuestList(BlackboardNote note) => note.valueGameQuestList;

        public List<string> blackboardNotesToSave = new List<string>();
        
        // Used in the Save and Load module
        public object SaveState()
        {
            blackboardNotesToSave.Clear();
            foreach (var note in notes)
                blackboardNotesToSave.Add(JsonUtility.ToJson(note));
            
            var saveData = new BlackboardSaveData
            {
                dictionaries = dictionaries,
                jsonNotes = blackboardNotesToSave,
                writeToConsole = writeToConsole
            };

            return saveData;
        }

        // Used in the Save and Load module
        public void LoadState(string jsonEncodedState)
        {
            var data = JsonUtility.FromJson<BlackboardSaveData>(jsonEncodedState);

            dictionaries = data.dictionaries;
            blackboardNotesToSave = data.jsonNotes;
            writeToConsole = data.writeToConsole;
            
            foreach(var note in blackboardNotesToSave)
                AddNote(JsonUtility.FromJson<BlackboardNote>(note));
        }

        public string SaveableObjectId() => "Blackboard";
        
        [Serializable]
        private struct BlackboardSaveData
        {
            public Dictionaries dictionaries;
            public List<string> jsonNotes;
            public bool writeToConsole;
        }

        public void PreSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PostSaveActions()
        {
            throw new NotImplementedException();
        }

        public void PreLoadActions()
        {
            throw new NotImplementedException();
        }

        public void PostLoadActions()
        {
            throw new NotImplementedException();
        }
    }
}
