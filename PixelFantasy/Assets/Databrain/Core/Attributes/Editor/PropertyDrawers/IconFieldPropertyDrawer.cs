/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using UnityEngine.UIElements;
using UnityEditor.UIElements;

using Databrain.Helpers;
using System.Threading.Tasks;

namespace Databrain.Attributes
{
	[CustomPropertyDrawer(typeof(IconFieldAttribute))]
	public class IconFieldAttributePropertyDrawer : PropertyDrawer
	{
		VisualElement _icon;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var _root = new VisualElement();

			if (_icon == null)
			{
				_icon = new VisualElement();
				_icon.style.marginLeft = 140;
				_icon.style.width = 70;
				_icon.style.height = 70;
				DatabrainHelpers.SetBorder(_icon, 1, Color.black);
			}

			var _prop = new PropertyField();
			_prop.label = property.displayName;

			_prop.BindProperty(property);
			_prop.RegisterCallback<ChangeEvent<Object>>(x =>
			{
				if (property.objectReferenceValue != null)
				{
                    _icon.style.display = DisplayStyle.Flex;

                    if (x.previousValue != x.newValue)
					{
						if ((property.objectReferenceValue as Sprite) != null)
						{
							_icon.style.backgroundImage = (property.objectReferenceValue as Sprite).texture;
							property.serializedObject.Update();
							property.serializedObject.ApplyModifiedProperties();

							RebuildList(property);
						}
                    }
				}
				else
				{
					_icon.style.display = DisplayStyle.None; 
                }
			});


			_root.Add(_icon);
			_root.Add(_prop);

			return _root;
		}

		async void RebuildList(SerializedProperty property)
		{
			await Task.Delay(200);

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