using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class LoadingScreen : Singleton<LoadingScreen>
    {
        [SerializeField] private GameObject _handle;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _loadingInfoText;
        [SerializeField] private Image _fillBarFill;

        private int _totalSteps;
        private int _completedSteps;

        protected override void Awake()
        {
            base.Awake();
            _handle.SetActive(false);
        }

        public void Show(string loadingMsg, string loadingInfoMsg, int totalSteps)
        {
            _handle.SetActive(true);

            _loadingText.text = loadingMsg;
            _loadingInfoText.text = loadingInfoMsg;
            _fillBarFill.fillAmount = 0;
            _totalSteps = totalSteps;
            _completedSteps = 0;
        }

        public void Hide()
        {
            _handle.SetActive(false);
        }

        public void SetLoadingText(string loadingText)
        {
            _loadingText.text = loadingText;
        }

        public void SetLoadingInfoText(string loadingInfoText)
        {
            _loadingInfoText.text = loadingInfoText;
        }

        public void StepCompleted()
        {
            _completedSteps++;
            _fillBarFill.fillAmount = (float)_completedSteps / _totalSteps;
        }
    }
}