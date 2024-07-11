using System;
using Newtonsoft.Json;
using TaskSystem;
using UnityEngine;

namespace DataPersistence
{
    [Serializable]
    public class BasicResourceData
    {
        // Runtime
        public int SpriteIndex;
        public float Health;
        public Task CurrentTask;
        public string SettingsName;
        
        [JsonRequired] private float _posX;
        [JsonRequired] private float _posY;
    
        [JsonIgnore]
        public Vector2 Position
        {
            get => new(_posX, _posY);
            set
            {
                _posX = value.x;
                _posY = value.y;
            }
        }
        
        [JsonIgnore]
        public virtual ResourceSettings Settings => Resources.Load<ResourceSettings>($"Settings/Resource/Basic Resources/{SettingsName}");
        
        public virtual void InitData(ResourceSettings settings)
        {
            SettingsName = settings.name;
            Health = settings.MaxHealth;
            SpriteIndex = settings.GetRandomSpriteIndex();
        }
        
        /// <summary>
        /// The percentage of durability remaining ex: 0.5 = 50%
        /// </summary>
        public float DurabilityPercent
        {
            get
            {
                var percent = (float)Health / (float)Settings.MaxHealth;
                return percent;
            }
        }

        public virtual float MaxHealth => Settings.MaxHealth;
    }
}
