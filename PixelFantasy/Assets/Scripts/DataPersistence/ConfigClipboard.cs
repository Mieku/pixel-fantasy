using System.Collections.Generic;
using Managers;
using UnityEngine;

public class ConfigClipboard : Singleton<ConfigClipboard>
{
    private readonly Dictionary<EConfigType, ICopyPasteConfig> _clipboard = new Dictionary<EConfigType, ICopyPasteConfig>();

    public void Copy(ICopyPasteConfig config)
    {
        if (config == null)
        {
            Debug.LogError("Attempted to Copy a null config");
            return;
        }
            
        _clipboard[config.ConfigType] = config;

        GameEvents.Trigger_OnConfigClipboardChanged();
    }

    public ICopyPasteConfig Paste(EConfigType configType)
    {
        if (!HasConfig(configType))
        {
            Debug.LogError($"Attempted to Paste when no config of type:{configType} was available");
            return null;
        }
            
        return _clipboard[configType];
    }

    public bool HasConfig(EConfigType configType)
    {
        return _clipboard.ContainsKey(configType);
    }
}

public enum EConfigType
{
    Stockpile,
}
