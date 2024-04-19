using System;
using System.Collections.Generic;
using System.Linq;
using Systems.Stats.Scripts;

namespace TaskSystem
{
    [Serializable]
    public class TaskPriorities
    {
        public List<TaskPriority> Priorities = new()
        {
            new TaskPriority(ETaskType.Emergency),
            new TaskPriority(ETaskType.Healing),
            new TaskPriority(ETaskType.Construction),
            new TaskPriority(ETaskType.AnimalHandling),
            new TaskPriority(ETaskType.Cooking),
            new TaskPriority(ETaskType.Hunting),
            new TaskPriority(ETaskType.Farming),
            new TaskPriority(ETaskType.Mining),
            new TaskPriority(ETaskType.Harvesting),
            new TaskPriority(ETaskType.Forestry),
            new TaskPriority(ETaskType.Crafting),
            new TaskPriority(ETaskType.Hauling),
            new TaskPriority(ETaskType.Research),
        };
        
        private Dictionary<ETaskType, int> _inherentPriorities = new()
        {
            { ETaskType.Emergency, 1 },
            { ETaskType.Healing, 2 },
            { ETaskType.Construction, 3 },
            { ETaskType.AnimalHandling, 4 },
            { ETaskType.Cooking, 5 },
            { ETaskType.Hunting, 6 },
            { ETaskType.Farming, 7 },
            { ETaskType.Mining, 8 },
            { ETaskType.Harvesting, 9 },
            { ETaskType.Forestry, 10 },
            { ETaskType.Crafting, 11 },
            { ETaskType.Hauling, 12 },
            { ETaskType.Research, 13 }
        };

        public List<ETaskType> SortedPriorities()
        {
            var sortedPriorities = Priorities
                .OrderByDescending(p => p.Priority) // First, sort by ETaskPriority
                .ThenBy(p => _inherentPriorities[p.TaskType]) // Then, sort by the inherent priority within the same ETaskPriority
                .Select(p => p.TaskType)
                .ToList();

            return sortedPriorities;
        }

        public List<ETaskType> GetAssociatedTaskTypes(ESkillType skillType)
        {
            switch (skillType)
            {
                case ESkillType.Mining:
                    return new List<ETaskType>() { ETaskType.Mining };
                case ESkillType.Cooking:
                    return new List<ETaskType>() { ETaskType.Cooking };
                case ESkillType.Melee:
                    return new List<ETaskType>() {  };
                case ESkillType.Ranged:
                    return new List<ETaskType>() { ETaskType.Hunting };
                case ESkillType.Construction:
                    return new List<ETaskType>() { ETaskType.Construction };
                case ESkillType.Botany:
                    return new List<ETaskType>() { ETaskType.Farming, ETaskType.Forestry };
                case ESkillType.Crafting:
                    return new List<ETaskType>() { ETaskType.Crafting };
                case ESkillType.BeastMastery:
                    return new List<ETaskType>() { ETaskType.AnimalHandling };
                case ESkillType.Medical:
                    return new List<ETaskType>() { ETaskType.Healing };
                case ESkillType.Social:
                    return new List<ETaskType>() {  };
                case ESkillType.Intelligence:
                    return new List<ETaskType>() { ETaskType.Research };
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
    }

    [Serializable]
    public class TaskPriority
    {
        public ETaskType TaskType;
        public ETaskPriority Priority;

        public TaskPriority(ETaskType taskType, ETaskPriority priority = ETaskPriority.Normal)
        {
            TaskType = taskType;
            Priority = priority;
        }
    }

    public enum ETaskPriority
    {
        Ignore = 0,
        Urgent = 1,
        High = 2,
        Normal = 3,
        Low = 4,
        Last = 5,
    }
}
