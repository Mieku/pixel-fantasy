/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
#if UNITY_EDITOR
using UnityEngine.UIElements;
using Databrain.Helpers;

namespace Databrain.UI.Elements
{
	public static class NamespaceFoldoutElement
	{
		public static Foldout Foldout(VisualElement _target, string _name)
		{
			VisualTreeAsset _foldoutAsset = DatabrainHelpers.GetVisualAsset("BaseFoldout.uxml");

			_foldoutAsset.CloneTree(_target);

			VisualElement _baseFoldout = _target.Q<VisualElement>("baseFoldout");
			_baseFoldout.name = _name;

			Foldout _foldout = _baseFoldout.Q<Foldout>("foldout");
			_foldout.name = _name;


			VisualElement _foldoutChecked = _baseFoldout.Q<VisualElement>("foldoutChecked");

			_foldout.RegisterCallback<MouseOverEvent>(mouseOverEvent =>
			{
				_foldoutChecked.EnableInClassList("baseFoldout--checked", true);
			});
			_foldout.RegisterCallback<MouseLeaveEvent>(mouseLeaveEvent =>
			{
				_foldoutChecked.EnableInClassList("baseFoldout--checked", false);
			});


			_foldout.text = _name;

			_target.Add(_baseFoldout);


			return _foldout;
		}
	}
}
#endif