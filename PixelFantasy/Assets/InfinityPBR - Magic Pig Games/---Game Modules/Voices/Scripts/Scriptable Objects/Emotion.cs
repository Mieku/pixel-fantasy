using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InfinityPBR
{
    [Serializable]
    public class Emotion
    {
        public string emotion; // Name of the emotion
        public List<AudioClip> audioClip = new List<AudioClip>(); // List of AudioClips for this voice/line/emotion

        // Plumbing for Voices Editor
        [HideInInspector] public AudioClip newAudioClip;
        [HideInInspector] public string newName;
        
        /// <summary>
        /// Returns the AudioClip by the index provided. If index = -1, will return a random clip
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(int index = -1)
        {
            if (audioClip.Count == 0) return default;
            
            // Random
            if (index < 0) return audioClip[Random.Range(0, audioClip.Count)];

            if (audioClip.Count <= index) return default;
            
            // Chosen index
            return audioClip[Random.Range(0, audioClip.Count)];
        }
        
        /// <summary>
        /// Returns true if the list contains the provided lookupClip
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
    }
}