using System.Collections.Generic;
using Controllers;
using Managers;
using UnityEngine;

namespace Popups
{
    public abstract class Popup<T> : Popup where T : Popup<T>
    {
        public static T Instance { get; private set; }
        protected Animator _animator;
        protected static bool _shouldPause;
        protected static GameSpeed _prevSpeed;
        
        private static readonly int MoveOut = Animator.StringToHash("MoveOut");
        private static readonly int MoveIn = Animator.StringToHash("MoveIn");

        protected virtual void Awake() {
            Instance = (T) this;
            _animator = GetComponent<Animator>();
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
                Instance._animator.SetTrigger(MoveIn);
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
                    Instance._animator.SetTrigger(MoveIn);
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
            Instance._animator.SetTrigger(MoveOut);
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
        
        void AnimMoveOutHandler()
        {
            Close();
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
