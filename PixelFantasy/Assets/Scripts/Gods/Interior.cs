using System;
using System.Collections;
using Buildings;
using UnityEngine;

namespace Gods
{
    public class Interior : MonoBehaviour
    {
        public Transform EntrancePos;
        public Building Building;
        
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
