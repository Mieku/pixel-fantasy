using System;
using System.Collections.Generic;
using InfinityPBR.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * This is the main "Voices" scriptable object that is created with the Voices Module. Once you have this populated,
 * attach this to a Game Controller object, or other object in your game. A reference to this object is required for
 * switching or assigning Voice objects at run time.
 */

namespace InfinityPBR
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/voices")]
    [CreateAssetMenu(fileName = "Voices", menuName = "Game Modules/Create/Voices", order = 1)]
    public class Voices : ModulesScriptableObject
    {
        public List<Voice> voices = new List<Voice>(); // List of all the Voice objects
        public List<string> types = new List<string>(); // List of all the types you have created

        public List<string> lineNames = new List<string>(); // string list of all the lineNames each voice has
        public List<string> emotionNames = new List<string>(); // String list of all the emotionNames each line has

        [HideInInspector] public int menubarIndex;
        
        /// <summary>
        /// Returns the AudioClip based on the variables you provide. EmotionName and index are optional. If index
        /// is -1, a randomly selected clip will be returned.
        /// </summary>
        /// <param name="voiceName"></param>
        /// <param name="lineName"></param>
        /// <param name="emotionName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(string voiceName, string lineName, string emotionName = "", int index = -1)
        {
            voiceName = voiceName.Replace(" ", "");
            Voice voice = GetVoice(voiceName);
            if (voice == null)
            {
                Debug.LogError($"No voice found with name {voiceName}");
                return default;
            }
            return voice.GetAudioClip(lineName, emotionName, index);
        }
        
        // -------------------------------------------------------------------
        // GETS
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns a voice object by a given name.
        /// </summary>
        /// <param name="voiceName"></param>
        /// <returns></returns>
        public Voice GetVoice(string voiceName)
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i].voice != voiceName)
                    continue;
                return GetVoice(i);
            }

            return default;
        }
        
        /// <summary>
        /// Returns the index of a named voice
        /// </summary>
        /// <param name="voiceName"></param>
        /// <returns></returns>
        public int GetVoiceIndex(string voiceName)
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i].voice != voiceName)
                    continue;
                return i;
            }

            return default;
        }

        /// <summary>
        /// Returns a voice object by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Voice GetVoice(int index)
        {
            return voices[index];
        }
        
        /// <summary>
        /// Will return a random voice
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Voice GetRandomVoice()
        {
            if (voices.Count == 0)
            {
                Debug.LogError("Error: There were no voices to return");
                return default;
            }

            return voices[Random.Range(0, voices.Count)];
        }
        
        /// <summary>
        /// Will return a random voice of a specified type
        /// </summary>
        /// <returns></returns>
        public Voice GetRandomVoiceOfType(string typeToGet)
        {
            List<Voice> voicesOfType = GetVoicesOfType(typeToGet);
            if (voicesOfType.Count == 0)
            {
                Debug.LogError("Error: There were no voices of type " + typeToGet + " to return");
                return default;
            }

            return voicesOfType[Random.Range(0, voicesOfType.Count)];
        }
        
        /// <summary>
        /// This will return the index of a named voiceType
        /// </summary>
        /// <param name="voiceType"></param>
        /// <returns></returns>
        public int GetTypeIndex(string voiceType)
        {
            for (var i = 0; i < types.Count; i++)
            {
                if (types[i] == voiceType)
                    return i;
            }
            
            Debug.LogError("Error: No type found called " + voiceType);
            return default;
        }

        /// <summary>
        /// Will return a list of voices of a given type.
        /// </summary>
        /// <param name="typeToGet"></param>
        /// <returns></returns>
        public List<Voice> GetVoicesOfType(string typeToGet)
        {
            List<Voice> voicesOfType = new List<Voice>();
            foreach (Voice voice in voices)
            {
                if (voice.type == typeToGet)
                    voicesOfType.Add(voice);
            }

            return voicesOfType;
        }

        /// <summary>
        /// Given the current voice, will return the next Voice of this type. Provide -1 for parameter next to select previous voice
        /// </summary>
        /// <param name="typeToGet"></param>
        /// <param name="currentVoice"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public Voice GetNextVoiceOfType(string typeToGet, Voice currentVoice, int next = 1, List<Voice> customList = null)
        {
            var voicesOfType = customList;
            if (customList == null)
                voicesOfType = GetVoicesOfType(typeToGet);
            if (voicesOfType.Count == 0) // If we have no results, we need to return default
                return default;

            // Get the index we are currently using in this temporary list.
            int currentIndex = -1; // Default value
            for (int i = 0; i < voicesOfType.Count; i++)
            {
                if (voicesOfType[i] == currentVoice) // If the voice objects match
                {
                    currentIndex = i; // Set our current index value
                    break; // Break the for loop
                }
            }

            if (currentIndex == -1) return voicesOfType[0]; // If we didn't find our current voice, return the first voice

            int newIndex = currentIndex + next; // Set the index for the next voice
            if (newIndex < 0) newIndex = voicesOfType.Count - 1; // If we are below 0 return the last voice
            if (newIndex >= voicesOfType.Count) newIndex = 0; // If we are above the Count, return the first voice

            return voicesOfType[newIndex]; // Return the voice
        }
        
        /// <summary>
        /// Given the current voice, will return the next Voice. provide a negative value for next to get previous Voice
        /// </summary>
        /// <param name="typeToGet"></param>
        /// <param name="currentIndex"></param>
        /// /// <param name="next"></param>
        /// <returns></returns>
        public Voice GetNextVoice(Voice currentVoice, int next = 1)
        {
            if (voices.Count == 0) // If we have no results, we need to return default
                return default;

            int newIndex = GetVoiceIndex(currentVoice.voice) + next; // Set the index for the next voice
            if (newIndex < 0) newIndex = voices.Count - 1; // If we are below 0 return the last voice
            if (newIndex >= voices.Count) newIndex = 0; // If we are above the Count, return the first voice

            return voices[newIndex]; // Return the voice
        }

        // -------------------------------------------------------------------
        // HAS
        // -------------------------------------------------------------------

        /// <summary>
        /// Returns true if the given voice exists
        /// </summary>
        /// <param name="voiceName"></param>
        /// <returns></returns>
        public bool HasVoice(string voiceName)
        {
            foreach(Voice voice in voices)
            {
                if (voice.voice == voiceName)
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Returns true if the given line exists
        /// </summary>
        /// <param name="lineName"></param>
        /// <returns></returns>
        public bool HasLine(string lineName)
        {
            foreach(string line in lineNames)
            {
                if (line == lineName)
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Returns true if the given emotion exists
        /// </summary>
        /// <param name="emotionName"></param>
        /// <returns></returns>
        public bool HasEmotion(string emotionName)
        {
            foreach(string emotion in emotionNames)
            {
                if (emotion == emotionName)
                    return true;
            }

            return false;
        }
        
        
        // -------------------------------------------------------------------
        // ADDS
        // -------------------------------------------------------------------
        
        /// <summary>
        /// Adds a new voice to the system, and will populate all the lines etc.
        /// </summary>
        /// <param name="newVoiceName"></param>
        public void AddVoice(string newVoiceName)
        {
            if (HasVoice(newVoiceName)) return;

            Voice newVoice = new Voice();
            newVoice.voice = newVoiceName;
            
            foreach (string line in lineNames)
            {
                Line newLine = new Line();
                newLine.line = line;
                AddEmotionsToLine(newLine);
                newVoice.lines.Add(newLine);
            }
            
            voices.Add(newVoice);
        }
        
        /// <summary>
        /// Adds a new type to the list of Types
        /// </summary>
        /// <param name="newTypeName"></param>
        public void AddType(string newTypeName)
        {
            if (HasType(newTypeName)) return;

            types.Add(newTypeName);
        }

        /// <summary>
        /// Returns true if the named type exists
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public bool HasType(string typeName)
        {
            foreach (string type in types)
            {
                if (type == typeName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the current emotions to the line provided (newLine)
        /// </summary>
        /// <param name="newLine"></param>
        public void AddEmotionsToLine(Line newLine)
        {
            foreach (string emotion in emotionNames)
            {
                Emotion newEmotion = new Emotion();
                newEmotion.emotion = emotion;
                newLine.emotions.Add(newEmotion);
            }
        }
        
        /// <summary>
        /// Adds a new line to the system, and will populate all the voices etc.
        /// </summary>
        /// <param name="newLineName"></param>
        public void AddLine(string newLineName)
        {
            if (HasLine(newLineName)) return;

            lineNames.Add(newLineName);
            
            AddLineToVoices(newLineName);
        }

        /// <summary>
        /// Adds the newLine to all the voices
        /// </summary>
        /// <param name="newLineName"></param>
        public void AddLineToVoices(string newLineName)
        {
            foreach (Voice voice in voices)
            {
                Line newLine = new Line();
                newLine.line = newLineName;
                AddEmotionsToLine(newLine);
                voice.lines.Add(newLine);
            }
        }
        
        /// <summary>
        /// Adds a new emotion to the system, and will populate all the voices & lines etc.
        /// </summary>
        /// <param name="newEmotionName"></param>
        public void AddEmotion(string newEmotionName)
        {
            if (HasEmotion(newEmotionName)) return;

            emotionNames.Add(newEmotionName);
            
            AddEmotionToLines(newEmotionName);
        }

        /// <summary>
        /// Adds the new emotion to all the lines on all the voices
        /// </summary>
        /// <param name="newEmotionName"></param>
        public void AddEmotionToLines(string newEmotionName)
        {
            foreach (Voice voice in voices)
            {
                foreach (Line line in voice.lines)
                {
                    Emotion newEmotion = new Emotion();
                    newEmotion.emotion = newEmotionName;
                    line.emotions.Add(newEmotion);
                }
            }
        }
        
        
        
        /// Used in the editor scripts
        [HideInInspector] public bool showAutoFill = true;
        [HideInInspector] public string autoFillDirectory = "";
    }
}