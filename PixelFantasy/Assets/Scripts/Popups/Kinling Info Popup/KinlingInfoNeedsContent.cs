using System;
using System.Collections.Generic;
using Characters;
using Systems.Needs.Scripts;
using UnityEngine;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoNeedsContent : MonoBehaviour
    {
        [SerializeField] private NeedDisplay _leftNeedPrefeb;
        [SerializeField] private NeedDisplay _rightNeedPrefeb;
        [SerializeField] private Transform _leftNeedParent;
        [SerializeField] private Transform _rightNeedParent;
        
        private Kinling _kinling;
        private bool _isVisible => gameObject.activeSelf;
        private List<NeedDisplay> _displayedNeeds = new List<NeedDisplay>();
        
    
        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            
            gameObject.SetActive(true);
            DisplayNeeds();
            Refresh();
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void DisplayNeeds()
        {
            // foreach (var displayedNeed in _displayedNeeds)
            // {
            //     Destroy(displayedNeed.gameObject);
            // }
            // _displayedNeeds.Clear();
            //
            // var allStatConfigs = _unit.NeedsAI.AllStatConfigurations;
            // List<AIStat> allNeeds = new List<AIStat>();
            // foreach (var statConfig in allStatConfigs)
            // {
            //     allNeeds.Add(statConfig.LinkedStat);
            // }
            //
            // bool isLeft = true;
            // foreach (var need in allNeeds)
            // {
            //     if (isLeft)
            //     {
            //         var needValue = _unit.NeedsAI.GetStatValue(need);
            //         var needDisplay = Instantiate(_leftNeedPrefeb, _leftNeedParent);
            //         needDisplay.Init(need, needValue);
            //         _displayedNeeds.Add(needDisplay);
            //     }
            //     else
            //     {
            //         var needValue = _unit.NeedsAI.GetStatValue(need);
            //         var needDisplay = Instantiate(_rightNeedPrefeb, _rightNeedParent);
            //         needDisplay.Init(need, needValue);
            //         _displayedNeeds.Add(needDisplay);
            //     }
            //
            //     isLeft = !isLeft;
            // }
        }

        private void Refresh()
        {
            if(!_isVisible) return;

            // foreach (var needDisplay in _displayedNeeds)
            // {
            //     var stat = needDisplay.Stat;
            //     var value = _unit.NeedsAI.GetStatValue(stat);
            //     needDisplay.RefreshValue(value);
            // }
        }

        private void Update()
        {
            Refresh();
        }
    }
}
