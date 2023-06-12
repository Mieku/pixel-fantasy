using System;
using Characters;
using Controllers;
using TMPro;
using UnityEngine;

namespace HUD.Room_Panel
{
    public class AssignedWorkerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _workerName;
        [SerializeField] private TextMeshProUGUI _skillDisplay;

        private Action<UnitState> _onUnitRemoved;
        private UnitState _unitState;

        public void Init(UnitState unitState, Action<UnitState> onUnitRemoved)
        {
            _unitState = unitState;
            _onUnitRemoved = onUnitRemoved;

            _workerName.text = _unitState.FullName;
        }

        public void RemoveWorkerPressed()
        {
            _onUnitRemoved.Invoke(_unitState);
        }

        public void ViewUnitPressed()
        {
            var click = _unitState.GetComponent<ClickObject>();
            if (click != null)
            {
                click.TriggerSelected(true);
            }
        }
    }
}
