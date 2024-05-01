using System;
using UnityEngine;

namespace Systems.Appearance.Scripts
{
    [Serializable]
    public class AvatarData
    {
        // Data looks like this: PirateCostume#FFFFFF/0:0:0
        
        public string Body;
        public string Hands;
        public string Eyes;
        public string Blush;
        public string Hair;
        public string Beard;

        public string Clothing;
        public string Hat;
        public string FaceAccessory; // Glasses, Mask...

        public string Weapon;
        public string Offhand;
    }
}
