using Databrain;
using Databrain.Attributes;
using TaskSystem;
using UnityEngine;

namespace Data.Resource
{
    [DataObjectAddToRuntimeLibrary]
    public class ResourceData : DataObject
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public int SpriteIndex;
        [ExposeToInspector, DatabrainSerialize] public float Health;
        [ExposeToInspector, DatabrainSerialize] public float RemainingExtractWork;
        [ExposeToInspector, DatabrainSerialize] public Task CurrentTask;
        [ExposeToInspector, DatabrainSerialize] public Vector2 Position;
        [ExposeToInspector, DatabrainSerialize]  public ResourceSettings Settings;
        
        
        public virtual void InitData(ResourceSettings settings)
        {
            Settings = settings;
            Health = Settings.MaxHealth;
            RemainingExtractWork = Settings.WorkToExtract;
            SpriteIndex = Settings.GetRandomSpriteIndex();
        }
    }
}
