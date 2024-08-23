using System;
using AI;
using Systems.Stats.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Controls_Details.Scripts
{
    public class PriorityDisplay : MonoBehaviour
    {
        [SerializeField] private Image _frameImg;
        [SerializeField] private Image _passionImg;
        [SerializeField] private TextMeshProUGUI _priorityText;
        [SerializeField] private Sprite _majorPassionSpr;
        [SerializeField] private Sprite _minorPassionSpr;
        [SerializeField] private Color _positiveColour;
        [SerializeField] private Color _negativeColour;
        [SerializeField] private Color _noSkillColour;

        private Action<ETaskType> _onIncreaseCallback;
        private Action<ETaskType> _onDecreaseCallback;
        private Color _frameColour;
        private TaskPriority _taskPriority;

        public void Init(TaskPriority priority, 
            float averageSkill, ESkillPassion highestPassion, 
            Action<ETaskType> onIncreaseCallback, Action<ETaskType> onDecreaseCallback)
        {
            _taskPriority = priority;
            _onIncreaseCallback = onIncreaseCallback;
            _onDecreaseCallback = onDecreaseCallback;
            
            // Colour frame by skill
            float skillStrength = averageSkill / 10f;
            _frameColour = Color.Lerp(_negativeColour, _positiveColour, Mathf.Clamp(skillStrength, 0.0f, 1.0f));
            _frameImg.color = _frameColour;

            _priorityText.text = priority.GetPriorityValueText();

            switch (highestPassion)
            {
                case ESkillPassion.None:
                    _passionImg.gameObject.SetActive(false);
                    break;
                case ESkillPassion.Minor:
                    _passionImg.gameObject.SetActive(true);
                    _passionImg.sprite = _minorPassionSpr;
                    break;
                case ESkillPassion.Major:
                    _passionImg.gameObject.SetActive(true);
                    _passionImg.sprite = _majorPassionSpr;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(highestPassion), highestPassion, null);
            }
        }
        
        public void InitNoSkill(TaskPriority priority, Action<ETaskType> onIncreaseCallback, Action<ETaskType> onDecreaseCallback)
        {
            _onIncreaseCallback = onIncreaseCallback;
            _onDecreaseCallback = onDecreaseCallback;
            
            // Colour frame by skill
            _frameColour = _noSkillColour;
            _frameImg.color = _frameColour;

            _priorityText.text = priority.GetPriorityValueText();
            
            _passionImg.gameObject.SetActive(false);
        }

        public void SetUnavailable(TaskPriority priority)
        {
            _frameImg.enabled = false;
            _passionImg.gameObject.SetActive(false);
            _priorityText.text = "";
        }

        public void OnLeftClicked()
        {
            _onIncreaseCallback?.Invoke(_taskPriority.TaskType);
        }

        public void OnRightClicked()
        {
            _onDecreaseCallback?.Invoke(_taskPriority.TaskType);
        }

        public void OnHoverStart()
        {
            _frameImg.color = Color.white;
        }

        public void OnHoverEnd()
        {
            _frameImg.color = _frameColour;
        }
    }
}
