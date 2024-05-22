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
    public class ToggleButton : VisualElement
    {
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
                SetBorder();
            }
        }


        public ToggleButton(string label, bool value)
		{
            _value = value;

            var _label = new Label(label);
            DatabrainHelpers.SetMargin(_label, 5, 5, 5, 5);
            Add(_label);

            this.RegisterCallback<ClickEvent>(evt => 
            {
                _value = !_value;
                SetBorder();
                evt.StopPropagation();
            });
        }

        void SetBorder()
        {
            if (_value)
            {
                DatabrainHelpers.SetBorder(this, 2, Color.white);
            }
            else
            {
                DatabrainHelpers.SetBorder(this, 0, Color.white);
            }
        }

    }
}
#endif