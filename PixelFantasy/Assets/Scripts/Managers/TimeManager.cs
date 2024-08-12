using System;
using QFSW.QC;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class TimeManager : Singleton<TimeManager>
    {
        public GameSpeed GameSpeed;
        public float GameSpeedMod;

        private GameSpeed _prevSpeed;
        private const float _fastSpeedMod = 2f;
        private const float _fastestSpeedMod = 3f;

        protected override void Awake()
        {
            base.Awake();

            GameEvents.OnGameLoadStart += OnDoneLoading;
        }

        private void OnDestroy()
        {
            GameEvents.OnGameLoadStart -= OnDoneLoading;
        }

        private void Start()
        {
            SetGameSpeed(GameSpeed.Play);
        }

        private void OnDoneLoading()
        {
            // Remind everything the game's speed after loading
            GameEvents.Trigger_OnGameSpeedChanged(GameSpeedMod);
        }

        public void SetGameSpeed(GameSpeed newSpeed)
        {
            _prevSpeed = GameSpeed;
            GameSpeed = newSpeed;

            GameSpeedMod = GetSpeedMod(GameSpeed);

            GameEvents.Trigger_OnGameSpeedChanged(GameSpeedMod);
        }

        private float GetSpeedMod(GameSpeed speed)
        {
            return speed switch
            {
                GameSpeed.Paused => 0f,
                GameSpeed.Play => 1f,
                GameSpeed.Fast => _fastSpeedMod,
                GameSpeed.Fastest => _fastestSpeedMod,
                _ => throw new ArgumentOutOfRangeException(nameof(speed), speed, null)
            };
        }

        public float DeltaTime => Time.deltaTime * GameSpeedMod;

        private bool IsConsoleOpen()
        {
            if (QuantumConsole.Instance == null) return false;

            return QuantumConsole.Instance.IsActive;
        }

        public void OnSetGameSpeed_Paused(InputValue value)
        {
            if (IsConsoleOpen()) return;
            
            if (value.isPressed)
            {
                if (GameSpeed == GameSpeed.Paused)
                {
                    if (_prevSpeed == GameSpeed.Paused)
                    {
                        _prevSpeed = GameSpeed.Play;
                    }
                    SetGameSpeed(_prevSpeed);
                }
                else
                {
                    SetGameSpeed(GameSpeed.Paused);
                }
            }
        }
        
        public void OnSetGameSpeed_Play(InputValue value)
        {
            if (IsConsoleOpen()) return;
            
            SetGameSpeed(GameSpeed.Play);
        }
        
        public void OnSetGameSpeed_Fast(InputValue value)
        {
            if (IsConsoleOpen()) return;
            
            SetGameSpeed(GameSpeed.Fast);
        }
        
        public void OnSetGameSpeed_Fastest(InputValue value)
        {
            if (IsConsoleOpen()) return;
            
            SetGameSpeed(GameSpeed.Fastest);
        }
    }

    public enum GameSpeed
    {
        Paused,
        Play,
        Fast,
        Fastest
    }
}
