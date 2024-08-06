using Player;
using Sirenix.OdinInspector;
using Systems.Game_Setup.Scripts;
using UnityEngine;

namespace DataPersistence
{
    public class AutoSaveController : MonoBehaviour
    {
        [Button("Force Autosave")]
        public void DEBUG_ForceAutosave()
        {
            StartCoroutine(DataPersistenceManager.Instance.AutoSaveGameCoroutine(null));
        }

        private void Start()
        {
            GameEvents.DayTick += GameEvent_DayTick;
        }

        private void OnDestroy()
        {
            GameEvents.DayTick -= GameEvent_DayTick;
        }

        private void GameEvent_DayTick()
        {
            CheckIfAutosave();
        }

        private void CheckIfAutosave()
        {
            if (PlayerSettings.AutoSaveFrequency == 0) return;

            GameManager.Instance.GameData.DaysSinceAutosave++;
            if (GameManager.Instance.GameData.DaysSinceAutosave >= PlayerSettings.AutoSaveFrequency)
            {
                GameManager.Instance.GameData.DaysSinceAutosave = 0;
                StartCoroutine(DataPersistenceManager.Instance.AutoSaveGameCoroutine(null));
            }
        }
    }
}
