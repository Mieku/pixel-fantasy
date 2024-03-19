/*
 *	DATABRAIN | Blackboard
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using Databrain.Attributes;
using Databrain.Blackboard;
using System.Reflection;

namespace Databrain.Events
{
    [DataObjectIcon("event2", DatabrainColor.Black)]
    [DataObjectTypeName("Blackboard Events")]
    public class BlackboardEvent : DataObject
    {
        private List<Action<BlackboardVariable>> listeners = new List<Action<BlackboardVariable>>();

       
        public void Raise(BlackboardVariable _variable)
        {
            for (int a = listeners.Count - 1; a >= 0; a--)
            {
                listeners[a].Invoke(_variable);
            }
        }


        public void RegisterListener(Action<BlackboardVariable> _action)
        {
            if (!ActionExistsInListener(_action))
            {
                listeners.Add(_action);
            }
        }


        public void UnregisterListener(Action<BlackboardVariable> _action)
        {
            if (ActionExistsInListener(_action))
            {
                listeners.Remove(_action);
            }
        }

        bool ActionExistsInListener(Action<BlackboardVariable> _action)
        {
            for (int i = 0; i < listeners.Count; i ++)
            {
                if (listeners[i].Target.GetType() == _action.Target.GetType() && listeners[i].GetMethodInfo().Name == _action.GetMethodInfo().Name)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public override VisualElement EditorGUI(SerializedObject _object, DatabrainEditorWindow _editorWindow)
        {
            var _raiseButton = new Button();
            _raiseButton.text = "Raise";
            _raiseButton.style.height = 40;

            _raiseButton.RegisterCallback<ClickEvent>(click =>
            {
                Raise(null);
            });

            return _raiseButton;
        }
#endif
    }
}