using System;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemObjectRepository))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemObjectRepositoryEditor : Editor
    {
        private ItemObjectRepository ThisObject;

        private string _lastType = "";
        //private string lastTypeAttribute = "";

        private Vector2 _scrollPos;

        public void OnEnable()
        {
            ThisObject = (ItemObjectRepository) target;
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
            ShowItemObjects();
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
            LabelSized("ItemObject Repository", 14, true);
            Label("Keep an object with this script in your project. It will allow you to easily reference the " +
                  "ItemObject and ItemAttribute scriptable objects you've created. This script will auto populated " +
                  "with all of the objects in your project each time you view it in the Inspector.", false, true);
            
            Space();
            StartRow();
            Label("Item Object Types: ", 200);
            Label(ThisObject.itemObjectTypes.Count.ToString());
            EndRow();
            StartRow();
            Label("Item Objects: ", 200);
            Label(ThisObject.scriptableObjects.Count.ToString());
            EndRow();
        }

        private void ShowItemObjects()
        {
            foreach (ItemObject itemObject in ThisObject.scriptableObjects)
                ShowObject(itemObject);
        }

        private void ShowObject(ItemObject itemObject)
        {
            if (_lastType != itemObject.objectType)
                ShowTypeHeader(itemObject.objectType);
            
            StartRow();
            Label(itemObject.objectName, 150);
            Object(itemObject, typeof(ItemObject), 250);
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
