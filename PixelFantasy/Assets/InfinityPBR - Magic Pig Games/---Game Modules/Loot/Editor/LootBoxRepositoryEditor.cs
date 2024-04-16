using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(LootBoxRepository))]
    [CanEditMultipleObjects]
    [Serializable]
    public class LootBoxRepositoryEditor : Editor
    {
        private LootBoxRepository ThisObject;

        //private string lastType = "";

        public void OnEnable()
        {
            ThisObject = (LootBoxRepository) target;
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
            ShowListOfObjects();
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
            LabelSized("Loot Box Repository", 14, true);
            Label("Keep an object with this script in your project. It will allow you to easily reference the " +
                  "Loot Box scriptable objects you've created. This script will auto populated with all of the " +
                  "objects in your project each time you view it in the Inspector.", false, true);
            Space();
            StartRow();
            Label("Loot Box Objects: ", 200);
            Label(ThisObject.scriptableObjects.Count.ToString());
            EndRow();

            EndVerticalBox();
            Space();
        }

        private void ShowListOfObjects()
        {
            List<LootBox> listToShow = ThisObject.scriptableObjects;
            
            foreach (LootBox lootBox in listToShow)
                ShowLootBoxRow(lootBox);
        }

        private void ShowLootBoxRow(LootBox lootBox)
        {
            StartRow();
            Label(lootBox.objectName, 150);
            Object(lootBox, typeof(LootBox), 250);
            ContentColor(Color.white);
            EndRow();
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
