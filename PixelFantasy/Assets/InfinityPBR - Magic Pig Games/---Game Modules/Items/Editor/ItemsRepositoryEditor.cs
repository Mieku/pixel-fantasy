using System;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemsRepository))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemsRepositoryEditor : Editor
    {
        private ItemsRepository ThisObject;

        private string _lastType = "";
        //private string lastTypeAttribute = "";

        private Vector2 _scrollPos;

        public void OnEnable()
        {
            ThisObject = (ItemsRepository) target;
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
            ShowMainButtons();
            ShowItemObjects();
            ShowItemAttributes();
            ShowDefaultInspector();
        }

        private void ShowMainButtons()
        {
            Space();
            if (!HasKey("Items Repository Show Item Objects"))
                SetBool("Items Repository Show Item Objects", true);
            
            bool showObjects = GetBool("Items Repository Show Item Objects");
            bool showAttributes = GetBool("Items Repository Show Item Attributes");

            StartRow();
            ShowButton("Item Objects");
            ShowButton("Item Attributes");
            EndRow();

            if (!showObjects && GetBool("Items Repository Show Item Objects"))
                SetBool("Items Repository Show Item Attributes", false);
            if (!showAttributes && GetBool("Items Repository Show Item Attributes"))
                SetBool("Items Repository Show Item Objects", false);

            BackgroundColor(Color.white);
        }

        private void ShowButton(string label)
        {
            BackgroundColor(GetBool("Items Repository Show " + label) ? Color.green : Color.grey);
            if (Button(label))
                SetBool("Items Repository Show " + label, !GetBool("Items Repository Show " + label));
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
            LabelSized("Items Repository", 14, true);
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
            Label(ThisObject.itemObjects.Count.ToString());
            EndRow();
            
            Space();
            StartRow();
            Label("Item Attribute Types: ", 200);
            Label(ThisObject.itemAttributeTypes.Count.ToString());
            EndRow();
            StartRow();
            Label("Item Attributes: ", 200);
            Label(ThisObject.itemAttributes.Count.ToString());
            EndRow();
            EndVerticalBox();
        }

        private void ShowItemObjects()
        {
            if (!GetBool("Items Repository Show Item Objects")) return;

            foreach (ItemObject itemObject in ThisObject.itemObjects)
                ShowObject(itemObject);
        }

        private void ShowItemAttributes()
        {
            if (!GetBool("Items Repository Show Item Attributes")) return;
            
            foreach (ItemAttribute itemAttribute in ThisObject.itemAttributes)
                ShowAttribute(itemAttribute);
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
