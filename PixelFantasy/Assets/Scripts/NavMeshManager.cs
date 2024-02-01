using System;
using Managers;
using NavMeshPlus.Components;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : Singleton<NavMeshManager>
{
    private NavMeshSurface _meshSurface;

    protected override void Awake()
    {
        base.Awake();
        _meshSurface = GetComponent<NavMeshSurface>();
        _meshSurface.hideEditorLogs = true;
    }
    
    private void Start()
    {
        _meshSurface.BuildNavMesh();
    }

    [Button("Update NavMesh")]
    public void UpdateNavMesh()
    {
        if (_meshSurface != null)
        {
            _meshSurface.UpdateNavMesh(_meshSurface.navMeshData);
        }
    }
}
