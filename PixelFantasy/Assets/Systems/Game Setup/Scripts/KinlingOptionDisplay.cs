using Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class KinlingOptionDisplay : MonoBehaviour
    {
        [SerializeField] private Image _avatar;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private TextMeshProUGUI _historyText;
        [SerializeField] private Image _bg;
        [SerializeField] private Sprite _defaultBG, _activeBG;

        public KinlingData KinlingData;
        private ChooseKinlingsPanel _parent;

        public void Init(KinlingData kinlingData, ChooseKinlingsPanel parent)
        {
            KinlingData = kinlingData;
            _parent = parent;

            _nicknameText.text = KinlingData.Nickname;
            _historyText.text = KinlingData.Stats.History.HistoryName;
            _avatar.sprite = KinlingData.Avatar.GetBaseAvatarSprite();
            
            SetHighlight(false);
        }

        public void OnPressed()
        {
            _parent.OnKinlingSelected(KinlingData);
        }

        public void SetHighlight(bool showHighlight)
        {
            if (showHighlight)
            {
                _bg.sprite = _activeBG;
            }
            else
            {
                _bg.sprite = _defaultBG;
            }
        }
    }
}
