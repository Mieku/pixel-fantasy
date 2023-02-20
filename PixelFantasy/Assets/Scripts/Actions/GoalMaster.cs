using System;
using System.Collections.Generic;
using Gods;
using SGoap;
using UnityEngine;

namespace Actions
{
    public class GoalMaster : God<GoalMaster>
    {
        public List<GoalRequest> FellingGoals = new List<GoalRequest>();
        
        
        public GoalRequest GetNextGoalByCategory(TaskCategory category)
        {
            GoalRequest nextGoal = category switch
            {
                // TaskCategory.Emergency => EmergencyTaskSystem.RequestNextTask(),
                // TaskCategory.Healing => HealingTaskSystem.RequestNextTask(),
                // TaskCategory.Cooking => CookingTaskSystem.RequestNextTask(),
                // TaskCategory.Hunting => HuntingTaskSystem.RequestNextTask(),
                // TaskCategory.Construction => ConstructionTaskSystem.RequestNextTask(),
                // TaskCategory.Farming => FarmingTaskSystem.RequestNextTask(),
                // TaskCategory.Mining => MiningTaskSystem.RequestNextTask(),
                TaskCategory.Felling => FellingGoals[0],
                // TaskCategory.Smithing => SmithingTaskSystem.RequestNextTask(),
                // TaskCategory.Tailoring => TailoringTaskSystem.RequestNextTask(),
                // TaskCategory.Carpentry => CarpentryTaskSystem.RequestNextTask(),
                // TaskCategory.Masonry => MasonryTaskSystem.RequestNextTask(),
                // TaskCategory.Hauling => HaulingTaskSystem.RequestNextTask(),
                // TaskCategory.Cleaning => CleaningTaskSystem.RequestNextTask(),
                // TaskCategory.Research => ResearchTaskSystem.RequestNextTask(),
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };

            return nextGoal;
        }

        public void AddGoalByCategory(TaskCategory category, GoalRequest goalRequest)
        {
            switch (category)
            {
                case TaskCategory.Emergency:
                    break;
                case TaskCategory.Healing:
                    break;
                case TaskCategory.Cooking:
                    break;
                case TaskCategory.Hunting:
                    break;
                case TaskCategory.Construction:
                    break;
                case TaskCategory.Farming:
                    break;
                case TaskCategory.Mining:
                    break;
                case TaskCategory.Felling:
                    FellingGoals.Add(goalRequest);
                    break;
                case TaskCategory.Smithing:
                    break;
                case TaskCategory.Tailoring:
                    break;
                case TaskCategory.Carpentry:
                    break;
                case TaskCategory.Masonry:
                    break;
                case TaskCategory.Hauling:
                    break;
                case TaskCategory.Cleaning:
                    break;
                case TaskCategory.Research:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}
