using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DataPersistence
{
    
    public class MountainResourceData : BasicResourceData
    {
        [JsonIgnore]
        public MountainSettings MountainSettings => Settings as MountainSettings;
        
        [JsonIgnore]
        public override ResourceSettings Settings => Resources.Load<MountainSettings>($"Settings/Resource/Mountains/{SettingsName}");

        public override void InitData(ResourceSettings settings)
        {
            base.InitData(settings);
        }
    }
}