using UnityEngine;

namespace Characters
{
    public class KinlingPositionPreview : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _kinlingRenderer;
        [SerializeField] private SpriteRenderer _standCircleRenderer;

        public void Init(KinlingData kinlingData)
        {
            var avatar = kinlingData.Avatar.GetBaseAvatarSprite();
            _kinlingRenderer.sprite = avatar;
        }
    }
}
