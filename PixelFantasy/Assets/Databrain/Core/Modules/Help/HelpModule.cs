/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

using Databrain.UI;

namespace Databrain.Modules
{
	[DatacoreModuleAttribute("Help", 3, "help.png")]
	public class HelpModule : DatabrainModuleBase
	{
		[SerializeField]
		private DatabrainWelcome welcomeWindow;

		public override UnityEngine.UIElements.VisualElement DrawGUI(DataLibrary _container, DatabrainEditorWindow _editorWindow)
		{
			if (welcomeWindow == null)
			{
                welcomeWindow = EditorWindow.CreateInstance<DatabrainWelcome>();
			}

			var _root = new VisualElement();
			_root.style.flexGrow = 1;

            welcomeWindow.WelcomeGUI(_root, 2, false);
			

			return _root;
		}
	}
}
#endif