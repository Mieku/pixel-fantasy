/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using UnityEngine;

namespace Databrain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)] 
	public class TitleAttribute : PropertyAttribute 
	{
		public string title;
		public Color borderColor;
		public Color textColor;
		private const DatabrainColor defaultBorderColor = DatabrainColor.Clear;
        private const DatabrainColor defaultTextColor = DatabrainColor.LightGrey;

        public TitleAttribute(string _title, DatabrainColor _textColor = defaultTextColor, DatabrainColor _borderColor = defaultBorderColor)
		{
			title = _title;
			borderColor = _borderColor.GetColor();
			textColor = _textColor.GetColor();
		}
	}
}