using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Managers;

namespace Popups
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] Transform _popupCanvas;
        
        [Header("Popups:")]
        [SerializeField] private Popup[] _popups;

        private readonly List<Popup> _popupStack = new List<Popup>();
        private readonly Dictionary<Type, Popup> _cachedPopups = new Dictionary<Type, Popup>();
        private readonly Queue<PopupRequest> RequestQueue = new Queue<PopupRequest>();
        
        public void CreateInstance<T>() where T : Popup
        {
            var prefab = GetPrefab<T>();
            var go = Instantiate(prefab, _popupCanvas);
            go.name = prefab.name;
        }
        
        public bool OpenPopup(Popup popupInstance)
        {
            if (_popupStack.Contains(popupInstance))
                return false;
            
            _popupStack.Add(popupInstance);
            
            return true;
        }
        
        public void QueuePopup<T>(System.Action action) where T : Popup
        {
            PopupRequest request = new PopupRequest();
            request.PopupPrefab = GetPrefab<T>();
            request.OpenAction = action;
            RequestQueue.Enqueue(request);
        }

        private T GetPrefab<T>() where T : Popup
        {
            if (_cachedPopups.ContainsKey(typeof(T)))
            {
                return _cachedPopups[typeof(T)] as T;
            }

            foreach (var popup in _popups)
            {
                var prefab = popup as T;
                if (prefab != null)
                {
                    _cachedPopups[typeof(T)] = prefab;
                    return prefab;
                }
            }
            throw new MissingReferenceException("Prefab not found for type " + typeof(T));
        }

        public void ClosePopup(Popup popup)
        {
            if (_popupStack.Count == 0)
            {
                Debug.LogErrorFormat(popup, "{0} cannot be closed because the popup stack is empty", popup.GetType());
                return;
            }

            if (!_popupStack.Contains(popup))
            {
                Debug.LogErrorFormat(popup, "{0} cannot be closed because it is not in the popup stack", popup.GetType());
                return;
            }
            
            if(popup.RequestQueue.Count > 0)
            {
                System.Action action = popup.RequestQueue.Dequeue();
                if(action != null)
                {
                    action();
                }
            }
            else
            {
                _popupStack.Remove(popup); 
                Destroy(popup.gameObject);
                
            }
        }

        public void CloseTopPopup()
        {
            var popupInstance = _popupStack.Last();
            ClosePopup(popupInstance);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_popupStack.Count > 0)
                {
                    _popupStack.Last().OnBackPressed();
                }
                else
                {
                    PauseMenuPopup.Show();
                }
            }
        }
    }
    
    public class PopupRequest
    {
        public Popup PopupPrefab;
        public Action OpenAction;
    }
}
