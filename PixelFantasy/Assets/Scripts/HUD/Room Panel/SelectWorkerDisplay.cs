using System;
using Characters;
using TMPro;
using UnityEngine;

namespace HUD.Room_Panel
{
    public class SelectWorkerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _workerName;
        [SerializeField] private TextMeshProUGUI _workerSkill;

        private UnitState _unitState;
        private Action<UnitState> _onWorkerSelected;

        public void Init(UnitState unitState, Action<UnitState> onWorkerSelected)
        {
            _unitState = unitState;
            _onWorkerSelected = onWorkerSelected;

            _workerName.text = _unitState.FullName;
        }

        public void DisplayPressed()
        {
            _onWorkerSelected.Invoke(_unitState);
        }
    }
}
