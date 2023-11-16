using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Systems.Notifications.Scripts
{
    public class Toast : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _toastMsg;

        private CanvasGroup _canvasGroup;
        private const float DISPLAY_LENGTH = 1f;
        private const float FADE_IN_DURATION = 0.2f;
        private const float FADE_OUT_DURATION = 0.5f;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        public void Init(string message)
        {
            _toastMsg.text = message;
            StartCoroutine(ToastSequence());
        }

        IEnumerator ToastSequence()
        {
            StartCoroutine(FadeIn());
            
            yield return new WaitForSeconds(DISPLAY_LENGTH);
            
            StartCoroutine(FadeOut());
        }
        
        
        IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            var curAlpha = _canvasGroup.alpha;

            while (elapsedTime < FADE_IN_DURATION)
            {
                float value = Mathf.Lerp(curAlpha, 1f, elapsedTime / FADE_IN_DURATION);
                _canvasGroup.alpha = value;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 1;
        }

        IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            var curAlpha = _canvasGroup.alpha;

            while (elapsedTime < FADE_OUT_DURATION)
            {
                float value = Mathf.Lerp(curAlpha, 0f, elapsedTime / FADE_OUT_DURATION);
                _canvasGroup.alpha = value;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _canvasGroup.alpha = 0;
            
            Destroy(gameObject);
        }
    }
}
