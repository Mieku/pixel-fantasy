using System;
using System.Collections.Generic;
using Systems.Mood.Scripts;
using UnityEngine;


namespace Managers
{
    public class Librarian : Singleton<Librarian>
    {
        [SerializeField] private List<ColourData> _colourLibrary;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private List<Command> _commandLibrary;
        [SerializeField] private List<EmotionSettings> _emotions;
        [SerializeField] private List<EmotionalBreakdownSettings> _emotionalBreakdowns;
        
        public EmotionalBreakdownSettings GetEmotionalBreakdown(string breakdownTaskId)
        {
            var result = _emotionalBreakdowns.Find(emotion => emotion.BreakdownTaskId == breakdownTaskId);
            if (result == null)
            {
                Debug.LogError($"Unknown Emotional Breakdown: {breakdownTaskId}");
            }

            return result;
        }
        
        public EmotionSettings GetEmotion(string emotionName)
        {
            var result = _emotions.Find(emotion => emotion.DisplayName == emotionName);
            if (result == null)
            {
                Debug.LogError($"Unknown Emotion: {emotionName}");
            }

            return result;
        }

        public Command GetCommand(string commandId)
        {
            var result = _commandLibrary.Find(cmd => cmd.CommandID == commandId);
            if (result == null)
            {
                Debug.LogError("Unknown Command for Command Id: " + commandId);
            }
            return result;
        }
        
        public Color GetColour(string colourName)
        {
            var result = _colourLibrary.Find(c => c.Name == colourName);
            if (result != null)
            {
                return result.Colour;
            }
            else
            {
                Debug.LogError("Unknown Colour: " + colourName);
                return Color.magenta;
            }
        }
 
        public Sprite GetSprite(string spriteName)
        {
            var result = _sprites.Find(s => s.name == spriteName);
            return result;
        }
    }

    [Serializable]
    public class ColourData
    {
        public string Name;
        public Color Colour;
    }
}
