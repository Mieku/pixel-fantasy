using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Handlers;
using UnityEngine;

[CreateAssetMenu(fileName = "Raw Food Settings", menuName = "Settings/Raw Food Settings")]
public class RawFoodSettings : ItemSettings
{
    [SerializeField] private EFoodType _foodType;
    [SerializeField] private float _nutrition;

    public EFoodType FoodType => _foodType;
    public float Nutrition => _nutrition;
    
    public override ItemData CreateItemData(Vector2 spawnPos)
    {
        var data = new RawFoodData();
        data.InitData(this, spawnPos);
        ItemsDatabase.Instance.RegisterItem(data);
        return data;
    }
}

public enum EFoodType
{
    [Description("Produce")] Produce = 0,
    [Description("Grain")] Grain = 1,
    [Description("Meat")] Meat = 2,
    [Description("Dairy")] Dairy = 3,
    [Description("Meal")] Meal = 5,
}
