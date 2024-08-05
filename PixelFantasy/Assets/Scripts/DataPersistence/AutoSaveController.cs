using System.Collections;
using Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DataPersistence
{
    public class AutoSaveController : MonoBehaviour
    {
        private Coroutine autoSaveCoroutine;

        [Button("Force Autosave")]
        public void DEBUG_ForceAutosave()
        {
            StartCoroutine(DataPersistenceManager.Instance.AutoSaveGameCoroutine(null));
        }

        private void Start()
        {
            if (PlayerSettings.AutoSaveEnabled)
            {
                StartAutoSave();
            }

            PlayerSettings.OnAutoSaveEnabledChanged += OnAutoSaveEnabledChanged;
            PlayerSettings.OnAutoSaveFrequencyChanged += OnAutoSaveFrequencyChanged;
        }

        private void OnDestroy()
        {
            PlayerSettings.OnAutoSaveEnabledChanged -= OnAutoSaveEnabledChanged;
            PlayerSettings.OnAutoSaveFrequencyChanged -= OnAutoSaveFrequencyChanged;
        }

        private void StartAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
            autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }

        private void StopAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }

        private IEnumerator AutoSaveRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(PlayerSettings.AutoSaveFrequency * 60); // Convert minutes to seconds
                yield return StartCoroutine(DataPersistenceManager.Instance.AutoSaveGameCoroutine(null));
            }
        }

        private void OnAutoSaveEnabledChanged(bool isEnabled)
        {
            if (isEnabled)
            {
                StartAutoSave();
            }
            else
            {
                StopAutoSave();
            }
        }

        private void OnAutoSaveFrequencyChanged(float frequency)
        {
            if (PlayerSettings.AutoSaveEnabled)
            {
                StartAutoSave();
            }
        }
    }
}
