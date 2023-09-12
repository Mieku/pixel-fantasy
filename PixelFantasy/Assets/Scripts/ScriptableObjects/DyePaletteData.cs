using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DyePaletteData", menuName = "CraftedData/DyePaletteData", order = 1)]
    public class DyePaletteData : ScriptableObject
    {
        [ColorUsage(true, true)][Tooltip("Recolours Red")] public Color Primary;
        [ColorUsage(true, true)][Tooltip("Recolours Green")] public Color PrimaryShade;
        [ColorUsage(true, true)][Tooltip("Recolours Blue")] public Color Accent;
    }
}
