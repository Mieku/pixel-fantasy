using System;
using DataPersistence;
using DataPersistence.States;
using UnityEngine;

namespace Handlers
{
    public class ResourcesHandler : MonoBehaviour, IDataPersistence
    {
        public string tester;

        private ResourcesState GatherStateValues()
        {
            return new ResourcesState()
            {
                Tester = tester
            };
        }

        private void ApplyStateValues(ResourcesState state)
        {
            tester = state.Tester;
        }

        public void LoadData(GameState state)
        {
            ApplyStateValues(state.ResourcesState);
        }

        public void SaveState(ref GameState state)
        {
            state.ResourcesState = GatherStateValues();
        }
    }
}
