using System.Collections;
using System.Collections.Generic;
using Characters;
using Items;
using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "Furniture Settings", menuName = "Settings/Furniture Settings")]
public class FurnitureSettings : CraftedItemSettings
{
    // Furniture Settings
    [SerializeField] protected string _description;
    [SerializeField] protected Furniture _furniturePrefab;
    [SerializeField] protected NeedChange _inUseNeedChange;
    [SerializeField] protected ColourOptions _colourOptions;
    [SerializeField] protected List<FurnitureVariant> _varients;
    [SerializeField] protected PlacementDirection _defaultDirection;
    [SerializeField] protected DyeSettings _defaultDye;
    [SerializeField] protected int _numOwners;
        
    // Accessors
    public string Description => _description;
    public Furniture FurniturePrefab => _furniturePrefab;
    public NeedChange InUseNeedChange => _inUseNeedChange;
    public ColourOptions ColourOptions => _colourOptions;
    public List<FurnitureVariant> Varients => _varients;
    public PlacementDirection DefaultDirection => _defaultDirection;
    public DyeSettings DefaultDye => _defaultDye;
    public int NumberOfPossibleOwners => _numOwners;
}
