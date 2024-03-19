/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using Databrain.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#pragma warning disable 0162
namespace Databrain
{
    [CreateAssetMenu(menuName = "Databrain / Theme Template")]
    public class DatabrainThemeTemplate : ScriptableObject
    {

        [System.Serializable]
        public class SerializedGroup
        {
            public lightTheme light = new lightTheme();
            public darkTheme dark = new darkTheme();

            [System.Serializable]
            public class lightTheme
            {
                public VisualTreeAsset visualAsset;
                public VisualTreeAsset foldoutAsset;
                public VisualTreeAsset typeListElementAsset;
                public VisualTreeAsset dataListElementAsset;
                public VisualTreeAsset searchFieldAsset;
                public VisualTreeAsset moduleButtonAsset;
                public StyleSheet styleSheet;
            }

            [System.Serializable]
            public class darkTheme
            {
                public VisualTreeAsset visualAsset;
                public VisualTreeAsset foldoutAsset;
                public VisualTreeAsset typeListElementAsset;
                public VisualTreeAsset dataListElementAsset;
                public VisualTreeAsset searchFieldAsset;
                public VisualTreeAsset moduleButtonAsset;
                public StyleSheet styleSheet;
            }

            public SerializedGroup() { }
        }

        public SerializedGroup serializedGroup = new SerializedGroup();
    }
}
#pragma warning restore