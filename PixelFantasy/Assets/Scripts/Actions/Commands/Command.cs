using System.Collections;
using System.Collections.Generic;
using Gods;
using SGoap;
using TaskSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Task Task;
}
