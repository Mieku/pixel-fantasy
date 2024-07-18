using System.Collections;
using System.Collections.Generic;
using Systems.Buildings.Scripts;
using UnityEngine;

[CreateAssetMenu(fileName = "Door Settings", menuName = "Settings/Door Settings")]
public class DoorSettings : ConstructionSettings
{
    [SerializeField] private string _doorName;
    [SerializeField] private Door _doorPrefab;
    [SerializeField] private Sprite _optionIcon;
    [SerializeField] private int _maxDurability;
    [SerializeField] private Sprite _horizontalDoorframe;
    [SerializeField] private Sprite _horizontalDoormat;
    [SerializeField] private Sprite _verticalDoorframe;
    [SerializeField] private Sprite _verticalDoormat;

    public string DoorName => _doorName;
    public Door DoorPrefab => _doorPrefab;
    public Sprite OptionIcon => _optionIcon;
    public int MaxDurability => _maxDurability;
    public Sprite HorizontalDoorframe => _horizontalDoorframe;
    public Sprite HorizontalDoormat => _horizontalDoormat;
    public Sprite VerticalDoorframe => _verticalDoorframe;
    public Sprite VerticalDoormat => _verticalDoormat;


    public List<string> GetStatsList()
    {
        // TODO: build me
        return new List<string>();
    }
}
