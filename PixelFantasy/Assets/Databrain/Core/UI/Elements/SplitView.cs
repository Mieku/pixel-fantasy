
/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace Databrain.UI.Elements
{

	#if UNITY_6000_0_OR_NEWER
	[UxmlElement("DatabrainSplitView")]
	// #endif
	public partial class SplitView : TwoPaneSplitView
	{
		[UxmlAttribute("defaultValue")]
		float defaultValue; 
	}
	#else
	
	public class SplitView : TwoPaneSplitView
	{
		public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }
	}

	#endif
}
#endif