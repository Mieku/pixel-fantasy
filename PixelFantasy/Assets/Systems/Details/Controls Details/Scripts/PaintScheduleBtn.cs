using System;
using Characters;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Controls_Details.Scripts
{
    public class PaintScheduleBtn : MonoBehaviour
    {
        public ScheduleOption ScheduleOption;
        
        [SerializeField] private Image _bg;
        [SerializeField] private Image _colourPalette;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _activeBG;

        [SerializeField] private ScheduleMenu _menu;

        private void Start()
        {
            _bg.sprite = _defaultBG;

            Color colour;
            switch (ScheduleOption)
            {
                case ScheduleOption.Sleep:
                    colour = GameSettings.Instance.SleepScheduleColour;
                    break;
                case ScheduleOption.Work:
                    colour = GameSettings.Instance.WorkScheduleColour;
                    break;
                case ScheduleOption.Recreation:
                    colour = GameSettings.Instance.RecreationScheduleColour;
                    break;
                case ScheduleOption.Anything:
                    colour = GameSettings.Instance.AnythingScheduleColour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _colourPalette.color = colour;
        }

        public void SetHighlight(bool showHighlight)
        {
            if (showHighlight)
            {
                _bg.sprite = _activeBG;
            }
            else
            {
                _bg.sprite = _defaultBG;
            }
        }

        public void OnPressed()
        {
            _menu.OnPaintSchedulePressed(ScheduleOption);
        }
    }
}
