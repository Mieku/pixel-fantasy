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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ShowAssetPreviewAttribute : PropertyAttribute
    {
        public const int defaultWidth = 64;
        public const int defaultHeight = 64;

        public int width { get; private set; }
        public int height { get; private set; }

        public ShowAssetPreviewAttribute(int width = defaultWidth, int height = defaultHeight)
        {
            this.width = width;
            this.height = height;
        }
    }
}
