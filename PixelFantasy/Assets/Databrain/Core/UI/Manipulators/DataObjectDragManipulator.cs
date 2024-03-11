/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;


namespace Databrain.UI.Manipulators
{
	public class DataObjectDragManipulator : Manipulator
	{
		private DataObject dataObject;
		private int index;

		public DataObjectDragManipulator(int _index, VisualElement _target, DataObject _data)
		{
			dataObject = _data;
			index = _index;
			this.target = _target;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
			target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
			target.RegisterCallback<DragUpdatedEvent>(DragUpdateHandler);

			EditorApplication.hierarchyWindowItemOnGUI += (id, rect) => OnDragEnd();
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
			target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);

			EditorApplication.hierarchyWindowItemOnGUI -= (id, rect) => OnDragEnd();
		}

		public void PointerDownHandler(PointerDownEvent _evt)
		{
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.StartDrag("Dragging");
			DragAndDrop.objectReferences = new Object[] { dataObject };

			_evt.StopImmediatePropagation();
		}

		public void DragUpdateHandler(DragUpdatedEvent _evt)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Move;
		}

		public void PointerUpHandler(PointerUpEvent _evt)
		{
			OnDragEnd();

            _evt.StopImmediatePropagation();
        }



		private void OnDragEnd()
		{
			if (Event.current.type == EventType.DragPerform)
			{
				//isDragPerformed = true;
			}
		}
	}
}
#endif