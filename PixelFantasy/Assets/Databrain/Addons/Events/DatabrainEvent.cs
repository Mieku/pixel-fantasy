/*
 *	DATABRAIN | Events
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using Databrain.Helpers;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Databrain.Events
{
    /// <summary>
    /// Databrain Event is the base class of all event objects
    /// It is possible to call and listen to a global databrain event
    /// but it's not possible to assign parameters. For parameters
    /// derive from DatabrainGenericEvent class
    /// </summary>
    [DataObjectIcon("event2", DatabrainColor.Gold)]
    [DataObjectTypeName("Databrain Events")]
    [DataObjectFirstClassType("Events", "event2", DatabrainColor.Gold)]
    public class DatabrainEvent : DataObject
	{
        private List<Action> listeners = new List<Action>();

        public override void OnEnd()
        {
            listeners = new List<Action>();
        }

        public void Raise()
        {
            for (int a = listeners.Count - 1; a >= 0; a--)
            {
                listeners[a].Invoke();
            }  
        }

        public void RegisterListener(Action _action)
        {
            if (!listeners.Contains(_action))
            {
                listeners.Add(_action);
            }
        }

        public void UnregisterListener(Action _action)
        {
            if (listeners.Contains(_action))
            {
                listeners.Remove(_action);
            }
        }

#if UNITY_EDITOR
        public override VisualElement EditorGUI(SerializedObject _object, DatabrainEditorWindow _editorWindow)
        {
            var _container = new VisualElement();
       
            var _infoText = new Label();
            _infoText.text = "Raise this event. Works only at runtime and if the event has any listeners.";
            _infoText.style.backgroundColor = DatabrainColor.Grey.GetColor();
            DatabrainHelpers.SetPadding(_infoText, 10, 10, 10, 10);


            var _raiseButton = new Button();
            _raiseButton.text = "Raise";
            _raiseButton.style.height = 40;

            _raiseButton.RegisterCallback<ClickEvent>(click =>
            {
                Raise();
            });

            _container.Add(_infoText);
            _container.Add(_raiseButton);

            return _container;
        }
#endif
    }
}