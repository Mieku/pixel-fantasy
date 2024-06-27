using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "DyeSettings", menuName = "Settings/Dye Settings")]
    public class DyeSettings : ScriptableObject
    {
        [SerializeField] private string _colourName;
        
        [ColorUsage(true, true)][Tooltip("Recolours Red")] public Color Primary;
        [ColorUsage(true, true)][Tooltip("Recolours Green")] public Color PrimaryShade;
        [ColorUsage(true, true)][Tooltip("Recolours Blue")] public Color Accent;
        
        public string ColourName => _colourName;
    }
}
