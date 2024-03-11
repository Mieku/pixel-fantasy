/*
 *	DATABRAIN | Events
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine.UIElements;

using Databrain.Attributes;

namespace Databrain.Events
{
	[HideDataObjectType]
	[DataObjectIcon("event2", DatabrainColor.Gold)]
    [DataObjectOrder(200)]
    public class DatabrainGenericEvent<T> : DatabrainEvent
	{
		private List<Action<T>> listeners = new List<Action<T>>();



		public void Raise(T eventData)
		{
			base.Raise();

			for (int a = listeners.Count - 1; a >= 0; a--)
			{
				listeners[a].Invoke(eventData);
			}
		}


		public void RegisterListener(Action<T> _action)
		{
			if (!listeners.Contains(_action))
			{
				listeners.Add(_action);
			}
		}


		public void UnregisterListener(Action<T> _action)
		{
			if (listeners.Contains(_action))
			{
				listeners.Remove(_action);
			}
		}

#if UNITY_EDITOR
		public override VisualElement EditorGUI(SerializedObject _object, DatabrainEditorWindow _editorWindow)
		{
			var _root = new VisualElement();
			//var _listenersContainer = new VisualElement();

			//for (int i = 0; i < listeners.Count; i ++)
			//{
			//	var _label = new Label();
			//	_label.text = listeners[i].Method.Name;

			//	_listenersContainer.Add(_label);
			//}

			var _raiseButton = new Button();
			_raiseButton.text = "Raise";
			_raiseButton.style.height = 40;

			_raiseButton.RegisterCallback<ClickEvent>(click =>
			{
				Raise();
			});

			//_root.Add(_listenersContainer);
			_root.Add(_raiseButton);

			return _root;
		}
#endif
	}
}