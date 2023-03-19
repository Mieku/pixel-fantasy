using System;
using System.Collections;
using UnityEngine;

namespace Gods
{
    public class Interior : MonoBehaviour
    {
        public Transform EntrancePos;
        
        private void Start()
        {
            StartCoroutine(UpdateNavSequence());
        }

        private IEnumerator UpdateNavSequence()
        {
            yield return new WaitForSeconds(2);
            NavMeshManager.Instance.UpdateNavMesh();
        }
    }
}
