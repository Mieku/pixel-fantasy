using ScriptableObjects;
using TaskSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Command", menuName ="Commands/Create Command")]
public class Command : ScriptableObject
{
    public string Name;
    public Sprite Icon;
    public Task Task;
    public JobData Job => Task.Job;
    public EToolType RequiredToolType => Task.RequiredToolType;
}
