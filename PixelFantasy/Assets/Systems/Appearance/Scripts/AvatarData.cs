using System;
using Characters;
using Databrain.Attributes;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Systems.Appearance.Scripts
{
    [Serializable]
    [UseOdinInspector]
    public class AvatarData
    {
        // Data looks like this: PirateCostume/#FFFFFF/0:0:0
        
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

        private KinlingData _kinlingData;
        private EGender _gender;
        private RaceSettings _race;
        private EMaturityStage _ageStage;
        
        public Color32 SkinTone;
        public Color32 HairColour;
        public HairSettings HairStyle;
        public HairSettings BeardStyle;
        public Color32 EyeColour;

        public AvatarData (KinlingData kinlingData, EGender gender, EMaturityStage ageStage, RaceSettings race)
        {
            _kinlingData = kinlingData;
            _gender = gender;
            _race = race;
            _ageStage = ageStage;

            SkinTone = _race.GetRandomSkinTone();
            HairColour = _race.GetRandomHairColour();
            HairStyle = _race.GetRandomHairStyleByGender(_gender);
            if (_gender == EGender.Male) BeardStyle = _race.GetRandomBeardStyle();
            EyeColour = _race.GetRandomEyeColour();

            RefreshSkinTone();
            RefreshHair();
            RefreshEyes();
        }
        
        private void RefreshEyes()
        {
            Eyes = $"{_race.RaceName} Eyes/{Helper.ColorToHex(EyeColour)}/0:0:0";
        }

        private void RefreshHair()
        {
            Hair = $"{HairStyle.ID}/{Helper.ColorToHex(HairColour)}/0:0:0";

            if (BeardStyle == null)
            {
                Beard = "";
            }
            else
            {
                Beard = $"{BeardStyle.ID}/{Helper.ColorToHex(HairColour)}/0:0:0";
            }
        }

        private void RefreshSkinTone()
        {
            Body = $"{_race.RaceName} Body/{Helper.ColorToHex(SkinTone)}/0:0:0";
            Hands = $"{_race.RaceName} Hands/{Helper.ColorToHex(SkinTone)}/0:0:0";

            if (_gender == EGender.Female)
            {
                Blush = $"{_race.RaceName} Blush/{Helper.ColorToHex(SkinTone)}/0:0:0";
            }
            else
            {
                Blush = "";
            }
        }
    }

    public enum EGender
    {
        Male,
        Female,
    }
}
