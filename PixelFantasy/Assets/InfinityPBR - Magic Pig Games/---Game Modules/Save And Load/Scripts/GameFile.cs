using System;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameFile
    {
        public string saveGameId;
        public float gametimeNow;
        public string systemTime;
        public long systemTimecode;
        public string gameName;
        public string sceneName;
    }
}