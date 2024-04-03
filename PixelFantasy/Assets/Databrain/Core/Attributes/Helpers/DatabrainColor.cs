/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;

namespace Databrain.Attributes
{
    public enum DatabrainColor
    {
        Clear,
        White,
        Black,
        DarkGrey,
        Grey,
        LightGrey,
        Red,
        Rose,
        Orange,
        Yellow,
        Green,
        Blue,
        DarkBlue,
        Indigo,
        Coal,
        Gold
    }

    public static class DatabrainColorColorExtensions
    {
        public static Color GetColor(this DatabrainColor color)
        {
            switch (color)
            {
                case DatabrainColor.Clear:
                    return new Color(0f / 255f, 0f / 255f, 0f / 255f, 0f / 255f);
                case DatabrainColor.White:
                    return new Color32(255, 255, 255, 255);
                case DatabrainColor.Black:
                    return new Color32(0, 0, 0, 255);
                case DatabrainColor.DarkGrey:
                    return new Color(40f / 255f, 40f / 255f, 40f / 255f);
                case DatabrainColor.Grey:
                    return new Color(80f / 255f, 80f / 255f, 80f / 255f);
                case DatabrainColor.LightGrey:
                    return new Color(110f / 255f, 110f / 255f, 110f / 255f);
                case DatabrainColor.Red:
                    return new Color32(243, 24, 24, 255);
                case DatabrainColor.Rose:
                    return new Color32(255, 146, 239, 255);
                case DatabrainColor.Orange:
                    return new Color32(229, 103, 41, 255);
                case DatabrainColor.Yellow:
                    return new Color32(255, 211, 0, 255);
                case DatabrainColor.Green: //#1CFF7D
                    return new Color32(28, 255, 125, 255);
                case DatabrainColor.Blue: //#16ACFF
                    return new Color32(22, 172, 255, 255);
                case DatabrainColor.DarkBlue: //#16ACFF
                    return new Color(0f / 255f, 55f / 255f,  85f / 255f, 255f / 255f);
                case DatabrainColor.Indigo: // #A353E6
                    return new Color32(163, 83, 230, 255);
                case DatabrainColor.Coal: //#755F55
                    return new Color32(35, 34, 30, 255);
                case DatabrainColor.Gold: //#D0A844
                    return new Color32(208, 168, 68, 255);
                default:
                    return new Color32(0, 0, 0, 255);
            }
        }
    }
}
