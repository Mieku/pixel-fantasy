using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Change_Job_Popup
{
    public class ChangeJobOption : MonoBehaviour
    {
        [SerializeField] private JobData _job;
        [SerializeField] private Image _jobIcon;
        [SerializeField] private Image _bg;
        [SerializeField] private GameObject _selectionIndicator;
        [SerializeField] private Material _greyScaleMat;
        [SerializeField] private Sprite _isCurrentBG;
        [SerializeField] private Sprite _isDefaultBG;

        private Action<JobData, ChangeJobOption> _onSelectedCallback;
        private JobOptionState _state;

        public JobData Job => _job;

        public void Init(JobOptionState optionState, Action<JobData, ChangeJobOption> onSelectedCallback)
        {
            _onSelectedCallback = onSelectedCallback;
            SetState(optionState);
        }

        private void SetState(JobOptionState state)
        {
            _state = state;

            if (_job != null && _job.JobIcon != null)
            {
                _jobIcon.sprite = _job.JobIcon;
            }
            
            switch (_state)
            {
                case JobOptionState.Unavailable:
                    _jobIcon.material = _greyScaleMat;
                    _bg.sprite = _isDefaultBG;
                    _bg.material = _greyScaleMat;
                    break;
                case JobOptionState.Available:
                    _jobIcon.material = null;
                    _bg.sprite = _isDefaultBG;
                    _bg.material = null;
                    break;
                case JobOptionState.Current:
                    _jobIcon.material = null;
                    _bg.sprite = _isCurrentBG;
                    _bg.material = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ToggleSelected(bool isSelected)
        {
            _selectionIndicator.SetActive(isSelected);
        }
        
        public void OnPressed()
        {
            if (_job != null)
            {
                _onSelectedCallback.Invoke(_job, this);
            }
        }

        public enum JobOptionState
        {
            Unavailable,
            Available,
            Current,
        }
    }
}
