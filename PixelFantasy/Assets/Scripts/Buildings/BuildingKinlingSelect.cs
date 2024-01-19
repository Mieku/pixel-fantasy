using System;
using Characters;
using TMPro;
using UnityEngine;

namespace Buildings
{
    public class BuildingKinlingSelect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _kinlingName;
        [SerializeField] private TextMeshProUGUI _kinlingJobTitle;

        private Action<Unit> _onPressedCallback;
        private Unit _kinling;

        public void Init(Unit kinling, Action<Unit> onPressedCallback)
        {
            _kinling = kinling;
            _onPressedCallback = onPressedCallback;

            _kinlingName.text = _kinling.FullName;
            _kinlingJobTitle.text = _kinling.JobName;
        }

        public void OnPressed()
        {
            _onPressedCallback.Invoke(_kinling);
        }
    }
}
