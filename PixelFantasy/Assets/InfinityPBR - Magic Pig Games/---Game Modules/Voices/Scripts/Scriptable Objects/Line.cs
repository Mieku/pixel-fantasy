using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * The Line object holds a List<AudioClip>() of default audio clips. When 
 */

namespace InfinityPBR
{
    [Serializable]
    public class Line
    {
        public string line; // Name of the line
        public List<AudioClip> audioClip = new List<AudioClip>(); // List of the "Default" AudioClips for this voice/line
        public List<Emotion> emotions = new List<Emotion>(); // List of emotion objects attached to this line
        
        // Plumbing for Voices Editor
        [HideInInspector] public AudioClip newAudioClip;
        [HideInInspector] public string newName;

        /// <summary>
        /// Returns the AudioClip at this index. If index is -1 will return a random clip.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(int index = -1)
        {
            if (audioClip.Count == 0) return GetRandomEmotionAudioClip();
            
            // Random
            if (index < 0) return audioClip[Random.Range(0, audioClip.Count)];

            if (audioClip.Count <= index) return default;
            
            // Chosen index
            return audioClip[Random.Range(0, audioClip.Count)];
        }

        /// <summary>
        /// This will return a random clip from one of the emotions, if any are available.
        /// </summary>
        /// <returns></returns>
        public AudioClip GetRandomEmotionAudioClip()
        {
            if (emotions.Count == 0) return default;
            
            List<AudioClip> clips = new List<AudioClip>();
            foreach (Emotion emotion in emotions)
            {
                foreach (AudioClip clip in emotion.audioClip)
                {
                    clips.Add(clip);
                }
            }

            if (clips.Count == 0) return default;
            
            return clips[Random.Range(0, clips.Count)];
        }
        
        /// <summary>
        /// This will return true if the audioClip you provide is in this list already.
        /// </summary>
        /// <param name="lookupClip"></param>
        /// <returns></returns>
        public bool HasAudioClip(AudioClip lookupClip)
        {
            foreach (AudioClip clip in audioClip)
            {
                if (clip == lookupClip) 
                    return true;
            }

            return false;
        }
        
        /// <summary>
        /// Returns the Emotion object by the name provided.
        /// </summary>
        /// <param name="emotionName"></param>
        /// <returns></returns>
        public Emotion GetEmotion(string emotionName)
        {
            for (int i = 0; i < emotions.Count; i++)
            {
                if (emotions[i].emotion == emotionName)
                    return GetEmotion(i);
            }

            Debug.LogWarning("Warning: Did not find an emotion named " + emotionName);
            return default;
        }

        /// <summary>
        /// Return the Emotion object by the index provided.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Emotion GetEmotion(int index)
        {
            return emotions[index];
        }
    }
}