using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    private static GameSettings _instance;

    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameSettings>($"Settings/GameSettings");
            }
            return _instance;
        }
    }

    [BoxGroup("Social"), ShowInInspector] public float BasePregnancyChance { get; private set; } = 50f;
}
