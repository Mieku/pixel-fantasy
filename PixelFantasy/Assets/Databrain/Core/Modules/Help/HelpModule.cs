/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine.UIElements;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Databrain.UI;

namespace Databrain.Modules
{
	#if UNITY_EDITOR
	[DatabrainModuleAttribute("Help", 3, "help.png")]
	#endif
	public class HelpModule : DatabrainModuleBase
	{
		#if UNITY_EDITOR
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
		
		#endif
	}
}
