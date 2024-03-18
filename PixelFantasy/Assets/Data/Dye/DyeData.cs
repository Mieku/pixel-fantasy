using Databrain;
using UnityEngine;

namespace Data.Dye
{
    public class DyeData : DataObject
    {
        [ColorUsage(true, true)][Tooltip("Recolours Red")] public Color Primary;
        [ColorUsage(true, true)][Tooltip("Recolours Green")] public Color PrimaryShade;
        [ColorUsage(true, true)][Tooltip("Recolours Blue")] public Color Accent;
    }
}
