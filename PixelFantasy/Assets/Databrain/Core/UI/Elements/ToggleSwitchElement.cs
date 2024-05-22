/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Databrain.Attributes;
using Databrain.Helpers;

using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.UI.Elements
{
    public class ToggleSwitchElement : VisualElement
    {
        private VisualElement switchContainer;
        // private Button foldoutButton;
        private Label label;


        private Color activeColor;
        private Color defaultColor = new Color(55f/255f,55f/255f,55f/255f);
        public Action<bool> onFoldout;
        private VisualElement switchButton;
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
                SwitchToggle(_value);
            }
        }

        // private List<VisualElement> items = new List<VisualElement>();

        public ToggleSwitchElement(string _label, Color _activeColor, Action<bool> _onFoldoutCallback)
        {
            // totalHeight = 0;
            // arrowRight = DatabrainHelpers.LoadIcon("arrowRight");
            // arrowDown = DatabrainHelpers.LoadIcon("arrowDown");
            onFoldout = _onFoldoutCallback;
            activeColor = _activeColor;
            // style.flexGrow = 1;

            // items = new List<VisualElement>();

            // header = new VisualElement();

            // header.style.marginTop = 5;
            // header.style.flexDirection = FlexDirection.Row;
            // header.style.backgroundColor = DatabrainColor.Grey.GetColor();
            // header.style.borderBottomWidth = 1;
            // header.style.borderBottomColor = DatabrainColor.DarkGrey.GetColor();

            // foldoutButton = DatabrainHelpers.DatabrainButton("");
            // foldoutButton.style.backgroundImage = arrowRight;
            // foldoutButton.style.width = 24;
            // foldoutButton.style.height = 24;
            // foldoutButton.RegisterCallback<ClickEvent>(click =>
            // {
            //     //OpenFoldout(container.style.height == 0 ? true : false);
            //     OpenFoldout(container.style.display == DisplayStyle.None ? true : false);
            // });

            this.style.flexDirection = FlexDirection.Row;
            this.style.marginBottom = 10;
            DatabrainHelpers.SetPadding(this, 5, 5, 5, 5);
            this.style.backgroundColor = new Color(100f/255f, 100f/255f, 100f/255f);
            label = new Label();
            label.text = _label;
            label.style.marginLeft = 10;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;

            // container = new VisualElement();
            // container.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) };
            // //container.style.height = 0;
            // container.style.overflow = Overflow.Hidden;
            // container.style.display = DisplayStyle.None;
            // DatabrainHelpers.SetBorder(container, 1, DatabrainColor.Grey.GetColor());



            // header.Add(foldoutButton);
            // header.Add(title);

            switchContainer = new VisualElement();
            switchContainer.style.width = 40;
            switchContainer.style.height = 19;
            switchContainer.style.paddingTop = 1;
            switchContainer.style.borderTopWidth = 2;
            switchContainer.style.borderTopColor = Color.black;
            switchContainer.style.borderLeftColor = Color.black;
            switchContainer.style.borderRightColor = Color.black;
            switchContainer.style.backgroundColor = defaultColor;
            DatabrainHelpers.SetBorderRadius(switchContainer, 9, 9, 9, 9);
            // DatabrainHelpers.SetPadding(this, 2, 2, 2, 2);

            switchButton = new VisualElement();
            switchButton.style.backgroundColor = new Color(210f/255f,210f/255f,210f/255f);
            switchButton.style.width = 14;
            switchButton.style.height = 14;
            switchButton.style.marginLeft = 2;
            
            DatabrainHelpers.SetBorder(switchButton, 2, new Color(220f/255f,220f/255f,220f/255f));
            DatabrainHelpers.SetBorderRadius(switchButton, 10, 10, 10, 10);
            
            this.RegisterCallback<ClickEvent>((evt) => 
            {
                SwitchToggle(_value);
            });

            switchButton.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) };
            switchContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(0.2f) };


            switchContainer.Add(switchButton);

           
            Add(switchContainer);
            Add(label);
            // Add(container);


        }

        // public void AddContent(VisualElement _content)
        // {
        //     items.Add(_content);
        //     this.Add(_content);
        //     this.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        // }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

            // for (int i = 0; i < items.Count; i++)
            // {
            //     totalHeight += items[i].resolvedStyle.height;
            //     container.Add(items[i]);
            // }
        }


        public void SwitchToggle(bool _switchValue)
        {
            if (!_switchValue)
            {
                _value = true;
                // container.style.display = DisplayStyle.Flex;
                // DatabrainHelpers.SetPadding(container, 5, 5, 5, 5);
                // foldoutButton.style.backgroundImage = arrowDown;
                // this.style.alignItems = Align.FlexEnd; 
                switchButton.style.marginLeft = 24;
                switchContainer.style.backgroundColor = activeColor;
            }
            else
            {
                _value = false;
                // container.style.display = DisplayStyle.None;
                // DatabrainHelpers.SetPadding(container, 0, 0, 0, 0);
                // foldoutButton.style.backgroundImage = arrowRight; 
                // this.style.alignItems = Align.FlexStart; 
                switchButton.style.marginLeft = 2;
                switchContainer.style.backgroundColor = defaultColor;
            }

            WaitForToggle();
            //container.style.height = _open ? totalHeight : 0;
        }

        async void WaitForToggle()
        {
            await Task.Delay(200);

            onFoldout?.Invoke(_value);
        }
    }
}
#endif