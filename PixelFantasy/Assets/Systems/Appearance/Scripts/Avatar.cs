using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Appearance.Scripts
{
    public class Avatar : MonoBehaviour
    {
        public SpriteRenderer Appearance;
        public Animator Animator;
        public AudioSource AudioSource;
        public AppearanceBuilder AppearanceBuilder;

        private AvatarLayer.EAppearanceDirection _direction;
        private bool _isFlipped;

        public AvatarLayer.EAppearanceDirection GetDirection()
        {
            return _direction;
        }
        
        [Button("Set Direction")]
        public void SetDirection(AvatarLayer.EAppearanceDirection direction)
        {
            _direction = direction;
            AppearanceBuilder.Rebuild();

            switch (direction)
            {
                case AvatarLayer.EAppearanceDirection.Down:
                case AvatarLayer.EAppearanceDirection.Up:
                case AvatarLayer.EAppearanceDirection.Right:
                    Appearance.flipX = false;
                    break;
                case AvatarLayer.EAppearanceDirection.Left:
                    Appearance.flipX = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
