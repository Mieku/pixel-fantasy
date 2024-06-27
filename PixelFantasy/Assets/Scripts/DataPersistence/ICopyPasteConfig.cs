using UnityEngine;

public interface ICopyPasteConfig
{
    public EConfigType ConfigType { get; }

    public void PasteConfigs(ICopyPasteConfig otherConfigs);
}
