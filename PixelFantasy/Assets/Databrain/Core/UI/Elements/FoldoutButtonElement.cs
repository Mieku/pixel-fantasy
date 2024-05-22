/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using Databrain.Helpers;

using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.UI.Elements
{   
    /// <summary>
    /// Similar to DatabrainFoldoutElement but contains only the button.
    /// </summary>
    public class FoldoutButtonElement : VisualElement
    {
        private Button foldoutButton;  
        private Texture2D arrowRight;
        private Texture2D arrowDown;

        public Action<bool> onFoldout;

        private bool _value;
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OpenFoldout(_value);
            }
        }

        public FoldoutButtonElement( Action<bool> _onFoldoutCallback)
        {
            arrowRight = DatabrainHelpers.LoadIcon("arrowRight");
            arrowDown = DatabrainHelpers.LoadIcon("arrowDown");
            onFoldout = _onFoldoutCallback;

            style.marginLeft = -5;
            style.marginRight = 5;


            foldoutButton = DatabrainHelpers.DatabrainButton("");
            foldoutButton.style.backgroundImage = arrowRight;
            foldoutButton.style.width = 23;
            foldoutButton.style.height = 23;
            foldoutButton.RegisterCallback<ClickEvent>(click =>
            {
                OpenFoldout(!_value ? true : false);
            });

        
            Add(foldoutButton);
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }


        public void SetOpenWithoutCallback()
        {
            _value = true;            
            foldoutButton.style.backgroundImage = arrowDown; 
        }

        public void SetCloseWithoutCallback()
        {
            _value = false;
            foldoutButton.style.backgroundImage = arrowDown; 
        }

        private void OpenFoldout(bool _open)
        {
            if (_open)
            {
                _value = true;
                foldoutButton.style.backgroundImage = arrowDown;
            }
            else
            {
                _value = false;
                foldoutButton.style.backgroundImage = arrowRight; 
            }

            onFoldout?.Invoke(_open);
        }
    }
}
#endif