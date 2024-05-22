/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using System.Threading.Tasks;
using Databrain.Helpers;

using UnityEngine;


namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(ColorFieldAttribute))]
	public class ColorFieldPropertyDrawer : PropertyDrawer
	{

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var _root = new VisualElement();

			var _colorField = new ColorField();
			_colorField.BindProperty(property);
			_colorField.label = property.displayName;

			// Mirror the value of the UXML field into the C# field.
			_colorField.RegisterCallback<ChangeEvent<Color>>((evt) =>
			{
				property.serializedObject.Update();
				property.serializedObject.ApplyModifiedProperties();

				RebuildList(property);

				evt.StopPropagation();
			});
			/// </sample>

			_root.Add(_colorField);

			return _root;
		}

		async void RebuildList(SerializedProperty property)
		{	
			await Task.Delay(200);

			if (property.serializedObject.FindProperty("relatedLibraryObject") == null)
				return;

			var _editorWindow = DatabrainHelpers.GetOpenEditor((property.serializedObject.FindProperty("relatedLibraryObject").objectReferenceValue as DataLibrary).GetInstanceID());
			if (_editorWindow != null)
			{
				if (_editorWindow.dataTypelistView != null)
				{
					var _index = (property.serializedObject.FindProperty("index").intValue);

					_editorWindow.dataTypelistView.RefreshItem(_index);
				}
			}
		}
	}
}
#endif