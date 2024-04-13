using UnityEngine;

namespace Data.Item
{
    public interface ICopyPasteConfig
    {
        public EConfigType ConfigType { get; }

        public void PasteConfigs(ICopyPasteConfig otherConfigs);
    }
}
