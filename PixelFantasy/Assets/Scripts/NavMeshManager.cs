using System.Collections;
using System.Collections.Generic;
using Managers;
using NavMeshPlus.Components;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : Singleton<NavMeshManager>
{
    private NavMeshSurface _meshSurface;
    private void Awake()
    {
        _meshSurface = GetComponent<NavMeshSurface>();
        _meshSurface.hideEditorLogs = true;
    }

    private void Start()
    {
        _meshSurface.BuildNavMeshAsync();
    }

    [ContextMenu("Update Nav Mesh")]
    public void UpdateNavMesh()
    {
        if (_meshSurface.navMeshData != null)
        {
            _meshSurface.UpdateNavMesh(_meshSurface.navMeshData);
        }
        else
        {
            _meshSurface.BuildNavMeshAsync();
        }
    }
}
