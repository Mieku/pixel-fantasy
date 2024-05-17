using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Data.Item;
using Databrain;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Mood.Scripts;
using Systems.Needs.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;
using CropSettings = Data.Resource.CropSettings;
using Random = UnityEngine.Random;

namespace Managers
{
    public class Librarian : Singleton<Librarian>
    {
        [SerializeField] private DataLibrary _dataLibrary;
        [SerializeField] private List<ColourData> _colourLibrary;
        [SerializeField] private List<Sprite> _sprites;
        [SerializeField] private List<SpriteRef> _orderIcons;
        [SerializeField] private List<CropSettings> _cropLibrary;
        [SerializeField] private List<Command> _commandLibrary;
        [SerializeField] private List<RaceSettings> _races;
        [SerializeField] private List<NeedSettings> _stats;
        [SerializeField] private List<EmotionSettings> _emotions;
        [SerializeField] private List<EmotionalBreakdownSettings> _emotionalBreakdowns;
        
        [ShowInInspector] public readonly List<Color32> Palette = new List<Color32>
        {
            new Color32(255, 0, 64, 0), 
            new Color32(19, 19, 19, 255), 
            new Color32(27, 27, 27, 255), 
            new Color32(39, 39, 39, 255), 
            new Color32(61, 61, 61, 255), 
            new Color32(93, 93, 93, 255), 
            new Color32(133, 133, 133, 255), 
            new Color32(180, 180, 180, 255), 
            new Color32(255, 255, 255, 255), 
            new Color32(199, 207, 221, 255), 
            new Color32(146, 161, 185, 255), 
            new Color32(101, 115, 146, 255), 
            new Color32(66, 76, 110, 255), 
            new Color32(42, 47, 78, 255), 
            new Color32(26, 25, 50, 255), 
            new Color32(14, 7, 27, 255), 
            new Color32(28, 18, 28, 255), 
            new Color32(57, 31, 33, 255), 
            new Color32(93, 44, 40, 255), 
            new Color32(138, 72, 54, 255), 
            new Color32(191, 111, 74, 255), 
            new Color32(230, 156, 105, 255), 
            new Color32(246, 202, 159, 255), 
            new Color32(249, 230, 207, 255), 
            new Color32(237, 171, 80, 255), 
            new Color32(224, 116, 56, 255), 
            new Color32(198, 69, 36, 255), 
            new Color32(142, 37, 29, 255), 
            new Color32(255, 80, 0, 255), 
            new Color32(237, 118, 20, 255), 
            new Color32(255, 162, 20, 255), 
            new Color32(255, 200, 37, 255), 
            new Color32(255, 235, 87, 255), 
            new Color32(211, 252, 126, 255), 
            new Color32(153, 230, 95, 255), 
            new Color32(90, 197, 79, 255), 
            new Color32(51, 152, 75, 255), 
            new Color32(30, 111, 80, 255), 
            new Color32(19, 76, 76, 255), 
            new Color32(12, 46, 68, 255), 
            new Color32(0, 57, 109, 255), 
            new Color32(0, 105, 170, 255), 
            new Color32(0, 152, 220, 255), 
            new Color32(0, 205, 249, 255), 
            new Color32(12, 241, 255, 255), 
            new Color32(148, 253, 255, 255), 
            new Color32(253, 210, 237, 255), 
            new Color32(243, 137, 245, 255), 
            new Color32(219, 63, 253, 255), 
            new Color32(122, 9, 250, 255), 
            new Color32(48, 3, 217, 255), 
            new Color32(12, 2, 147, 255), 
            new Color32(3, 25, 63, 255), 
            new Color32(59, 20, 67, 255), 
            new Color32(98, 36, 97, 255), 
            new Color32(147, 56, 143, 255), 
            new Color32(202, 82, 201, 255), 
            new Color32(200, 80, 134, 255), 
            new Color32(246, 129, 135, 255), 
            new Color32(245, 85, 93, 255), 
            new Color32(234, 50, 60, 255), 
            new Color32(196, 36, 48, 255), 
            new Color32(137, 30, 43, 255), 
            new Color32(87, 28, 39, 255)
        };

        public DataLibrary DataLibrary => _dataLibrary;

        public List<ItemSettings> GetAllItemSettings()
        {
            List<ItemSettings> itemSettingsList = _dataLibrary.GetAllInitialDataObjectsByType<ItemSettings>(true);
            List<ItemSettings> clone = new List<ItemSettings>(itemSettingsList);
            return clone;
        }

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
        
        public NeedSettings GetStat(string statName)
        {
            var result = _stats.Find(stat => stat.DisplayName == statName);
            if (result == null)
            {
                Debug.LogError($"Unknown Stat: {_stats}");
            }

            return result;
        }
        
        public RaceSettings GetRace(string raceName)
        {
            var result = _races.Find(race => race.RaceName == raceName);
            if (result == null)
            {
                Debug.LogError($"Unknown Race: {raceName}");
            }

            return result;
        }

        public Command GetCommand(string taskId)
        {
            var result = _commandLibrary.Find(cmd => cmd.Task.TaskId == taskId);
            if (result == null)
            {
                Debug.LogError("Unknown Command for Task Id: " + taskId);
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
        
        public CropSettings GetCropData(string key)
        {
            var result = _cropLibrary.Find(s => s.CropName == key);
            if (result == null)
            {
                Debug.LogError("Unknown Crop: " + key);
            }
            return result;
        }

        public List<CropSettings> GetAllCropData()
        {
            return new List<CropSettings>(_cropLibrary);
        }

        public Sprite GetSprite(string spriteName)
        {
            var result = _sprites.Find(s => s.name == spriteName);
            return result;
        }

        public Sprite GetOrderIcon(string spriteName)
        {
            return _orderIcons.Find(i => i.Name == spriteName).Sprite;
        }
    }

    [Serializable]
    public class ColourData
    {
        public string Name;
        public Color Colour;
    }

    [Serializable]
    public class SpriteRef
    {
        [HorizontalGroup("Split", Width = 50), HideLabel, PreviewField(50)]
        public Sprite Sprite;
        [VerticalGroup("Split/Properties")]
        public string Name;
    }
}
