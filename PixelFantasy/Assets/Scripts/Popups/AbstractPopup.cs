using System.Collections.Generic;
using Controllers;
using DG.Tweening;
using Managers;
using UnityEngine;

namespace Popups
{
    public abstract class Popup<T> : Popup where T : Popup<T>
    {
        public static T Instance { get; private set; }
        protected CanvasGroup _canvasGroup;
        protected static bool _shouldPause;
        protected static GameSpeed _prevSpeed;

        private const float FADE_DURATION = 0.25f;

        protected virtual void Awake() {
            Instance = (T) this;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void OnDestroy() {
            Instance = null;
        }

        protected static void Open(System.Action action = null, bool pauseGame = false)
        {
            _shouldPause = pauseGame;
            if (Instance == null) {
                PopupManager.Instance.CreateInstance<T>();
                PopupManager.Instance.OpenPopup(Instance);
                Instance._canvasGroup.alpha = 0;
                Instance._canvasGroup.DOFade(1, FADE_DURATION);
                Instance.State = Popup.PopupState.MoveIn;

                if (_shouldPause && TimeManager.Instance != null)
                {
                    _prevSpeed = TimeManager.Instance.GameSpeed;
                    TimeManager.Instance.SetGameSpeed(GameSpeed.Paused);
                }

                if(action != null)
                {
                    action();
                }
            }
            else
            {
                System.Action combinedAction = ()=> {
                    Instance._canvasGroup.alpha = 0;
                    Instance._canvasGroup.DOFade(1, FADE_DURATION);
                    Instance.State = Popup.PopupState.MoveIn;

                    if(action != null)
                    {
                        action();
                    }
                };
                Instance.RequestQueue.Enqueue(combinedAction);
            }
        }

        public static void Hide() {
            Instance.State = Popup.PopupState.MoveOut;
            Instance._canvasGroup.DOFade(0, FADE_DURATION).OnComplete(Close);
        }

        protected static void Close() {
            if (Instance == null) {
                Debug.LogErrorFormat("Trying to close popup {0} but Instance is null", typeof(T));
                return;
            }

            if (_shouldPause && TimeManager.Instance != null)
            {
                TimeManager.Instance.SetGameSpeed(_prevSpeed);
            }
            
            PopupManager.Instance.ClosePopup(Instance);
        }
    }
    
    public abstract class Popup : MonoBehaviour {
        public abstract void OnBackPressed();
        public void OnPopupIn()
        {
            State = PopupState.In;
        }
        public readonly Queue<System.Action> RequestQueue = new Queue<System.Action>();
        public enum PopupState {New,MoveIn,In,MoveOut,Out}
        public PopupState State;
    }
}
