using System.Collections.Generic;
using DataPersistence;
using UnityEngine;

namespace Handlers
{
    public class UnitsHandler : Saveable
    {
        protected override string StateName => "Units";
        public override int LoadOrder => 2;

        [SerializeField] private GameObject _unitPrefab;

        protected override void ClearChildStates(List<object> childrenStates)
        {
            // Delete current persistent children
            var currentChildren = GetPersistentChildren();
            foreach (var child in currentChildren)
            {
                child.GetComponent<UID>().RemoveUID();
            }
            
            foreach (var child in currentChildren)
            {
                Destroy(child);
            }
            currentChildren.Clear();
        }

        protected override void SetChildStates(List<object> childrenStates)
        {
            // Instantiate all the children in data, Trigger RestoreState with their state data
            foreach (var childState in childrenStates)
            {
                var data = (Characters.Unit.UnitData)childState;
                var childObj = Instantiate(_unitPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(data);
            }
        }
    }
}
