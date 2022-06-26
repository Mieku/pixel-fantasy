using System;
using System.Collections.Generic;
using DataPersistence;
using Items;
using UnityEngine;

namespace Handlers
{
    public class FlooringHandler : Saveable
    {
        protected override string StateName => "Flooring";
        public override int LoadOrder => 1;
        
        [SerializeField] private GameObject _flooringPrefab;
        [SerializeField] private GameObject _dirtPrefab;

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
                if (childState is Floor.Data floorData)
                {
                    var childObj = Instantiate(_flooringPrefab, transform);
                    childObj.GetComponent<IPersistent>().RestoreState(floorData);
                } 
                else if (childState is DirtTile.DirtData dirtData)
                {
                    var childObj = Instantiate(_dirtPrefab, transform);
                    childObj.GetComponent<IPersistent>().RestoreState(dirtData);
                }
                
            }
        }
    }
}
