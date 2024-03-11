/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.Callbacks;

using Databrain.Helpers;

namespace Databrain.UI
{

	[CustomEditor(typeof(DataLibrary))]
	public class DataLibraryInspector : UnityEditor.Editor
    {
		
		public DataLibrary container;

        void OnEnable()
        {
            try
            {
                container = (DataLibrary)target;
            }
            catch { }
        }

		
		[OnOpenAsset]
		public static bool OnOpenAsset(int instanceID, int line)
		{

            var _window = DatabrainHelpers.OpenEditor(instanceID, false);
            if (_window != null)
            {
                return true;
            }
           
            return false;
            
        }


        public override VisualElement CreateInspectorGUI()
		{
            var _logo = new VisualElement();
            _logo.style.backgroundImage = DatabrainHelpers.LoadLogoLarge();
            _logo.style.width = 218;
            _logo.style.height = 144;
            _logo.style.alignSelf = Align.Center;
            _logo.style.marginBottom = 20;
            _logo.style.marginTop = 20;
            _logo.style.marginLeft = 20;
            _logo.style.marginRight = 20;


            return _logo;
		}
	}
}
#endif