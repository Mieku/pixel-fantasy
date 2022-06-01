using DataPersistence.States;
using UnityEngine;

namespace DataPersistence
{
    public abstract class Saveable : MonoBehaviour
    {
        protected abstract string StateName { get; }

        public void LoadData(GameState state)
        {
            RestoreState(state.States[StateName]);
        }
        
        public void SaveState(ref GameState state)
        {
            state.States[StateName] = CaptureState();
        }

        protected abstract void RestoreState(object stateData);
        protected abstract object CaptureState();
    }
}
