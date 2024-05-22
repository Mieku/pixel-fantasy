/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using Databrain.Attributes;
using Databrain.Helpers;

using UnityEngine;
using UnityEngine.UIElements;


namespace Databrain.UI.Elements
{
    public class FoldoutElement : VisualElement
    {
        private VisualElement header;
        private Button foldoutButton;
        private Label title;
        private VisualElement container;

        private float totalHeight = 0;
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

        private List<VisualElement> items = new List<VisualElement>();

        public FoldoutElement(string _title, Action<bool> _onFoldoutCallback, params Action[] _additionalButtons)
        {
            totalHeight = 0;
            arrowRight = DatabrainHelpers.LoadIcon("arrowRight");
            arrowDown = DatabrainHelpers.LoadIcon("arrowDown");
            onFoldout = _onFoldoutCallback;

            style.flexGrow = 1;

            items = new List<VisualElement>();

            header = new VisualElement();

            header.style.marginTop = 5;
            header.style.flexDirection = FlexDirection.Row;
            header.style.backgroundColor = DatabrainColor.Grey.GetColor();
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = DatabrainColor.DarkGrey.GetColor();

            foldoutButton = DatabrainHelpers.DatabrainButton("");
            foldoutButton.style.backgroundImage = arrowRight;
            foldoutButton.style.width = 24;
            foldoutButton.style.height = 24;
            foldoutButton.RegisterCallback<ClickEvent>(click =>
            {
                //OpenFoldout(container.style.height == 0 ? true : false);
                OpenFoldout(container.style.display == DisplayStyle.None ? true : false);
            });

            title = new Label();
            title.text = _title;
            title.style.marginLeft = 10;
            title.style.unityTextAlign = TextAnchor.MiddleLeft;

            container = new VisualElement();
            container.style.transitionDuration = new List<TimeValue> { new TimeValue(0.1f) };
            //container.style.height = 0;
            container.style.overflow = Overflow.Hidden;
            container.style.display = DisplayStyle.None;
            DatabrainHelpers.SetBorder(container, 1, DatabrainColor.Grey.GetColor());



            header.Add(foldoutButton);
            header.Add(title);

            Add(header);
            Add(container);


        }

        public void AddContent(VisualElement _content)
        {
            items.Add(_content);
            this.Add(_content);
            this.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            this.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

            for (int i = 0; i < items.Count; i++)
            {
                totalHeight += items[i].resolvedStyle.height;
                container.Add(items[i]);
            }
        }


        public void OpenFoldout(bool _open)
        {
            if (_open)
            {
                _value = true;
                container.style.display = DisplayStyle.Flex;
                DatabrainHelpers.SetPadding(container, 5, 5, 5, 5);
                foldoutButton.style.backgroundImage = arrowDown;
            }
            else
            {
                _value = false;
                container.style.display = DisplayStyle.None;
                DatabrainHelpers.SetPadding(container, 0, 0, 0, 0);
                foldoutButton.style.backgroundImage = arrowRight; 
            }

            onFoldout?.Invoke(_open);
            //container.style.height = _open ? totalHeight : 0;
        }
    }
}
#endif