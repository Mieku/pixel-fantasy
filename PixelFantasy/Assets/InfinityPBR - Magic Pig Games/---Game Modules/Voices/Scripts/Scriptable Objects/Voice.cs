using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is the Voice object which should be attached to whichever character or object is "speaking". The Voice object
 * has a name (voice) and a type name (type), and a List<Line>() of Line objects. These are created by the Voice
 * Module editor script. Each Voice will have the same Lines as others, but each Line object will have it's own unique
 * AudioClips assigned.
 *
 * The PlayClip method provided as a quick way to play an AudioClip using this script. Depending on your project, you
 * may choose to write your own PlayClip method which utilizes volume or other options that your game requires.
 */

namespace InfinityPBR
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/voices")]
    [Serializable]
    public class Voice
    {
        public string voice; // Name of the voice
        public string type; // Type of the voice
        public List<Line> lines = new List<Line>(); // List of all the lines attached to this voice

        /// <summary>
        /// Pass in an audioSource and the line, optionally emotionName and index (if there are more than one clip
        /// of the same kind), and this will play it through the audioSource. Index defaults to random.
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="line"></param>
        /// <param name="emotionName"></param>
        /// <param name="index"></param>
        public void PlayClip(AudioSource audioSource, string line, string emotionName = "", int index = -1)
        {
            audioSource.PlayOneShot(GetAudioClip(line, emotionName, index));
        }
        
        /// <summary>
        /// Returns and AudioClip based on the lineName and optional emotionName and index if there are multiple
        /// clips of the same type. Index defaults to random.
        /// </summary>
        /// <param name="lineName"></param>
        /// <param name="emotionName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public AudioClip GetAudioClip(string lineName, string emotionName = "", int index = -1)
        {
            lineName = lineName.Replace(" ", ""); // Removes any spaces, to match file name
            Line line = GetLine(lineName); // Retrieve the Line object

            // return an AudioClip from the default line clips or if emotionName is populated, one from the emotions
            return emotionName == "" ? line.GetAudioClip(index) : line.GetEmotion(emotionName).GetAudioClip(index);
        }
        
        /// <summary>
        /// Returns a line object by the lineName you provide.
        /// </summary>
        /// <param name="lineName"></param>
        /// <returns></returns>
        public Line GetLine(string lineName)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].line == lineName)
                    return GetLine(i);
            }

            Debug.LogWarning("Warning: Did not find a line named " + lineName);
            return default;
        }

        /// <summary>
        /// Returns a line object based on the index provided.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Line GetLine(int index)
        {
            return lines[index];
        }
    }
}