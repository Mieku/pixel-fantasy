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
        // Data looks like this: {ID:Overalls}{Colour:#FFFFFF}{Exempt:#FFFFFF,#FFFFFF}{HSV:0:0:0}
        
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
        
        public SpriteLibraryAsset SideSpriteLibraryAsset;
        public SpriteLibraryAsset UpSpriteLibraryAsset;
        public SpriteLibraryAsset DownSpriteLibraryAsset;

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
            Eyes = $"{{ID:{_race.RaceName} Eyes}}{{Colour:{Helper.ColorToHex(EyeColour)}}}";
        }

        private void RefreshHair()
        {
            Hair = $"{{ID:{HairStyle.ID}}}{{Colour:{Helper.ColorToHex(HairColour)}}}";

            if (BeardStyle == null)
            {
                Beard = "";
            }
            else
            {
                Beard = $"{{ID:{BeardStyle.ID}}}{{Colour:{Helper.ColorToHex(HairColour)}}}";
            }
        }

        private void RefreshSkinTone()
        {
            Body = $"{{ID:{_race.RaceName} Body}}{{Colour:{Helper.ColorToHex(SkinTone)}}}";
            Hands = $"{{ID:{_race.RaceName} Hands}}{{Colour:{Helper.ColorToHex(SkinTone)}}}";

            if (_gender == EGender.Female)
            {
                Blush = $"{{ID:{_race.RaceName} Blush}}{{Colour:{Helper.ColorToHex(SkinTone)}}}";
            }
            else
            {
                Blush = "";
            }
        }
        
        public Sprite GetBaseAvatarSprite()
        {
            return SideSpriteLibraryAsset.GetSprite("idle", "0");
        }
    }

    public enum EGender
    {
        Male,
        Female,
    }
}
