using System;
using System.Collections.Generic;
using Data.Resource;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;
using Systems.World_Building.Scripts; // Ensure this namespace matches where your BiomeData is located

[CustomEditor(typeof(BiomeSettings))]
public class BiomeEditor : Editor
{
    private MountainResourceData _selectedMountainSettings;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draws the default inspector for non-custom fields

        BiomeSettings biome = (BiomeSettings)target;

        // Start of Mountains Box Group
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("Mountains", EditorStyles.boldLabel); // Optional: Add a label for the group
        // Dropdown or ObjectField for selecting MountainData
        _selectedMountainSettings = (MountainResourceData)EditorGUILayout.ObjectField("Select MountainData", _selectedMountainSettings, typeof(MountainResourceData), false);

        // Button for adding the selected MountainData to the list
        if (GUILayout.Button("Add Selected Mountain") && _selectedMountainSettings != null)
        {
            AddNewMountain(biome, _selectedMountainSettings);
            _selectedMountainSettings = null; // Reset selection
        }

        if (biome.Mountains == null)
        {
            biome.Mountains = new List<MountainDataPercentage>(); // Ensure the list is initialized
        }

        EditorGUI.BeginChangeCheck(); // Start tracking changes

        for (int i = 0; i < biome.Mountains.Count; i++)
        {
            var mountain = biome.Mountains[i];
            if (mountain != null && mountain.MountainData != null) // Null check for mountain and mountainData
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(mountain.MountainData.name, GUILayout.MaxWidth(200)); // Display the mountain name
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
        
        DisplayPercentagesOptions<ResourceData>("Forest Trees", biome.ForestTreeResources);
        DisplayPercentagesOptions<GrowingResourceData>("Forest Additionals", biome.ForestAdditionalResources);
        DisplayPercentagesOptions<ResourceData>("Vegitation", biome.VegitationResources);
        DisplayPercentagesOptions<ResourceData>("Additional Resources", biome.AdditionalResources);
    }
    
    // Adjust the DisplayPercentagesOptions method to be generic
    private void DisplayPercentagesOptions<T>(string header, List<ResourceDataPercentage> biomePercentages) where T : ResourceData
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(header, EditorStyles.boldLabel);
        
        if (biomePercentages == null)
        {
            biomePercentages = new List<ResourceDataPercentage>();
        }

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < biomePercentages.Count; i++)
        {
            var resource = biomePercentages[i];
            if (resource != null && resource.ResourceData != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(resource.ResourceData.name, GUILayout.MaxWidth(200));
                resource.SpawnPercentage = EditorGUILayout.Slider(resource.SpawnPercentage, 0f, 1f);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Uninitialized Resource Data", GUILayout.MaxWidth(200));
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            NormalizePercentages(biomePercentages);
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Normalize Percentages"))
        {
            NormalizePercentages(biomePercentages);
        }
        
        if (GUILayout.Button("Equalize Percentages"))
        {
            EqualizePercentages(biomePercentages);
        }
    
        EditorGUILayout.EndVertical();
    }
    
    private void AddNewMountain(BiomeSettings biome, MountainResourceData mountainData)
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
    
    private void NormalizePercentages(List<ResourceDataPercentage> resources)
    {
        if (resources == null || resources.Count < 2) return; // Need at least 2 resources to normalize

        // Calculate the total percentage of non-default resources
        float nonDefaultTotalPercentage = 0f;
        for (int i = 1; i < resources.Count; i++) // Start from 1 to exclude the default mountain
        {
            nonDefaultTotalPercentage += resources[i].SpawnPercentage;
        }

        // Check if adjustments are needed
        if (nonDefaultTotalPercentage > 1f)
        {
            // Scale down non-default mountains if total exceeds 100%
            float scale = 1f / nonDefaultTotalPercentage; // Calculate scale factor
            for (int i = 1; i < resources.Count; i++)
            {
                resources[i].SpawnPercentage *= scale; // Apply scaling
            }
            // After scaling, the non-default total should now be 1, making the default mountain's percentage 0
            nonDefaultTotalPercentage = 1f;
        }

        // Adjust the default mountain's percentage
        resources[0].SpawnPercentage = 1f - nonDefaultTotalPercentage; // This ensures the sum equals 100%
    }

    private void EqualizePercentages(List<ResourceDataPercentage> resources)
    {
        if (resources == null || resources.Count < 2) return; // Need at least 2 resources to equalize
        float percent = 1f / resources.Count;
        foreach (var resource in resources)
        {
            resource.SpawnPercentage = percent;
        }
    }
}