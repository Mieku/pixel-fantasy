/*
 *	DATABRAIN
 *	(c) 2024 Giant Grey
 *	www.databrain.cc
 *	
 */
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector.Editor;


namespace Databrain.Attributes
{
    /// <summary>
    /// Hides default fields when drawing data object fields with Odin Inspector
    /// </summary>
    public class OdinHideAttributeDrawer : OdinAttributeDrawer<OdinHideAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label){}
    }
}
#endif
#endif