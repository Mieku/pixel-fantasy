using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ConditionsRepository))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ConditionsRespositoryEditor : Editor
    {
        private ConditionsRepository ThisObject;

        private string lastType = "";

        public void OnEnable()
        {
            ThisObject = (ConditionsRepository) target;
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
            ShowButtons();
            ShowListOfObjects();
            ShowDefaultInspector();
        }

        private void ShowButtons()
        {
            Space();
            if (!HasKey("Stat Repository Show All"))
                SetBool("Stat Repository Show All", true);
            
            bool showAll = GetBool("Stat Repository Show All");
            bool showModifiable = GetBool("Stat Repository Show Modifiable");
            bool showTrainable = GetBool("Stat Repository Show Trainable");

            StartRow();
            ShowButton("All");
            ShowButton("Modifiable");
            ShowButton("Trainable");
            EndRow();

            if (!showAll && GetBool("Stat Repository Show All"))
            {
                SetBool("Stat Repository Show Modifiable", false);
                SetBool("Stat Repository Show Trainable", false);
            }
            if (!showModifiable && GetBool("Stat Repository Show Modifiable"))
            {
                SetBool("Stat Repository Show All", false);
                SetBool("Stat Repository Show Trainable", false);
            }
            if (!showTrainable && GetBool("Stat Repository Show Trainable"))
            {
                SetBool("Stat Repository Show Modifiable", false);
                SetBool("Stat Repository Show All", false);
            }
            
            BackgroundColor(Color.white);
        }

        private void ShowButton(string label)
        {
            BackgroundColor(GetBool("Stat Repository Show " + label) ? Color.green : Color.grey);
            if (Button(label))
                SetBool("Stat Repository Show " + label, !GetBool("Stat Repository Show " + label));
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
            LabelSized("Conditions Repository", 14, true);
            Label("Keep an object with this script in your project. It will allow you to easily reference the " +
                  "Condition scriptable objects you've created. This script will auto populated with all of the " +
                  "objects in your project each time you view it in the Inspector.", false, true);
            Space();
            StartRow();
            Label("Condition Types: ", 200);
            Label(ThisObject.conditionTypes.Count.ToString());
            EndRow();
            StartRow();
            Label("Condition Objects: ", 200);
            Label(ThisObject.scriptableObjects.Count.ToString());
            EndRow();
            Space();

            Label($"{ThisObject.lookupTables.Count} Lookup Tables", true);
            foreach (LookupTable lookupTable in ThisObject.lookupTables)
            {
                Label($"{lookupTable.name} ({lookupTable.table.Count} Input/Outputs)");
            }
            EndVerticalBox();
        }

        private void ShowListOfObjects()
        {
            List<Condition> listToShow = ThisObject.scriptableObjects;
            foreach (Condition condition in listToShow)
                ShowConditionRow(condition);
        }

        private void ShowConditionRow(Condition condition)
        {
            if (lastType != condition.objectType)
                ShowTypeHeader(condition.objectType);
            
            StartRow();
            Label(condition.objectName, 150);
            Object(condition, typeof(Condition), 250);
            ContentColor(Color.white);
            EndRow();
        }

        private void ShowTypeHeader(string objectType)
        {
            Space();
            Label(objectType, 150, true);
            lastType = objectType;
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
