/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Databrain.UI.Elements
{
	public class TabsElement : VisualElement
	{
		public List<VisualElement> buttons = new List<VisualElement>();

		public void SetHighlight(int _index)
		{
			buttons[_index].style.borderTopWidth = 2;
            buttons[_index].style.borderBottomWidth = 2;
            buttons[_index].style.borderLeftWidth = 2;
            buttons[_index].style.borderRightWidth = 2;
            buttons[_index].style.borderBottomColor = Color.white;
            buttons[_index].style.borderTopColor = Color.white;
            buttons[_index].style.borderLeftColor = Color.white;
            buttons[_index].style.borderRightColor = Color.white;

			for (int i = 0; i < buttons.Count; i++)
			{
				if (i != _index)
				{
                    buttons[i].style.borderTopWidth = 0;
                    buttons[i].style.borderBottomWidth = 0;
                    buttons[i].style.borderLeftWidth = 0;
                    buttons[i].style.borderRightWidth = 0;
                }
			}
        }

		public TabsElement(List<string> _options, List<Action> _callbacks)
		{
            buttons = new List<VisualElement>();

            //var _root = new VisualElement();
			this.style.marginBottom = 10;

			var _topBar = new VisualElement();
			_topBar.style.flexDirection = FlexDirection.Row;
			_topBar.style.backgroundColor = new Color(50f / 255f, 50f / 255f, 50f / 255f);
			_topBar.style.paddingBottom = 5;
			_topBar.style.paddingTop = 5;
			_topBar.style.paddingLeft = 5;
			_topBar.style.paddingRight = 5;

			var _line = new VisualElement();
			_line.style.height = 1;
			_line.style.backgroundColor = new Color(40f / 255f, 40f / 255f, 40f / 255f);

			for (int i = 0; i < _options.Count; i++)
			{
				var _button = new Button();
				_button.text = _options[i];
				_button.name = _options[i];

				buttons.Add(_button);

                if (i == 0)
				{
					_button.style.borderTopWidth = 2;
					_button.style.borderBottomWidth = 2;
					_button.style.borderLeftWidth = 2;
					_button.style.borderRightWidth = 2;
					_button.style.borderBottomColor = Color.white;
					_button.style.borderTopColor = Color.white;
					_button.style.borderLeftColor = Color.white;
					_button.style.borderRightColor = Color.white;
				}

				int j = i;

				_button.RegisterCallback<ClickEvent>(click =>
				{
					if (_callbacks[j] != null)
					{
						_callbacks[j].Invoke();
					}

					var _buttonList = _topBar.Query<Button>().ToList();

					for (int b = 0; b < _buttonList.Count; b++)
					{
						if (_buttonList[b].name != _button.name)
						{
							_buttonList[b].style.borderTopWidth = 0;
							_buttonList[b].style.borderBottomWidth = 0;
							_buttonList[b].style.borderLeftWidth = 0;
							_buttonList[b].style.borderRightWidth = 0;
						}
						else
						{
							_buttonList[b].style.borderTopWidth = 2;
							_buttonList[b].style.borderBottomWidth = 2;
							_buttonList[b].style.borderLeftWidth = 2;
							_buttonList[b].style.borderRightWidth = 2;
							_buttonList[b].style.borderBottomColor = Color.white;
							_buttonList[b].style.borderTopColor = Color.white;
							_buttonList[b].style.borderLeftColor = Color.white;
							_buttonList[b].style.borderRightColor = Color.white;
						}
					}

				});

				_topBar.Add(_button);
			}

            this.Add(_topBar);
            this.Add(_line);

		}

	}
}
#endif