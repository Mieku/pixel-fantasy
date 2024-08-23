using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationToTime : MonoBehaviour
{
    private Animator _anim;
    
    private void Awake()
    {
        _anim = GetComponent<Animator>();
        
        GameEvents.OnGameSpeedChanged += OnSpeedUpdated;
    }

    private void OnDestroy()
    {
        GameEvents.OnGameSpeedChanged -= OnSpeedUpdated;
    }
    
    private void OnSpeedUpdated(float speedMod)
    {
        _anim.speed = speedMod;
    }
}
