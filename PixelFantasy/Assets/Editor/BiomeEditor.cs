using System.Collections.Generic;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using Systems.World_Building.Scripts; // Ensure this namespace matches where your BiomeData is located

[CustomEditor(typeof(BiomeData))]
public class BiomeEditor : Editor
{
    private MountainData selectedMountainData;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draws the default inspector for non-custom fields

        BiomeData biome = (BiomeData)target;

        // Start of Box Group
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Mountains", EditorStyles.boldLabel); // Optional: Add a label for the group
        // Dropdown or ObjectField for selecting MountainData
        selectedMountainData = (MountainData)EditorGUILayout.ObjectField("Select MountainData", selectedMountainData, typeof(MountainData), false);

        // Button for adding the selected MountainData to the list
        if (GUILayout.Button("Add Selected Mountain") && selectedMountainData != null)
        {
            AddNewMountain(biome, selectedMountainData);
            selectedMountainData = null; // Reset selection
        }

        if (biome.Mountains == null)
        {
            biome.Mountains = new List<MountainDataPercentage>(); // Ensure the list is initialized
        }

        EditorGUI.BeginChangeCheck(); // Start tracking changes

        for (int i = 0; i < biome.Mountains.Count; i++)
        {
            var mountain = biome.Mountains[i];
            if (mountain != null && mountain.mountainData != null) // Null check for mountain and mountainData
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(mountain.mountainData.name, GUILayout.MaxWidth(200)); // Display the mountain name
                mountain.spawnPercentage = EditorGUILayout.Slider(mountain.spawnPercentage, 0f, 1f); // Slider for percentage
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Uninitialized Mountain Data", GUILayout.MaxWidth(200));
            }
        }

        if (EditorGUI.EndChangeCheck()) // If any changes were made
        {
            NormalizePercentages(biome.Mountains); // Normalize percentages
            EditorUtility.SetDirty(target); // Mark the object as dirty to ensure changes are saved
        }

        if (GUILayout.Button("Normalize Percentages")) // A button to manually trigger normalization
        {
            NormalizePercentages(biome.Mountains);
        }
        
        EditorGUILayout.EndVertical();
        // End of Mountains Box Group
    }
    
    private void AddNewMountain(BiomeData biome, MountainData mountainData)
    {
        if (biome.Mountains == null)
        {
            biome.Mountains = new List<MountainDataPercentage>();
        }

        // Create a new MountainDataPercentage object with the selected MountainData
        MountainDataPercentage newMountain = new MountainDataPercentage(mountainData, 0f); // Assuming a constructor exists that takes these parameters

        // Add the new MountainDataPercentage object to the Mountains list
        biome.Mountains.Add(newMountain);

        // Mark the BiomeData object as dirty to ensure the change is saved
        EditorUtility.SetDirty(biome);
    }

    private void NormalizePercentages(List<MountainDataPercentage> mountains)
    {
        if (mountains == null || mountains.Count < 2) return; // Need at least 2 mountains to normalize

        // Calculate the total percentage of non-default mountains
        float nonDefaultTotalPercentage = 0f;
        for (int i = 1; i < mountains.Count; i++) // Start from 1 to exclude the default mountain
        {
            nonDefaultTotalPercentage += mountains[i].spawnPercentage;
        }

        // Check if adjustments are needed
        if (nonDefaultTotalPercentage > 1f)
        {
            // Scale down non-default mountains if total exceeds 100%
            float scale = 1f / nonDefaultTotalPercentage; // Calculate scale factor
            for (int i = 1; i < mountains.Count; i++)
            {
                mountains[i].spawnPercentage *= scale; // Apply scaling
            }
            // After scaling, the non-default total should now be 1, making the default mountain's percentage 0
            nonDefaultTotalPercentage = 1f;
        }

        // Adjust the default mountain's percentage
        mountains[0].spawnPercentage = 1f - nonDefaultTotalPercentage; // This ensures the sum equals 100%
    }
}