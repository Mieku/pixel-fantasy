using System.Collections.Generic;
using DataPersistence.States;
using UnityEngine;

namespace DataPersistence
{
    public abstract class Saveable : MonoBehaviour
    {
        protected abstract string StateName { get; }
        public abstract int LoadOrder { get; }

        public virtual void ClearData(GameState state)
        {
            if (state.States.ContainsKey(StateName))
            {
                ClearState(state.States[StateName]);
            }
        }
        
        public virtual void LoadData(GameState state)
        {
            if (state.States.ContainsKey(StateName))
            {
                RestoreState(state.States[StateName]);
            }
        }
        
        public virtual void SaveState(ref GameState state)
        {
            state.States[StateName] = CaptureState();
        }
        
        protected List<GameObject> GetPersistentChildren()
        {
            List<GameObject> results = new List<GameObject>();
            var numChildren = transform.childCount;
            for (int i = 0; i < numChildren; i++)
            {
                var child = transform.GetChild(i).gameObject.GetComponent<IPersistent>();
                if (child != null)
                {
                    results.Add(transform.GetChild(i).gameObject);
                }
            }
            return results;
        }
        
        private List<object> GetChildStates()
        {
            List<object> results = new List<object>();
            var children = GetPersistentChildren();
            foreach (var child in children)
            {
                results.Add(child.GetComponent<IPersistent>().CaptureState());
            }
            return results;
        }
        
        
        
        private object CaptureState()
        {
            var childStates = GetChildStates();
            
            return new Data()
            {
                ChildrenStates = childStates,
            };
        }

        private void RestoreState(object stateData)
        {
            var data = (Data)stateData;
            var childrenStates = data.ChildrenStates;
            SetChildStates(childrenStates);
        }
        
        private void ClearState(object stateData)
        {
            var data = (Data)stateData;
            var childrenStates = data.ChildrenStates;
            ClearChildStates(childrenStates);
        }
        
        public struct Data
        {
            public List<object> ChildrenStates;
        }

        protected abstract void ClearChildStates(List<object> childrenStates);
        protected abstract void SetChildStates(List<object> childrenStates);
    }
}
