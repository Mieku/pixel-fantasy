using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Systems.Mood.Scripts;
using UnityEditor;
using UnityEngine;

public class KinlingCheatsMenu : EditorWindow
{

    [MenuItem("Window/Cheats/Kinling Cheats")]
    public static void ShowWindow()
    {
        GetWindow<KinlingCheatsMenu>("Kinling Cheats");
    }

    private void OnGUI()
    {
        GUILayout.Label("Apply Cheats to the Selected Kinlings", EditorStyles.boldLabel);
        
        // Must be in play mode
        if (!Application.isPlaying)
        {
            GUILayout.Label("Must be in play mode for cheats to function", EditorStyles.helpBox);
            return;
        }
        
        AddEmotionCheats();
    }

    private void AddEmotionCheats()
    {
        GUILayout.Label("Cheat Emotions on selected Kinlings", EditorStyles.label);
        if (GUILayout.Button("Apply Emotion +50"))
        {
            var units = GetSelectedUnits();
            foreach (var unit in units)
            {
                unit.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("DEBUG_PLUS_50"));
            }
        }
        
        if (GUILayout.Button("Apply Emotion -50"))
        {
            var units = GetSelectedUnits();
            foreach (var unit in units)
            {
                unit.MoodData.ApplyEmotion(Librarian.Instance.GetEmotion("DEBUG_NEG_50"));
            }
        }

        if (GUILayout.Button("Trigger Breakdown"))
        {
            var breakdown = Librarian.Instance.GetEmotionalBreakdown("Wander Breakdown");
            var units = GetSelectedUnits();
            foreach (var unit in units)
            {
                unit.MoodData.DEBUG_TriggerBreakdown(breakdown);
            }
        }
        
        if (GUILayout.Button("End Current Breakdown"))
        {
            var units = GetSelectedUnits();
            foreach (var unit in units)
            {
                unit.MoodData.DEBUG_EndBreakdown();
            }
        }
    }

    private List<Kinling> GetSelectedUnits()
    {
        List<Kinling> results = new List<Kinling>();
        foreach (var obj in Selection.gameObjects)
        {
            var unit = obj.GetComponent<Kinling>();
            if (unit != null)
            {
                results.Add(unit);
            }
        }

        return results;
    }
}
