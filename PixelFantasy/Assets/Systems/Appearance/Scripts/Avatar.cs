using UnityEngine;

namespace Systems.Appearance.Scripts
{
    public class Avatar : MonoBehaviour
    {
        public SpriteRenderer Appearance;
        public Animator Animator;
        public AudioSource AudioSource;

        public AvatarLayer.EAppearanceDirection Direction;
    }
}
