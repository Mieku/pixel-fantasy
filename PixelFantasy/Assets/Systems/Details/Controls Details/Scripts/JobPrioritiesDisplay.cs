using System;
using AI;
using Characters;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;

namespace Systems.Details.Controls_Details.Scripts
{
    public class JobPrioritiesDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _kinlingName;

        [SerializeField] private PriorityDisplay _healing,
                                                _construction,
                                                _animalHandling,
                                                _cooking,
                                                _hunting,
                                                _farming,
                                                _mining,
                                                _harvesting,
                                                _forestry,
                                                _crafting,
                                                _hauling,
                                                _cleaning,
                                                _research;

        private KinlingData _kinlingData;

        public void Init(KinlingData kinlingData)
        {
            _kinlingData = kinlingData;
            _kinlingName.text = kinlingData.Nickname;

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            foreach (var priority in _kinlingData.TaskPriorities.Priorities)
            {
                var associatedSkills = _kinlingData.Stats.GetAssociatedSkillsForTaskType(priority.TaskType);
                float averageSkill = 0;
                ESkillPassion highestPassion = ESkillPassion.None;

                if (_kinlingData.Stats.CanDoTaskType(priority.TaskType))
                {
                    if (associatedSkills.Count > 0)
                    {
                        foreach (var skill in associatedSkills)
                        {
                            averageSkill += skill.Level;

                            if (skill.Passion > highestPassion)
                            {
                                highestPassion = skill.Passion;
                            }
                        }
                    
                        averageSkill /= associatedSkills.Count;
                    
                        var display = GetPriorityDisplayByTaskType(priority.TaskType);
                        if (display != null)
                        {
                            display.Init(priority, averageSkill, highestPassion, OnPriorityIncrease, OnPriorityDecrease);
                        }
                    }
                    else
                    {
                        var display = GetPriorityDisplayByTaskType(priority.TaskType);
                        if (display != null)
                        {
                            display.InitNoSkill(priority, OnPriorityIncrease, OnPriorityDecrease);
                        }
                    }
                }
                else // Can't do the task type
                {
                    var display = GetPriorityDisplayByTaskType(priority.TaskType);
                    if (display != null)
                    {
                        display.SetUnavailable(priority);
                    }
                }
            }
        }

        private void OnPriorityIncrease(ETaskType taskType)
        {
            var priority = _kinlingData.TaskPriorities.GetPriorityByTaskType(taskType);
            if (priority != null)
            {
                switch (priority.Priority)
                {
                    case ETaskPriority.Ignore:
                        priority.Priority = ETaskPriority.Last;
                        break;
                    case ETaskPriority.Urgent:
                        priority.Priority = ETaskPriority.Ignore;
                        break;
                    case ETaskPriority.High:
                        priority.Priority = ETaskPriority.Urgent;
                        break;
                    case ETaskPriority.Normal:
                        priority.Priority = ETaskPriority.High;
                        break;
                    case ETaskPriority.Low:
                        priority.Priority = ETaskPriority.Normal;
                        break;
                    case ETaskPriority.Last:
                        priority.Priority = ETaskPriority.Low;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
                RefreshDisplay();
            }
        }
        
        private void OnPriorityDecrease(ETaskType taskType)
        {
            var priority = _kinlingData.TaskPriorities.GetPriorityByTaskType(taskType);
            if (priority != null)
            {
                switch (priority.Priority)
                {
                    case ETaskPriority.Ignore:
                        priority.Priority = ETaskPriority.Urgent;
                        break;
                    case ETaskPriority.Urgent:
                        priority.Priority = ETaskPriority.High;
                        break;
                    case ETaskPriority.High:
                        priority.Priority = ETaskPriority.Normal;
                        break;
                    case ETaskPriority.Normal:
                        priority.Priority = ETaskPriority.Low;
                        break;
                    case ETaskPriority.Low:
                        priority.Priority = ETaskPriority.Last;
                        break;
                    case ETaskPriority.Last:
                        priority.Priority = ETaskPriority.Ignore;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
                RefreshDisplay();
            }
        }

        public void HighlightTaskType(ETaskType taskType, bool shouldHighlight)
        {
            var display = GetPriorityDisplayByTaskType(taskType);

            if (shouldHighlight)
            {
                display.OnHoverStart();
            }
            else
            {
                display.OnHoverEnd();
            }
            
        }

        private PriorityDisplay GetPriorityDisplayByTaskType(ETaskType taskType)
        {
            switch (taskType)
            {
                case ETaskType.Healing:
                    return _healing;
                case ETaskType.Construction:
                    return _construction;
                case ETaskType.AnimalHandling:
                    return _animalHandling;
                case ETaskType.Cooking:
                    return _cooking;
                case ETaskType.Hunting:
                    return _hunting;
                case ETaskType.Farming:
                    return _farming;
                case ETaskType.Mining:
                    return _mining;
                case ETaskType.Harvesting:
                    return _harvesting;
                case ETaskType.Forestry:
                    return _forestry;
                case ETaskType.Crafting:
                    return _crafting;
                case ETaskType.Hauling:
                    return _hauling;
                case ETaskType.Cleaning:
                    return _cleaning;
                case ETaskType.Research:
                    return _research;
                case ETaskType.Emergency:
                case ETaskType.Personal:
                case ETaskType.Misc:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null);
            }
        }
    }
}
