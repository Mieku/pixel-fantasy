
using UnityEngine;
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using Databrain.Attributes;


namespace Databrain.Examples
{
    [DataObjectTypeName("05_OdinInspector")]
    [UseOdinInspector]
    public class MyOdinDataObject : DataObject
    {
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3 || ODIN_INSPECTOR_3_1
        [Sirenix.OdinInspector.InfoBox("This DataObject uses Odin Inspector attributes to create a custom UI - Simply use the attribute [UseOdinInspector] to enable Odin Inspector support for specific class.")]
        public string dataName;

        [FoldoutGroup("Foldout")] public float foldout1;
        [FoldoutGroup("Foldout")] public float foldout2;
        [FoldoutGroup("Foldout")] public float foldout3;
        
        [AssetList]
        public GameObject prefab;

        [TabGroup("Movement")]
        public float movementSpeed;
        public float rotationSpeed;

        [TabGroup("Properties")]
        public float health;
        public float strength;

        [ColorPalette("Underwater")]
        public Color underwaterColor;

        public enum Test 
        {
            a,
            b,
            c
        }

        [OnValueChanged("StateChanged")]
        [EnumToggleButtons]
        public Test enumTest;

        [ListDrawerSettings(ShowPaging = false)]
        public List<string> myList = new List<string>();

        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, List<int>> StringListDictionary = new Dictionary<string, List<int>>()
        {
            { "Numbers", new List<int>(){ 1, 2, 3, 4, } },
        };

        [Button(ButtonSizes.Medium)]
        public void TestMethod()
        {
            Debug.Log("CLICK");
        }

        public void StateChanged()
        {
            Debug.Log("State changed to " + enumTest);
        }
#endif
    }
}
