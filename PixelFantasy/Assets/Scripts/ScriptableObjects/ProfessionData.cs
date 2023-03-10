using System.Collections.Generic;
using Gods;
using TaskSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfessionData", menuName = "ProfessionData", order = 1)]
public class ProfessionData : ScriptableObject
{
    public string ProfessionName;
    public List<TaskCategory> SortedPriorities;
}
