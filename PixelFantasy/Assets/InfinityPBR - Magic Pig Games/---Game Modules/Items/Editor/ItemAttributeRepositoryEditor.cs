using System;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemAttributeRepository))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemAttributeRepositoryEditor : Editor
    {
        private ItemAttributeRepository ThisObject;

        private string _lastType = "";
        //private string lastTypeAttribute = "";

        private Vector2 _scrollPos;

        public void OnEnable()
        {
            ThisObject = (ItemAttributeRepository) target;
            ThisObject.PopulateList();
            EditorUtility.SetDirty(ThisObject);
        }
        
        private void OnValidate()
        {
            ThisObject.PopulateList();
        }

        public override void OnInspectorGUI()
        {
            ShowHeader();
            ShowItemAttributes();
            ShowDefaultInspector();
        }

        private void ShowHeader()
        {
            SetBool("Show Header " + name, 
                LeftCheck("Repository Information", 
                    GetBool("Show Header " + name)));
            
            ShowHeaderCopy();
        }

        private void ShowHeaderCopy()
        {
            if (!GetBool("Show Header " + name)) return;

            StartVerticalBox();
            LabelSized("ItemAttribute Repository", 14, true);
            Label("Keep an object with this script in your project. It will allow you to easily reference the " +
                  "ItemObject and ItemAttribute scriptable objects you've created. This script will auto populated " +
                  "with all of the objects in your project each time you view it in the Inspector.", false, true);

            Space();
            StartRow();
            Label("Item Attribute Types: ", 200);
            Label(ThisObject.itemAttributeTypes.Count.ToString());
            EndRow();
            StartRow();
            Label("Item Attributes: ", 200);
            Label(ThisObject.scriptableObjects.Count.ToString());
            EndRow();
            EndVerticalBox();
        }
        
        private void ShowItemAttributes()
        {
            foreach (ItemAttribute itemAttribute in ThisObject.scriptableObjects)
                ShowAttribute(itemAttribute);
        }

        private void ShowAttribute(ItemAttribute itemAttribute)
        {
            if (_lastType != itemAttribute.objectType)
                ShowTypeHeader(itemAttribute.objectType);
            
            StartRow();
            Label(itemAttribute.objectName, 150);
            Object(itemAttribute, typeof(ItemObject), 250);
            EndRow();
        }

        private void ShowTypeHeader(string objectType)
        {
            Space();
            Label(objectType, 150, true);
            _lastType = objectType;
        }

        private void ShowDefaultInspector()
        {
            Space();
            SetBool("Draw Inspector " + name, 
                LeftCheck("Draw Full Inspector", 
                    GetBool("Draw Inspector " + name)));
            if (GetBool("Draw Inspector " + name))
            {
                DrawDefaultInspector();
            }
        }
    }
}
