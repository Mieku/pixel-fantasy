using System;
using UnityEngine;
using static InfinityPBR.Modules.Timeboard;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/blackboard")]
    [Serializable]
    public class BlackboardNote
    {
        public string name; // Mostly useful for the inspector
        public string topic; // The broad topic. Examples could be "Kills" or "Dungeon Doors"
        public string subject; // The specific subject. Such as "Goblins" or "Crypt" 
        public object value;
        
        // Time
        public float startTime; // The Gametime time the note was posted
        public float updateTime; // The Gametime the note was last updated

        // General types
        public float valueFloat; 
        public int valueInt;
        public string valueString; 
        public bool valueBool;
        public GameObject valueGameObject;
        
        // Less General types
        public Animation valueAnimation;
        public Sprite valueSprite;
        public Texture2D valueTexture2D;
        public AudioClip valueAudioClip;
        public Vector3 valueVector3;
        public Vector2 valueVector2;
        public Color valueColor;
        public Vector4 valueVector4;
        
        // Game Module types
        public GameStat valueGameStat;
        public GameItemObject valueGameItemObject;
        public GameItemAttribute valueGameItemAttribute;
        public GameCondition valueGameCondition;
        public GameQuest valueGameQuest;
        public GameLootBox valueGameLootBox;
        
        // Game Lists
        public GameStatList valueGameStatList;
        public GameItemObjectList valueGameItemObjectList;
        public GameConditionList valueGameConditionList;
        public GameQuestList valueGameQuestList;

        public float Now => timeboard ? timeboard.gametime.Now() : -1;
        
        /*
         * Whenever you construct a new note, construct it first, then add the values to it using
         * UpdateNote(). Set sendNotification = false in that method if you are adding multiple values, and allow
         * the last UpdateNote() call send the notification.
         *
         * IMPORTANT: You can pass in an array of objects, but each must be a different type. If multiple objects
         * are the same type, only the last one will be utilized.
         */
        public BlackboardNote(string newTopic, string newSubject, object[] objects = null)
        {
            name = $"{newTopic} - {newSubject}";
            topic = newTopic;
            subject = newSubject;
            startTime = Now;
            updateTime = Now;
            
            if (objects == null) return;

            foreach (var obj in objects)
            {
                // Simple types
                if (obj is float floatObj) valueFloat = floatObj;
                else if (obj is int intObj) valueInt = intObj;
                else if (obj is string stringObj) valueString = stringObj;
                else if (obj is bool boolObj) valueBool = boolObj;
                else if (obj is GameObject gameObject) valueGameObject = gameObject;

                // Less General types
                else if (obj is Animation animation) valueAnimation = animation;
                else if (obj is Sprite sprite) valueSprite = sprite;
                else if (obj is Texture2D texture2D) valueTexture2D = texture2D;
                else if (obj is AudioClip audioClip) valueAudioClip = audioClip;
                else if (obj is Vector3 vector3) valueVector3 = vector3;
                else if (obj is Vector2 vector2) valueVector2 = vector2;
                else if (obj is Color color) valueColor = color;
                else if (obj is Vector4 vector4) valueVector4 = vector4;

                // Game Module object types
                else if (obj is GameStat stat) valueGameStat = stat;
                else if (obj is GameItemObject itemObject) valueGameItemObject = itemObject;
                else if (obj is GameItemAttribute itemAttribute) valueGameItemAttribute = itemAttribute;
                else if (obj is GameCondition gameCondition) valueGameCondition = gameCondition;
                else if (obj is GameLootBox gameLootBox) valueGameLootBox = gameLootBox;
                else if (obj is GameQuest gameQuest) valueGameQuest = gameQuest;
            
                // Game Module list types
                else if (obj is GameStatList gameStatList) valueGameStatList = gameStatList;
                else if (obj is GameItemObjectList gameItemObjectList) valueGameItemObjectList = gameItemObjectList;
                else if (obj is GameConditionList gameConditionList) valueGameConditionList = gameConditionList;
                else if (obj is GameQuestList gameQuestList) valueGameQuestList = gameQuestList;

                else value = obj; // This could be used for custom types
            }

            
        }

        public BlackboardNote Clone() => JsonUtility.FromJson<BlackboardNote>(JsonUtility.ToJson(this));
        
        
    }
}