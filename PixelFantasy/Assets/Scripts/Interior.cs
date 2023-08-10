using System.Collections;
using Buildings;
using UnityEngine;
using UnityEngine.Serialization;

public class Interior : MonoBehaviour
{
    public Transform EntrancePos;
    [FormerlySerializedAs("Building")] public BuildingOld buildingOld;
        
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