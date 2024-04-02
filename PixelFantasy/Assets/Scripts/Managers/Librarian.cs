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
using Systems.Skills.Scripts;
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
        [SerializeField] private List<HairSettings> _hairLibrary;
        [SerializeField] private List<Command> _commandLibrary;
        [SerializeField] private List<RaceSettings> _races;
        [SerializeField] private List<NeedSettings> _stats;
        [SerializeField] private List<EmotionSettings> _emotions;
        [SerializeField] private List<EmotionalBreakdownSettings> _emotionalBreakdowns;
        [SerializeField] private List<TalentSettings> _talents;

        public DataLibrary DataLibrary => _dataLibrary;

        public ItemData GetInitialItemDataByGuid(string guid)
        {
            return _dataLibrary.GetInitialDataObjectByGuid(guid) as ItemData;
        }
        
        public TalentSettings GetTalent(string talentName)
        {
            var result = _talents.Find(talent => talent.TalentName == talentName);
            if (result == null)
            {
                Debug.LogError($"Unknown Talent: {talentName}");
            }

            return result;
        }

        public TalentSettings GetRandomTalent()
        {
            int index = Random.Range(0, _talents.Count);
            return _talents[index];
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

        public HairSettings GetHairData(string hairName)
        {
            var result = _hairLibrary.Find(s => s.Name == hairName);
            if (result == null)
            {
                Debug.LogError("Unknown Hair Data: " + hairName);
            }
            return result;
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
