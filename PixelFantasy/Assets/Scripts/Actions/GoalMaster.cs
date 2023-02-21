using System;
using System.Collections.Generic;
using Gods;
using SGoap;
using UnityEngine;

namespace Actions
{
    public class GoalMaster : God<GoalMaster>
    {
        public GoalQueue EmergencyGoals = new GoalQueue();
        public GoalQueue HealingGoals = new GoalQueue();
        public GoalQueue CookingGoals = new GoalQueue();
        public GoalQueue HuntingGoals = new GoalQueue();
        public GoalQueue ConstructionGoals = new GoalQueue();
        public GoalQueue FarmingGoals = new GoalQueue();
        public GoalQueue MiningGoals = new GoalQueue();
        public GoalQueue FellingGoals = new GoalQueue();
        public GoalQueue SmithingGoals = new GoalQueue();
        public GoalQueue TailoringGoals = new GoalQueue();
        public GoalQueue CarpentryGoals = new GoalQueue();
        public GoalQueue MasonryGoals = new GoalQueue();
        public GoalQueue HaulingGoals = new GoalQueue();
        public GoalQueue CleaningGoals = new GoalQueue();
        public GoalQueue ResearchGoals = new GoalQueue();
        
        
        public GoalRequest GetNextGoalByCategory(TaskCategory category)
        {
            GoalRequest nextGoal = category switch
            {
                TaskCategory.Emergency => EmergencyGoals.NextRequest,
                TaskCategory.Healing => HealingGoals.NextRequest,
                TaskCategory.Cooking => CookingGoals.NextRequest,
                TaskCategory.Hunting => HuntingGoals.NextRequest,
                TaskCategory.Construction => ConstructionGoals.NextRequest,
                TaskCategory.Farming => FarmingGoals.NextRequest,
                TaskCategory.Mining => MiningGoals.NextRequest,
                TaskCategory.Felling => FellingGoals.NextRequest,
                TaskCategory.Smithing => SmithingGoals.NextRequest,
                TaskCategory.Tailoring => TailoringGoals.NextRequest,
                TaskCategory.Carpentry => CarpentryGoals.NextRequest,
                TaskCategory.Masonry => MasonryGoals.NextRequest,
                TaskCategory.Hauling => HaulingGoals.NextRequest,
                TaskCategory.Cleaning => CleaningGoals.NextRequest,
                TaskCategory.Research => ResearchGoals.NextRequest,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };

            return nextGoal;
        }

        public void AddGoal(GoalRequest goalRequest)
        {
            switch (goalRequest.Category)
            {
                case TaskCategory.Emergency:
                    EmergencyGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Healing:
                    HealingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Cooking:
                    CookingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Hunting:
                    HuntingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Construction:
                    ConstructionGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Farming:
                    FarmingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Mining:
                    MiningGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Felling:
                    FellingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Smithing:
                    SmithingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Tailoring:
                    TailoringGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Carpentry:
                    CarpentryGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Masonry:
                    MasonryGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Hauling:
                    HaulingGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Cleaning:
                    CleaningGoals.AddRequest(goalRequest);
                    break;
                case TaskCategory.Research:
                    ResearchGoals.AddRequest(goalRequest);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(goalRequest.Category), goalRequest.Category, null);
            }
        }

        public void CancelGoal(GoalRequest goalRequest)
        {
            switch (goalRequest.Category)
            {
                case TaskCategory.Emergency:
                    EmergencyGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Healing:
                    HealingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Cooking:
                    CookingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Hunting:
                    HuntingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Construction:
                    ConstructionGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Farming:
                    FarmingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Mining:
                    MiningGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Felling:
                    FellingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Smithing:
                    SmithingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Tailoring:
                    TailoringGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Carpentry:
                    CarpentryGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Masonry:
                    MasonryGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Hauling:
                    HaulingGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Cleaning:
                    CleaningGoals.CancelRequest(goalRequest);
                    break;
                case TaskCategory.Research:
                    ResearchGoals.CancelRequest(goalRequest);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(goalRequest.Category), goalRequest.Category, null);
            }
        }
    }
}
